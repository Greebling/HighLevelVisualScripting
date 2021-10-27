using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Runtime
{
	public class HLVSBehaviour : MonoBehaviour
	{
		public HlvsGraph graph;

		[SerializeReference] public List<ExposedParameter> graphParameters = new List<ExposedParameter>();

		public void SetVariable(string name, object value)
		{
			Debug.Assert(graph, "SetVariable called on behaviour without a graph");
			var param = graphParameters.Find(parameter => parameter.name == name);
			Debug.Assert(param != null, $"Parameter {name} not found in graph {graph.name}");
			param.value = value;
		}

		public void CreateFittingParamList()
		{
			if (!graph)
			{
				graphParameters.Clear();
				return;
			}

			if (graphParameters.Count != 0)
			{
				// check if we have to migrate this behaviours parameters to a new version, so we keep the inputted values
				bool hasSimilarFields = graphParameters.Any(parameter =>
					graph.parametersBlueprint.Any(exposedParameter => parameter.guid == exposedParameter.guid));

				if (hasSimilarFields)
				{
					// remove values that are not in the blueprint anymore
					graphParameters.RemoveAll(parameter =>
						graph.parametersBlueprint.All(exposedParameter => parameter.guid != exposedParameter.guid));

					OnParamListChanged();
					return;
				}
			}

			graphParameters = graph.parametersBlueprint
				.Select(ExposedParameter.CopyParameter)
				.ToList();
		}

		public void OnParamListChanged()
		{
			if (graph.parametersBlueprint.Count == 0)
			{
				graphParameters.Clear();
				return;
			}


			if (graph.parametersBlueprint.Count > graphParameters.Count)
			{
				// add missing entries
				var missingParams = graph.parametersBlueprint.Where(parameter =>
					graphParameters.All(exposedParameter => exposedParameter.guid != parameter.guid));
				foreach (var parameter in missingParams)
				{
					graphParameters.Add(ExposedParameter.CopyParameter(parameter));
				}
			}
			else if (graph.parametersBlueprint.Count < graphParameters.Count)
			{
				// TODO: Somewhere in here is a bug
				Debug.Assert(graph);
				Debug.Assert(graph.parametersBlueprint != null);
				Debug.Assert(graphParameters != null);
				// removed entries
				var removedElements = graphParameters.FindAll(parameter =>
					graph.parametersBlueprint.All(blueprintP => blueprintP.guid != parameter.guid)).ToArray();
				Array.ForEach(removedElements, parameter => graphParameters.Remove(parameter));
			}

			Debug.Assert(graph.parametersBlueprint.Count == graphParameters.Count);

			// correct parameter order
			{
				Dictionary<string, int> guidToIndex = new Dictionary<string, int>();
				var blueprintParams = graph.parametersBlueprint;
				for (int i = 0; i < blueprintParams.Count; i++)
				{
					guidToIndex.Add(blueprintParams[i].guid, i);
				}

				var newParamList = new List<ExposedParameter>();
				newParamList.AddRange(Enumerable.Repeat((ExposedParameter)null, graphParameters.Count));
				for (int i = 0; i < blueprintParams.Count; i++)
				{
					var currParam = graphParameters[i];
					var index = guidToIndex[currParam.guid];
					newParamList[index] = currParam;
				}

				newParamList.ForEach(parameter => Debug.Assert(parameter != null, "Reordering did not succeed"));

				graphParameters = newParamList;
			}

			// correct non matching names
			foreach (var parameter in graphParameters)
			{
				var graphParam =
					graph.parametersBlueprint.Find(exposedParameter => exposedParameter.guid == parameter.guid);
				if (graphParam != null)
					parameter.name = graphParam.name;
			}
		}

		private void OnEnable()
		{
			if (graph)
				graph.LinkToScene(gameObject.scene);
		}

		private void Start()
		{
			if (graph)
				graph.RunStartNodes(graphParameters);
		}

		private void Update()
		{
			if (graph)
				graph.RunUpdateNodes(graphParameters);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (graph)
				graph.RunOnTriggerEnteredNodes(graphParameters);
		}
	}
}