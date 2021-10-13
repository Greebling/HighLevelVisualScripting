using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public abstract class HlvsActionNode : HlvsNode
	{
		[Input("Action")]
		public ExecutionLink previousAction;
		
		[Output("Action")]
		public ExecutionLink followingAction;

		public abstract void OnEvaluate();
	}
}