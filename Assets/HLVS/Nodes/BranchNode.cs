using System;
using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Branch")]
	public class BranchNode : HlvsNode
	{
		[Input(" ", true)] public ExecutionLink inputLink;
		[Output("True", false)] public ExecutionLink trueLink;
		[Output("False", false)] public ExecutionLink falseLink;
	}
}