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
		public readonly Blackboard blackboard;
		private GenericMenu _addMenu;
		private readonly BlackboardSection _mainSection;

		public ParameterView(HlvsGraphView graphView)
		{
			base.graphView = graphView;

			blackboard = new Blackboard();
			blackboard.title = "Parameters";
			blackboard.subTitle = "";
			blackboard.scrollable = true;
			blackboard.windowed = true;
			blackboard.style.height = 400;

			_mainSection = new BlackboardSection();
			blackboard.Add(_mainSection);

			InitBlackboardMenu();
			blackboard.addItemRequested += OnClickedAdd;
			blackboard.moveItemRequested += (blackboard1, index, element) => _mainSection.Insert(index, element);
			blackboard.moveItemRequested += (blackboard1, index, element) => OnItemMoved(index, element);
		}

		private void OnClickedAdd(Blackboard b)
		{
			_addMenu.ShowAsContext();
		}

		/// <summary>
		/// Reorders the data lists to keep in sync with the ui
		/// </summary>
		/// <param name="index"></param>
		/// <param name="element"></param>
		private void OnItemMoved(int index, VisualElement element)
		{
			var list = graph.parametersBlueprint;
			int previousIndex = list.FindIndex(parameter => parameter.guid == element.name);
			if (previousIndex == -1)
				return;

			var paramToMove = list[previousIndex];
			list.RemoveAt(previousIndex);

			if (index >= list.Count)
				list.Add(paramToMove);
			else
				list.Insert(index, paramToMove);

			graph.onParameterListChanged.Invoke();
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
					var finalName = GetUniqueName(niceParamName);
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

			if (graph == null)
				return;

			foreach (var field in graph.parametersBlueprint)
			{
				Debug.Assert(field.GetValueType() != null, $"Field {field.name} has no type");
				var row = CreateBlackboardRow(field.name, field);
				_mainSection.Add(row);
			}
		}

		protected void AddBlackboardEntry(Type entryType, string entryName)
		{
			Undo.RecordObject(graph, "Create Parameter Field");
			ExposedParameter param = CreateParamFor(entryType);

			object initValue = null;
			if (param.GetValueType().IsValueType)
				initValue = Activator.CreateInstance(param.GetValueType());

			param.Initialize(entryName, initValue);

			param.name = entryName;
			graph.parametersBlueprint.Add(param);

			// ui
			var field = CreateBlackboardRow(entryName, param);
			_mainSection.Add(field);

			graph.onParameterListChanged.Invoke();
		}

		private void OnParamRenamed(ExposedParameter param, string newName)
		{
			param.name = GetUniqueName(newName);
			graph.onParameterListChanged.Invoke();
		}

		protected BlackboardField CreateBlackboardRow(string entryName, ExposedParameter param)
		{
			var field = new BlackboardField();
			field.name = param.guid;
			field.AddToClassList("hlvs-blackboard-field");
			field.text = entryName;
			field.typeText = param.GetValueType().Name;
			field.Q("node-border").style.overflow = Overflow.Hidden;

			// add updates for name changes
			var nameField = field.Q<TextField>();
			nameField.RegisterValueChangedCallback(evt => OnParamRenamed(param, evt.newValue));

			// display remove option
			var removeButton = new Button(() =>
			{
				graph.parametersBlueprint.Remove(param);
				_mainSection.Remove(field);
				graph.onParameterListChanged.Invoke();
			});
			removeButton.text = " - ";
			removeButton.tooltip = "Remove entry";
			removeButton.style.flexGrow = 0;
			removeButton.style.borderBottomLeftRadius = 7;
			removeButton.style.borderBottomRightRadius = 7;
			removeButton.style.borderTopLeftRadius = 7;
			removeButton.style.borderTopRightRadius = 7;
			field.Add(removeButton);
			return field;
		}
	}
}