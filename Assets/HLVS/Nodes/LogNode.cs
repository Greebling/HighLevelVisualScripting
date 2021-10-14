using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text"), ShowAsDrawer] public string textToLog;

		public override string name => "Log in Console";

		protected override void Process()
		{
			if (!string.IsNullOrEmpty(textToLog))
				Debug.Log(textToLog);
		}
	}
}