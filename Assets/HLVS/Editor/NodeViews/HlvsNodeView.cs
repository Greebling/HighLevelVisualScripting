using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(HlvsNode))]
	public class HlvsNodeView : BaseNodeView
	{
		protected override PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
			=> HlvsPortView.CreatePortView(graph, (HlvsGraphView)owner, this, (HlvsNode)nodeTarget, direction, fieldInfo, portData, listener);

		public HlvsGraph graph => owner.graph as HlvsGraph;

		public override void Enable(bool fromInspector = false)
		{
			base.Enable(fromInspector);

			var nodeStyle = Resources.Load<StyleSheet>("HlvsNodeStyling");
			styleSheets.Add(nodeStyle);

			var portStyle = Resources.Load<StyleSheet>("PortVariableSelector");
			styleSheets.Add(portStyle);
		}

		internal void CheckInputtedData()
		{
			bottomPortContainer.Clear();
			var node = (HlvsNode)nodeTarget;

			node.ParseExpressions();
			List<string> errors = new List<string>();
			foreach (PortView v in inputPortViews)
			{
				var port = (HlvsPortView)v;
				port.errorBox.Clear();
				var portErrors = port.TryApplyInputtedValue(node);
				if (portErrors == null || portErrors.Count == 0)
					continue;

				errors.AddRange(portErrors);

				var errorButton = new Button()
				{
					style =
					{
						backgroundColor = new Color(0.85f, 0.2f, 0.17f),
						color = Color.white
					},
					text = "!",
				};
				foreach (string error in portErrors)
				{
					bottomPortContainer.Add(new Label(error));
					if (errorButton.tooltip.Length != 0)
						errorButton.tooltip += "\n";
					errorButton.tooltip += error;
				}

				port.errorBox.Add(errorButton);
			}


			ClearErrorMessages();
			if (errors.Count != 0)
			{
				StringBuilder errorsMessage = new StringBuilder();
				foreach (var error in errors)
				{
					errorsMessage.AppendLine(error);
				}

				// remove additional line break from end
				errorsMessage.Remove(errorsMessage.Length - 1, 1);

				AddMessageView(errorsMessage.ToString(), EditorGUIUtility.IconContent("CollabConflict").image, new Color(0.75f, 0.11f, 0.21f));
			}
		}

		private void ClearErrorMessages()
		{
			ClearAllBadges();
			foreach (HlvsPortView port in inputPortViews.Cast<HlvsPortView>())
			{
				port.errorBox.Clear();
			}
		}
	}
}