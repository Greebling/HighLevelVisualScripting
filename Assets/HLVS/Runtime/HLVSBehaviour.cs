using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Runtime
{
	public class HLVSBehaviour : MonoBehaviour
	{
		public HlvsGraph graph;

		[SerializeReference] public List<ExposedParameter> graphParameters = new List<ExposedParameter>();

		public void CreateFittingParamList()
		{
			if (!graph)
			{
				graphParameters.Clear();
				return;
			}

			graphParameters = graph.parametersBlueprint
				.Select(ExposedParameter.CopyParameter)
				.ToList();
		}

		public void OnParamListChanged()
		{
			if (graph.parametersBlueprint.Count == graphParameters.Count)
			{
				return;
			}

			if (graph.parametersBlueprint.Count == 0)
			{
				graphParameters.Clear();
				return;
			}

			if (graph.parametersBlueprint.Count > graphParameters.Count)
			{
				// added an entry at the end
				var missingParam = graph.parametersBlueprint.Last();
				graphParameters.Add(ExposedParameter.CopyParameter(missingParam));
			}
			else
			{
				// removed an entry somewhere
				graphParameters.FindAll(parameter => graph.parametersBlueprint.All(exposedParameter => exposedParameter.name != parameter.name))
					.ForEach(parameter => graphParameters.Remove(parameter));
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