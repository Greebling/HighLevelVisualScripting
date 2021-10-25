using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Load Scene")]
	public class LoadSceneNode : HlvsActionNode
	{
		[Input("Scene")] public Scene scene;

		[Input("Load Additive"), ShowAsDrawer] public bool loadAdditive;
		
		public override string name => "Load Scene";
		protected override void Process()
		{
			SceneManager.LoadScene(scene.buildIndex, loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		}
	}
}