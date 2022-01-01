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
		public string name = "";

		[SerializeReference]
		public List<ExposedParameter> parameters = new();
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

		internal List<HlvsEvent> GetAllEvents()
		{
			return _eventDefinitions.Select(pair => pair.Value).ToList();
		}

		internal void ClearAllEvents()
		{
			_eventDefinitions.Clear();
			_savedEvents.Clear();
			_savedNames.Clear();
			_listenerNodes.Clear();
			
			DoSave();
		}

		public void RegisterListener(INodeEventListener listener, string eventName)
		{
			if (_listenerNodes.TryGetValue(eventName, out List<INodeEventListener> eventListeners))
			{
				Debug.Assert(!eventListeners.Contains(listener), "Already registered this listener");
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
			if (_eventDefinitions.ContainsKey(e.name))
			{
				_eventDefinitions[e.name] = e;
			}
			else
			{
				_eventDefinitions.Add(e.name, e);
			}

			DoSave();
		}

		public void RemoveEventDefinition(string eventName)
		{
			_eventDefinitions.Remove(eventName);
			DoSave();
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

		private void DoSave()
		{
			SerializeData();

			Save(true);
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
			
			EditorUtility.SetDirty(this);
		}

		public void OnBeforeSerialize()
		{
			SerializeData();
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