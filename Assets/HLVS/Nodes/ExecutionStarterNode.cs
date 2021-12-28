using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

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

	[Serializable, NodeMenuItem("Start/On Event")]
	public class OnEventNode : ExecutionStarterNode
	{
		public override string name => "On Event";

		public string eventName;

		public override ProcessingStatus Evaluate()
		{
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Start/On Zone Event")]
	public class OnZoneEventNode : ExecutionStarterNode
	{
		public override string name => "On Zone Event";

		
		public string               zoneName = "";
		
		public ZoneNotificationType activationType;

		[Output("Other Object")]
		public GameObject other;

		public override ProcessingStatus Evaluate()
		{
			return ProcessingStatus.Finished;
		}
	}
}