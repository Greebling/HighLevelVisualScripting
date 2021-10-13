using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public abstract class HlvsActionNode : HlvsNode
	{
		[Input("Action", false)]
		public ExecutionLink previousAction;
		
		[Output("Action", false)]
		public ExecutionLink followingAction;
	}
}