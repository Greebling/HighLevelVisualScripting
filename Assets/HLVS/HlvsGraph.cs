using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using IkTools.FormulaParser;
using UnityEngine;

namespace HLVS
{
	public class HlvsGraph : BaseGraph, IVariableProvider<HlvsGraph>
	{
		private HlvsGraphProcessor<OnStartNode> _startNodeProcessor;
		private HlvsGraphProcessor<OnUpdateNode> _updateNodeProcessor;
		private HlvsGraphProcessor<OnTriggerEnteredNode> _triggerNodeProcessor;

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
		private readonly Dictionary<string, ExposedParameter> _upperCaseNameToVar = new Dictionary<string, ExposedParameter>();
		private readonly Dictionary<string, ExposedParameter> _guidToVar = new Dictionary<string, ExposedParameter>();

		protected override void OnEnable()
		{
			blackboards = blackboards == null ? new List<HlvsBlackboard>() : blackboards.Where(blackboard => blackboard).ToList();
			
			BuildVariableDict();
			onParameterListChanged += BuildVariableDict;
			onBlackboardListChanged += BuildVariableDict;
			base.OnEnable();
			

			_startNodeProcessor = new HlvsGraphProcessor<OnStartNode>(this);
			_updateNodeProcessor = new HlvsGraphProcessor<OnUpdateNode>(this);
			_triggerNodeProcessor = new HlvsGraphProcessor<OnTriggerEnteredNode>(this);
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

		public ExposedParameter GetVariableByUppercaseName(string variableName)
		{
			return _upperCaseNameToVar.ContainsKey(variableName) ? _upperCaseNameToVar[variableName] : null;
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
			
			_upperCaseNameToVar.Clear();
			blackboards.ForEach(blackboard => blackboard.fields.ForEach(parameter => _upperCaseNameToVar.Add(parameter.name.ToUpperInvariant().Replace(' ', '_'), parameter)));
			parametersBlueprint.ForEach(parameter => _upperCaseNameToVar.Add(parameter.name.ToUpperInvariant().Replace(' ', '_'), parameter));
			
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
			
			_startNodeProcessor.UpdateComputeOrder();
			_updateNodeProcessor.UpdateComputeOrder();
			_triggerNodeProcessor.UpdateComputeOrder();
		}

		public void RunStartNodes()
		{
			_startNodeProcessor.Run();
		}

		public void RunUpdateNodes()
		{
			if (_startNodeProcessor.status == ProcessingStatus.Unfinished)
			{
				_startNodeProcessor.Run();
			}
			
			_updateNodeProcessor.Run();
		}

		public void RunOnTriggerEnteredNodes()
		{
			_triggerNodeProcessor.Run();
		}

		public Func<HlvsGraph, double> Get(string name)
		{
			return graph => Convert.ToDouble(graph.GetVariableByUppercaseName(name).value);
		}
	}
}