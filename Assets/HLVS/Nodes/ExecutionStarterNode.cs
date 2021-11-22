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
}