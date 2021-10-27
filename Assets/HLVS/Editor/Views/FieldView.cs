using System;
using GraphProcessor;
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
					objField.style.flexGrow = new StyleFloat(1.5f);
					objField.style.width =
						0; //so it does not have such a long minimum width and is aligned with other fields
					objField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);

					if (parameter.value != null)
						objField.value = (UnityEngine.Object) parameter.value;
					return objField;
				case "String":
					var stringField = new TextField();
					parameter.value ??= "";
					stringField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					stringField.style.width = 0;
					stringField.style.flexGrow = new StyleFloat(1.5f);
					
					if (parameter.value != null)
						stringField.value = (string) parameter.value;
					return stringField;
				case "Color":
					var colorField = new ColorField();
					parameter.value ??= new Color(1, 1, 1, 1);
					colorField.value = (Color)parameter.value;
					colorField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					colorField.style.width = 0;
					colorField.style.flexGrow = new StyleFloat(1.5f);
					
					if (parameter.value != null)
						colorField.value = (Color) parameter.value;
					return colorField;
				case "Single":
					var floatField = new FloatField();
					floatField.value = (float)parameter.value;
					floatField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					floatField.style.width = 0;
					floatField.style.flexGrow = new StyleFloat(1.5f);
					
					floatField.value = (float) parameter.value;
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
					boolField.style.flexGrow = new StyleFloat(1.5f);
					return boolField;
				case "Int32":
					var intField = new IntegerField();
					intField.value = (int)parameter.value;
					intField.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					intField.style.width = 0;
					intField.style.flexGrow = new StyleFloat(1.5f);
					
					intField.value = (int) parameter.value;
					return intField;
				case "Vector2":
					var vector2Field = new Vector2Field();
					vector2Field.value = (Vector2)parameter.value;
					vector2Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector2Field.style.width = 0;
					vector2Field.style.flexGrow = new StyleFloat(1.5f);
					
					vector2Field.value = (Vector2) parameter.value;
					return vector2Field;
				case "Vector3":
					var vector3Field = new Vector3Field();
					parameter.value ??= new Vector3();
					vector3Field.value = (Vector3)parameter.value;
					vector3Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector3Field.style.width = 0;
					vector3Field.style.flexGrow = new StyleFloat(1.5f);
					
					vector3Field.value = (Vector3) parameter.value;
					return vector3Field;
				case "Vector4":
					var vector4Field = new Vector4Field();
					parameter.value ??= new Vector4();
					vector4Field.value = (Vector4)parameter.value;
					vector4Field.RegisterValueChangedCallback(evt => parameter.value = evt.newValue);
					vector4Field.style.width = 0;
					vector4Field.style.flexGrow = new StyleFloat(1.5f);
					
					vector4Field.value = (Vector4) parameter.value;
					return vector4Field;
			}
		}
	}
}