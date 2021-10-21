using System;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Move Object")]
	public class MoveObjectNode : HlvsActionNode
	{
		public override string name => "Move Object";
		
		/*
		[Input("Gameobject")] public GameObject objectToMove;

		[Input("Velocity"), ShowAsDrawer] public float velocity;

		[Input("Direction (Optional)")] public Direction direction;


		protected override void Process()
		{
			if (inputPorts.FirstOrDefault(port => port.fieldInfo.Name == nameof(direction))?.GetEdges().Count == 1)
			{
				objectToMove.transform.position += velocity * Time.deltaTime * direction.value.normalized;
			}
			else
			{
				objectToMove.transform.position += velocity * Time.deltaTime * objectToMove.transform.forward;
			}
		}
		*/
	}
}