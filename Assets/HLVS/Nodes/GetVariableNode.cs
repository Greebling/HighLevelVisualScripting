using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Data/Get Variable")]
	public class GetVariableNode : HlvsDataNode
	{
		[Input("Name"), ShowAsDrawer] public string variableName;

		[Output("Data")] public object data;

		public override string name  {
			get
			{
				if (variableName is "")
				{
					return "Get Variable";
				}
				else
				{
					return $"Get {variableName}";
				}
			}
		}

		protected override void Process()
		{
			Debug.Assert(!(variableName is ""), "A variable name must be given");
			var blackboardField =
				(graph as HlvsGraph).blackboardFields.Find(parameter => parameter.name == variableName);
			var parameterField =
				(graph as HlvsGraph).parametersValues.Find(parameter => parameter.name == variableName);

			if (blackboardField != null)
			{
				data = blackboardField.value;
			}
			else if (parameterField != null)
			{
				data = parameterField.value;
			}
			else
			{
				Debug.Assert(false, $"No blackboard variable or parameter variable called {variableName} found");
			}
		}
	}
}