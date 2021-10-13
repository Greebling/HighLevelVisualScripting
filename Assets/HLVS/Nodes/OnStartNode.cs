using System;
using GraphProcessor;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/On Start")]
	public class OnStartNode : ExecutionStarterNode
	{
		public override string name => "On Start";
	}
}