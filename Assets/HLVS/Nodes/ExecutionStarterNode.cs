using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public class ExecutionStarterNode : HlvsNode
	{
		[Output("Action", false)]
		public ExecutionLink followingAction;
	}
}