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
				.Select(param => Activator.CreateInstance(param.GetType()) as ExposedParameter)
				.ToList();

			for (int i = 0; i < graphParameters.Count; i++)
			{
				graphParameters[i].name = graph.parametersBlueprint[i].name;
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