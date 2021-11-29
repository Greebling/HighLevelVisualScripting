using System.Reflection;
using GraphProcessor;
using HLVS.Editor.Views;
using HLVS.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(HlvsNode))]
	public class HlvsNodeView : BaseNodeView
	{
		protected override PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
			=> HlvsPortView.CreatePortView(graph, (HlvsGraphView) owner, (HlvsNode) nodeTarget, direction, fieldInfo, portData, listener);

		public HlvsGraph graph => owner.graph as HlvsGraph;

		public override void Enable(bool fromInspector = false)
		{
			base.Enable(fromInspector);

			var nodeStyle = Resources.Load<StyleSheet>("HlvsNodeStyling");
			styleSheets.Add(nodeStyle);
			
			var portStyle = Resources.Load<StyleSheet>("PortVariableSelector");
			styleSheets.Add(portStyle);
		}
	}
}