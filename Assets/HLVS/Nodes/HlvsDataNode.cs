using System.Linq;
using GraphProcessor;

namespace HLVS.Nodes
{
	public class HlvsDataNode : HlvsNode
	{
		public override HlvsNode GetPreviousNode()
		{
			// we want to check for non data dependencies first
			var dependency = GetNonDataDependency();
			if (dependency != null)
			{
				return dependency;
			}
			
			//else  we want to use any data input as all should have the same execution origin
			var aDataPort = inputPorts.FirstOrDefault(port => port.GetEdges().Count != 0);
			if (aDataPort != null)
			{
				return (HlvsNode) aDataPort.GetEdges().FirstOrDefault()?.outputNode;
			}

			return null;
		}

		private HlvsNode GetNonDataDependency()
		{
			foreach (BaseNode baseNode in inputPorts.Where(port => port.GetEdges().Count != 0).SelectMany(port => port.GetEdges(), (port, edge) => edge.outputNode))
			{
				if (baseNode is not HlvsDataNode dataNode)
				{
					return (HlvsNode) baseNode;
				}
				else
				{
					var dep = dataNode.GetNonDataDependency();
					if (dep != null)
					{
						return dataNode; // we do not return the actual dependency but the previous node that brings us to that dependency
					}
				}
			}

			return null;
		}
	}
}