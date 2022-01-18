using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Branch/Conditional Branch")]
	public class BranchNode : HlvsFlowNode
	{
		public override string name => "Condition";

		[Input(" ")]
		public ExecutionLink previousAction;

		[Input("Condition")]
		public bool condition = true;

		[Output("True")]
		public ExecutionLink trueLink;

		[Output("False")]
		public ExecutionLink falseLink;

		public override string[] GetNextExecutionLinks()
		{
			return condition ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}
	
	
	[Serializable, NodeMenuItem("Branch/Is Same Gameobject")]
	public class IsSameGameobjectNode : HlvsFlowNode
	{
		public override string name => "Is Same Gameobject";

		[Input(" ")]
		public ExecutionLink previousAction;

		[Input("Object")]
		public GameObject obj1;
		
		[Input("Other")]
		public GameObject obj2;

		[Output("Same")]
		public ExecutionLink trueLink;

		[Output("Different")]
		public ExecutionLink falseLink;

		public override string[] GetNextExecutionLinks()
		{
			return obj1 == obj2 ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}
}