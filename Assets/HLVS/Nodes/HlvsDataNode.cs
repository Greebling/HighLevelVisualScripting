using System.Linq;

namespace HLVS.Nodes
{
	public class HlvsDataNode : HlvsNode
	{
		public override HlvsNode GetPreviousNode()
		{
			// we want to use any data input as all should have the same execution origin
			var aDataPort = inputPorts.FirstOrDefault(port => port.GetEdges().Count != 0);
			if (aDataPort != null)
			{
				return (HlvsNode) aDataPort.GetEdges().FirstOrDefault()?.outputNode;
			}

			return null;
		}
	}
}