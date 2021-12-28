using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEditor;
using UnityEngine;

namespace HLVS
{
	public class HlvsGraphProcessor
	{
		List<ExecutionStarterNode> _startNodes;

		private Dictionary<Type, (HashSet<HlvsNode> currentPausedNodes, HashSet<HlvsNode> nextPausedNodes)> _pausedNodes =
			new Dictionary<Type, (HashSet<HlvsNode> currentPausedNodes, HashSet<HlvsNode> nextPausedNodes)>();


		private readonly HlvsGraph _graph;

		public ProcessingStatus status { get; private set; } = ProcessingStatus.Finished;

		public HlvsGraphProcessor(HlvsGraph graph)
		{
			_graph = graph;
		}

		public void RegisterType(Type starterType)
		{
			if (!_pausedNodes.ContainsKey(starterType))
			{
				_pausedNodes.Add(starterType, (new HashSet<HlvsNode>(), new HashSet<HlvsNode>()));
			}
		}
		
		public void Run(Type starterType)
		{
			if (_startNodes == null)
				UpdateComputeOrder();

			foreach (var startNode in _startNodes)
			{
				ProcessGraph(startNode, starterType);
			}

			RunPreviouslyPausedNodes(starterType);
		}
		
		public void RunFromNodes(IEnumerable<HlvsNode> nodes, Type starterType)
		{
			foreach (var startNode in nodes)
			{
				ProcessGraph(startNode, starterType);
			}
		}

		private void RunPreviouslyPausedNodes(Type starterType)
		{
			var (currentPausedNodes, nextPausedNodes) = _pausedNodes[starterType];
			var pausedNodes = currentPausedNodes.ToArray();
			currentPausedNodes.Clear();
			foreach (HlvsNode node in pausedNodes)
			{
				ProcessGraph(node, starterType);
			}

			(currentPausedNodes, nextPausedNodes) = (nextPausedNodes, currentPausedNodes);

			status = currentPausedNodes.Count == 0 ? ProcessingStatus.Finished : ProcessingStatus.Unfinished;
		}

		public void RunAllPausedNodes()
		{
			foreach (KeyValuePair<Type,(HashSet<HlvsNode> currentPausedNodes, HashSet<HlvsNode> nextPausedNodes)> keyValuePair in _pausedNodes)
			{
				var (currentPausedNodes, nextPausedNodes) = keyValuePair.Value;
				var pausedNodes = currentPausedNodes.ToArray();
				currentPausedNodes.Clear();
				foreach (HlvsNode node in pausedNodes)
				{
					ProcessGraph(node, keyValuePair.Key);
				}

				(currentPausedNodes, nextPausedNodes) = (nextPausedNodes, currentPausedNodes);

				status = currentPausedNodes.Count == 0 ? ProcessingStatus.Finished : ProcessingStatus.Unfinished;
			}
		}

		public void UpdateComputeOrder()
		{
			foreach (KeyValuePair<Type,(HashSet<HlvsNode> currentPausedNodes, HashSet<HlvsNode> nextPausedNodes)> pair in _pausedNodes)
			{
				pair.Value.currentPausedNodes.Clear();
			}

			_startNodes = _graph.nodes.Where(node => node is ExecutionStarterNode).Cast<ExecutionStarterNode>().ToList();
			if (_startNodes.Count == 0)
				return;

			_startNodes.Sort((node1, node2) => Mathf.CeilToInt((node1.position.y - node2.position.y) * 1024));
			List<HlvsNode> computedNodes = new List<HlvsNode>();

			// First iteration working only on nodes with ExecutionLinks
			{
				HashSet<HlvsNode> alreadyComputedNodes = new HashSet<HlvsNode>();
				int executionOrder = -1;

				foreach (var startNode in _startNodes)
				{
					List<HlvsNode> currNodes = new List<HlvsNode> { startNode };
					List<HlvsNode> nextNodes = new List<HlvsNode>();
					while (currNodes.Count != 0)
					{
						// stops executing infinite loops
						if (executionOrder > 5000)
						{
							var assetPath = AssetDatabase.GetAssetPath(_graph);
							Debug.LogError("Infinite loop in nodes of graph " + assetPath.Split('/').Last() + " detected", _graph);
							break;
						}

						nextNodes.Clear();

						executionOrder++;
						foreach (var node in currNodes)
						{
							if (node.computeOrder < executionOrder)
							{
								node.computeOrder = executionOrder;

								var currNextNodes = GetPossibleNextNodes(node);
								nextNodes.AddRange(currNextNodes);

								if (!alreadyComputedNodes.Contains(node))
								{
									alreadyComputedNodes.Add(node);
									computedNodes.Add(node);
								}
							}
						}

						// swap current with next
						(currNodes, nextNodes) = (nextNodes, currNodes);
					}
				}
			}

			// second iteration taking care of data dependencies
			if (computedNodes.Count != 0)
			{
				int computeOrderOffset = 0;
				var finalNodes = computedNodes.OrderBy(node => node.computeOrder).ToList();

				for (var lowIndex = 0; lowIndex < finalNodes.Count; lowIndex++)
				{
					int currComputeOrder = finalNodes[lowIndex].computeOrder;

					// find out all of the nodes processed at the same time
					int highIndex = finalNodes.Count;
					for (int i = lowIndex; i < finalNodes.Count; i++)
					{
						if (finalNodes[i].computeOrder != currComputeOrder)
						{
							highIndex = i;
							break;
						}
					}

					// set nodes compute order and get longest dependency chain length
					currComputeOrder += computeOrderOffset;
					int longestDependency = 0;
					for (int i = lowIndex; i < highIndex; i++)
					{
						var node = finalNodes[i];

						if (!node.GetDataInputNodes().Any())
							continue;

						int calculatedDependency = LongestDependencyChainLength(node);
						longestDependency = Math.Max(longestDependency, calculatedDependency);
					}

					currComputeOrder += longestDependency;
					for (int i = lowIndex; i < highIndex; i++)
					{
						var node = finalNodes[i];
						node.computeOrder = currComputeOrder;
					}

					for (int i = lowIndex; i < highIndex; i++)
					{
						var node = finalNodes[i];
						ComputeDependencyOrder(node);
					}

					computeOrderOffset += longestDependency;
					lowIndex = highIndex - 1; // advance to the next frontier
				}
			}
		}

		private static int LongestDependencyChainLength(HlvsNode start)
		{
			List<BaseNode> frontier = new List<BaseNode>();
			List<BaseNode> nextFrontier = new List<BaseNode>();

			frontier.AddRange(start.GetDataInputNodes().OfType<HlvsDataNode>());
			int dependencyChainLength = 0;

			while (frontier.Count != 0)
			{
				dependencyChainLength++;
				nextFrontier.Clear();

				foreach (var node in frontier)
				{
					nextFrontier.AddRange(node.GetInputNodes().OfType<HlvsDataNode>());
				}

				(frontier, nextFrontier) = (nextFrontier, frontier);
			}

			return dependencyChainLength;
		}

		private static void ComputeDependencyOrder(HlvsNode start)
		{
			List<BaseNode> frontier = new List<BaseNode>();
			List<BaseNode> nextFrontier = new List<BaseNode>();

			frontier.AddRange(start.GetDataInputNodes().OfType<HlvsDataNode>());
			int computeOrder = start.computeOrder;

			while (frontier.Count != 0)
			{
				computeOrder--;
				nextFrontier.Clear();

				foreach (var node in frontier)
				{
					node.computeOrder = computeOrder;

					nextFrontier.AddRange(node.GetInputNodes().OfType<HlvsDataNode>());
				}

				(frontier, nextFrontier) = (nextFrontier, frontier);
			}
		}

		/// <summary>
		/// Applies a given function to all nodes, beginning AFTER startNode
		/// </summary>
		/// <param name="beginNode"></param>
		/// <param name="starterType"></param>
		private void ProcessGraph(HlvsNode beginNode, Type starterType)
		{
			Queue<HlvsNode> currNodes = new Queue<HlvsNode>();
			currNodes.Enqueue(beginNode);

			var (currentPausedNodes, nextPausedNodes) = _pausedNodes.ContainsKey(starterType)? _pausedNodes[starterType] : (new HashSet<HlvsNode>(), new HashSet<HlvsNode>());

			while (currNodes.Count > 0)
			{
				var currNode = currNodes.Dequeue();
				if (currentPausedNodes.Contains(currNode))
				{
					currNode.Reset();
					currentPausedNodes.Remove(currNode);
				}


				var result = EvaluateNodeAndDependencies(currNode);

				if (result == ProcessingStatus.Unfinished)
				{
					nextPausedNodes.Add(currNode);
				}
				else
				{
					if (currNode is HlvsFlowNode flowNode)
					{
						var nextNodes = GetNextNodes(flowNode);
						if (nextNodes != null)
						{
							foreach (HlvsNode nextNode in nextNodes)
							{
								currNodes.Enqueue(nextNode);
							}
						}

						continue;
					}

					HlvsNode next = GetNextNode(currNode);
					if (next != null)
					{
						currNodes.Enqueue(next);
					}
				}
			}
		}

		private static ProcessingStatus EvaluateNodeAndDependencies(HlvsNode currNode)
		{
			// get own dependencies
			foreach (var node in currNode.GetInputNodes())
			{
				// ignore action flow as input
				if (node is HlvsActionNode || node is ExecutionStarterNode)
					continue;

				EvaluateNodeAndDependencies((HlvsNode)node);
			}

			var res = currNode.DoEvaluation();

			return res;
		}

		private static HlvsNode[] GetNextNodes(HlvsFlowNode actionNode)
		{
			var nextLinks = actionNode.GetNextExecutionLinks();
			var nextPort = actionNode.outputPorts
			                         .Where(port => port.fieldInfo.FieldType == typeof(ExecutionLink))
			                         .Where(port => nextLinks.Contains(port.fieldName));
			return nextPort.Select(port => port.GetEdges().FirstOrDefault()).Where(edge => edge != null).Select(edge => (HlvsNode)edge.inputNode)
			               .ToArray();
		}

		private static HlvsNode GetNextNode(HlvsNode node)
		{
			var nextLink = node.nextExecutionLink;
			var nextPort = node.outputPorts
			                   .Where(port => port.fieldInfo.FieldType == typeof(ExecutionLink))
			                   .FirstOrDefault(port => port.fieldName == nextLink);

			return (HlvsNode)nextPort?.GetEdges().FirstOrDefault()?.inputNode;
		}


		public static IEnumerable<HlvsNode> GetPossibleNextNodes(HlvsNode node)
		{
			return node.outputPorts
			           .Where(port => port.fieldInfo.FieldType == typeof(ExecutionLink))
			           .Select(port => port.GetEdges().FirstOrDefault()).Where(edge => edge != null)
			           .Select(edge => edge.inputNode as HlvsNode);
		}
	}
}