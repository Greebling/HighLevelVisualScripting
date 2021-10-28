using System;
using System.Linq;
using System.Runtime.InteropServices;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class BlackboardView : FieldView
	{
		private readonly HlvsGraphView _graphView;
		public readonly Blackboard blackboard;
		private GenericMenu _addMenu;
		private readonly BlackboardSection _mainSection;

		private HlvsGraph graph => _graphView.graph as HlvsGraph;

		public BlackboardView(HlvsGraphView graphView)
		{
			_graphView = graphView;

			blackboard = new Blackboard();
			blackboard.title = "Blackboard";
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
						ObjectNames.GetUniqueName(graph.blackboardFields.Select(parameter => parameter.name).ToArray(),
							niceParamName);
					finalName =
						ObjectNames.GetUniqueName(graph.parametersBlueprint.Select(parameter => parameter.name).ToArray(),
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
			_addMenu.AddItem(new GUIContent("Clear Blackboard"), false,
				() =>
				{
					blackboard.Clear();
					graph.blackboardFields.Clear();
				});
		}

		public void DisplayExistingBlackboardEntries()
		{
			_mainSection.Clear();
			
			if(graph == null)
				return;

			foreach (var field in graph.blackboardFields)
			{
				Debug.Assert(field.GetValueType() != null, $"Field {field.name} has no type");
				var row = CreateBlackboardRow(field.GetValueType(), field.name, field);
				_mainSection.Add(row);
			}
		}

		protected void AddBlackboardEntry(Type entryType, string entryName)
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
			var field = CreateBlackboardRow(entryType, entryName, param);
			_mainSection.Add(field);
		}
		
		private void OnVarRenamed(ExposedParameter param, string newName)
		{
			param.name = newName;
			graph.onBlackboardListChanged.Invoke();
		}

		protected BlackboardField CreateBlackboardRow(Type entryType, string entryName, ExposedParameter param)
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
			var objField = CreateEntryValueField(entryType, param);
			field.Add(objField);

			field.Q<TextField>().RegisterValueChangedCallback(evt => OnVarRenamed(param, evt.newValue));

			// display remove option
			var removeButton = new Button(() =>
			{
				graph.blackboardFields.Remove(param);
				_mainSection.Remove(field);
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