using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using UnityEngine;

namespace HLVS
{
	public class HlvsGraphProcessor<TExecutionStartNode> where TExecutionStartNode : HlvsNode
	{
		List<HlvsNode> _startNodes;

		private readonly HlvsGraph _graph;

		public ProcessingStatus status { get; private set; } = ProcessingStatus.Finished;

		public HlvsGraphProcessor(HlvsGraph graph)
		{
			_graph = graph;
		}

		public void UpdateComputeOrder()
		{
			_startNodes = _graph.nodes.Where(node => node is TExecutionStartNode).Cast<HlvsNode>().ToList();
			if (_startNodes.Count == 0)
				return;

			_startNodes.Sort((node1, node2) => Mathf.CeilToInt(node1.position.y - node2.position.y));

			// reset computation order
			_graph.nodes.ForEach(node => node.computeOrder = Int32.MaxValue);

			// evaluate compute order for all nodes
			int executionOrder = -1;
			foreach (var startNode in _startNodes)
			{
				executionOrder++;
				startNode.computeOrder = executionOrder;
				Apply(startNode, node =>
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
					Debug.Assert(dependencyNode.computeOrder <= computeOrder);
					continue;
				}

				ComputeDependencyComputeOrder(dependencyNode, ref computeOrder);

				computeOrder++;
				dependencyNode.computeOrder = computeOrder;
			}
		}


		public void Run()
		{
			if (_startNodes == null)
				UpdateComputeOrder();

			status = ProcessingStatus.Finished;
			List<HlvsNode> nextStartNode = new List<HlvsNode>();
			foreach (var startNode in _startNodes)
			{
				var node = ProcessGraph(startNode);

				if (!(node is TExecutionStartNode))
				{
					status = ProcessingStatus.Unfinished;
				}

				nextStartNode.Add(node);
			}

			_startNodes = nextStartNode;
		}

		protected delegate void ApplyFunction(HlvsNode node);

		protected void Apply(HlvsNode beginNode, ApplyFunction func)
		{
			HlvsNode currNode = GetNextNode(beginNode);
			while (currNode != null)
			{
				func(currNode);

				currNode = GetNextNode(currNode);
			}
		}

		/// <summary>
		/// Applies a given function to all nodes, beginning AFTER startNode
		/// </summary>
		/// <param name="beginNode"></param>
		private HlvsNode ProcessGraph(HlvsNode beginNode)
		{
			HlvsNode currNode = beginNode;
			while (currNode != null)
			{
				GetNodeDataDependencies(currNode);

				currNode.OnProcess();
				var result = currNode.Evaluate();

				if (result == ProcessingStatus.Unfinished)
				{
					return currNode;
				}

				currNode = GetNextNode(currNode);
			}

			// we ran fully through the network so we will begin with the start node next time
			return beginNode is TExecutionStartNode ? beginNode : GetStarterNodeOf(beginNode);
		}

		private TExecutionStartNode GetStarterNodeOf(HlvsNode node)
		{
			while (!(node is TExecutionStartNode))
			{
				Debug.Assert(node.GetInputNodes().Count(baseNode => true) != 0);
				node = GetPreviousNode(node);
			}

			return (TExecutionStartNode)node;
		}

		private static void GetNodeDataDependencies(HlvsNode currNode)
		{
			// get own dependencies
			foreach (var node in currNode.GetInputNodes())
			{
				// ignore action flow as input
				if (node is HlvsActionNode || node is ExecutionStarterNode)
					continue;

				// fetch own data dependencies
				foreach (var nodeInputPort in node.GetInputNodes())
				{
					GetNodeDataDependencies(nodeInputPort as HlvsNode);
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
			return node.GetInputNodes().Last() as HlvsNode;
		}
	}
}