using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Runtime
{
	public class HLVSBehaviour : MonoBehaviour
	{
		public HlvsGraph graph;

		[SerializeReference]
		public List<ExposedParameter> graphParameters = new List<ExposedParameter>();

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