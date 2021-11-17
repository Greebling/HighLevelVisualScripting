using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using IkTools.FormulaParser;
using UnityEngine;

namespace HLVS.Nodes
{
	public enum ProcessingStatus
	{
		Finished,
		Unfinished
	}

	[Serializable]
	public abstract class HlvsNode : BaseNode, ISerializationCallbackReceiver
	{

		[SerializeField, HideInInspector] internal List<FormulaPair> fieldToFormula = new List<FormulaPair>();
		
		/// <summary>
		/// Maps the name of a node field to the guid of an exposed parameter in the graph and gives its reference type
		/// </summary>
		internal Dictionary<string, string> fieldToParamGuid = new Dictionary<string, string>();

		/// <summary>
		/// Used for serialization of fieldToParamGuid
		/// </summary>
		[SerializeField] private List<StringStringPair> varToGuidSerialization;
		
		internal BaseGraph Graph
		{
			get => graph;
			set => graph = value;
		}
		
		protected sealed override void Process()
		{
			ParseExpressions();
			UpdateParameterValues();
		}

		/// <summary>
		/// Called when node shall be evaluated and execute its actions
		/// </summary>
		/// <returns>Whether the node finished all of its evaluation in this frame</returns>
		public virtual ProcessingStatus Evaluate()
		{
			return ProcessingStatus.Finished;
		}

		/// <summary>
		/// Useful for coroutine like nodes to reset their status
		/// </summary>
		public virtual void Reset()
		{
		}

		internal void ParseExpressions()
		{
			var graph = this.graph as HlvsGraph;
			foreach (var formulaPair in fieldToFormula)
			{
				if(formulaPair.formula.Expression == string.Empty)
					continue;
				
				try
				{
					var targetField = GetType().GetField(formulaPair.fieldName);

					var template = formulaPair.formula.Template();
					var function = template.Resolve(graph);
					var trueValue = function(graph);
					
					var value = Convert.ChangeType(trueValue, targetField.FieldType);
					targetField.SetValue(this, value);
				}
				catch (Exception e)
				{
					Debug.Log($"Formula error in {formulaPair.fieldName}: {e.Message}");
				}
			}
		}

		public static bool CanBeExpression(Type type)
		{
			return  type == typeof(float) || type == typeof(int);
		}

		/// <summary>
		/// Gets the values of blackboard and graph parameter variables
		/// </summary>
		internal void UpdateParameterValues()
		{
			var graph = this.graph as HlvsGraph;
			foreach (var fieldToParam in fieldToParamGuid)
			{
				var parameter = graph.GetVariableByGuid(fieldToParam.Value);
				GetType().GetField(fieldToParam.Key).SetValue(this, parameter.value);
			}
		}

		public bool HasExpressionField(string fieldName)
		{
			return fieldToFormula.Any(pair => pair.fieldName == fieldName);
		}

		public int AddExpressionField(string fieldName)
		{
			var addedFormula = new RawFormula();
			addedFormula.Expression = String.Empty;
			var pair = new FormulaPair { formula = addedFormula, fieldName = fieldName };
			fieldToFormula.Add(pair);
			return fieldToFormula.Count - 1;
		}

		public int IndexOfExpression(string fieldName)
		{
			if (HasExpressionField(fieldName))
				return fieldToFormula.FindIndex(pair => pair.fieldName == fieldName);

			return AddExpressionField(fieldName);
		}

		public RawFormula GetFieldExpression(string fieldName)
		{
			if (HasExpressionField(fieldName))
				return fieldToFormula.Find(pair => pair.fieldName == fieldName).formula;

			var addedFormula = new RawFormula();
			var pair = new FormulaPair { formula = addedFormula, fieldName = fieldName };
			fieldToFormula.Add(pair);
			return addedFormula;
		}

		public void SetFieldToReference(string fieldName, string parameterGuid)
		{
			if (fieldToParamGuid.ContainsKey(fieldName))
				fieldToParamGuid[fieldName] = parameterGuid;
			else
			{
				for (int i = 0; i < fieldToFormula.Count; i++)
				{
					var formulaPair = fieldToFormula[i];
					if (formulaPair.fieldName == fieldName)
					{
						fieldToFormula.RemoveAt(i);
						break;
					}
				}
				
				fieldToFormula.RemoveAll(pair => pair.fieldName == fieldName);
				fieldToParamGuid.Add(fieldName, parameterGuid);
			}
		}

		public void UnsetFieldReference(string fieldName)
		{
			fieldToParamGuid.Remove(fieldName);
		}

		public void OnBeforeSerialize()
		{
			// save dictionary as list
			varToGuidSerialization = new List<StringStringPair>();
			if (fieldToParamGuid != null)
			{
				foreach (var keyValuePair in fieldToParamGuid)
				{
					varToGuidSerialization.Add(new StringStringPair(keyValuePair.Key, keyValuePair.Value));
				}
			}
		}

		public void OnAfterDeserialize()
		{
			// transform list back to dictionary
			if (varToGuidSerialization != null)
			{
				fieldToParamGuid = new Dictionary<string, string>();
				foreach (var serializedValues in varToGuidSerialization)
				{
					fieldToParamGuid.Add(serializedValues.Item1, serializedValues.Item2);
				}
			}
		}

		[Serializable]
		public class FormulaPair
		{
			public string fieldName;
			[SerializeField] public RawFormula formula;
		}


		[Serializable]
		internal struct StringStringPair
		{
			public string Item1;
			public string Item2;

			public StringStringPair(string item1, string item2)
			{
				Item1 = item1;
				Item2 = item2;
			}
		}
	}
}