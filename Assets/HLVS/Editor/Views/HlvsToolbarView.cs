using GraphProcessor;
using UnityEditor;

namespace HLVS.Editor.Views
{
	public class HlvsToolbarView : ToolbarView
	{
		
		protected ToolbarButtonData showBlackboard;
		protected HlvsGraphView view;


		public HlvsToolbarView(BaseGraphView graphView) : base(graphView)
		{
			view = graphView as HlvsGraphView;
		}

		protected override void AddButtons()
		{
			showBlackboard = AddToggle("Show Blackboard", view.blackboardView.blackboard.visible, v =>
			{
				view.blackboardView.blackboard.visible = v;
			});

			AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
		}
	}
}