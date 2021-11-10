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
		public Action<HlvsBlackboard> onBlackboardRemoved      = (HlvsBlackboard) => { };

		private readonly Dictionary<string, ExposedParameter> _nameToVar = new Dictionary<string, ExposedParameter>();
		private readonly Dictionary<string, ExposedParameter> _guidToVar = new Dictionary<string, ExposedParameter>();

		protected override void OnEnable()
		{
			blackboards = blackboards == null ? new List<HlvsBlackboard>() : blackboards.Where(blackboard => blackboard).ToList();
			
			BuildVariableDict();
			onParameterListChanged += BuildVariableDict;
			onBlackboardListChanged += BuildVariableDict;
			base.OnEnable();
			

			startNodeProcessor = new HlvsGraphProcessor<OnStartNode>(this);
			updateNodeProcessor = new HlvsGraphProcessor<OnUpdateNode>(this);
			triggerNodeProcessor = new HlvsGraphProcessor<OnTriggerEnteredNode>(this);
		}

		public void AddBlackboard(HlvsBlackboard board)
		{
			if (blackboards.Contains(board))
				return;
			
			blackboards.Add(board);
			onBlackboardAdded(board);
		}

		public void RemoveBlackboard(HlvsBlackboard board)
		{
			Debug.Assert(blackboards.Contains(board));
			blackboards.Remove(board);
			onBlackboardRemoved(board);
		}

		public ExposedParameter GetVariableByName(string variableName)
		{
			return _nameToVar.ContainsKey(variableName) ? _nameToVar[variableName] : null;
		}

		public ExposedParameter GetVariableByGuid(string guid)
		{
			return _guidToVar.ContainsKey(guid) ? _guidToVar[guid] : null;
		}

		private void BuildVariableDict()
		{
			_nameToVar.Clear();
			blackboards.ForEach(blackboard => blackboard.fields.ForEach(parameter => _nameToVar.Add(parameter.name, parameter)));
			parametersBlueprint.ForEach(parameter => _nameToVar.Add(parameter.name, parameter));
			
			_guidToVar.Clear();
			blackboards.ForEach(blackboard => blackboard.fields.ForEach(parameter => _guidToVar.Add(parameter.guid, parameter)));
			parametersBlueprint.ForEach(parameter => _guidToVar.Add(parameter.guid, parameter));
		}

		public IEnumerable<ExposedParameter> GetParameters()
		{
			return parametersBlueprint.OrderBy(parameter => parameter.name);
		}

		public IEnumerable<ExposedParameter> GetBlackboardFields()
		{
			return blackboards.SelectMany(blackboard => blackboard.fields).OrderBy(parameter => parameter.name);
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

		public void Init()
		{
			foreach (BaseNode baseNode in nodes)
			{
				((HlvsNode) baseNode).Reset();
			}
			
			startNodeProcessor.UpdateComputeOrder();
			updateNodeProcessor.UpdateComputeOrder();
			triggerNodeProcessor.UpdateComputeOrder();
		}

		public void RunStartNodes()
		{
			startNodeProcessor.Run();
		}

		public void RunUpdateNodes()
		{
			if (startNodeProcessor.status == ProcessingStatus.Unfinished)
			{
				startNodeProcessor.Run();
			}
			
			updateNodeProcessor.Run();
		}

		public void RunOnTriggerEnteredNodes()
		{
			triggerNodeProcessor.Run();
		}
	}
}