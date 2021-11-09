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
		public ParameterView(HlvsGraphView graphView)
		{
			this.graphView = graphView;

			blackboard.title = "Parameters";
			blackboard.subTitle = "";
			blackboard.scrollable = true;
			blackboard.windowed = true;
			blackboard.style.height = 400;

			blackboard.addItemRequested += OnClickedAdd;
			blackboard.moveItemRequested += (blackboard1, index, element) => element.parent.Insert(index, element);
			blackboard.moveItemRequested += (blackboard1, index, element) => OnItemMoved(index, element);

			InitBlackboardMenu();
		}

		private void OnClickedAdd(Blackboard b)
		{
			addMenu.ShowAsContext();
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
			foreach (var type in HlvsTypes.BuiltInTypes)
			{
				string niceParamName = type.Name switch
				{
					"Single" => "Float",
					"Int32" => "Int",
					_ => ObjectNames.NicifyVariableName(type.Name)
				};

				addMenu.AddItem(new GUIContent("Add " + niceParamName), false, () =>
				{
					var finalName = GetUniqueName("My " + niceParamName);
					AddBlackboardEntry(type, finalName);
				});
			}

			addMenu.AddSeparator("");

			foreach (var type in HlvsTypes.GetUserTypes())
			{
				var niceParamName = ObjectNames.NicifyVariableName(type.Name);
				addMenu.AddItem(new GUIContent("Add " + niceParamName), false,
					() => { AddBlackboardEntry(type, niceParamName); });
			}

			// Debug Stuff
			addMenu.AddSeparator("");
			addMenu.AddSeparator("");
			addMenu.AddItem(new GUIContent("Clear Parameters"), false, ClearBoard);
		}

		private void ClearBoard()
		{
			blackboard.Clear();
			categorySections.Clear();
			CreateDefaultSection();
			graph.parametersBlueprint.Clear();
			graph.onParameterListChanged.Invoke();
		}

		public void DisplayExistingParameterEntries()
		{
			// need to clear existing shown entries as each gets created anew
			blackboard.Clear();
			categorySections.Clear();
			CreateDefaultSection();

			if (graph == null)
				return;

			foreach (var field in graph.parametersBlueprint)
			{
				Debug.Assert(field.GetValueType() != null, $"Field {field.name} has no type");
				CreateBlackboardField(field.name, field);
			}
		}

		private void AddBlackboardEntry(Type entryType, string entryName)
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
			CreateBlackboardField(entryName, param);

			graph.onParameterListChanged();
		}

		private void OnParamRenamed(ExposedParameter param, string newName)
		{
			param.name = GetUniqueName(newName);
		}

		private BlackboardField CreateBlackboardField(string entryName, ExposedParameter param)
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
			// callback to show category names when editing name
			nameField.RegisterCallback<FocusInEvent>(evt => nameField.SetValueWithoutNotify(param.name));
			nameField.RegisterCallback<FocusOutEvent>(evt =>
			{
				AfterFieldRenamed(param, field, nameField);
				graph.onParameterListChanged();
			});
			
			// display remove option
			var removeButton = new Button(() =>
			{
				graph.parametersBlueprint.Remove(param);
				RemoveField(field);
				graph.onParameterListChanged();
			});
			removeButton.text = " - ";
			removeButton.tooltip = "Remove entry";
			removeButton.style.flexGrow = 0;
			removeButton.style.borderBottomLeftRadius = 7;
			removeButton.style.borderBottomRightRadius = 7;
			removeButton.style.borderTopLeftRadius = 7;
			removeButton.style.borderTopRightRadius = 7;
			field.Add(removeButton);
			
			// not renamed, but needs categorization
			AfterFieldRenamed(param, field, field.Q<TextField>());
			return field;
		}
	}
}