using System;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class BlackboardView : FieldView
	{
		public BlackboardView(HlvsGraphView graphView)
		{
			this.graphView = graphView;

			blackboard.title = "Blackboard";
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
			var list = graph.blackboardFields;
			int previousIndex = list.FindIndex(parameter => parameter.guid == element.name);
			if (previousIndex == -1)
				return;

			var paramToMove = list[previousIndex];
			list.RemoveAt(previousIndex);

			if (index >= list.Count)
				list.Add(paramToMove);
			else
				list.Insert(index, paramToMove);
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
					var finalName = GetUniqueName(niceParamName);
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
			addMenu.AddItem(new GUIContent("Clear Blackboard"), false, ClearBoard);
		}

		private void ClearBoard()
		{
			blackboard.Clear();
			categorySections.Clear();
			CreateDefaultSection();
			graph.blackboardFields.Clear();
			graph.onBlackboardListChanged.Invoke();
		}

		public void DisplayExistingBlackboardEntries()
		{
			// need to clear existing shown entries as each gets created anew
			blackboard.Clear();
			categorySections.Clear();
			CreateDefaultSection();

			if (graph == null)
				return;

			foreach (var field in graph.blackboardFields)
			{
				Debug.Assert(field.GetValueType() != null, $"Field {field.name} has no type");
				CreateBlackboardField(field.GetValueType(), field.name, field);
			}
		}

		private void AddBlackboardEntry(Type entryType, string entryName)
		{
			Undo.RecordObject(graph, "Create Blackboard Field");
			ExposedParameter param = CreateParamFor(entryType);

			object initValue = null;
			if (param.GetValueType().IsValueType)
				initValue = Activator.CreateInstance(param.GetValueType());

			param.Initialize(entryName, initValue);

			param.name = entryName;
			param.guid = Guid.NewGuid().ToString();
			graph.blackboardFields.Add(param);

			// ui
			CreateBlackboardField(entryType, entryName, param);
		}

		private void OnVarRenamed(ExposedParameter param, string newName)
		{
			param.name = GetUniqueName(newName);
			graph.onBlackboardListChanged.Invoke();
		}

		private BlackboardField CreateBlackboardField(Type entryType, string entryName, ExposedParameter param)
		{
			var field = new BlackboardField();
			field.name = param.guid;
			field.AddToClassList("hlvs-blackboard-field");
			field.text = entryName;
			field.typeText = "";

			var typeL = field.Q<Label>("typeLabel");
			field.Q("contentItem").Remove(typeL);
			field.Q("node-border").style.overflow = Overflow.Hidden;

			// displays the value of the field
			var objField = param.GetPropertyDrawer();
			field.Add(objField);

			var nameField = field.Q<TextField>();
			nameField.RegisterValueChangedCallback(evt =>
			{
				OnVarRenamed(param, evt.newValue);
				nameField.value = param.name;
			});
			// callback to show category names when editing name
			nameField.RegisterCallback<FocusInEvent>(evt => nameField.SetValueWithoutNotify(param.name));
			nameField.RegisterCallback<FocusOutEvent>(evt => AfterFieldRenamed(param, field, nameField));

			// display remove option
			var removeButton = new Button(() =>
			{
				graph.blackboardFields.Remove(param);
				RemoveField(param, field);
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