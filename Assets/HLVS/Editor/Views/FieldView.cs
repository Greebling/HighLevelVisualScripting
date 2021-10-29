using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	/// <summary>
	/// Contains classes for created ExposeParameters for a given Type. Useful for Blackboards and Graph-Parameters
	/// </summary>
	public class FieldView
	{
		public readonly Blackboard blackboard;
		protected HlvsGraph graph => graphView.graph as HlvsGraph;
		protected HlvsGraphView graphView;
		protected readonly GenericMenu addMenu;

		protected readonly Dictionary<string, BlackboardSection> categorySections;
		protected BlackboardSection defaultSection;

		public FieldView()
		{
			blackboard = new Blackboard();
			addMenu = new GenericMenu();
			categorySections = new Dictionary<string, BlackboardSection>();

			// create default category section
			CreateDefaultSection();
		}

		protected void CreateDefaultSection()
		{
			defaultSection = new BlackboardSection();
			defaultSection.name = String.Empty;
			blackboard.Add(defaultSection);
			categorySections.Add(String.Empty, defaultSection);
		}

		public static string RemoveCategoriesFromName(string name)
		{
			var slashIndex = name.LastIndexOf('/');

			return slashIndex != -1 ? name.Substring(slashIndex + 1, name.Length - (slashIndex + 1)) : name;
		}
		
		public static string GetCategoriesFromName(string name)
		{
			var slashIndex = name.LastIndexOf('/');

			return slashIndex != -1 ? name.Substring(0, slashIndex) : name;
		}

		protected void AfterFieldRenamed(ExposedParameter field, VisualElement fieldContainer, TextField nameField = null)
		{
			var previousParent = fieldContainer.parent;
			bool sectionHasOneChild = false;
			// remove from current parent
			if(previousParent != null)
			{
				// delete parent if there are no more field in the section
				if (previousParent.contentContainer.childCount == 1)
				{
					Debug.Assert(previousParent is BlackboardSection, "Visual Element hierarchy was not as expected");
					sectionHasOneChild = true;
				}
				previousParent.contentContainer.Remove(fieldContainer);
			}
			

			var paramName = field.name;
			var slashIndex = paramName.LastIndexOf('/');

			if (nameField != null)
			{
				// set text display to not show categories
				var textVal = paramName.Substring(slashIndex + 1, paramName.Length - (slashIndex + 1));
				nameField.hierarchy.parent.Q<Label>("title-label").text = textVal;
			}

			// no category case, add to default section
			if (slashIndex == -1)
			{
				categorySections[String.Empty].Add(fieldContainer);
				
				if(sectionHasOneChild && previousParent.name != String.Empty)
				{
					previousParent.parent.Remove(previousParent);
				}
				return;
			}

			var allCategories = paramName.Substring(0, slashIndex);
			if (categorySections.ContainsKey(allCategories))
			{
				// add to existing section
				var section = categorySections[allCategories];
				section.Add(fieldContainer);
			}
			else
			{
				// create new section
				var categories = GetSectionNameFromCategories(allCategories);
				var section = new BlackboardSection();
				section.title = categories;
				section.name = allCategories;
				section.Add(fieldContainer);

				categorySections.Add(allCategories, section);
				blackboard.Add(section);
			}
			
			if(sectionHasOneChild && previousParent.name != allCategories)
			{
				previousParent.parent.Remove(previousParent);
			}
		}

		public void RemoveField(ExposedParameter field, VisualElement container)
		{
			var previousParent = container.parent;
			// remove from current parent
			if(previousParent != null)
			{
				previousParent.contentContainer.Remove(container);
				// delete parent if there are no more field in the section
				if (previousParent.contentContainer.childCount == 1)
				{
					Debug.Assert(previousParent is BlackboardSection, "Visual Element hierarchy was not as expected");
					previousParent.parent.Remove(previousParent);
				}
			}
		}

		public static string GetSectionNameFromCategories(string categories)
		{
			return categories.Replace("/", " | ");
		}

		protected string GetUniqueName(string name)
		{
			return ObjectNames.GetUniqueName(graph.blackboardFields.Select(parameter => parameter.name).ToArray(),
				ObjectNames.GetUniqueName(graph.parametersBlueprint.Select(parameter => parameter.name).ToArray(),
					name));
		}

		/// <summary>
		/// Creates a ExposedParameter for a given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ExposedParameter CreateParamFor(Type type)
		{
			switch (type.Name)
			{
				default:
					Debug.Log($"Unknown param type {type.Name}");
					return null;
				case "String":
					return new StringParameter();
				case "Color":
					return new ColorParameter();
				case "Single":
					return new FloatParameter();
				case "Boolean":
					return new BoolParameter();
				case "Int32":
					return new IntParameter();
				case "Vector2":
					return new Vector2Parameter();
				case "Vector3":
					return new Vector3Parameter();
				case "Vector4":
					return new Vector4Parameter();
				case "GameObject":
					return new GameObjectParameter();
				case "Material":
					return new MaterialParameter();
			}
		}

		private const float fieldWidth = 1.0f;

		/// <summary>
		/// Creates an editor field for a given parameter
		/// </summary>
		/// <param name="entryType">Of the parameter to be shown. Needed for reference types, when parameter.value==null</param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public VisualElement CreateEntryValueField(Type entryType, ExposedParameter parameter)
		{
			switch (entryType.Name)
			{
				default:
					var objField = new ObjectField();
					objField.objectType = entryType;
					objField.label = "";
					objField.allowSceneObjects = false;
					objField.style.flexGrow = new StyleFloat(fieldWidth);
					objField.style.width =
						0; //so it does not have such a long minimum width and is aligned with other fields
					objField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);

					if (parameter.value != null)
						objField.value = (UnityEngine.Object)parameter.value;
					return objField;
				case "String":
					var stringField = new TextField();
					parameter.value ??= "";
					stringField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					stringField.style.width = 0;
					stringField.style.flexGrow = new StyleFloat(fieldWidth);

					if (parameter.value != null)
						stringField.value = (string)parameter.value;
					return stringField;
				case "Color":
					var colorField = new ColorField();
					parameter.value ??= new Color(1, 1, 1, 1);
					colorField.value = (Color)parameter.value;
					colorField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					colorField.style.width = 0;
					colorField.style.flexGrow = new StyleFloat(fieldWidth);

					if (parameter.value != null)
						colorField.value = (Color)parameter.value;
					return colorField;
				case "Single":
					var floatField = new FloatField();
					floatField.value = (float)parameter.value;
					floatField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					floatField.style.width = 0;
					floatField.style.flexGrow = new StyleFloat(fieldWidth);

					floatField.value = (float)parameter.value;
					return floatField;
				case "Boolean":
					var boolField = new Button();
					parameter.value ??= true;
					boolField.text = (bool)parameter.value ? "Yes" : "No ";
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
					// not sure why, but this is needed aligns it with other entries:
					boolField.style.marginLeft = new Length(12, LengthUnit.Pixel);
					boolField.style.width = 0;
					boolField.style.flexGrow = new StyleFloat(fieldWidth);
					return boolField;
				case "Int32":
					var intField = new IntegerField();
					intField.value = (int)parameter.value;
					intField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					intField.style.width = 0;
					intField.style.flexGrow = new StyleFloat(fieldWidth);

					intField.value = (int)parameter.value;
					return intField;
				case "Vector2":
					var vector2Field = new Vector2Field();
					vector2Field.value = (Vector2)parameter.value;
					vector2Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector2Field.style.width = 0;
					vector2Field.style.flexGrow = new StyleFloat(fieldWidth);

					vector2Field.value = (Vector2)parameter.value;
					return vector2Field;
				case "Vector3":
					var vector3Field = new Vector3Field();
					parameter.value ??= new Vector3();
					vector3Field.value = (Vector3)parameter.value;
					vector3Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector3Field.style.width = 0;
					vector3Field.style.flexGrow = new StyleFloat(fieldWidth);

					vector3Field.value = (Vector3)parameter.value;
					return vector3Field;
				case "Vector4":
					var vector4Field = new Vector4Field();
					parameter.value ??= new Vector4();
					vector4Field.value = (Vector4)parameter.value;
					vector4Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector4Field.style.width = 0;
					vector4Field.style.flexGrow = new StyleFloat(fieldWidth);

					vector4Field.value = (Vector4)parameter.value;
					return vector4Field;
			}
		}
	}
}