using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using UnityEngine;

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
			if (_startNodes.Count == 0)
				return;
			
			_startNodes.Sort((node1, node2) => Mathf.CeilToInt(node1.position.y - node2.position.y));
			

			// eval compute order for all nodes
			int executionOrder = -1;
			foreach (var startNode in _startNodes)
			{
				executionOrder++;
				startNode.computeOrder = executionOrder;
				ApplyToGraph(startNode, node =>
				{
					ComputeDependencyComputeOrder(node, ref executionOrder);
					
					executionOrder++;
					node.computeOrder = executionOrder;
				});
			}
		}

		private static void ComputeDependencyComputeOrder(BaseNode node, ref int computeOrder)
		{
			foreach (var dependencyNode in node.GetInputNodes())
			{
				if (dependencyNode is HlvsActionNode || dependencyNode is ExecutionStarterNode)
				{
					// TODO: Check if this dependency works
					continue;
				}
				
				ComputeDependencyComputeOrder(dependencyNode, ref computeOrder);
				
				computeOrder++;
				dependencyNode.computeOrder = computeOrder;
			}
		}

		public override void Run()
		{
			foreach (var startNode in _startNodes)
			{
				ApplyToGraph(startNode, hlvsNode =>
				{
					GetNodeDataDependencies(hlvsNode);
			
					hlvsNode.OnProcess();
				});
			}
		}

		protected delegate void ApplyFunction(HlvsNode node);

		/// <summary>
		/// Applies a given function to all nodes, beginning AFTER startNode
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="func">To apply to following nodes</param>
		protected void ApplyToGraph(TExecutionStartNode startNode, ApplyFunction func)
		{
			HlvsNode currNode = GetNextNode(startNode);
			while (currNode != null)
			{
				func(currNode);
				currNode = GetNextNode(currNode);
			}
		}

		private static void GetNodeDataDependencies(BaseNode currNode) // TODO: Make this a HlvsNode
		{
			// get own dependencies
			foreach (var node in currNode.GetInputNodes())
			{
				// ignore action flow as input TODO: Will the node design force action nodes to have no other outputs except the action flow?
				if (node is HlvsActionNode || node is ExecutionStarterNode)
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