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
	[CustomEditor(typeof(HLVSBehaviour))]
	public class HlvsBehaviourEditor : UnityEditor.Editor
	{
		private HlvsGraph _graph;

		private HLVSBehaviour _behaviour => target as HLVSBehaviour;

		protected VisualElement root;

		private VisualElement _defaultContainer;
		private VisualElement _parameterContainer;

		private void OnEnable()
		{
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
			_graph = (target as HLVSBehaviour).graph;
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

					_behaviour.CreateFittingParamList();
					CreateInspector();
				});

			VisualElement view = new VisualElement();
			view.Add(propertyField);


			// open button
			var button = new Button(() =>
			{
				if (_graph)
					EditorWindow.GetWindow<HlvsWindow>().InitializeGraph(_graph);
			});
			button.text = "Open";
			button.name = "open-button";
			button.SetEnabled(_graph);
			view.Add(button);

			container.Add(view);
		}

		void AddGraphParametersTo(VisualElement container)
		{
			container.Clear();
			var paramsContainer = new VisualElement();

			var titleLabel = new Label("Parameters:");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.fontSize = 13;
			titleLabel.style.marginTop = 6;
			titleLabel.style.marginBottom = 3;
			paramsContainer.Add(titleLabel);


			// add label if no params are drawn
			if (_behaviour.graphParameters.Count == 0)
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
					return "zzzzzzzzz"; // TODO: There must be a more elegant solution to this
				}
				else
				{
					return parameter.name.Substring(0, slashIndex);
				}
			});

			void AddCategoryLabel(string categoryName, int depth)
			{
				var categoryLabel = new Label(categoryName.PadLeft(depth + categoryName.Length));
				categoryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
				categoryLabel.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
				categoryLabel.style.width = 150;
				paramsContainer.Add(categoryLabel);
			}

			List<int> prevCategoriesHash = new List<int>();
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

				// labeling
				var nameLabel = new Label(fieldName.PadLeft(prevCategoriesHash.Count + fieldName.Length + 1));
				nameLabel.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
				nameLabel.style.width = 150;
				fieldContainer.Add(nameLabel);

				// value field
				var field = graphParameter.GetPropertyDrawer(o =>
				{
					EditorUtility.SetDirty(_behaviour);
					EditorSceneManager.MarkSceneDirty(_behaviour.gameObject.scene);
				});
				field.RegisterCallback<FocusInEvent>(evt =>
				{
					Undo.RecordObject(_behaviour, $"Changed {graphParameter.name}");
				});
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