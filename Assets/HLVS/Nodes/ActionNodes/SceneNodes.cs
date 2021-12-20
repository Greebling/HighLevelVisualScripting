using System;
using GraphProcessor;
using UnityEngine.SceneManagement;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Scenes/Load Scene")]
	public class LoadSceneNode : HlvsActionNode
	{
		public override string name => "Load Scene";

		[Input("Scene name")]
		public string toLoad;
		
		[Input("Mode")] public LoadSceneMode loadAdditive = LoadSceneMode.Single;

		public override ProcessingStatus Evaluate()
		{
			SceneManager.LoadScene(toLoad, loadAdditive);
			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Scenes/Unload Scene")]
	public class UnloadSceneNode : HlvsActionNode
	{
		public override string name => "Unload Scene";

		[Input("Scene name")]
		public string toUnload;

		public override ProcessingStatus Evaluate()
		{
			SceneManager.UnloadSceneAsync(toUnload);
			return ProcessingStatus.Finished;
		}
	}
}