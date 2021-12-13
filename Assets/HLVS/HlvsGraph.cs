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

		[NonSerialized] public GameObject activeGameObject;

		public Action                 onParameterListChanged  = () => { };
		public Action                 onBlackboardListChanged = () => { };
		public Action<HlvsBlackboard> onBlackboardAdded       = (HlvsBlackboard) => { };
		public Action<HlvsBlackboard> onBlackboardRemoved      = (HlvsBlackboard) => { };

		private readonly Dictionary<string, ExposedParameter> _nameToVar = new Dictionary<string, ExposedParameter>();
		private readonly Dictionary<string, ExposedParameter> _upperCaseNameToVar = new Dictionary<string, ExposedParameter>();
		private readonly Dictionary<string, ExposedParameter> _guidToVar = new Dictionary<string, ExposedParameter>();

		public readonly Dictionary<HlvsNode, int> nodeToIndex = new Dictionary<HlvsNode, int>();

		protected override void OnEnable()
		{
			blackboards = blackboards == null ? new List<HlvsBlackboard>() : blackboards.Where(blackboard => blackboard).ToList();

			BuildNodeDict();
			onGraphChanges += changes =>
			{
				if (changes.addedNode != null)
				{
					nodeToIndex.Add((HlvsNode)changes.addedNode, nodes.Count - 1);
				}
			};
			
			onGraphChanges += changes =>
			{
				if (changes.removedNode != null)
				{
					nodeToIndex.Clear();
					BuildNodeDict();
				}
			};
			
			BuildVariableDict();
			onParameterListChanged += BuildVariableDict;
			onBlackboardListChanged += BuildVariableDict;
			base.OnEnable();

			foreach (var baseNode in nodes)
			{
				var node = (HlvsNode)baseNode;
				node.Graph = this;
			}
		}

		public void Init()
		{
			foreach (BaseNode baseNode in nodes)
			{
				var node = (HlvsNode) baseNode;
				node.Reset();
				node.ParseExpressions();
			}

			UpdateComputeOrder();
		}

		public override void UpdateComputeOrder()
		{
			foreach (var baseNode in nodes)
			{
				baseNode.computeOrder = -1;
			}

			if (_startNodeProcessor == null)
			{
				_startNodeProcessor = new HlvsGraphProcessor<OnStartNode>(this);
				_updateNodeProcessor = new HlvsGraphProcessor<OnUpdateNode>(this);
				_triggerNodeProcessor = new HlvsGraphProcessor<OnTriggerEnteredNode>(this);
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

		public void AddBlackboard(HlvsBlackboard board)
		{
			if (blackboards.Contains(board))
			{
				Debug.LogError("This graph already contains this blackboard");
				return;
			}

			
			// check for duplicate variables
			{
				HashSet<string> currentVariableNames = new HashSet<string>();
				foreach (var param in blackboards.SelectMany(blackboard => blackboard.fields))
				{
					currentVariableNames.Add(param.name.ToUpperInvariant());
				}
				foreach (var param in parametersBlueprint)
				{
					currentVariableNames.Add(param.name.ToUpperInvariant());
				}

				bool canAddBoard = true;
				foreach (ExposedParameter exposedParameter in board.fields)
				{
					var currVar = exposedParameter.name.ToUpperInvariant();
					if (currentVariableNames.Contains(currVar))
					{
						Debug.LogError($"This graph already contains a parameter or a blackboard with a variable similar to '{exposedParameter.name}'");
						canAddBoard = false;
					}
				}

				if (!canAddBoard)
					return;
			}

			blackboards.Add(board);
			onBlackboardAdded(board);
		}

		public void RemoveBlackboard(HlvsBlackboard board)
		{
			Debug.Assert(blackboards.Contains(board));
			blackboards.Remove(board);
			onBlackboardRemoved(board);
		}

		public IEnumerable<ExposedParameter> GetParameters()
		{
			return parametersBlueprint.OrderBy(parameter => parameter.name);
		}

		public IEnumerable<ExposedParameter> GetBlackboardFields()
		{
			return blackboards.SelectMany(blackboard => blackboard.fields).OrderBy(parameter => parameter.name);
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

		private void BuildNodeDict()
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodeToIndex.Add((HlvsNode) nodes[i], i);
			}
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

		public void SetParameterValues(GameObject currentGameObject, List<ExposedParameter> parameters)
		{
			Debug.Assert(parameters.Count == parametersBlueprint.Count, "Parameter lists don't match");
			this.activeGameObject = currentGameObject;
			
			for (int i = 0; i < parametersBlueprint.Count; i++)
			{
				parametersBlueprint[i].value = parameters[i].value;
			}

			parametersBlueprint = parameters;
		}

		public Func<HlvsGraph, double> Get(string name)
		{
			return graph => Convert.ToDouble(GetFromInternalFunction(name) ?? graph.GetVariableByUppercaseName(name).value);
		}

		public object GetFromInternalFunction(string name)
		{
			switch (name)
			{
				default:
					return null;
				case "TIME":
					return Time.time;
				case "DELTA_TIME":
					return Time.deltaTime;
				case "THIS":
					return activeGameObject;
			}
		}
	}
}