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
	}
}