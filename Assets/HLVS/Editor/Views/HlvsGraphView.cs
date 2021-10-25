using GraphProcessor;
using UnityEditor;

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
	}
}