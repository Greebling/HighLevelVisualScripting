using System;
using GraphProcessor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Scenes/Load Scene")]
	public class LoadSceneNode : HlvsActionNode
	{
		public override string name => "Load Scene";

		[Input("Scene name")]
		public string toLoad;

		[Input("Mode")]
		public LoadSceneMode loadAdditive = LoadSceneMode.Single;

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

	[Serializable, NodeMenuItem("Game/Quit")]
	public class QuitGameNode : HlvsActionNode
	{
		public override string name => "Quit Game";

		public override ProcessingStatus Evaluate()
		{
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
			Debug.LogWarning("Quitting Game..."); // give users a message so they wont wonder why their editor suddenly stopped
#else
			Application.Quit();
#endif

			return ProcessingStatus.Finished;
		}
	}
}