using System;
using UnityEngine;

namespace HLVS.Runtime
{
	public class HLVSBehaviour : MonoBehaviour
	{
		public HlvsGraph graph;

		private void OnEnable()
		{
			if (graph)
				graph.LinkToScene(gameObject.scene);
		}

		private void Start()
		{
			graph.RunStartNodes();
		}

		private void Update()
		{
			graph.RunUpdateNodes();
		}
	}
}