using System;
using GraphProcessor;
using UnityEngine.Serialization;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Begin/On Trigger")]
	public class OnTriggerEnteredNode : ExecutionStarterNode
	{
		public override string name => "On Trigger Entered";
	}
}