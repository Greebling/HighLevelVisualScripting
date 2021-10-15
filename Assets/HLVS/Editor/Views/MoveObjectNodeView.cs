using GraphProcessor;
using HLVS.Nodes;
using UnityEditor.UIElements;
using UnityEngine;

namespace HLVS.Editor.Views
{
	[NodeCustomEditor(typeof(MoveObjectNode))]
	public class MoveObjectNodeView : BaseNodeView
	{
		public override void Enable()
		{
			base.Enable();
			var gobjField = new ObjectField("Gameobject") { objectType = typeof(GameObject) };

			inputContainer.Add(gobjField);
			inputContainer.Add(new FloatField("Speed"));
		}
	}
}