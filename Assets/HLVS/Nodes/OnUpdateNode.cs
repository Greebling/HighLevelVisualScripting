using System;
using GraphProcessor;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Begin/On Update")]
	public class OnUpdateNode : ExecutionStarterNode
	{
		public override string name => "On Update";
	}
}