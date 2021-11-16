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
				var valueField = CreateValueField(graph, owner, fieldInfo, node, out var serializedProp);
				pv.Add(valueField);

				var varButton = AddSetVariableButton(graph, owner, fieldInfo, node, valueField, serializedProp);
				pv.Add(varButton);
			}

			return pv;
		}

		private static PropertyField CreateValueField(HlvsGraph graph, BaseGraphView owner,
			FieldInfo fieldInfo, HlvsNode node, out SerializedProperty serializedProp)
		{
			var nodeIndex = graph.nodes.FindIndex(n => n == node);
			serializedProp = owner.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(nodeIndex);
			bool isExpressionField = HlvsNode.CanBeExpression(fieldInfo.FieldType);

			if (isExpressionField)
			{
				int formulaIndex;
				if (node.HasExpressionField(fieldInfo.Name))
					formulaIndex = node.IndexOfExpression(fieldInfo.Name);
				else
				{
					formulaIndex = node.AddExpressionField(fieldInfo.Name);
					owner.serializedGraph.Update(); // mark addition to the list of expression at serialized object
				}

				serializedProp = serializedProp
					.FindPropertyRelative("fieldToFormula").GetArrayElementAtIndex(formulaIndex)
					.FindPropertyRelative("formula").FindPropertyRelative("Expression");
			}
			else
			{
				serializedProp = serializedProp.FindPropertyRelative(fieldInfo.Name);
			}

			var valueField = new PropertyField(serializedProp);
			valueField.Bind(owner.serializedGraph);

			if (node.fieldToParamGuid.ContainsKey(fieldInfo.Name))
			{
				valueField.SetEnabled(false);
				// TODO: Reference blackboard property in field, if referenced variables stems from a blackboard
			}

			valueField.Bind(owner.serializedGraph);
			valueField.style.width = 100f;
			valueField.style.height = 18f;
			valueField.style.marginRight = 0;
			valueField.style.flexGrow = 0;
			valueField.AddToClassList("variable-selectable-field");

			return valueField;
		}

		private static Button AddSetVariableButton(HlvsGraph graph, BaseGraphView owner,
			FieldInfo fieldInfo, HlvsNode node, PropertyField valueField, SerializedProperty serializedProp)
		{
			var varButton = new Button(() =>
			{
				var menu = new GenericMenu();

				menu.AddItem(new GUIContent("Reset"), false, () =>
				{
					node.UnsetFieldReference(fieldInfo.Name);
					owner.serializedGraph.Update();
					valueField.SetEnabled(true);
					valueField.BindProperty(serializedProp);
					valueField.Bind(owner.serializedGraph);
					valueField.tooltip = "";
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
							OnReferenceVariable(node, fieldInfo.Name, blackboardParam.guid);


							var paramProp = serializedBlackboard.FindProperty("fields")
								.GetArrayElementAtIndex(blackboardIndex)
								.FindPropertyRelative("val");
							valueField.Bind(serializedBlackboard);
							valueField.BindProperty(paramProp);
							valueField.SetEnabled(false);
							valueField.tooltip = "From " + blackboardParam.name;
						});
						owner.serializedGraph.Update();
					}
				}

				// graph parameters
				menu.AddSeparator("");
				foreach (var parameter in graph.GetParameters()
					.Where(parameter => parameter.GetValueType() == fieldInfo.FieldType))
				{
					menu.AddItem(new GUIContent(parameter.name), false, () =>
					{
						OnReferenceVariable(node, fieldInfo.Name, parameter.guid);

						valueField.SetEnabled(false);
						valueField.tooltip = "From " + parameter.name;
					});
					owner.serializedGraph.Update();
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
			return varButton;
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