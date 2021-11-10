using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	[CustomEditor(typeof(HlvsBlackboard))]
	public class HlvsBlackboardEditor : UnityEditor.Editor
	{
		private HlvsBlackboard _target => target as HlvsBlackboard;

		protected VisualElement root;

		public override VisualElement CreateInspectorGUI()
		{
			root ??= new VisualElement();
			root.Clear();
			root.style.marginTop = 10;
			
			CreateInspector();
			
			return root;
		}

		private void OnEnable()
		{
			root = new VisualElement();
		}
		
		void OnValidate()
		{
			Debug.Log("Val");
		}

		private void CreateInspector()
		{
			if (_target.fields.Count == 0)
			{
				var noParamsLabel = new Label("No variables saved in blackboard");
				noParamsLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
				root.Add(noParamsLabel);
			} else
			{
				// TODO: Moving objects breaks these property fields
				var fieldList = serializedObject.FindProperty("fields");

				for (int i = 0; i < _target.fields.Count; i++)
				{
					var property = fieldList.GetArrayElementAtIndex(i);
					var fieldDrawer = new PropertyField(property);
					fieldDrawer.Bind(serializedObject);
					root.Add(fieldDrawer);
				}
			}
		}
	}
}