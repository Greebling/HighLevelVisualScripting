using System;
using System.Globalization;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Convert/Read Integer from String"), ConverterNode(typeof(string), typeof(int))]
	public class ParseIntNode : HlvsDataNode, IConversionNode
	{
		[Input("In")]
		public string input;

		[Output("Out")]
		public int output;

		public override string name => "Read Integer";

		public string GetConversionInput()
		{
			return nameof(input);
		}

		public string GetConversionOutput()
		{
			return nameof(output);
		}

		public override ProcessingStatus Evaluate()
		{
			output = int.Parse(input);
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Convert/Read Float from String"), ConverterNode(typeof(string), typeof(float))]
	public class ParseFloatNode : HlvsDataNode, IConversionNode
	{
		[Input("In")]
		public string input;

		[Output("Out")]
		public float output;

		public override string name => "Read Float";

		public string GetConversionInput()
		{
			return nameof(input);
		}

		public string GetConversionOutput()
		{
			return nameof(output);
		}

		public override ProcessingStatus Evaluate()
		{
			output = float.Parse(input, CultureInfo.InvariantCulture);
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Convert/Float to String"), ConverterNode(typeof(float), typeof(string))]
	public class FloatToStringNode : HlvsDataNode, IConversionNode
	{
		[Input("In")]
		public float input;

		[Input("Decimals")]
		[LargerOrEqual(0)]
		public int decimalPlace = 2;

		[Output("Out")]
		public string output;

		public override string name => "To String";

		public string GetConversionInput()
		{
			return nameof(input);
		}

		public string GetConversionOutput()
		{
			return nameof(output);
		}

		public override ProcessingStatus Evaluate()
		{
			var mult = Mathf.Pow(10, decimalPlace);
			float val = Mathf.Round(input * mult) / mult;
			output = val.ToString(CultureInfo.InvariantCulture);
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Convert/Int to String"), ConverterNode(typeof(int), typeof(string))]
	public class IntToStringNode : HlvsDataNode, IConversionNode
	{
		[Input("In")]
		public float input;

		[Output("Out")]
		public string output;

		public override string name => "To String";

		public string GetConversionInput()
		{
			return nameof(input);
		}

		public string GetConversionOutput()
		{
			return nameof(output);
		}

		public override ProcessingStatus Evaluate()
		{
			output = input.ToString(CultureInfo.InvariantCulture);
			return ProcessingStatus.Finished;
		}
	}
}