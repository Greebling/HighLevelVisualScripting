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

		public override ProcessingStatus Evaluate()
		{
			if(variableName == string.Empty)
				return ProcessingStatus.Finished;
			
			var graph = (HlvsGraph) Graph;

			var variableData = graph.GetVariableByName(variableName);
			if (variableData != null)
			{
				variableData.value = inputData;
			}
			else
			{
				Debug.LogError($"Did not find variable '{variableName}'");
			}
			
			return ProcessingStatus.Finished;
		}
		
		[CustomPortBehavior(nameof(inputData))]
		IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges)
		{
			var selectedVariable = (graph as HlvsGraph).GetVariableByName(variableName);
			if (selectedVariable == null)
				yield return null;
			
			yield return new PortData
			{
				displayName = selectedVariable.name,
				displayType = selectedVariable.GetValueType(),
				identifier = "0",
			};
		}
	}
}