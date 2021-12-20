using System;
using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Branch/Conditional Branch")]
	public class BranchNode : HlvsFlowNode
	{
		public override string name => "Condition";

		[Input(" ")]
		public ExecutionLink inputLink;

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
}