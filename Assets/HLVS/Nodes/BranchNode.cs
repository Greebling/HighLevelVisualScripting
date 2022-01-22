using System;
using System.Linq;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes
{
	public abstract class BranchingNode : HlvsFlowNode
	{
		[Input(" ")]
		public ExecutionLink previousAction;
		
		public override HlvsNode GetPreviousNode()
		{
			return (HlvsNode) inputPorts.Find(port => port.fieldName == nameof(previousAction)).GetEdges().FirstOrDefault()?.outputNode;
		}
	}

	[Serializable, NodeMenuItem("Branch/Conditional Branch")]
	public class BranchNode : BranchingNode
	{
		public override string name => "Condition";

		[Input("Condition")]
		public bool condition = true;

		[Output("True", false)]
		public ExecutionLink trueLink;

		[Output("False", false)]
		public ExecutionLink falseLink;

		public override string[] GetNextExecutionLinks()
		{
			return condition ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}


	[Serializable, NodeMenuItem("Branch/Is Same Gameobject")]
	public class IsSameGameobjectNode : BranchingNode
	{
		public override string name => "Is Same Gameobject";

		[Input("Object")]
		public GameObject obj1;

		[Input("Other")]
		public GameObject obj2;

		[Output("Same", false)]
		public ExecutionLink trueLink;

		[Output("Different", false)]
		public ExecutionLink falseLink;

		public override string[] GetNextExecutionLinks()
		{
			return obj1 == obj2 ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}
}