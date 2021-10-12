using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace HLVS.Editor
{
	public class HlvsWindow : BaseGraphWindow
	{
		BaseGraph         tmpGraph;
		HlvsToolbarView toolbarView;

		[MenuItem("HLVS/Graph Editor")]
		public static BaseGraphWindow OpenWithTmpGraph()
		{
			var graphWindow = CreateWindow<HlvsWindow>();

			// When the graph is opened from the window, we don't save the graph to disk
			graphWindow.tmpGraph = CreateInstance<BaseGraph>();
			graphWindow.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
			graphWindow.InitializeGraph(graphWindow.tmpGraph);

			graphWindow.Show();

			return graphWindow;
		}

		protected override void OnDestroy()
		{
			graphView?.Dispose();
			DestroyImmediate(tmpGraph);
		}

		protected override void InitializeWindow(BaseGraph startingGraph)
		{
			titleContent = new GUIContent($"{HlvsSettings.Name} Graph");

			if (graphView == null)
			{
				graphView = new HlvsGraphView(this);
				toolbarView = new HlvsToolbarView(graphView);
				graphView.Add(toolbarView);
			}

			rootView.Add(graphView);
		}
	}
}