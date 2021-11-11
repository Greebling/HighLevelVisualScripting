using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Test Node")]
	public class TestNode : HlvsActionNode
	{
		[Input("Gobj")] public GameObject gobj;
		[Input("Text")] public string string1Field;
		[Input("Float Val")] public float floatVal;
		[Input("Integer")] public int intValue;
		[Input("Vector")] public Vector3 vecValue;
		[Input("Vec4")] public Vector4 vec4Field;
		[Input("A Color")] public Color colorVal;
		[Input("A Truth Value")] public bool boolField;
		[Input("An enumeration")] public MyEnum enumField;

		public enum MyEnum
		{
			FirstOption, SecondOption, AnotherOption,
		}

		public override ProcessingStatus Evaluate()
		{
			Debug.Log(colorVal);
			return ProcessingStatus.Finished;
		}
	}
}