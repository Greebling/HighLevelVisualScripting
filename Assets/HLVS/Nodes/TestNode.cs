using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Test Node")]
	public class TestNode : HlvsActionNode
	{
		[Input("Gobj")] public GameObject gobj;
		[Input("Float Val")] public float floatVal;
		[Input("A Color")] public Color colorVal;
		[Input("An enumeration"), ShowAsDrawer] public MyEnum enumField;

		public enum MyEnum
		{
			FirstOption, SecondOption, AnotherOption,
		}
	}
}