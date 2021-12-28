using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.DataNodes
{
	[Serializable, NodeMenuItem("Components/Get Rigidbody"), ConverterNode(typeof(GameObject), typeof(Rigidbody))]
	public class GetRigidbodyNode : HlvsDataNode, IConversionNode
	{
		public override string name => "Get Rigidbody";
		
		[Input("Gameobject")]
		public GameObject target;
		
		[Output("Out")]
		public Rigidbody output;

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
				Debug.Assert(false, "No Gameobject given to get a Rigidbody component of");
				return ProcessingStatus.Finished;
			}

			output = target.GetComponent<Rigidbody>();
			
			Debug.Assert(output, "Given Gameobject does not have a Rigidbody");
			
			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Components/Get Animator"), ConverterNode(typeof(GameObject), typeof(Animator))]
	public class GetLightNode : HlvsDataNode, IConversionNode
	{
		public override string name => "Get Animator";
		
		[Input("Gameobject")]
		public GameObject target;
		
		[Output("Out")]
		public Animator output;

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
				Debug.Assert(false, "No Gameobject given to get an Animator component of");
				return ProcessingStatus.Finished;
			}

			output = target.GetComponent<Animator>();
			
			Debug.Assert(output, "Given Gameobject does not have an Animator component");
			
			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Gameobject/Name"), ConverterNode(typeof(GameObject), typeof(string))]
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