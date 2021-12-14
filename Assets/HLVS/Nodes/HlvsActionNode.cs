using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public abstract class HlvsActionNode : HlvsNode
	{
		[Input(" ", true)]
		public ExecutionLink previousAction;
		
		[Output(" ", false)]
		public readonly ExecutionLink followingAction = new ExecutionLink();

		public override string nextExecutionLink => nameof(followingAction);

		public virtual bool hasMultipleFollowingNodes => false;
		
		public virtual string[] nextExecutionLinks => new []{nameof(followingAction)};
	}
}