using System;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

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
		public static HlvsPortView CreatePortView(HlvsGraph graph, BaseGraphView owner, BaseNode targetNode,
			Direction direction,
			FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
		{
			var pv = new HlvsPortView(direction, fieldInfo, portData, edgeConnectorListener);
			pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
			pv.AddManipulator(pv.m_EdgeConnector);
			AddDefaultPortElements(portData, pv);

			var node = (HlvsNode)targetNode;

			if (direction == Direction.Input && fieldInfo.FieldType != typeof(ExecutionLink))
			{
				var nodeIndex = graph.nodes.FindIndex(n => n == targetNode);
				SerializedProperty serializedProp = owner.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(nodeIndex);
				bool isExpressionField = fieldInfo.FieldType == typeof(float) || fieldInfo.FieldType == typeof(int);
				
				if (isExpressionField)
				{
					int formulaIndex;
					if (node.HasExpressionField(fieldInfo.Name))
						formulaIndex= node.IndexOfExpression(fieldInfo.Name);
					else
					{
						formulaIndex = node.AddExpressionField(fieldInfo.Name);
						serializedProp.serializedObject.Update(); // mark changes of serialized object (addition to the list of epxressions)
					}
					
					serializedProp = serializedProp
						.FindPropertyRelative("fieldToFormula").GetArrayElementAtIndex(formulaIndex)
						.FindPropertyRelative("formula").FindPropertyRelative("Expression");
				}
				else
				{
					serializedProp = serializedProp.FindPropertyRelative(fieldInfo.Name);
				}

				var field = new PropertyField(serializedProp);
				field.Bind(owner.serializedGraph);


				// add value field
				field.style.width = 100f;
				field.style.height = 18f;
				field.style.marginRight = 0;
				field.style.flexGrow = 0;
				field.AddToClassList("variable-selectable-field");
				pv.Add(field);

				// add button to link to blackboard variables and graph parameters
				var varButton = new Button(() =>
				{
					var menu = new GenericMenu();

					menu.AddItem(new GUIContent("Reset"), false, () =>
					{
						((HlvsNode)targetNode).UnsetFieldReference(field.name);

						field.SetEnabled(true);
						field.BindProperty(serializedProp);
						field.Bind(owner.serializedGraph);
						field.tooltip = "";
					});
					menu.AddSeparator("");

					foreach (HlvsBlackboard blackboard in graph.blackboards)
					{
						var serializedBlackboard = new SerializedObject(blackboard);
						var fields = blackboard.fields;

						for (int i = 0; i < fields.Count; i++)
						{
							ExposedParameter blackboardParam = fields[i];

							if (blackboardParam.GetValueType() != fieldInfo.FieldType)
								continue;

							int blackboardIndex = i;
							menu.AddItem(new GUIContent(blackboardParam.name), false, () =>
							{
								OnReferenceVariable(targetNode as HlvsNode, fieldInfo.Name, blackboardParam.guid);
								field.SetEnabled(false);

								var paramProp = serializedBlackboard.FindProperty("fields")
									.GetArrayElementAtIndex(blackboardIndex)
									.FindPropertyRelative("val");
								field.Bind(serializedBlackboard);
								field.BindProperty(paramProp);
								field.tooltip = "From " + blackboardParam.name;
							});
						}
					}

					menu.AddSeparator("");
					foreach (var parameter in graph.GetParameters()
						.Where(parameter => parameter.GetValueType() == fieldInfo.FieldType))
					{
						menu.AddItem(new GUIContent(parameter.name), false, () =>
						{
							field.SetEnabled(false);
							OnReferenceVariable(targetNode as HlvsNode, fieldInfo.Name, parameter.guid);

							field.tooltip = "From " + parameter.name;
						});
					}


					if (menu.GetItemCount() > 1)
					{
						menu.ShowAsContext();
					}
				});
				varButton.AddToClassList("variable-selector");
				var imageHolder = new VisualElement();
				imageHolder.AddToClassList("selector-image");
				varButton.Add(imageHolder);


				pv.Add(varButton);
			}

			return pv;
		}

		public void SetPortType(Type type)
		{
			RemoveFromClassList("Port_" + portType.Name);
			portType = type;
			AddToClassList("Port_" + portType.Name);
		}

		private static void OnReferenceVariable(HlvsNode node, string nameOfField, string parameterGuid)
		{
			Debug.Assert(node != null);

			node.SetFieldToReference(nameOfField, parameterGuid);
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