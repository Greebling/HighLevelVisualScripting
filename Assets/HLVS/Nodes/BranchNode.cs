using System;
using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Branch")]
	public class BranchNode : HlvsFlowNode
	{
		[Input(" ", true)]
		public ExecutionLink inputLink;

		[Input("Condition", true)]
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
}