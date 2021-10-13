using System.Collections.Generic;
using GraphProcessor;
using HLVS.Nodes;
using UnityEngine;

namespace HLVS
{
	public class HlvsGraph : BaseGraph
	{
		protected List<OnStartNode> startNodes;

		protected override void OnEnable()
		{
			base.OnEnable();

			InitStartNodesList();
			onGraphChanges += RegisterExecutionNode;
		}

		private void InitStartNodesList()
		{
			startNodes = new List<OnStartNode>();
			foreach (BaseNode baseNode in nodes)
			{
				if (baseNode is OnStartNode startNode)
				{
					startNodes.Add(startNode);
				}
			}
		}

		protected override void OnDisable()
		{
			onGraphChanges -= RegisterExecutionNode;
			base.OnDisable();
		}

		protected void RegisterExecutionNode(GraphChanges change)
		{
			if (change.addedNode == null)
				return;

			if (change.addedNode is OnStartNode startNode)
			{
				startNodes.Add(startNode);
			}
		}

		public void Run()
		{
			Debug.Log("Running...");
			HlvsGraphProcessor<OnStartNode> processor = new HlvsGraphProcessor<OnStartNode>(this);
			processor.UpdateComputeOrder();
			processor.Run();
			Debug.Log("Run finished");
		}
	}
}