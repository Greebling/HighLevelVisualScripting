using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text")]
		public string textToLog;

		public override void OnEvaluate()
		{
			Debug.Log(textToLog);
		}
	}
}