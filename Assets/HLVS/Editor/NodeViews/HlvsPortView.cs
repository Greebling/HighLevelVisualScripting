using System.Linq;
using System.Reflection;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	public class HlvsPortView : PortView
	{
		protected HlvsPortView(Direction direction, FieldInfo fieldInfo, PortData portData,
			BaseEdgeConnectorListener edgeConnectorListener) : base(direction, fieldInfo, portData,
			edgeConnectorListener)
		{
		}

		/// <summary>
		/// Used in HlvsNodeView
		/// </summary>
		public static HlvsPortView CreatePortView(HlvsGraph graph, BaseNode targetNode, Direction direction,
			FieldInfo fieldInfo, PortData portData,
			BaseEdgeConnectorListener edgeConnectorListener)
		{
			var pv = new HlvsPortView(direction, fieldInfo, portData, edgeConnectorListener);
			pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
			pv.AddManipulator(pv.m_EdgeConnector);
			AddDefaultPortElements(portData, pv);


			if (direction == Direction.Input && fieldInfo.FieldType != typeof(ExecutionLink))
			{
				// add value field
				var field = FieldView.CreateEntryValueField(fieldInfo.FieldType);
				field.style.width = 100f;
				field.style.height = 18f;
				field.style.flexGrow = 0;
				field.AddToClassList("variable-selectable-field");
				pv.Add(field);


				// add button to link to blackboard variables and graph parameters
				var varButton = new Button(() =>
				{
					var menu = new GenericMenu();
					foreach (var parameter in graph.GetParameters()
						.Where(parameter => parameter.GetValueType() == fieldInfo.FieldType))
					{
						menu.AddItem(new GUIContent(parameter.name), false, () => Debug.Log(parameter.name));
					}

					menu.AddSeparator("");
					foreach (var parameter in graph.GetBlackboardFields()
						.Where(parameter => parameter.GetValueType() == fieldInfo.FieldType))
					{
						menu.AddItem(new GUIContent(parameter.name), false, () => Debug.Log(parameter.name));
					}

					if (menu.GetItemCount() > 1)
					{
						menu.ShowAsContext();
					}
				});
				varButton.AddToClassList("variable-selector");
				pv.Add(varButton);
			}

			return pv;
		}

		private static void AddDefaultPortElements(PortData portData, HlvsPortView pv)
		{
			// Force picking in the port label to enlarge the edge creation zone
			var portLabel = pv.Q("type");
			if (portLabel != null)
			{
				portLabel.pickingMode = PickingMode.Position;
				portLabel.style.flexGrow = 1;
			}

			// hide label when the port is vertical
			if (portData.vertical && portLabel != null)
				portLabel.style.display = DisplayStyle.None;

			// Fixup picking mode for vertical top ports
			if (portData.vertical)
				pv.Q("connector").pickingMode = PickingMode.Position;
		}
	}
}