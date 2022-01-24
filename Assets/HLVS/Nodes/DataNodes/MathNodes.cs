using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Math/Create Position"), ConverterNode(typeof(float), typeof(Vector3))]
	public class CreateVector3Node : HlvsDataNode, IConversionNode
	{
		public override string name => "Create Position";

		[Input("X")]
		public float x;

		[Input("Y")]
		public float y;

		[Input("Z")]
		public float z;

		[Output("Result")]
		public Vector3 result;

		public override ProcessingStatus Evaluate()
		{
			result = new Vector3(x, y, z);

			return ProcessingStatus.Finished;
		}

		public string GetConversionInput()
		{
			return nameof(x);
		}

		public string GetConversionOutput()
		{
			return nameof(result);
		}
	}
	
	[Serializable, NodeMenuItem("Math/Normalize")]
	public class NormalizeNode : HlvsDataNode
	{
		public override string name => "Normalize";

		[Input("Value")]
		public Vector3 value;

		[Output("Result")]
		public Vector3 result;

		public override ProcessingStatus Evaluate()
		{
			result = value.normalized;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Math/Multiply Vector")]
	public class MultiplyVectorNode : HlvsDataNode
	{
		public override string name => "Multiply Vector";

		[Input("Value")]
		public Vector3 value;

		[Input("Factor")]
		public float factor;

		[Output("Result")]
		public Vector3 result;

		public override ProcessingStatus Evaluate()
		{
			result = value * factor;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Math/Vector Length")]
	public class VectorLengthNode : HlvsDataNode
	{
		public override string name => "Vector Length";

		[Input("Value")]
		public Vector3 value;

		[Output("Length")]
		public float length;

		public override ProcessingStatus Evaluate()
		{
			length = value.magnitude;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Conditions/All True")]
	public class BoolAndNode : HlvsDataNode
	{
		public override string name => "All True";

		[Input("Condition 1")]
		public bool cond1;
		[Input("Condition 1")]
		public bool cond2;

		[Output("Result")]
		public bool result;

		public override ProcessingStatus Evaluate()
		{
			result = cond1 & cond2;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Conditions/Any True")]
	public class BoolOrNode : HlvsDataNode
	{
		public override string name => "Any True";

		[Input("Condition 1")]
		public bool cond1;
		[Input("Condition 1")]
		public bool cond2;

		[Output("Result")]
		public bool result;

		public override ProcessingStatus Evaluate()
		{
			result = cond1 | cond2;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Conditions/Not Condition")]
	public class BoolNorNode : HlvsDataNode
	{
		public override string name => "Not Condition";

		[Input("Condition")]
		public bool cond;

		[Output("Result")]
		public bool result;

		public override ProcessingStatus Evaluate()
		{
			result = !cond;

			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Conditions/Condition Value")]
	public class BoolValueNode : HlvsDataNode
	{
		public override string name => "Condition";

		[Input("Condition")]
		public bool cond;

		[Output("Result")]
		public bool result;

		public override ProcessingStatus Evaluate()
		{
			result = cond;

			return ProcessingStatus.Finished;
		}
	}
}