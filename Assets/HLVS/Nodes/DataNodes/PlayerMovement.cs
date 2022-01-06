using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Input/Top-Down Movement")]
	public class TopDownMovementNode : HlvsActionNode
	{
		public override string name => "Top-Down Movement";

		[Input("Object")]
		public GameObject target;

		[Input("Speed")] [LargerOrEqual(0)]
		public float maxSpeed = 1;

		[Input("Rotation Speed")] [LargerOrEqual(0)]
		public float rotSpeed = 180;

		[Output("Speed")]
		public float speed;
		
		[Output("Direction")]
		public Vector3 movementDir;

		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogWarning("No object given to move");
				return ProcessingStatus.Finished;
			}
			
			movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
			
			// rotation
			if(movementDir.sqrMagnitude != 0)
			{
				var rot = Quaternion.LookRotation(movementDir);
				rot = Quaternion.RotateTowards(target.transform.rotation, rot, rotSpeed * Time.deltaTime);
				target.transform.rotation = rot;
			}
			
			movementDir *= maxSpeed;
			speed = movementDir.magnitude;
			
			// translation
			var rb = target.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.MovePosition(target.transform.position + Time.deltaTime * movementDir);
			} else
			{
				target.transform.position += Time.deltaTime * movementDir;
			}

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Input/Side-Scroller Movement")]
	public class SideScrollerMovementNode : HlvsActionNode
	{
		public override string name => "Side-Scroller Movement";

		[Input("Object")]
		public GameObject target;

		[Input("Speed")] [LargerOrEqual(0)]
		public float maxSpeed = 1;

		[Input("Rotation Speed")] [LargerOrEqual(0)]
		public float rotSpeed = 180;

		[Output("Speed")]
		public float speed;
		
		[Output("Direction")]
		public Vector3 movementDir;

		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogWarning("No object given to move");
				return ProcessingStatus.Finished;
			}
			
			movementDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
			
			// rotation
			if(movementDir.sqrMagnitude != 0)
			{
				var rot = Quaternion.LookRotation(movementDir);
				rot = Quaternion.RotateTowards(target.transform.rotation, rot, rotSpeed * Time.deltaTime);
				target.transform.rotation = rot;
			}
			
			movementDir *= maxSpeed;
			speed = movementDir.magnitude;
			
			// translation
			var rb = target.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.MovePosition(target.transform.position + Time.deltaTime * movementDir);
			} else
			{
				target.transform.position += Time.deltaTime * movementDir;
			}

			return ProcessingStatus.Finished;
		}
	}
}