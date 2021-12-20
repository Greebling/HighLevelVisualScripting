using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Rendering/Change Material")]
	public class ChangeMaterialNode : HlvsActionNode
	{
		public override string name => "Change Material";

		[Input("Target")]
		public GameObject target;

		[Input("Material")]
		public Material material;

		public override ProcessingStatus Evaluate()
		{
			var renderer = target.GetComponent<MeshRenderer>();

			Debug.Assert(renderer != null, "Selected target does not have a mesh renderer to change a material of");
			if (renderer != null)
				renderer.material = material;

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Rendering/Change Color")]
	public class ChangeColorNode : HlvsActionNode
	{
		public override string name => "Change Color";

		[Input("Target")]
		public GameObject target;

		[Input("color")]
		public Color targetColor;

		public override ProcessingStatus Evaluate()
		{
			var renderer = target.GetComponent<MeshRenderer>();

			Debug.Assert(renderer != null, "Selected target does not have a mesh renderer to change a color of");
			if (renderer != null)
				renderer.material.color = targetColor;

			return ProcessingStatus.Finished;
		}
	}
}