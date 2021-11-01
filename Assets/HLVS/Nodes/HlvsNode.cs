using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable]
	internal enum ReferenceType
	{
		Blackboard,
		GraphParameter
	}

	[Serializable]
	public abstract class HlvsNode : BaseNode, ISerializationCallbackReceiver
	{
		/// <summary>
		/// Maps the name of a node field to the guid of an exposed parameter in the graph and gives its reference type
		/// </summary>
		internal Dictionary<string, (string, ReferenceType)> fieldToParamGuid = new Dictionary<string, (string, ReferenceType)>();

		/// <summary>
		/// Used for serialization of fieldToParamGuid
		/// </summary>
		[SerializeField] private List<(string, string, ReferenceType)> varToGuidSerialization;

		protected sealed override void Process()
		{
			UpdateParameterValues();
			Evaluate();
		}

		/// <summary>
		/// Called when node shall be evaluated and execute its actions
		/// </summary>
		public virtual void Evaluate(){}

		/// <summary>
		/// Gets the values of blackboard and graph parameter variables
		/// </summary>
		internal void UpdateParameterValues()
		{
			var graph = this.graph as HlvsGraph;
			foreach (var valueTuple in fieldToParamGuid)
			{
				var parameter = graph.GetVariable(valueTuple.Value.Item1);
				GetType().GetField(valueTuple.Key).SetValue(this, parameter.value);
			}
		}

		public void OnBeforeSerialize()
		{
			// save dictionary as list
			varToGuidSerialization = new List<(string, string, ReferenceType)>();
			if (fieldToParamGuid != null)
			{
				foreach (var keyValuePair in fieldToParamGuid)
				{
					varToGuidSerialization.Add((keyValuePair.Key, keyValuePair.Value.Item1, keyValuePair.Value.Item2));
				}
			}
		}

		public void OnAfterDeserialize()
		{
			if (varToGuidSerialization != null)
			{
				fieldToParamGuid = new Dictionary<string, (string, ReferenceType)>();
				foreach (var serializedValues in varToGuidSerialization)
				{
					fieldToParamGuid.Add(serializedValues.Item1, (serializedValues.Item2, serializedValues.Item3));
				}
			}
		}
	}
}