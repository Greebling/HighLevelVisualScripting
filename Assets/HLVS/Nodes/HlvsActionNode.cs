using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public abstract class HlvsActionNode : HlvsNode
	{
		[Input(" ")]
		public ExecutionLink previousAction;
		
		[Output(" ", false)]
		public readonly ExecutionLink followingAction = new ExecutionLink();

		public override string nextExecutionLink => nameof(followingAction);

		public override bool isRenamable => true;
	}
}