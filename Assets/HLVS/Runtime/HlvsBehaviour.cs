using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Runtime
{
	public class HlvsBehaviour : MonoBehaviour
	{
		[HideInInspector] 
		public HlvsGraph graph;

		private HlvsGraph _runtimeGraph;

		[SerializeReference] [HideInInspector] public List<ExposedParameter> graphParameters = new List<ExposedParameter>();

		public HlvsGraph CurrentGraph => 
			_runtimeGraph && Application.isPlaying ? 
				_runtimeGraph : graph;

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

		public bool IsParameterListOutdated()
		{
			if (!graph || graphParameters.Count == 0)
				return false;

			return graphParameters.Zip(graph.parametersBlueprint,
				(parameter, exposedParameter) => parameter.guid == exposedParameter.guid).All(b => b);
		}

		public void OnParamListChanged()
		{
			if (!graph || graph.parametersBlueprint.Count == 0)
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

		private void Awake()
		{
			if (graph)
			{
				_runtimeGraph = Instantiate(graph);
				_runtimeGraph.activeGameObject = gameObject;
				_runtimeGraph.name = graph.name + " (Instance)";
				_runtimeGraph.Init();
				_runtimeGraph.LinkToScene(gameObject.scene);
			}

			ZoneCleaner.Instantiate();
		}

		private void OnDestroy()
		{
			Destroy(_runtimeGraph);
			_runtimeGraph = null;
		}

		private void Start()
		{
			if (_runtimeGraph)
			{
				_runtimeGraph.SetParameterValues(gameObject, graphParameters);
				_runtimeGraph.RunStartNodes();
			}
		}

		private void Update()
		{
			if (_runtimeGraph)
			{
				_runtimeGraph.SetParameterValues(gameObject, graphParameters);
				_runtimeGraph.RunUpdateNodes();
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (_runtimeGraph)
			{
				_runtimeGraph.SetParameterValues(gameObject, graphParameters);
				_runtimeGraph.RunOnTriggerEnteredNodes();
			}
		}
	}
}