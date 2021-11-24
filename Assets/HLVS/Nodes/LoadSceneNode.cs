using System;
using GraphProcessor;
using UnityEngine.SceneManagement;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Scene/Load Scene")]
	public class LoadSceneNode : HlvsActionNode
	{
		[Input("Scene")] public Scene scene;

		[Input("Load Additive")] public bool loadAdditive;
		
		public override string name => "Load Scene";
		
		public override ProcessingStatus Evaluate()
		{
			SceneManager.LoadScene(scene.buildIndex, loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			return ProcessingStatus.Finished;
		}
	}
}