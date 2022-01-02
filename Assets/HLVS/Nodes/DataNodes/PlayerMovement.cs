using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Input/Player Movement")]
	public class PlayerMovement : HlvsDataNode
	{
		public override string name => "Player Movement";

		[Input("Type")]
		public MovementType inputType;

		[Input("Max Speed")] [LargerThan(0)]
		public float maxSpeed = 1;

		[Output("Movement")]
		public Vector3 direction;

		[Output("Speed")]
		public float speed;

		public enum MovementType
		{
			TopDown,
			SideScroller
		}

		public override ProcessingStatus Evaluate()
		{
			switch (inputType)
			{
				case MovementType.TopDown:
					direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
					break;
				case MovementType.SideScroller:
					direction = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			direction *= maxSpeed;
			speed = direction.magnitude;

			return ProcessingStatus.Finished;
		}
	}
}