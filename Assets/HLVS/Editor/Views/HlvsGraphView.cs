using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using HLVS.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class HlvsGraphView : BaseGraphView
	{
		public Blackboard blackboard;

		private HlvsGraph Graph => graph as HlvsGraph;

		public HlvsGraphView(EditorWindow window) : base(window)
		{
			blackboard = new Blackboard(this);
			blackboard.style.alignItems = Align.Stretch;
			blackboard.scrollable = true;
			blackboard.addItemRequested += OnAddClicked;
		}

		protected void OnAddClicked(Blackboard b)
		{
			var addMenu = new GenericMenu();

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
					var finalName =
						ObjectNames.GetUniqueName(Graph.blackboardFields.Select(parameter => parameter.name).ToArray(),
							niceParamName);
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
			addMenu.AddItem(new GUIContent("Clear Blackboard"), false,
				() =>
				{
					blackboard.Clear();
					Graph.blackboardFields.Clear();
				});

			addMenu.ShowAsContext();
		}

		public void DisplayExistingBlackboardEntries()
		{
			blackboard.Clear();

			foreach (var field in Graph.blackboardFields)
			{
				if (field.type == null)
				{
					Debug.Log($"Field {field.name} has no type!");
				} else
				{
					var row = CreateBlackboardRow(field.type, field.name, field);
					blackboard.Add(row);
				}
			}
		}

		protected void AddBlackboardEntry(Type entryType, string entryName)
		{
			var param = new BlackboardEntry();
			param.type = entryType;
			param.name = entryName;
			param.guid = Guid.NewGuid().ToString();
			Graph.blackboardFields.Add(param);

			var finalName = ObjectNames.GetUniqueName(
				Graph.blackboardFields.Select(parameter => parameter.name).ToArray(),
				entryName);

			// displays the name of the new entry
			var field = CreateBlackboardRow(entryType, entryName, param);

			blackboard.Add(field);
		}

		private BlackboardField CreateBlackboardRow(Type entryType, string entryName, BlackboardEntry param)
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
				Graph.blackboardFields.Remove(param);
				blackboard.Remove(field);
			});
			removeButton.text = " - ";
			removeButton.tooltip = "Remove entry";
			removeButton.style.flexGrow = 0;
			field.Add(removeButton);
			return field;
		}

		private VisualElement CreateEntryValueField(Type entryType, BlackboardEntry parameter)
		{
			switch (entryType.Name)
			{
				default:
					var objField = new ObjectField();
					objField.objectType = entryType;
					objField.label = "";
					objField.allowSceneObjects = false;
					objField.style.flexGrow = new StyleFloat(1.5f);
					objField.style.width =
						0; //so it does not have such a long minimum width and is aligned with other fields
					objField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					return objField;
				case "String":
					var stringField = new TextField();
					parameter.value ??= "";
					stringField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					stringField.style.width = 0;
					stringField.style.flexGrow = new StyleFloat(1.5f);
					return stringField;
				case "Color":
					var colorField = new ColorField();
					parameter.value ??= new Color(1, 1, 1, 1);
					colorField.value = (Color) parameter.value;
					colorField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					colorField.style.width = 0;
					colorField.style.flexGrow = new StyleFloat(1.5f);
					return colorField;
				case "Single":
					var floatField = new FloatField();
					parameter.value ??= 0.0f;
					floatField.value = (float) parameter.value;
					floatField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					floatField.style.width = 0;
					floatField.style.flexGrow = new StyleFloat(1.5f);
					return floatField;
				case "Boolean":
					var boolField = new Button();
					parameter.value ??= true;
					boolField.text = (bool) parameter.value ? "Yes" : "No ";
					boolField.clicked += () =>
					{
						parameter.value = !(bool)parameter.value;
						if ((bool)parameter.value)
						{
							boolField.text = "Yes";
						}
						else
						{
							boolField.text = "No "; // add an extra space to keep the size of the button the same
						}
					};
					// not sure why its needed, but this aligns it with other entries
					boolField.style.marginLeft = new Length(12, LengthUnit.Pixel);
					boolField.style.width = 0;
					boolField.style.flexGrow = new StyleFloat(1.5f);
					return boolField;
				case "Int32":
					var intField = new IntegerField();
					parameter.value ??= 0;
					intField.value = (int) parameter.value;
					intField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					intField.style.width = 0;
					intField.style.flexGrow = new StyleFloat(1.5f);
					return intField;
				case "Vector2":
					var vector2Field = new Vector2Field();
					parameter.value ??= new Vector2();
					vector2Field.value = (Vector2) parameter.value;
					vector2Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector2Field.style.width = 0;
					vector2Field.style.flexGrow = new StyleFloat(1.5f);
					return vector2Field;
				case "Vector3":
					var vector3Field = new Vector3Field();
					parameter.value ??= new Vector3();
					vector3Field.value = (Vector3) parameter.value;
					vector3Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector3Field.style.width = 0;
					vector3Field.style.flexGrow = new StyleFloat(1.5f);
					return vector3Field;
				case "Vector4":
					var vector4Field = new Vector4Field();
					parameter.value ??= new Vector4();
					vector4Field.value = (Vector4) parameter.value;
					vector4Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector4Field.style.width = 0;
					vector4Field.style.flexGrow = new StyleFloat(1.5f);
					return vector4Field;
			}
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			BuildRunGraphMenu(evt);
			base.BuildContextualMenu(evt);
		}

		/// <summary>
		/// Add the View entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected void BuildRunGraphMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Run Graph", e => (graph as HlvsGraph)?.RunStartNodes());
			evt.menu.AppendSeparator();
		}
	}
}