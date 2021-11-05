using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text")] public string textToLog;

		public override string name => "Log in Console";

		public override void Evaluate()
		{
			if (!string.IsNullOrEmpty(textToLog))
				Debug.Log(textToLog);
		}
	}
}