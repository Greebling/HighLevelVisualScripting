using System.Reflection;
using GraphProcessor;
using UnityEditor.Experimental.GraphView;

namespace HLVS.Editor.NodeViews
{
	public class HlvsNodeView : BaseNodeView
	{
		// TODO: Create customized port view to create here
		protected override PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener listener)
			=> PortView.CreatePortView(direction, fieldInfo, portData, listener);
	}
}