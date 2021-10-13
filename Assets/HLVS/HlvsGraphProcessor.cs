using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using HLVS.Runtime;

namespace HLVS
{
	public class HlvsGraphProcessor<TExecutionStartNode> : BaseGraphProcessor where TExecutionStartNode : HlvsNode
	{
		List<TExecutionStartNode> _startNodes;

		public HlvsGraphProcessor(BaseGraph graph) : base(graph)
		{
		}

		public override void UpdateComputeOrder()
		{
			_startNodes = graph.nodes.OfType<TExecutionStartNode>().ToList();
			_startNodes.Sort((node1, node2) => node1.computeOrder - node2.computeOrder);
		}

		public override void Run()
		{
			foreach (var node in _startNodes)
			{
				RunFromNode(node);
			}
		}

		protected void RunFromNode(TExecutionStartNode startNode)
		{
			HlvsNode currNode = GetNextNode(startNode);
			while (currNode != null)
			{
				GetNodeDataDependencies(currNode);


				currNode.OnProcess();
				currNode = GetNextNode(currNode);
			}
		}

		private static void GetNodeDataDependencies(BaseNode currNode) // TODO: Make this a HlvsNode
		{
			// get own dependencies
			foreach (var node in currNode.GetInputNodes())
			{
				// ignore action flow as input TODO: Will the node design force action nodes to have no other outputs except the action flow?
				if (node is HlvsActionNode)
					continue;

				// fetch own data dependencies
				foreach (var nodeInputPort in node.GetInputNodes())
				{
					GetNodeDataDependencies(nodeInputPort);
				}
				node.OnProcess();

				foreach (var outputPort in node.outputPorts)
				{
					outputPort.PushData();
				}
			}
		}

		public static HlvsNode GetNextNode(HlvsNode node)
		{
			return node.GetOutputNodes().FirstOrDefault() as HlvsNode;
		}
		
		public static HlvsNode GetPreviousNode(HlvsNode node)
		{
			return  node.GetInputNodes().FirstOrDefault() as HlvsNode;
		}
	}
}