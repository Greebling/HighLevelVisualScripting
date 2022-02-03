using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Data/Save to")]
	public class SaveToVariableNode : HlvsActionNode
	{
		public override string name => "Save to";

		public string variableName = "";
		
		[Input("Data")]
		public object inputData;
		
		[Output("Data")]
		public object outputData;
		

		public override ProcessingStatus Evaluate()
		{
			if(variableName == string.Empty)
				return ProcessingStatus.Finished;
			
			var graph = (HlvsGraph) Graph;

			var variableData = graph.GetVariableByName(variableName);
			if (variableData != null)
			{
				if(inputData == null && variableData.GetValueType().IsValueType)
				{
					return ProcessingStatus.Finished;
				}
					
				variableData.value = inputData;
			}
			else
			{
				Debug.LogError($"Did not find variable '{variableName}'");
			}

			outputData = inputData;
			
			return ProcessingStatus.Finished;
		}
		
		[CustomPortBehavior(nameof(inputData))]
		IEnumerable<PortData> ListPortBehaviorInputs(List<SerializableEdge> edges)
		{
			var selectedVariable = (graph as HlvsGraph).GetVariableByName(variableName);
			if (selectedVariable == null)
			{
				yield return null;
			} else
			{
				yield return new PortData
				{
					displayName = selectedVariable.name,
					displayType = selectedVariable.GetValueType(),
					identifier = "0",
				};
			}
		}
		
		[CustomPortBehavior(nameof(outputData))]
		IEnumerable<PortData> ListPortBehaviorOutputs(List<SerializableEdge> edges)
		{
			var selectedVariable = (graph as HlvsGraph).GetVariableByName(variableName);
			if (selectedVariable == null)
			{
				yield return null;
			} else
			{
				yield return new PortData
				{
					displayName = selectedVariable.name,
					displayType = selectedVariable.GetValueType(),
					identifier = "1",
				};
			}
		}
	}
}