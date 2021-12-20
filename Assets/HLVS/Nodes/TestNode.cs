using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Internal/Test Node")]
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
		
		[Output("Out: Gobj")]           public GameObject outGobj;
		[Output("Out: Text")]           public string     outString1Field;
		[Output("Out: Float Val")]      public float      outFloatVal;
		[Output("Out: Integer")]        public int        outIntValue;
		[Output("Out: Vector")]         public Vector3    outVecValue;
		[Output("Out: Vec4")]           public Vector4    outVec4Field;
		[Output("Out: A Color")]        public Color      outColorVal;
		[Output("Out: A Truth Value")]  public bool       outBoolField;
		[Output("Out: An enumeration")] public MyEnum     outEnumField;

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