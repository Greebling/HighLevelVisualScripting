using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	[CustomEditor(typeof(HlvsGraph), false)]
	public class HlvsAssetInspector : GraphInspector
	{
		protected override void CreateInspector()
		{
			base.CreateInspector();

			root.Add(new Button(() => EditorWindow.GetWindow<HlvsWindow>().InitializeGraph(target as HlvsGraph))
			{
				text = "Open in Editor"
			});
		}

		[MenuItem("Assets/Create/HLVS Graph", false, 128)]
		public static void CreateGraphProcessor()
		{
			var graph = CreateInstance<HlvsGraph>();
			ProjectWindowUtil.CreateAsset(graph, HlvsSettings.DefaultAssetName);
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