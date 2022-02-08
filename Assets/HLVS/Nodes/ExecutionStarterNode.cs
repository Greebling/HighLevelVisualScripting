using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes
{
	public class ExecutionStarterNode : HlvsNode
	{
		[Output(" ", false)]
		public readonly ExecutionLink followingAction = null;

		public override HlvsNode GetPreviousNode()
		{
			return null;
		}

		public override string nextExecutionLink => nameof(followingAction);
		
		
		public override bool isRenamable => false;
	}

	[Serializable, NodeMenuItem("Start/Every Frame")]
	public class OnUpdateNode : ExecutionStarterNode
	{
		public override string name => "Every Frame";
	}

	[Serializable, NodeMenuItem("Start/On Start")]
	public class OnStartNode : ExecutionStarterNode
	{
		public override string name => "On Start";
	}

	[Serializable, NodeMenuItem("Start/On Trigger Enter")]
	public class OnTriggerEnteredNode : ExecutionStarterNode
	{
		[Output("Gameobject")]
		public GameObject enteredObject;

		public override string name => "On Trigger Entered";
	}

	[Serializable, NodeMenuItem("Start/On Trigger Exit")]
	public class OnTriggerExitNode : ExecutionStarterNode
	{
		[Output("Gameobject")]
		public GameObject exitingObject;

		public override string name => "On Trigger Exit";
	}


	[Serializable, NodeMenuItem("Start/On Collision")]
	public class OnCollisionNode : ExecutionStarterNode
	{
		public override string name => "On Collision";

		[Output("Gameobject")]
		public GameObject enteredObject;
	}

	[Serializable, NodeMenuItem("Start/On Event")]
	public class OnEventNode : ExecutionStarterNode
	{
		public override string name => "On Event";

		public string eventName = "";

		[SerializeReference]
		[Output("Event Data")]
		public List<ExposedParameter> eventData = new();

		public override void InitializePorts()
		{
			base.InitializePorts();
			if (EventManager.instance.HasEvent(eventName))
			{
				var def = EventManager.instance.GetEventDefinition(eventName);
				eventData = def.parameters.ToList();
			}
		}

		public override ProcessingStatus Evaluate()
		{
			return ProcessingStatus.Finished;
		}

		[CustomPortOutput(nameof(eventData), typeof(object))]
		void PushOutputs(List<SerializableEdge> connectedEdges)
		{
			foreach (SerializableEdge edge in connectedEdges)
			{
				var index = int.Parse(edge.outputPort.portData.identifier) - 1; // currently don't know a better solution
				edge.passThroughBuffer = eventData[index].value;
			}
		}

		[CustomPortBehavior(nameof(eventData))]
		IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges)
		{
			var portCount = eventData.Count;

			for (int i = 0; i < portCount; i++)
			{
				if(eventData[i] == null)
					 continue;
				
				yield return new PortData
				{
					displayName = eventData[i].name,
					displayType = eventData[i].GetValueType(),
					identifier = (i + 1).ToString(), // needed by PushOutputs!
				};
			}
		}
	}

	[Serializable, NodeMenuItem("Start/On Zone...")]
	public class OnZoneEventNode : ExecutionStarterNode
	{
		public override string name => "On Zone " + activationType;

		public string zoneName = "";

		public ZoneNotificationType activationType;

		[Output("Colliding Object")]
		public GameObject other;

		public override ProcessingStatus Evaluate()
		{
			return ProcessingStatus.Finished;
		}
	}
}