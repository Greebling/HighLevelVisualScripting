using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace HLVS.Runtime
{
	[Serializable]
	public class HlvsEvent
	{
		public string name;

		[SerializeReference]
		public List<ExposedParameter> parameters;
	}

	public interface INodeEventListener
	{
		public void OnEvent(HlvsEvent e);
	}

	[FilePath("Hlvs/EventManager.man", FilePathAttribute.Location.ProjectFolder)]
	[DefaultExecutionOrder(-128)]
	public class EventManager : ScriptableSingleton<EventManager>, ISerializationCallbackReceiver
	{
		private Dictionary<string, List<INodeEventListener>> _listenerNodes    = new Dictionary<string, List<INodeEventListener>>();
		private Dictionary<string, HlvsEvent>                _eventDefinitions = new Dictionary<string, HlvsEvent>();


		public IEnumerable<string> GetEventNames()
		{
			return _eventDefinitions.Select(pair => pair.Key);
		}

		public void RegisterListener(INodeEventListener listener, string eventName)
		{
			if (_listenerNodes.TryGetValue(eventName, out List<INodeEventListener> eventListeners))
			{
				eventListeners.Add(listener);
			}
			else
			{
				_listenerNodes.Add(eventName, new List<INodeEventListener> { listener });
			}
		}

		public void RemoveListener(INodeEventListener listener, string eventName)
		{
			if (_listenerNodes.TryGetValue(eventName, out List<INodeEventListener> eventListeners))
			{
				eventListeners.Remove(listener);
			}
		}

		public void RaiseEvent(HlvsEvent e)
		{
			if(!_listenerNodes.ContainsKey(e.name))
				return;
			
			foreach (INodeEventListener nodeEventListener in _listenerNodes[e.name])
			{
				nodeEventListener.OnEvent(e);
			}
		}

		public void AddEventDefinition(HlvsEvent e)
		{
			if (_eventDefinitions.TryGetValue(e.name, out HlvsEvent definition))
			{
				definition = e;
			}
			else
			{
				_eventDefinitions.Add(e.name, e);
			}

			DoSave();
		}

		public void RemoveEventDefinition(HlvsEvent e)
		{
			_eventDefinitions.Remove(e.name);
		}

		public bool HasEvent(string name)
		{
			return _eventDefinitions.ContainsKey(name);
		}

		public HlvsEvent GetEventDefinition(string name)
		{
			return _eventDefinitions[name];
		}

		[SerializeField]
		private List<HlvsEvent> _savedEvents = new List<HlvsEvent>();
		
		[SerializeField]
		private List<string> _savedNames = new List<string>();

		public void OnBeforeSerialize()
		{
			SerializeData();
		}

		private void SerializeData()
		{
			_savedEvents.Clear();
			_savedNames.Clear();
			foreach ((string key, HlvsEvent value) in _eventDefinitions)
			{
				_savedEvents.Add(value);
				_savedNames.Add(key);
			}
		}

		private void DoSave()
		{
			SerializeData();

			Save(true);
		}

		public void OnAfterDeserialize()
		{
			_eventDefinitions.Clear();

			for (int i = 0; i < _savedNames.Count; i++)
			{
				_eventDefinitions.Add(_savedNames[i], _savedEvents[i]);
				
			}
		}
	}
}