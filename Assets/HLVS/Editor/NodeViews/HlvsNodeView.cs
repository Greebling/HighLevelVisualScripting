using System.Reflection;
using GraphProcessor;
using HLVS.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(HlvsNode))]
	public class HlvsNodeView : BaseNodeView
	{
		// TODO: Create customized port view to create here
		protected override PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
			=> HlvsPortView.CreatePortView(direction, fieldInfo, portData, listener);

		public HlvsGraph graph => owner.graph as HlvsGraph;

		private StyleSheet _nodeStyle;

		public override void Enable(bool fromInspector = false)
		{
			base.Enable(fromInspector);

			_nodeStyle = Resources.Load<StyleSheet>("HlvsNodeStyling");
			styleSheets.Add(_nodeStyle);
		}
	}
}