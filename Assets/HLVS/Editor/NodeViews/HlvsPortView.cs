using System;
using System.Collections.Generic;
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

namespace HLVS.Editor.NodeViews
{
	public class HlvsPortView : PortView
	{
		protected HlvsPortView(FieldInfo                 fieldInfo,             Direction     direction, PortData portData,
		                       BaseEdgeConnectorListener edgeConnectorListener, HlvsGraphView owner)
			: base(direction, fieldInfo, portData, edgeConnectorListener)
		{
			isExpressionPort = HlvsNode.CanBeExpression(fieldInfo.FieldType);
			_mode = PortMode.ShowValue;
			serializedGraph = owner.serializedGraph;

			OnConnected += (view, edge) =>
			{
				_isConnected = true;

				if (_valueField != null)
					_valueField.SetEnabled(false);
			};
			OnDisconnected += (view, edge) =>
			{
				_isConnected = false;

				if (_valueField != null)
					_valueField.SetEnabled(true);
			};
		}

		/// <summary>
		/// Used in HlvsNodeView
		/// </summary>
		public static HlvsPortView CreatePortView(HlvsGraph graph, HlvsGraphView owner, HlvsNodeView nodeView, HlvsNode targetNode,
		                                          Direction direction,
		                                          FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
		{
			var pv = new HlvsPortView(fieldInfo, direction, portData, edgeConnectorListener, owner);
			pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
			pv.AddManipulator(pv.m_EdgeConnector);
			pv.Init(graph, owner, nodeView, targetNode);

			if (pv.isExpressionPort)
			{
				pv.OnConnected += (view, edge) => { targetNode.RemoveExpressionField(fieldInfo.Name); };
				pv.OnDisconnected += (view, edge) =>
				{
					if (!targetNode.HasExpressionField(fieldInfo.Name))
						targetNode.AddExpressionField(fieldInfo.Name);
				};
			}

			return pv;
		}

		public virtual void Init(HlvsGraph graph, HlvsGraphView view, HlvsNodeView nodeView, HlvsNode targetNode)
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
					graphParamProp = serializedGraph.FindProperty("parametersBlueprint").GetArrayElementAtIndex(paramIndex).FindPropertyRelative("name");
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

							if (blackboardParam.guid != paramGuid)
								continue;

							if (blackboardParam.GetValueType() != fieldInfo.FieldType)
								continue;

							var serializedBlackboard = new SerializedObject(blackboard);
							int blackboardIndex = i;

							blackboardProp = serializedBlackboard.FindProperty("fields").GetArrayElementAtIndex(blackboardIndex)
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

			_valueField.RegisterCallback<FocusOutEvent>(_ => { nodeView.CheckInputtedData(); });

			AddVisualElements();
		}

		protected virtual void AddVisualElements()
		{
			Add(_valueField);
			Add(resetButton);
			Add(errorBox);
		}

		public List<string> TryApplyInputtedValue(HlvsNode targetNode)
		{
			if (_mode != PortMode.ShowValue)
				return null;

			foreach (var formulaPair in targetNode.fieldToFormula)
			{
				if (formulaPair.formula.Expression == string.Empty)
					continue;

				if (formulaPair.fieldName != fieldInfo.Name)
					continue;

				try
				{
					var nodeType = targetNode.GetType();
					var targetField = nodeType.GetField(formulaPair.fieldName);

					var trueValue = formulaPair.function(null);
					var value = Convert.ChangeType(trueValue, targetField.FieldType);

					// check inputted values
					var fieldAttributes = targetField.GetCustomAttributes<NodeFieldAttribute>();
					List<string> errors = new List<string>();
					foreach (NodeFieldAttribute nodeFieldAttribute in fieldAttributes)
					{
						var isCorrect = nodeFieldAttribute.CheckField(trueValue);
						if (!isCorrect)
						{
							errors.Add(portName + ": " + nodeFieldAttribute.GetErrorMessage());
						}
					}

					if (errors.Count == 0)
					{
						targetField.SetValue(targetNode, value);
					}

					return errors;
				}
				catch (Exception)
				{
					// ignored
					return null;
				}
			}

			return null;
		}

		private void CreateValueField()
		{
			_valueField = new PropertyField(valueProp)
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

		public virtual void ShowValueField()
		{
			if(valueProp == null) // dont show non serializeable properties
				return;
			
			_valueField.BindProperty(isExpressionPort & !_isConnected ? expressionProp : valueProp);
			_valueField.Bind(serializedGraph);

			_valueField.SetEnabled(true);
		}

		private void ShowBlackboardField()
		{
			_valueField.BindProperty(blackboardProp);
			_valueField.Bind(serializedGraph);

			_valueField.SetEnabled(false);
		}

		private void ShowGraphParamField()
		{
			_valueField.BindProperty(graphParamProp);
			_valueField.Bind(serializedGraph);

			_valueField.SetEnabled(false);
		}

		public void SetDisplayMode(PortMode mode, bool force = false)
		{
			if (_mode == mode)
				return;

			OnBeforeSwitchDisplayMode();

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

		public virtual void OnBeforeSwitchDisplayMode()
		{
			//_valueField.Unbind();
		}

		private void InitResetButton(HlvsGraph graph, HlvsGraphView view, HlvsNode node)
		{
			resetButton = new Button(() =>
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


							blackboardProp = serializedBlackboard.FindProperty("fields")
							                                     .GetArrayElementAtIndex(blackboardIndex)
							                                     .FindPropertyRelative("name");
							_valueField.BindProperty(blackboardProp);
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

						graphParamProp = serializedGraph.FindProperty("parametersBlueprint").GetArrayElementAtIndex(index)
						                                .FindPropertyRelative("name");

						_valueField.BindProperty(graphParamProp);
						SetDisplayMode(PortMode.ReferenceGraphVariable);
					});
					view.serializedGraph.Update();
				}


				if (menu.GetItemCount() > 1)
				{
					menu.ShowAsContext();
				}
			});
			resetButton.AddToClassList("variable-selector");
			var imageHolder = new VisualElement();
			imageHolder.AddToClassList("selector-image");
			resetButton.Add(imageHolder);
		}

		private void InitValueProperty(HlvsGraph graph, HlvsGraphView view, HlvsNode node)
		{
			var nodeIndex = graph.nodeToIndex[node];
			valueProp = view.serializedGraph.FindProperty("nodes").GetArrayElementAtIndex(nodeIndex);
			if (isExpressionPort)
			{
				int formulaIndex;
				if (node.HasExpressionField(fieldInfo.Name))
					formulaIndex = node.IndexOfExpression(fieldInfo.Name);
				else
				{
					formulaIndex = node.AddExpressionField(fieldInfo.Name);

					var fieldVal = fieldInfo.GetValue(node).ToString();
					node.fieldToFormula[formulaIndex].formula.Expression = fieldVal;

					view.serializedGraph.Update(); // mark addition to the list of expression at serialized object
				}

				expressionProp = valueProp
				                 .FindPropertyRelative("fieldToFormula").GetArrayElementAtIndex(formulaIndex)
				                 .FindPropertyRelative("formula").FindPropertyRelative("Expression");
			}

			valueProp = valueProp.FindPropertyRelative(fieldInfo.Name);
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

		public enum PortMode
		{
			// only show name
			ReferenceGraphVariable,

			// show value of blackboard var
			ReferenceBlackboardVariable,

			// show serialized value
			ShowValue,
		}

		/// <summary>
		/// Current display mode of port
		/// </summary>
		private PortMode _mode;

		/// <summary>
		/// Whether this port has an expression as value
		/// </summary>
		public readonly bool isExpressionPort;

		private bool _isConnected = false;

		private PropertyField _valueField;

		protected Button resetButton;

		protected readonly SerializedObject   serializedGraph;
		protected          SerializedProperty valueProp;
		protected          SerializedProperty expressionProp;
		protected          SerializedProperty blackboardProp;
		protected          SerializedProperty graphParamProp;

		public readonly VisualElement errorBox = new VisualElement();
	}
}