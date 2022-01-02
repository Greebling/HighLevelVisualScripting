using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Gameobject/Clone")]
	public class InstantiateNode : HlvsActionNode
	{
		public override string name => "Clone Gameobject";

		[Input("Target")]
		public GameObject target;

		[Output("Clone")]
		public GameObject clone;


		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogError("No Gameobject was found to clone");
				return ProcessingStatus.Finished;
			}

			clone = Object.Instantiate(target);

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Set Active")]
	public class SetActiveNode : HlvsActionNode
	{
		public override string name => "Set Active";

		[Input("Target")]
		public GameObject target;

		[Input("Is Active")]
		public bool isActive;


		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogError("No Gameobject was found to set active");
				return ProcessingStatus.Finished;
			}

			target.SetActive(isActive);

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Has Tag")]
	public class CompareTagNode : HlvsDataNode
	{
		public override string name => "Has Tag";

		[Input("Object")]
		public GameObject target;

		[Input("Tag")]
		public string tag;

		[Output("Has Tag")]
		public bool hasTag;

		public override ProcessingStatus Evaluate()
		{
			hasTag = target && target.CompareTag(tag);
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Is on Layer")]
	public class IsOnLayerNode : HlvsDataNode
	{
		public override string name => "Is on Layer";

		[Input("Object")]
		public GameObject target;

		[Input("Any of")]
		public LayerMask layer;

		[Output("Is on Layer")]
		public bool isOnLayer;

		public override ProcessingStatus Evaluate()
		{
			isOnLayer = target && (target.layer & layer) != 0;
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Find Gameobject")]
	public class FindGameobjectNode : HlvsActionNode
	{
		public override string name => "Find Gameobject";

		[Input("Name")]
		public string gameobjectName;

		[Output("Gameobject")]
		public GameObject output;


		public override ProcessingStatus Evaluate()
		{
			output = GameObject.Find(gameobjectName);

			if (!output)
			{
				Debug.LogWarning($"No gameobject called '{gameobjectName}' found");
			}

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Object In Direction")]
	public class FirstObjectInDirectionNode : HlvsFlowNode
	{
		public override string name => "Object in Direction";

		[Input(" ")]
		public ExecutionLink inputLink;
		
		[Output("Some Found")]
		public ExecutionLink trueLink;

		[Output("None found")]
		public ExecutionLink falseLink;
		

		[Input("Start")]
		public GameObject origin;

		[Input("Direction")]
		public Vector3 direction;

		[Input("Max Distance")]
		[LargerThan(0)]
		public float distance = 10;

		[Output("Gameobject")]
		public GameObject output;


		public override ProcessingStatus Evaluate()
		{
			direction.Normalize();

			if (direction.sqrMagnitude == 0)
				direction = origin.transform.forward;

			var ray = new Ray(origin.transform.position, direction);
			if (Physics.Raycast(ray, out RaycastHit hit, distance))
			{
				output = hit.collider.gameObject;

				return ProcessingStatus.Finished;
			}
			else
			{
				output = null;
				return ProcessingStatus.Finished;
			}
		}

		public override string[] GetNextExecutionLinks()
		{
			return output ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Object In Front Of")]
	public class FirstObjectInFrontNode : HlvsFlowNode
	{
		public override string name => "Object in Front Of";

		[Input(" ")]
		public ExecutionLink inputLink;
		
		[Output("Some Found")]
		public ExecutionLink trueLink;

		[Output("None found")]
		public ExecutionLink falseLink;


		[Input("Start")]
		public GameObject origin;

		[Input("Max Distance")]
		[LargerThan(0)]
		public float distance = 10;

		[Output("Gameobject")]
		public GameObject output;


		public override ProcessingStatus Evaluate()
		{
			var direction = origin.transform.forward;

			var ray = new Ray(origin.transform.position, direction);
			if (Physics.Raycast(ray, out RaycastHit hit, distance))
			{
				output = hit.collider.gameObject;

				return ProcessingStatus.Finished;
			}
			else
			{
				output = null;
				return ProcessingStatus.Finished;
			}
		}

		public override string[] GetNextExecutionLinks()
		{
			return output ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}
}