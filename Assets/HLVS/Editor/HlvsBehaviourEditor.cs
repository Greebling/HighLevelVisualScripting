using GraphProcessor;
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
		private SerializedProperty _serializedGraph;
		private HlvsGraph          _graph;
		private HlvsGraph          _previousGraph;

		protected VisualElement                root;
		protected ExposedParameterFieldFactory parameterFactory;

		private VisualElement _defaultContainer;
		VisualElement         _parameterContainer;
		
		/// <summary>
		/// IMGUI elements have per default a margin of 0px, however Unity's properties have a 1px, 3px margin. This fixes the differences
		/// </summary>
		private StyleSheet _alignmentFixSheet;
		private const string ParameterFixSheet = "PropertyFieldAlignmentFix";

		protected virtual void OnEnable()
		{
			_alignmentFixSheet = Resources.Load<StyleSheet>(ParameterFixSheet);
			Debug.Assert(_alignmentFixSheet != null, "Did not find parameter fix style sheet");

			Init();
		}

		private void Init()
		{
			_graph = (target as HLVSBehaviour)?.graph;
			if (_graph)
			{
				RegisterExposedParameters(_graph);

				parameterFactory = new ExposedParameterFieldFactory(_graph);
			}
			else
			{
				parameterFactory = null;
			}
		}

		protected virtual void OnDisable()
		{
			if (_graph)
			{
				UnregisterExposedParameters(_graph);
			}

			parameterFactory?.Dispose();
			parameterFactory = null;
		}

		private void RegisterExposedParameters(HlvsGraph graph)
		{
			graph.onExposedParameterListChanged += UpdateExposedParameters;
			graph.onExposedParameterModified += UpdateExposedParameters;
		}

		private void UnregisterExposedParameters(HlvsGraph graph)
		{
			graph.onExposedParameterListChanged -= UpdateExposedParameters;
			graph.onExposedParameterModified -= UpdateExposedParameters;
		}

		public sealed override VisualElement CreateInspectorGUI()
		{
			Init();

			root = new VisualElement();
			root.styleSheets.Add(_alignmentFixSheet);
			CreateInspector();
			return root;
		}

		protected virtual void CreateInspector()
		{
			_defaultContainer = new VisualElement { name = "DefaultElements" };
			_parameterContainer = new VisualElement { name = "ExposedParameters" };

			DefaultElements(_defaultContainer);

			if (_graph)
				FillExposedParameters(_parameterContainer);


			var separatorBox = new Box();
			separatorBox.style.borderBottomColor = new Color { a = 1, r = 1, g = 1, b = 1 };
			separatorBox.style.marginTop = 10;
			separatorBox.style.marginBottom = 3;

			root.Add(_defaultContainer);
			root.Add(separatorBox);
			root.Add(_parameterContainer);
		}

		void DefaultElements(VisualElement container)
		{
			var property = serializedObject.FindProperty("graph");
			var propertyField = new PropertyField(property) { label = "Graph", tooltip = "The graph that describes this components actions" };
			propertyField.Bind(serializedObject);
			propertyField.RegisterValueChangeCallback(
				evt =>
				{
					if (evt.changedProperty.objectReferenceValue)
					{
						// unregister old callbacks
						if (_previousGraph)
							UnregisterExposedParameters(_previousGraph);

						Init();

						_parameterContainer.Clear();
						if (_graph)
							FillExposedParameters(_parameterContainer);

						Debug.Log($"graph changed to: {_graph.name}");
						_previousGraph = _graph;
					}
				});

			VisualElement view = new VisualElement();
			view.Add(propertyField);

			container.Add(view);
		}

		protected void FillExposedParameters(VisualElement parameterContainer)
		{
			// params title
			var paramsTitle = new Label("Parameters:");
			paramsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
			paramsTitle.tooltip =
				"Parameters are the inputs of a graph that can differ for each gameobject. They are the only way for graph to work with scene references.";
			parameterContainer.Add(paramsTitle);


			// in case there a re no parameters, communicate that clearly with the user
			if (_graph.exposedParameters.Count == 0)
			{
				var noParamsLabel = new Label("No parameters found");
				noParamsLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
				parameterContainer.Add(noParamsLabel);

				return;
			}


			// for the case that the custom editor is not registered (call Init()). NOTE: this seems odd, it should not be called in this case
			if (parameterFactory == null)
				return;

			// add parameter fields
			foreach (var param in _graph.exposedParameters)
			{
				if (param.settings.isHidden)
					continue;

				var field = parameterFactory.GetParameterValueField(param, (newValue) =>
				                                                                  {
					                                                                  param.value = newValue;
					                                                                  serializedObject.ApplyModifiedProperties();
					                                                                  _graph.NotifyExposedParameterValueChanged(param);
				                                                                  });
				parameterContainer.Add(field);
			}
		}

		void UpdateExposedParameters(ExposedParameter param) => UpdateExposedParameters();

		void UpdateExposedParameters()
		{
			if (!_graph)
				return;
			if (_parameterContainer == null)
			{
				CreateInspectorGUI();
				return;
			}

			_parameterContainer.Clear();
			FillExposedParameters(_parameterContainer);
		}

		public override void OnInspectorGUI()
		{
		}
	}
}