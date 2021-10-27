using GraphProcessor;
using HLVS.Editor.Views;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace HLVS.Editor
{
	public class HlvsWindow : BaseGraphWindow
	{
		private BaseGraph _tmpGraph;
		private HlvsToolbarView _toolbarView;
		private TwoPaneSplitView _splitView;

		private StyleSheet _customStyling;

		[MenuItem("HLVS/Graph Editor")]
		public static BaseGraphWindow OpenWithTmpGraph()
		{
			var graphWindow = CreateWindow<HlvsWindow>();

			// When the graph is opened from the window, we don't save the graph to disk
			graphWindow._tmpGraph = CreateInstance<BaseGraph>();
			graphWindow._tmpGraph.hideFlags = HideFlags.HideAndDontSave;
			graphWindow.InitializeGraph(graphWindow._tmpGraph);

			graphWindow.Show();

			return graphWindow;
		}

		protected override void OnDestroy()
		{
			graphView?.Dispose();
			DestroyImmediate(_tmpGraph);
		}

		protected override void InitializeWindow(BaseGraph startingGraph)
		{
			titleContent = new GUIContent($"{HlvsSettings.Name} Graph");

			_customStyling = Resources.Load<StyleSheet>("HlvsGraphStyle");
			rootView.styleSheets.Add(_customStyling);

			if (graphView == null)
			{
				var graphView = new HlvsGraphView(this);
				this.graphView = graphView;
				
				_toolbarView = new HlvsToolbarView(graphView);
				
				
				graphView.Add(_toolbarView);


				graphView.blackboardView.blackboard.graphView = graphView;
				graphView.paramView.blackboard.graphView = graphView;
				var vs = new VisualElement();
				vs.style.marginTop = 3;
				vs.style.alignSelf = new StyleEnum<Align>(Align.FlexStart);
				
				vs.Add(graphView.blackboardView.blackboard);
				vs.Add(graphView.paramView.blackboard);
				graphView.Add(vs);

				
				
				// background
				var bg = new GridBackground();
				bg.AddToClassList("my-grid");
				graphView.Insert(0, bg);
			}
			
			rootView.Add(graphView);
		}

		protected override void InitializeGraphView(BaseGraphView baseView)
		{
			var view = baseView as HlvsGraphView;
			view.blackboardView.DisplayExistingBlackboardEntries();
			view.paramView.DisplayExistingParameterEntries();
		}
	}
}