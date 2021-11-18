using System;
using GraphProcessor;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("String/Read Integer"), ConverterNode(typeof(string), typeof(int))]
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
	
	[Serializable, NodeMenuItem("String/Read Float"), ConverterNode(typeof(string), typeof(float))]
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
			output = float.Parse(input);
			return ProcessingStatus.Finished;
		}
	}
}