using GraphProcessor;
using UnityEditor;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	public class HlvsGraphView : BaseGraphView
	{
		public HlvsGraphView(EditorWindow window) : base(window)
		{
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			BuildRunGraphMenu(evt);
			base.BuildContextualMenu(evt);
		}

		/// <summary>
		/// Add the View entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected void BuildRunGraphMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Run Graph", e => (graph as HlvsGraph)?.RunStartNodes());
			evt.menu.AppendSeparator();
		}
	}
}