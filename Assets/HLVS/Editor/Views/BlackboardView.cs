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
		private readonly HlvsGraphView _graphView;
		public readonly Blackboard blackboard;
		private GenericMenu _addMenu;
		private readonly BlackboardSection _mainSection;

		private HlvsGraph graph => _graphView.graph as HlvsGraph;

		public BlackboardView(HlvsGraphView graphView)
		{
			_graphView = graphView;
			blackboard = new Blackboard();
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
						ObjectNames.GetUniqueName(graph.blackboardFields.Select(parameter => parameter.name).ToArray(),
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

			_addMenu.AddItem(new GUIContent("Log Blackboard"), false,
				() =>
				{
					string json = JsonUtility.ToJson(graph.blackboardFields[0], true);
					Debug.Log(json);
				});
		}

		public void DisplayExistingBlackboardEntries()
		{
			_mainSection.Clear();

			foreach (var field in graph.blackboardFields)
			{
				if (field.GetValueType() == null)
				{
					Debug.Log($"Field {field.name} has no type!");
				}
				else
				{
					var row = CreateBlackboardRow(field.GetValueType(), field.name, field);
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
			graph.blackboardFields.Add(param);

			// ui
			var field = CreateBlackboardRow(entryType, entryName, param);
			_mainSection.Add(field);
		}

		protected BlackboardField CreateBlackboardRow(Type entryType, string entryName, ExposedParameter param)
		{
			var field = new BlackboardField();
			field.AddToClassList("hlvs-blackboard-field");
			field.text = entryName;
			field.typeText = "";

			var typeL = field.Q<Label>("typeLabel");
			field.Q("contentItem").Remove(typeL);
			field.Q("node-border").style.overflow = Overflow.Hidden;

			// displays the value of the field
			var objField = CreateEntryValueField(entryType, param);
			field.Add(objField);


			// display remove option
			var removeButton = new Button(() =>
			{
				graph.blackboardFields.Remove(param);
				blackboard.Remove(field);
			});
			removeButton.text = " - ";
			removeButton.tooltip = "Remove entry";
			removeButton.style.flexGrow = 0;
			field.Add(removeButton);
			return field;
		}
	}
}