using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Input/Get Joystick Input")]
	public class GetJoystickInputNode : HlvsDataNode
	{
		[Input("Axis")] public string axisName;
		[Output("Value")] public float output;

		public override string name => "Get Joystick Input";

		public override ProcessingStatus Evaluate()
		{
			output = Input.GetAxisRaw(axisName);
			return base.Evaluate();
		}
	}
	
	[Serializable, NodeMenuItem("Input/Get Button Input")]
	public class GetButtonInputNode : HlvsDataNode
	{
		[Input("Button")] public string buttonName;
		[Output("Value")] public bool output;

		public override string name => "Get Button Input";

		public override ProcessingStatus Evaluate()
		{
			output = Input.GetButton(buttonName);
			return base.Evaluate();
		}
	}
}