using System;
using System.Reflection;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace HLVS.Editor.NodeViews
{
	public class HlvsPortView : PortView
	{
		protected HlvsPortView(FieldInfo fieldInfo, Direction direction, PortData portData,
			BaseEdgeConnectorListener edgeConnectorListener, HlvsGraphView owner) : base(direction, fieldInfo, portData,
			edgeConnectorListener)
		{
			_isExpressionPort = HlvsNode.CanBeExpression(fieldInfo.FieldType);
			_mode = PortMode.ShowValue;
			_serializedGraph = owner.serializedGraph;
		}

		/// <summary>
		/// Used in HlvsNodeView
		/// </summary>
		public static HlvsPortView CreatePortView(HlvsGraph graph, HlvsGraphView owner, HlvsNode targetNode,
			Direction direction,
			FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
		{
			var pv = new HlvsPortView(fieldInfo, direction, portData, edgeConnectorListener, owner);
			pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
			pv.AddManipulator(pv.m_EdgeConnector);
			pv.Init(graph, owner, targetNode);

			return pv;
		}

		public void Init(HlvsGraph graph, HlvsGraphView view, HlvsNode targetNode)
		{
			if (direction == Direction.Output || fieldInfo.FieldType == typeof(ExecutionLink))
				return;

			this.Q<Label>().style.width = 60;

			InitValueProperty(graph, view, targetNode);
			InitResetButton(graph, view, targetNode);

			CreateValueField();

			if (targetNode.fieldToParamGuid.TryGetValue(fieldInfo.Name, out string paramGuid))
			{
				int paramIndex = graph.parametersBlueprint.FindIndex(parameter => parameter.guid == paramGuid);
				if (paramIndex != -1)
				{
					_mode = PortMode.ReferenceGraphVariable;
					_graphParamProp = _serializedGraph.FindProperty("parametersBlueprint").GetArrayElementAtIndex(paramIndex).FindPropertyRelative("name");
					ShowGraphParamField();
				}
				else
				{
					_mode = PortMode.ReferenceBlackboardVariable;
					//find blackboard parameter
					foreach (HlvsBlackboard blackboard in graph.blackboards)
					{
						var fields = blackboard.fields;

						for (int i = 0; i < fields.Count; i++)
						{
							ExposedParameter blackboardParam = fields[i];

							if(blackboardParam.guid != paramGuid)
								continue;
							
							if (blackboardParam.GetValueType() != fieldInfo.FieldType)
								continue;

							var serializedBlackboard = new SerializedObject(blackboard);
							int blackboardIndex = i;
							
							_blackboardProp = serializedBlackboard.FindProperty("fields").GetArrayElementAtIndex(blackboardIndex)
								.FindPropertyRelative("name");
							goto ShowBlackboardField;
						}
					}
					ShowBlackboardField: 
					ShowBlackboardField();
				}
			}
			else
			{
				ShowValueField();
			}

			Add(_valueField);
			Add(_resetButton);
		}

		private void CreateValueField()
		{
			_valueField = new PropertyField(_valueProp)
			{
				style =
				{
					width = 100f,
					height = 18f,
					marginRight = 0,
					flexGrow = 0
				}
			};

			_valueField.AddToClassList("variable-selectable-field");
		}

		private void ShowValueField()
		{
			_valueField.BindProperty(_valueProp);
			_valueField.Bind(_serializedGraph);

			_valueField.SetEnabled(true);
		}

		private void ShowBlackboardField()
		{
			_valueField.BindProperty(_blackboardProp);
			_valueField.Bind(_serializedGraph);

			_valueField.SetEnabled(false);
		}

		private void ShowGraphParamField()
		{
			_valueField.BindProperty(_graphParamProp);
			_valueField.Bind(_serializedGraph);

			_valueField.SetEnabled(false);
		}

		public void SetDisplayMode(PortMode mode)
		{
			if (_mode == mode)
				return;

			// remove current
			_valueField.Unbind();

			// show next mode field
			switch (mode)
			{
				case PortMode.ReferenceGraphVariable:
					ShowGraphParamField();
					break;
				case PortMode.ReferenceBlackboardVariable:
					ShowBlackboardField();
					break;
				case PortMode.ShowValue:
					ShowValueField();
					break;
			}

			_mode = mode;
		}

		private void InitResetButton(HlvsGraph graph, HlvsGraphView view, HlvsNode node)
		{
			_resetButton = new Button(() =>
			{
				var menu = new GenericMenu();

				menu.AddItem(new GUIContent("Reset"), false, () =>
				{
					node.UnsetFieldReference(fieldInfo.Name);
					view.serializedGraph.Update();
					SetDisplayMode(PortMode.ShowValue);
				});

				menu.AddSeparator("");
				// following: set field to variable reference

				// choose a blackboard parameter
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


							_blackboardProp = serializedBlackboard.FindProperty("fields")
								.GetArrayElementAtIndex(blackboardIndex)
								.FindPropertyRelative("name");
							SetDisplayMode(PortMode.ReferenceBlackboardVariable);
						});
						view.serializedGraph.Update();
					}
				}

				// graph parameters
				menu.AddSeparator("");

				for (int i = 0; i < graph.parametersBlueprint.Count; i++)
				{
					var parameter = graph.parametersBlueprint[i];
					if (parameter.GetValueType() != fieldInfo.FieldType)
						continue;

					int index = i;
					menu.AddItem(new GUIContent(parameter.name), false, () =>
					{
						OnReferenceVariable(node, fieldInfo.Name, parameter.guid);

						_graphParamProp = _serializedGraph.FindProperty("parametersBlueprint").GetArrayElementAtIndex(index)
							.FindPropertyRelative("name");

						SetDisplayMode(PortMode.ReferenceGraphVariable);
					});
					view.serializedGraph.Update();
				}


				if (menu.GetItemCount() > 1)
				{
					menu.ShowAsContext();
				}
			});
			_resetButton.AddToClassList("variable-selector");
			var imageHolder = new VisualElement();
			imageHolder.AddToClassList("selector-image");
			_resetButton.Add(imageHolder);
		}

		private void InitValueProperty(HlvsGraph graph, HlvsGraphView view, HlvsNode node)
		{
			var nodeIndex = graph.nodeToIndex[node];
			_valueProp = view.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(nodeIndex);
			if (_isExpressionPort)
			{
				int formulaIndex;
				if (node.HasExpressionField(fieldInfo.Name))
					formulaIndex = node.IndexOfExpression(fieldInfo.Name);
				else
				{
					formulaIndex = node.AddExpressionField(fieldInfo.Name);
					view.serializedGraph.Update(); // mark addition to the list of expression at serialized object
				}

				_valueProp = _valueProp
					.FindPropertyRelative("fieldToFormula").GetArrayElementAtIndex(formulaIndex)
					.FindPropertyRelative("formula").FindPropertyRelative("Expression");
			}
			else
			{
				_valueProp = _valueProp.FindPropertyRelative(fieldInfo.Name);
			}
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

		public enum PortMode
		{
			// only show name
			ReferenceGraphVariable,

			// show value of blackboard var
			ReferenceBlackboardVariable,

			// show serialized value
			ShowValue
		}

		/// <summary>
		/// Current display mode of port
		/// </summary>
		private PortMode _mode;

		/// <summary>
		/// Whether this port has an expression as value
		/// </summary>
		private readonly bool _isExpressionPort;

		private PropertyField _valueField;

		private Button _resetButton;

		private readonly SerializedObject _serializedGraph;
		private SerializedProperty _valueProp;
		private SerializedProperty _blackboardProp;
		private SerializedProperty _graphParamProp;
	}
}