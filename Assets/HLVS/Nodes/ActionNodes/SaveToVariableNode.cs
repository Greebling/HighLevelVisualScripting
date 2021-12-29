using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Data/Save to")]
	public class SaveToVariableNode : HlvsActionNode
	{
		public override string name => "Save to";

		[Input("Name")]
		public string variableName = "";
		
		[Input("Input")]
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
	}
}