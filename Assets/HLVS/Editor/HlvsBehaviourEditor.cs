using System;
using System.Collections.Generic;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Runtime;
using UnityEditor;
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

		private readonly FieldView _parameterView = new FieldView();

		private void OnEnable()
		{
			Reinitialize();
		}

		private void OnDisable()
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

		public sealed override VisualElement CreateInspectorGUI()
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


			foreach (var graphParameter in _behaviour.graphParameters)
			{
				var fieldContainer = new VisualElement();
				fieldContainer.style.flexDirection = FlexDirection.Row;

				// labeling
				var nameLabel = new Label($"{graphParameter.name}");
				nameLabel.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
				nameLabel.style.width = 150;
				fieldContainer.Add(nameLabel);

				// value field
				var field = _parameterView.CreateEntryValueField(graphParameter.GetValueType(), graphParameter);
				field.style.width = new StyleLength(StyleKeyword.Auto);
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