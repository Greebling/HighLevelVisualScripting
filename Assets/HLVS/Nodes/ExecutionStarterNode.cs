using GraphProcessor;
using HLVS.Runtime;

namespace HLVS.Nodes
{
	public class ExecutionStarterNode : HlvsNode
	{
		[Output(" ", false)]
		public ExecutionLink followingAction;
	}
}