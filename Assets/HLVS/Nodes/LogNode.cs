using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text"), SerializeField] public string textToLog;

		public override string name => "Log in Console";

		public override void Evaluate()
		{
			if (!string.IsNullOrEmpty(textToLog))
				Debug.Log(textToLog);
		}

		[CustomPortOutput("Outp", typeof(bool))]
		public bool Print(List<SerializableEdge> s)
		{
			Debug.Log(s);
			return false;
		}
	}
}