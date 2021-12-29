using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HLVS.Runtime
{
	[FilePath("Hlvs/ZoneNameProvider.man", FilePathAttribute.Location.ProjectFolder)]
	public class ZoneNameProvider : ScriptableSingleton<ZoneNameProvider>, ISerializationCallbackReceiver
	{
		private readonly Dictionary<string, uint> _zoneNames = new Dictionary<string, uint>();
		private readonly Dictionary<string, uint> _sceneBasedZones = new Dictionary<string, uint>();

		/// <summary>
		/// Used only for serialization of _allNames
		/// </summary>
		[SerializeField]
		private List<string> _serializedZoneNames = new List<string>();
		[SerializeField]
		private List<uint> _serializedZoneOccurences = new List<uint>();

		public void AddZone(string zoneName)
		{
			if(zoneName == string.Empty)
				return;
			
			if (!_zoneNames.ContainsKey(zoneName))
			{
				_zoneNames.Add(zoneName, 1);
			}
			else
			{
				_zoneNames[zoneName]++;
			}
			DoSave();
		}

		public bool HasZone(string zone)
		{
			return _zoneNames.ContainsKey(zone);
		}

		public void RemoveZone(string zoneName)
		{
			if (_zoneNames.ContainsKey(zoneName))
			{
				_zoneNames[zoneName]--;

				if (_zoneNames[zoneName] == 0)
				{
					_zoneNames.Remove(zoneName);
				}
				DoSave();
			}
		}

		public IEnumerable<string> GetAllZoneNames()
		{
			return _zoneNames.Select(pair => pair.Key);
		}

		public void OnBeforeSerialize()
		{
			SerializeData();
		}

		private void SerializeData()
		{
			_serializedZoneNames.Clear();
			_serializedZoneOccurences.Clear();
			foreach (var pair in _zoneNames)
			{
				_serializedZoneNames.Add(pair.Key);
				_serializedZoneOccurences.Add(pair.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			_zoneNames.Clear();
			for (int i = 0; i < _serializedZoneNames.Count; i++)
			{
				_zoneNames.Add(_serializedZoneNames[i], _serializedZoneOccurences[i]);
			}
		}

		private void DoSave()
		{
			SerializeData();

			Save(true);
		}
	}
}