using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	public enum ProcessingStatus
	{
		Finished, Unfinished
	}
	
	[Serializable]
	public abstract class HlvsNode : BaseNode, ISerializationCallbackReceiver
	{
		/// <summary>
		/// Used for communicating when a node has not finished its processing in one frame. Useful for coroutine like nodes
		/// </summary>
		public ProcessingStatus status { get; protected set; } = ProcessingStatus.Finished;

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
		/// Useful for coroutine like nodes to reset their status
		/// </summary>
		public virtual void Reset(){}
		
		/// <summary>
		/// Maps the name of a node field to the guid of an exposed parameter in the graph and gives its reference type
		/// </summary>
		internal Dictionary<string, string> fieldToParamGuid = new Dictionary<string, string>();

		/// <summary>
		/// Used for serialization of fieldToParamGuid
		/// </summary>
		[SerializeField] private List<(string, string)> varToGuidSerialization;

		/// <summary>
		/// Gets the values of blackboard and graph parameter variables
		/// </summary>
		internal void UpdateParameterValues()
		{
			var graph = this.graph as HlvsGraph;
			foreach (var fieldToParam in fieldToParamGuid)
			{
				var parameter = graph.GetVariable(fieldToParam.Value);
				GetType().GetField(fieldToParam.Key).SetValue(this, parameter.value);
			}
		}

		public void OnBeforeSerialize()
		{
			// save dictionary as list
			varToGuidSerialization = new List<(string, string)>();
			if (fieldToParamGuid != null)
			{
				foreach (var keyValuePair in fieldToParamGuid)
				{
					varToGuidSerialization.Add((keyValuePair.Key, keyValuePair.Value));
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
	}
}