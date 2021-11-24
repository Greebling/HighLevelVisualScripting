using System;
using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public class ExecutionStarterNode : HlvsNode
	{
		[Output(" ", false)]
		public readonly ExecutionLink followingAction = null;

		public override string nextExecutionLink => nameof(followingAction);
	}
	
	[Serializable, NodeMenuItem("Start/Every Frame")]
	public class OnUpdateNode : ExecutionStarterNode
	{
		public override string name => "Every Frame";
	}
	
	[Serializable, NodeMenuItem("Start/On Start")]
	public class OnStartNode : ExecutionStarterNode
	{
		public override string name => "On Start";
	}
	
	[Serializable, NodeMenuItem("Start/On Trigger")]
	public class OnTriggerEnteredNode : ExecutionStarterNode
	{
		public override string name => "On Trigger Collider Entered";
	}
	
	[Serializable, NodeMenuItem("Start/On Collision")]
	public class OnCollisionNode : ExecutionStarterNode
	{
		public override string name => "On Collision";
	}
}