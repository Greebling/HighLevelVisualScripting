using System;
using System.Collections.Generic;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Start/Raise Event")]
	public class RaiseEventNode : HlvsActionNode
	{
		public override string name => "Raise Event";

		public string eventName = "";
		
		[SerializeReference]
		[Input("Data")]
		public List<ExposedParameter> eventData;
		
		public override ProcessingStatus Evaluate()
		{
			if(eventName == string.Empty || !EventManager.instance.HasEvent(eventName))
				return ProcessingStatus.Finished;

			HlvsEvent raisedEvent = new()
			{
				name = eventName,
				parameters = eventData
			};
			EventManager.instance.RaiseEvent(raisedEvent);
			
			return ProcessingStatus.Finished;
		}
		
		[CustomPortInput(nameof(eventData), typeof(object))]
		void PullInputs(List<SerializableEdge> connectedEdges)
		{
			if(eventName == string.Empty || !EventManager.instance.HasEvent(eventName))
				return;
			
			eventData = EventManager.instance.GetEventDefinition(eventName).parameters;
			
			foreach (SerializableEdge edge in connectedEdges)
			{
				var index = int.Parse(edge.inputPort.portData.identifier) - 1; // currently don't know a better solution
				eventData[index].value = edge.passThroughBuffer;
			}
		}

		[CustomPortBehavior(nameof(eventData))]
		IEnumerable<PortData> ListPortBehavior(List<SerializableEdge> edges)
		{
			if(!EventManager.instance.HasEvent(eventName))
			{
				yield return null;
			} else
			{
				HlvsEvent ev = EventManager.instance.GetEventDefinition(eventName);
				for (int index = 0; index < ev.parameters.Count; index++)
				{
					ExposedParameter exposedParameter = ev.parameters[index];
					yield return new PortData
					{
						displayName = exposedParameter.name,
						displayType = exposedParameter.GetValueType(),
						identifier = (index + 1).ToString(),
					};
				}
			}
		}
	}
}