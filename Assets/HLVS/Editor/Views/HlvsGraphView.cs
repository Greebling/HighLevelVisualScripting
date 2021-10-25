using GraphProcessor;
using UnityEditor;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class HlvsGraphView : BaseGraphView
	{
		public BlackboardView blackboardView;
		
		public HlvsGraphView(EditorWindow window) : base(window)
		{
			blackboardView = new BlackboardView(this);
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
		}
	}
}