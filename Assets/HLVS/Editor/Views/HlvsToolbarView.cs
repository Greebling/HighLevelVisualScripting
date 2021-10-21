using GraphProcessor;
using UnityEditor;
using UnityEngine.UIElements;

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
			showBlackboard = AddToggle("Show Blackboard", view.blackboard.visible, v =>
			{
				view.blackboard.visible = v;
			});

			bool exposedParamsVisible = graphView.GetPinnedElementStatus<ExposedParameterView>() !=
				DropdownMenuAction.Status.Hidden;
			showParameters = AddToggle("Show Parameters", exposedParamsVisible,
				(v) => graphView.ToggleView<ExposedParameterView>());


			AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
		}
	}
}