using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Data/Get Variable")]
	public class GetVariableNode : HlvsDataNode
	{
		public override string name => "Get Variable";

		public string variableName = "";
		
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
				outputData = variableData.value;
			}
			else
			{
				Debug.LogError($"Did not find variable '{variableName}'");
				outputData = null;
				return ProcessingStatus.Abort;
			}
			
			return ProcessingStatus.Finished;
		}
		
		[CustomPortBehavior(nameof(outputData))]
		IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges)
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
	}
}