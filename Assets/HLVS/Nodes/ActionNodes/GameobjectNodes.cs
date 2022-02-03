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

	[Serializable, NodeMenuItem("Gameobject/Set Position")]
	public class SetPositionNode : HlvsActionNode
	{
		public override string name => "Set Position";

		[Input("Target")]
		public GameObject target;

		[Input("Location")]
		public GameObject location;


		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogError("No Gameobject was found to set active");
				return ProcessingStatus.Finished;
			}
			
			if (target && location)
				target.transform.position = location.transform.position;

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Has Tag")]
	public class CompareTagNode : BranchingNode
	{
		public override string name => "Has Tag";

		[Input("Object")]
		public GameObject target;

		[Input("Tag")]
		public string tag;

		[Output("Has Tag", false)]
		public ExecutionLink trueLink;

		[Output("Else", false)]
		public ExecutionLink falseLink;

		public override string[] GetNextExecutionLinks()
		{
			return target && target.CompareTag(tag) ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Is on Layer")]
	public class IsOnLayerNode : BranchingNode
	{
		public override string name => "Is on Layer";

		[Input("Object")]
		public GameObject target;

		[Input("Any of")]
		public LayerMask layer;

		[Output("On Layer", false)]
		public ExecutionLink trueLink;

		[Output("Else", false)]
		public ExecutionLink falseLink;

		public override string[] GetNextExecutionLinks()
		{
			bool isOnLayer = target && (target.layer & layer) != 0;
			return isOnLayer ? new[] { nameof(trueLink) } : new[] { nameof(falseLink) };
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

	[Serializable, NodeMenuItem("Gameobject/Hide Gameobject")]
	public class HideGameobjectNode : HlvsActionNode
	{
		public override string name => "Hide Gameobject";

		[Input("Gameobject")]
		public GameObject gameobject;


		public override ProcessingStatus Evaluate()
		{
			if (!gameobject)
			{
				Debug.LogError($"No gameobject to hide was given");
				return ProcessingStatus.Finished;
			}

			var renderer = gameobject.GetComponent<Renderer>();
			if (renderer)
				renderer.enabled = false;
			else
			{
				Debug.LogError($"No rendering component found on {gameobject}");
			}

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Make Gameobject Visible")]
	public class UnhideGameobjectNode : HlvsActionNode
	{
		public override string name => "Make Gameobject Visible";

		[Input("Gameobject")]
		public GameObject gameobject;

		public override ProcessingStatus Evaluate()
		{
			if (!gameobject)
			{
				Debug.LogError("No gameobject to make visible was given");
				return ProcessingStatus.Finished;
			}

			gameobject.SetActive(true);

			var renderer = gameobject.GetComponent<Renderer>();
			if (renderer)
				renderer.enabled = true;
			else
			{
				Debug.LogError($"No rendering component found on {gameobject}");
			}

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Gameobject/Object In Direction")]
	public class FirstObjectInDirectionNode : BranchingNode
	{
		public override string name => "Object in Direction";

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

		public LayerMask checkedLayers = Int32.MaxValue;

		[Output("Gameobject")]
		public GameObject output;


		public override ProcessingStatus Evaluate()
		{
			direction.Normalize();

			if (direction.sqrMagnitude == 0)
				direction = origin.transform.forward;

			var ray = new Ray(origin.transform.position, direction);
			if (Physics.Raycast(ray, out RaycastHit hit, distance, checkedLayers))
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
	public class FirstObjectInFrontNode : BranchingNode
	{
		public override string name => "Object in Front Of";

		[Output("Some Found")]
		public ExecutionLink trueLink;

		[Output("None found")]
		public ExecutionLink falseLink;


		[Input("Start")]
		public GameObject origin;

		[Input("Max Distance")]
		[LargerThan(0)]
		public float distance = 10;

		public LayerMask checkedLayers = Int32.MaxValue;

		[Output("Gameobject")]
		public GameObject output;


		public override ProcessingStatus Evaluate()
		{
			var direction = origin.transform.forward;

			var ray = new Ray(origin.transform.position, direction);
			if (Physics.Raycast(ray, out RaycastHit hit, distance, checkedLayers))
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