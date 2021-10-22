using System.Collections.Generic;
using GraphProcessor;
using HLVS.Editor;
using HLVS.Nodes;
using UnityEngine;

namespace HLVS
{
	public class HlvsGraph : BaseGraph
	{
		protected List<OnStartNode> startNodes;
		
		protected 	HlvsGraphProcessor<OnStartNode> startNodeProcessor;
		protected 	HlvsGraphProcessor<OnUpdateNode> updateNodeProcessor;

		public List<BlackboardEntry> blackboardFields;
		protected override void OnEnable()
		{
			base.OnEnable();

			InitStartNodesList();
			onGraphChanges += RegisterExecutionNode;

			startNodeProcessor = new HlvsGraphProcessor<OnStartNode>(this);
			updateNodeProcessor = new HlvsGraphProcessor<OnUpdateNode>(this);
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

		public void RunStartNodes()
		{
			startNodeProcessor.UpdateComputeOrder();
			Debug.Log("Running...");
			startNodeProcessor.Run();
			Debug.Log("Run finished");
		}

		public void RunUpdateNodes()
		{
			updateNodeProcessor.UpdateComputeOrder();
			updateNodeProcessor.Run();
		}
	}
}