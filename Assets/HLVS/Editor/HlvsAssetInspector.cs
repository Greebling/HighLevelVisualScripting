using GraphProcessor;
using HLVS.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	[CustomEditor(typeof(HlvsGraph), false)]
	public class HlvsAssetInspector : UnityEditor.Editor
	{
		protected VisualElement root;
		protected BaseGraph     graph;
		
		protected void OnEnable()
		{
			graph = target as BaseGraph;
		}
		
		public sealed override VisualElement CreateInspectorGUI()
		{
			root = new VisualElement();
			
			root.Add(new Button(() => EditorWindow.GetWindow<HlvsWindow>().InitializeGraph(target as HlvsGraph))
			{
				text = "Open in Editor"
			});
			
			return root;
		}

		[MenuItem("Assets/Create/HLVS Graph", false, 128)]
		public static void CreateGraphProcessor()
		{
			var graph = CreateInstance<HlvsGraph>();
			ProjectWindowUtil.CreateAsset(graph, HlvsSettings.DefaultAssetName);
		}
		
		[MenuItem("Assets/Create/HLVS Blackboard", false, 128)]
		public static void CreateBlackboard()
		{
			var board = CreateInstance<HlvsBlackboard>();
			ProjectWindowUtil.CreateAsset(board, HlvsSettings.DefaultBlackboardAssetName);
		}

		[OnOpenAsset(0)]
		public static bool OnBaseGraphOpened(int instanceID, int line)
		{
			var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

			if (asset == null)
				return false;

			EditorWindow.GetWindow<HlvsWindow>().InitializeGraph(asset);
			return true;
		}
	}
}