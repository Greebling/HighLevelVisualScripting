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
		private HlvsGraph _graph;

		protected VisualElement root;

		private VisualElement _defaultContainer;
		VisualElement _parameterContainer;

		/// <summary>
		/// IMGUI elements have per default a margin of 0px, however Unity's properties have a 1px, 3px margin. This fixes the differences
		/// </summary>
		private StyleSheet _alignmentFixSheet;

		private const string ParameterFixSheet = "PropertyFieldAlignmentFix";

		protected void OnEnable()
		{
			_alignmentFixSheet = Resources.Load<StyleSheet>(ParameterFixSheet);
			Debug.Assert(_alignmentFixSheet != null, "Did not find parameter fix style sheet");

			Reinitialize();
		}

		private void Reinitialize()
		{
			_graph = (target as HLVSBehaviour)?.graph;
		}

		public sealed override VisualElement CreateInspectorGUI()
		{
			Reinitialize();

			root = new VisualElement();
			root.styleSheets.Add(_alignmentFixSheet);
			CreateInspector();
			return root;
		}

		protected void CreateInspector()
		{
			_defaultContainer = new VisualElement { name = "DefaultElements" };
			_parameterContainer = new VisualElement { name = "ExposedParameters" };

			AddDefaultElementsTo(_defaultContainer);

			root.Add(_defaultContainer);
			root.Add(new Button(() => EditorWindow.GetWindow<HlvsWindow>().InitializeGraph(_graph))
			{
				text = "Open"
			});


			var separatorBar = new Box
			{
				style =
				{
					borderBottomColor = new Color { a = 1, r = 1, g = 1, b = 1 },
					marginTop = 10,
					marginBottom = 3
				}
			};
			root.Add(separatorBar);
			root.Add(_parameterContainer);
		}

		void AddDefaultElementsTo(VisualElement container)
		{
			var property = serializedObject.FindProperty("graph");
			var propertyField = new PropertyField(property)
				{ label = "Graph", tooltip = "The graph asset that describes this components actions" };
			propertyField.Bind(serializedObject);
			propertyField.RegisterValueChangeCallback(
				evt =>
				{
					if (evt.changedProperty.objectReferenceValue)
					{
						Reinitialize();

						_parameterContainer.Clear();
						if (_graph)
							AddGraphParametersTo(_parameterContainer);
					}
				});

			VisualElement view = new VisualElement();
			view.Add(propertyField);

			container.Add(view);
		}

		void AddGraphParametersTo(VisualElement container)
		{
			var titleLabel = new Label("Parameters");
			container.Add(titleLabel);
		}
		
		
	}
}