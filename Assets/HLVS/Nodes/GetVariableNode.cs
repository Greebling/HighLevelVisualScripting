using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Data/Get Variable")]
	public class GetVariableNode : HlvsDataNode
	{
		public string variableName;

		[Output("out")] public object data;

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

		public override void Evaluate()
		{
			Debug.Assert(!(variableName is ""), "A variable name must be given");
			var field = (graph as HlvsGraph).GetVariableByName(variableName);

			if (field != null)
			{
				data = field.value;
			}
			else
			{
				throw new Exception($"No variable called '{variableName}' found");
			}
		}
	}
}