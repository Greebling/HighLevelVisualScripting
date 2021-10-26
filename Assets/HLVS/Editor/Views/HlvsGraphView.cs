using GraphProcessor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class HlvsGraphView : BaseGraphView
	{
		public BlackboardView blackboardView;
		public ParameterView paramView;
		
		public HlvsGraphView(EditorWindow window) : base(window)
		{
			blackboardView = new BlackboardView(this);
			paramView = new ParameterView(this);
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
			// insert menu items here
			// TODO: Remove current menu options as they are way too cluttered
		}
	}
}