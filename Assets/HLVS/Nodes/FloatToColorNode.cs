using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Convert/Create Color"), ConverterNode(typeof(float), typeof(Color))]
	public class FloatToColorNode : HlvsDataNode, IConversionNode
	{
		public override string name => "Create Color";

		[Input("Red")]
		public float r;
		[Input("Green")]
		public float g;
		[Input("Blue")]
		public float b;

		[Output("Out")]
		public Color col;

		public override ProcessingStatus Evaluate()
		{
			col.r = r;
			col.g = g;
			col.b = b;
			return ProcessingStatus.Finished;
		}

		public string GetConversionInput()
		{
			return nameof(r);
		}

		public string GetConversionOutput()
		{
			return nameof(col);
		}
	}
}