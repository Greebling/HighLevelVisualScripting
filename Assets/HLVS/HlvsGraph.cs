using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using UnityEngine;

namespace HLVS
{
	public class HlvsGraph : BaseGraph
	{
		protected HlvsGraphProcessor<OnStartNode> startNodeProcessor;
		protected HlvsGraphProcessor<OnUpdateNode> updateNodeProcessor;
		protected HlvsGraphProcessor<OnTriggerEnteredNode> triggerNodeProcessor;

		/// <summary>
		/// Blackboard variables of a graph. They are shared amongst all instances of a graph asset
		/// </summary>
		[SerializeReference] public List<ExposedParameter> blackboardFields = new List<ExposedParameter>();

		/// <summary>
		/// Blueprint of which types are needed as parameters
		/// </summary>
		[SerializeReference] public List<ExposedParameter> parametersBlueprint = new List<ExposedParameter>();
		
		public List<ExposedParameter> parametersValues { get; protected set; }

		public Action onParameterListChanged = () => { };

		protected override void OnEnable()
		{
			base.OnEnable();

			startNodeProcessor = new HlvsGraphProcessor<OnStartNode>(this);
			updateNodeProcessor = new HlvsGraphProcessor<OnUpdateNode>(this);
			triggerNodeProcessor = new HlvsGraphProcessor<OnTriggerEnteredNode>(this);
		}

		public List<ExposedParameter> GetParameters()
		{
			return parametersBlueprint.OrderBy(parameter => parameter.name).ToList();
		}

		public List<ExposedParameter> GetBlackboardFields()
		{
			return blackboardFields.OrderBy(parameter => parameter.name).ToList();
		}

		public void RunStartNodes(List<ExposedParameter> parameters)
		{
			parametersValues = parameters;
			startNodeProcessor.UpdateComputeOrder();
			startNodeProcessor.Run();
		}

		public void RunUpdateNodes(List<ExposedParameter> parameters)
		{
			parametersValues = parameters;
			updateNodeProcessor.UpdateComputeOrder();
			updateNodeProcessor.Run();
		}

		public void RunOnTriggerEnteredNodes(List<ExposedParameter> parameters)
		{
			parametersValues = parameters;
			triggerNodeProcessor.UpdateComputeOrder();
			triggerNodeProcessor.Run();
		}
	}
}