using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	[CustomEditor(typeof(HlvsBehaviour))]
	public class HlvsBehaviourEditor : UnityEditor.Editor
	{
		private HlvsGraph _graph;

		private HlvsBehaviour _behaviour => target as HlvsBehaviour;

		protected VisualElement root;

		private VisualElement _defaultContainer;
		private VisualElement _parameterContainer;

		private void OnEnable()
		{
			if (!target)
				return;

			Reinitialize();
			_behaviour.OnParamListChanged();
		}

		private void OnDisable()
		{
			Deinitialize();
		}

		private void OnDestroy()
		{
			Deinitialize();
		}

		private void Reinitialize()
		{
			Deinitialize();
			
			if (!target)
				return;

			_graph = (target as HlvsBehaviour).graph;
			if (_graph)
			{
				_graph.onParameterListChanged += OnParamsChanged;
			}
		}

		private void Deinitialize()
		{
			if (_graph)
			{
				_graph.onParameterListChanged -= OnParamsChanged;
			}
		}

		public override VisualElement CreateInspectorGUI()
		{
			Reinitialize();

			root = new VisualElement();
			CreateInspector();
			return root;
		}

		void OnParamsChanged()
		{
			_behaviour.OnParamListChanged();
			CreateInspector();
		}

		protected void CreateInspector()
		{
			if (_behaviour.IsParameterListOutdated())
			{
				_behaviour.OnParamListChanged();
			}

			root.Clear();
			_defaultContainer = new VisualElement { name = "DefaultElements" };
			_parameterContainer = new VisualElement { name = "ExposedParameters" };


			// Top
			AddDefaultElementsTo(_defaultContainer);
			root.Add(_defaultContainer);

			// Bottom
			AddGraphParametersTo(_parameterContainer);
			root.Add(_parameterContainer);
		}

		void AddDefaultElementsTo(VisualElement container)
		{
			// graph field
			var property = serializedObject.FindProperty("graph");
			var propertyField = new PropertyField(property)
				{ label = "Graph", tooltip = "The graph asset that describes this components behaviour" };
			propertyField.Bind(serializedObject);
			propertyField.RegisterValueChangeCallback(
				evt =>
				{
					// check if the graph value actually changed
					if (_graph == _behaviour.graph)
						return;


					Deinitialize();
					Reinitialize();
					serializedObject.Update();

					_behaviour.CreateFittingParamList();
					CreateInspector();
				});

			VisualElement view = new VisualElement();
			view.Add(propertyField);


			// open button
			var button = new Button(() =>
			{
				if (_behaviour.graph)
					EditorWindow.GetWindow<HlvsWindow>().InitializeGraph(_behaviour.graph);
			});
			button.text = "Open";
			button.name = "open-button";
			button.SetEnabled(_behaviour.CurrentGraph);
			view.Add(button);

			container.Add(view);
		}

		void AddGraphParametersTo(VisualElement container)
		{
			container.Clear();
			var paramsContainer = new VisualElement();

			var titleLabel = new Label("Parameters");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.fontSize = 13;
			titleLabel.style.marginTop = 6;
			titleLabel.style.marginBottom = 3;
			paramsContainer.Add(titleLabel);


			// add label if no params are drawn
			if (_behaviour.graphParameters.Count == 0 || _behaviour.graphParameters.All(parameter => parameter.name == "Self"))
			{
				var noneLabel = new Label("No Graph Parameters");
				noneLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
				noneLabel.style.marginTop = 3;
				noneLabel.style.marginBottom = 3;
				noneLabel.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
				paramsContainer.Add(noneLabel);
			}


			var displayedParams = _behaviour.graphParameters.OrderBy(parameter =>
			{
				var slashIndex = parameter.name.LastIndexOf('/');
				if (slashIndex == -1)
				{
					return "a"; // this should be at the top
				}
				else
				{
					return parameter.name.Substring(0, slashIndex);
				}
			}).Where(parameter => parameter.name != "Self");

			void AddCategoryLabel(string categoryName, int depth)
			{
				var categoryLabel = new Label(categoryName.PadLeft(depth + categoryName.Length));
				categoryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
				categoryLabel.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
				categoryLabel.style.width = 150;
				paramsContainer.Add(categoryLabel);
			}

			List<int> prevCategoriesHash = new List<int>();
			serializedObject.Update();
			foreach (var graphParameter in displayedParams)
			{
				var nameAndCategories = graphParameter.name.Split('/');
				string fieldName = nameAndCategories.Last();

				// add heading labels
				if (nameAndCategories.Length != 1)
				{
					for (int i = 0; i < nameAndCategories.Length - 1; i++)
					{
						var currHash = nameAndCategories[i].GetHashCode();
						if (prevCategoriesHash.Count <= i)
						{
							prevCategoriesHash.Add(currHash);
							AddCategoryLabel(nameAndCategories[i], i);
						}
						else if (prevCategoriesHash[i] != currHash)
						{
							prevCategoriesHash.RemoveRange(i, prevCategoriesHash.Count - i);
							prevCategoriesHash.Add(currHash);
							AddCategoryLabel(nameAndCategories[i], i);
						}
					}
				}
				else if (prevCategoriesHash.Count != 0)
				{
					// Add separator to separate uncategorized variables
					prevCategoriesHash.Clear();
					var separator = new Box();
					separator.style.height = 0;
					separator.style.marginTop = 3;
					separator.style.marginBottom = 3;
					separator.style.borderBottomColor = new Color(1f, 1f, 1f, 0.22f);
					paramsContainer.Add(separator);
				}


				var fieldContainer = new VisualElement();
				fieldContainer.style.flexDirection = FlexDirection.Row;

				// create value field
				int paramIndex =
					_behaviour.graphParameters.FindIndex(parameter => parameter.guid == graphParameter.guid);
				var prop = serializedObject.FindProperty("graphParameters").GetArrayElementAtIndex(paramIndex)
					.FindPropertyRelative("val");
				var field = new PropertyField(prop);
				field.Bind(serializedObject);
				field.label = fieldName.PadLeft(prevCategoriesHash.Count + fieldName.Length + 1);
				field.style.width = 1000;

				fieldContainer.Add(field);

				// fix alignment for buttons
				if (field.ClassListContains("unity-button"))
					field.style.marginLeft = new Length(3, LengthUnit.Pixel);

				paramsContainer.Add(fieldContainer);
			}

			container.Add(paramsContainer);
		}
	}
}