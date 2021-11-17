using System;
using GraphProcessor;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Math/Round"), ConverterNode(typeof(float), typeof(int))]
	public class RoundNode : HlvsDataNode, IConversionNode
	{
		[Input("In")]
		public float input;
		
		[Output("Out")]
		public int output;

		public override string name => "Round";

		public string GetConversionInput()
		{
			return nameof(input);
		}

		public string GetConversionOutput()
		{
			return nameof(output);
		}
	}
}