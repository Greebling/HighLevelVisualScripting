using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Input/Get Button Input")]
	public class GetButtonInputNode : HlvsDataNode
	{
		public override          string name => "Get Button Input";
		
		[Input("Button")] public string buttonName;
		[Output("Value")] public bool   output;

		public override ProcessingStatus Evaluate()
		{
			output = Input.GetButton(buttonName);
			return ProcessingStatus.Finished;
		}
	}
}