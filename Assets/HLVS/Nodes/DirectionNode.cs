using System;
using GraphProcessor;
using HLVS.Editor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable] [HlvsType]
	public class Direction
	{
		public Direction(Vector3 val)
		{
			_value = val.normalized;
		}

		public Vector3 value
		{
			get => _value;
			set => _value = value.normalized;
		}

		private Vector3 _value;
	}

	[Serializable, NodeMenuItem("HLVS/Direction")]
	public class DirectionNode : HlvsNode
	{
		[Input("Value"), ShowAsDrawer] public Vector3 direction = new Vector3(1, 0, 0);

		[Output("Value")] public Direction output;

		public override void Evaluate()
		{
			output = new Direction(direction);
		}
	}
}