using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public class ExecutionStarterNode : HlvsNode
	{
		[Output("Action")]
		public ExecutionLink followingAction;
	}
}