using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("Math/Round"), ConverterNode(typeof(float), typeof(int))]
	public class RoundNode : HlvsDataNode, IConversionNode
	{
		[Input("In")]
		public float input;
		
		[Output("Out")]
		public int output;
		
		public RoundMode rounding;

		public enum RoundMode
		{
			Standard, Ceil, Floor
		}

		public override string name => "Round";

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
			switch (rounding)
			{
				case RoundMode.Standard:
					output = Mathf.RoundToInt(input);
					break;
				case RoundMode.Ceil:
					output = Mathf.CeilToInt(input);
					break;
				case RoundMode.Floor:
					output = Mathf.FloorToInt(input);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return ProcessingStatus.Finished;
		}
	}
}