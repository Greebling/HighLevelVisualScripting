using System;
using System.Collections.Generic;
using GraphProcessor;
using IkTools.FormulaParser;
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

	[Serializable, NodeMenuItem("Conditions/Math Expression")]
	public class MathNode : HlvsDataNode, IVariableProvider<MathNode>
	{
		public override string name => "Math";

		[HideInInspector]
		public RawFormula formula = new RawFormula();

		[Input("Data")]
		public object inputData;

		[Output("Result")]
		public float result;

		[NonSerialized]
		public string errors;

		private FormulaTemplate        _template;
		private Func<MathNode, double> _parsedExpression;

		[SerializeField, HideInInspector]
		private List<string> variableNames = new();

		private Dictionary<string, double> _variableData = new();

		protected override void Enable()
		{
			RecompileExpression();
		}

		public override ProcessingStatus Evaluate()
		{
			if (_parsedExpression == null)
			{
				Debug.LogError($"Unparsed expression in node {name} of graph {graph}");
				return ProcessingStatus.Abort;
			}

			result = InvokeExpression();
			_variableData.Clear();
			return ProcessingStatus.Finished;
		}

		[CustomPortBehavior(nameof(inputData))]
		IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges)
		{
			foreach (string variableName in variableNames)
			{
				yield return new PortData
				{
					displayName = variableName,
					displayType = typeof(float),
					identifier = variableName,
				};
			}
		}

		[CustomPortInput(nameof(inputData), typeof(float))]
		void PullInputs(List<SerializableEdge> connectedEdges)
		{
			foreach (SerializableEdge edge in connectedEdges)
			{
				var varName = edge.inputPort.portData.identifier;

				_variableData.Add(varName, (float)edge.passThroughBuffer);
			}
		}

		public void RecompileExpression()
		{
			errors = null;

			try
			{
				if (!string.IsNullOrEmpty(formula.Expression))
				{
					_template = formula.Template();
					_parsedExpression = _template?.Compile(this);
				}
			}
			catch (Exception e)
			{
				// catch compile errors
				errors = e.Message;
			}
		}

		private float InvokeExpression()
		{
#if UNITY_EDITOR
			// needed for variable name collection
			if (!Application.isPlaying)
				variableNames.Clear();
#endif
			return (float)_parsedExpression(this);
		}

		public Func<MathNode, double> Get(string varName)
		{
			return node => node.GetVariable(varName);
		}

		private double GetVariable(string varName)
		{
#if UNITY_EDITOR
			// collect variable names
			if (!Application.isPlaying)
			{
				if (!variableNames.Contains(varName))
					variableNames.Add(varName);
			}
#endif
			if (_variableData.TryGetValue(varName, out double val))
			{
				return val;
			}

			return 0;
		}
	}
}