using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public abstract class HlvsActionNode : HlvsNode
	{
		[Input(" ", false)]
		public ExecutionLink previousAction;
		
		[Output(" ", false)]
		public ExecutionLink followingAction;
	}
}