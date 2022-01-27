using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Data/Game Time")]
	public class GameTimeNode : HlvsDataNode
	{
		public override string name => "Game Time";

		[Output("Current Time")]
		public float result;

		public override ProcessingStatus Evaluate()
		{
			result = Time.time;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Data/Delta Time")]
	public class DeltaTimeNode : HlvsDataNode
	{
		public override string name => "Delta Time";

		[Output("Delta Time")]
		public float result;

		public override ProcessingStatus Evaluate()
		{
			result = Time.deltaTime;

			return ProcessingStatus.Finished;
		}
	}
}