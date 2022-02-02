using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Gameobject/Get Name"), ConverterNode(typeof(GameObject), typeof(string))]
	public class GameobjectNameNode : HlvsDataNode, IConversionNode
	{
		public override string name => "Get Name";
		
		[Input("Gameobject")]
		public GameObject target;
		
		[Output("Name")]
		public string output;

		public string GetConversionInput()
		{
			return nameof(target);
		}

		public string GetConversionOutput()
		{
			return nameof(output);
		}

		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.Assert(false, "No Gameobject given to get the name of");
				return ProcessingStatus.Finished;
			}

			output = target.name;
			
			return ProcessingStatus.Finished;
		}
	}
}