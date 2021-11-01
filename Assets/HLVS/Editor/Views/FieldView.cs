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

		protected void AfterFieldRenamed(ExposedParameter field, VisualElement fieldContainer,
			TextField nameField = null)
		{
			var previousParent = fieldContainer.parent;
			bool sectionHasOneChild = false;
			// remove from current parent
			if (previousParent != null)
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

				if (sectionHasOneChild && previousParent.name != String.Empty)
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

				// sort categories displayed in blackboard
				blackboard.Sort((element, element2) =>
				{
					var name1 = element.name;
					var name2 = element2.name;
					if (name1.Length < name2.Length)
					{
						return string.CompareOrdinal(name1, name2.Substring(0, name1.Length));
					}
					else if (name1.Length > name2.Length)
					{
						return string.CompareOrdinal(name1.Substring(0, name2.Length), name2);
					}
					else
					{
						return string.CompareOrdinal(name1, name2);
					}
				});
			}

			if (sectionHasOneChild && previousParent.name != string.Empty && previousParent.name != allCategories)
			{
				previousParent.parent.Remove(previousParent);
			}
		}

		public void RemoveField(ExposedParameter field, VisualElement container)
		{
			var previousParent = container.parent;
			// remove from current parent
			if (previousParent != null)
			{
				previousParent.contentContainer.Remove(container);
				// delete parent if there are no more field in the section
				if (previousParent.contentContainer.childCount == 0)
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

		/// <summary>
		/// Creates an editor field for a given type
		/// </summary>
		/// <param name="entryType">Of the parameter to be shown. Needed for reference types, when parameter.value==null</param>
		/// <returns></returns>
		public static VisualElement CreateEntryValueField(Type entryType)
		{
			VisualElement field;
			if (entryType.IsEnum)
			{
				var enumField = new EnumField();
				enumField.Init((Enum)Activator.CreateInstance(entryType));
				field = enumField;
			}
			else
			{
				switch (entryType.Name)
				{
					default:
						var objField = new ObjectField();
						objField.objectType = entryType;
						objField.label = "";
						objField.allowSceneObjects = false;
						field = objField;
						break;
					case "String":
						var stringField = new TextField();
						field = stringField;
						break;
					case "Color":
						var colorField = new ColorField();
						colorField.value = new Color(1, 1, 1, 1);
						field = colorField;
						field.style.height = 20;
						break;
					case "Single":
						var floatField = new FloatField();
						field = floatField;
						break;
					case "Boolean":
						var boolField = new Toggle();
						field = boolField;
						break;
					case "Int32":
						var intField = new IntegerField();
						field = intField;
						break;
					case "Vector2":
						var vector2Field = new Vector2Field();
						field = vector2Field;
						break;
					case "Vector3":
						var vector3Field = new Vector3Field();
						field = vector3Field;
						break;
					case "Vector4":
						var vector4Field = new Vector4Field();
						field = vector4Field;
						break;
				}
			}
			
			

			return field;
		}
	}
}