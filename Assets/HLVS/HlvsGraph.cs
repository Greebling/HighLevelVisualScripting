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

		public List<HlvsBlackboard> blackboards;

		/// <summary>
		/// Blueprint of which types are needed as parameters
		/// </summary>
		[SerializeReference] public List<ExposedParameter> parametersBlueprint = new List<ExposedParameter>();

		public Action                 onParameterListChanged  = () => { };
		public Action                 onBlackboardListChanged = () => { };
		public Action<HlvsBlackboard> onBlackboardAdded       = (HlvsBlackboard) => { };

		private readonly Dictionary<string, ExposedParameter> _nameToVar = new Dictionary<string, ExposedParameter>();

		protected override void OnEnable()
		{
			base.OnEnable();
			
			if (blackboards == null)
			{
				blackboards = new List<HlvsBlackboard>();
			}
			else
			{
				// take care of deleted blackboards
				blackboards = blackboards.Where(blackboard => blackboard).ToList();
			}
			
			BuildVariableDict();
			onParameterListChanged += BuildVariableDict;
			onBlackboardListChanged += BuildVariableDict;

			startNodeProcessor = new HlvsGraphProcessor<OnStartNode>(this);
			updateNodeProcessor = new HlvsGraphProcessor<OnUpdateNode>(this);
			triggerNodeProcessor = new HlvsGraphProcessor<OnTriggerEnteredNode>(this);
		}

		public void AddBlackboard(HlvsBlackboard board)
		{
			blackboards.Add(board);
			onBlackboardAdded(board);
		}

		public ExposedParameter GetVariable(string variableName)
		{
			return _nameToVar.ContainsKey(variableName) ? _nameToVar[variableName] : null;
		}

		private void BuildVariableDict()
		{
			_nameToVar.Clear();
			blackboards.ForEach(blackboard => blackboard.fields.ForEach(parameter => _nameToVar.Add(parameter.name, parameter)));
			parametersBlueprint.ForEach(parameter => _nameToVar.Add(parameter.name, parameter));
		}

		public List<ExposedParameter> GetParameters()
		{
			return parametersBlueprint.OrderBy(parameter => parameter.name).ToList();
		}

		public List<ExposedParameter> GetBlackboardFields()
		{
			return blackboards.SelectMany(blackboard => blackboard.fields).OrderBy(parameter => parameter.name).ToList();
		}

		public void SetParameterValues(List<ExposedParameter> parameters)
		{
			Debug.Assert(parameters.Count == parametersBlueprint.Count, "Parameter lists don't match");
			foreach (var valueTuple in 
				parametersBlueprint.Zip(parameters, (graphParam, instanceParam) => (graphParam, instanceParam)))
			{
				valueTuple.graphParam.value = valueTuple.instanceParam.value;
			}
		}

		public void RunStartNodes()
		{
			startNodeProcessor.UpdateComputeOrder();
			startNodeProcessor.Run();
		}

		public void RunUpdateNodes()
		{
			updateNodeProcessor.UpdateComputeOrder();
			updateNodeProcessor.Run();
		}

		public void RunOnTriggerEnteredNodes()
		{
			triggerNodeProcessor.UpdateComputeOrder();
			triggerNodeProcessor.Run();
		}
	}
}