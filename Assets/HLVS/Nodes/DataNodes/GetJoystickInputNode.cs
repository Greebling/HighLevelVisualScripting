using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Input/Axis Input")]
	public class GetJoystickInputNode : HlvsDataNode
	{
		public override          string name => "Get Axis Input";
		
		[Input("Axis")]   public string axisName;
		[Output("Value")] public float  output;

		public override ProcessingStatus Evaluate()
		{
			output = Input.GetAxisRaw(axisName);
			return ProcessingStatus.Finished;
		}
	}
	
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