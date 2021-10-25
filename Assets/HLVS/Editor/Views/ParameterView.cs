using System;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class ParameterView : FieldView
	{
		private readonly HlvsGraphView _graphView;
		public readonly Blackboard blackboard;
		private GenericMenu _addMenu;
		private readonly BlackboardSection _mainSection;

		private HlvsGraph graph => _graphView.graph as HlvsGraph;

		public ParameterView(HlvsGraphView graphView)
		{
			_graphView = graphView;
			
			blackboard = new Blackboard();
			blackboard.title = "Parameters";
			blackboard.subTitle = "";
			
			_mainSection = new BlackboardSection();
			blackboard.Add(_mainSection);
			
			InitBlackboardMenu();
			blackboard.addItemRequested += OnClickedAdd;
		}

		private void OnClickedAdd(Blackboard b)
		{
			_addMenu.ShowAsContext();
		}

		private void InitBlackboardMenu()
		{
			_addMenu = new GenericMenu();

			foreach (var type in HlvsTypes.BuiltInTypes)
			{
				string niceParamName = type.Name switch
				{
					"Single" => "Float",
					"Int32" => "Int",
					_ => ObjectNames.NicifyVariableName(type.Name)
				};

				_addMenu.AddItem(new GUIContent("Add " + niceParamName), false, () =>
				{
					var finalName =
						ObjectNames.GetUniqueName(
							graph.parametersBlueprint.Select(parameter => parameter.name).ToArray(),
							niceParamName);
					AddBlackboardEntry(type, finalName);
				});
			}

			_addMenu.AddSeparator("");

			foreach (var type in HlvsTypes.GetUserTypes())
			{
				var niceParamName = ObjectNames.NicifyVariableName(type.Name);
				_addMenu.AddItem(new GUIContent("Add " + niceParamName), false,
					() => { AddBlackboardEntry(type, niceParamName); });
			}

			// Debug Stuff
			_addMenu.AddSeparator("");
			_addMenu.AddSeparator("");
			_addMenu.AddItem(new GUIContent("Clear Parameters"), false,
				() =>
				{
					blackboard.Clear();
					graph.parametersBlueprint.Clear();
				});
		}

		public void DisplayExistingParameterEntries()
		{
			_mainSection.Clear();

			foreach (var field in graph.parametersBlueprint)
			{
				if (field.GetValueType() == null)
				{
					Debug.Log($"Field {field.name} has no type!");
				}
				else
				{
					var row = CreateBlackboardRow(field.name, field);
					_mainSection.Add(row);
				}
			}
		}

		protected void AddBlackboardEntry(Type entryType, string entryName)
		{
			ExposedParameter param = CreateParamFor(entryType);

			object initValue = null;
			if (param.GetValueType().IsValueType)
				initValue = Activator.CreateInstance(param.GetValueType());

			param.Initialize(entryName, initValue);

			param.name = entryName;
			param.guid = Guid.NewGuid().ToString();
			graph.parametersBlueprint.Add(param);

			// ui
			var field = CreateBlackboardRow(entryName, param);
			_mainSection.Add(field);
		}

		protected BlackboardField CreateBlackboardRow(string entryName, ExposedParameter param)
		{
			var field = new BlackboardField();
			field.AddToClassList("hlvs-blackboard-field");
			field.text = entryName;
			field.typeText = "";

			var typeL = field.Q<Label>("typeLabel");
			field.Q("contentItem").Remove(typeL);
			field.Q("node-border").style.overflow = Overflow.Hidden;

			// display remove option
			var removeButton = new Button(() =>
			{
				graph.parametersBlueprint.Remove(param);
				_mainSection.Remove(field);
			});
			removeButton.text = " - ";
			removeButton.tooltip = "Remove entry";
			removeButton.style.flexGrow = 0;
			field.Add(removeButton);
			return field;
		}
	}
}