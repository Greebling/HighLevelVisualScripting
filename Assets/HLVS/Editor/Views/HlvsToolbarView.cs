using GraphProcessor;
using UnityEditor;

namespace HLVS.Editor.Views
{
	public class HlvsToolbarView : ToolbarView
	{
		private readonly HlvsGraphView _view;


		public HlvsToolbarView(BaseGraphView graphView) : base(graphView)
		{
			_view = graphView as HlvsGraphView;
			style.minHeight = 20; // fix for overlapping ui elements
		}

		protected override void AddButtons()
		{
			AddToggle("Show Boards", _view.boardContainer.visible, v => { _view.boardContainer.visible = v; });

			AddButton("Save", () =>
			{
				EditorUtility.SetDirty(_view.graph);
				AssetDatabase.SaveAssets();
			}, false);
			AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
		}
	}
}