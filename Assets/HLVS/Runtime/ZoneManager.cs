using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HLVS.Runtime
{
	public enum ZoneNotificationType
	{
		Enter,
		Stay,
		Exit
	}

	public class ZoneManager
	{
		private static ZoneManager _instance;
		public static  ZoneManager Instance => _instance ??= new ZoneManager();

		public delegate void OnZoneEvent(GameObject other);

		private readonly Dictionary<string, OnZoneEvent> _onZoneEntered = new();
		private readonly Dictionary<string, OnZoneEvent> _onZoneStay    = new();
		private readonly Dictionary<string, OnZoneEvent> _onZoneExit    = new();

		private readonly Dictionary<string, float> _nameToLastActivationTime = new();

		public void RegisterOnEnter(string zone, OnZoneEvent e)
		{
			var zoneName = zone + "_entered";
			if (_onZoneEntered.ContainsKey(zoneName))
			{
				_onZoneEntered[zoneName] += e;
			}
			else
			{
				_onZoneEntered.Add(zoneName, e);
			}
		}

		public void RegisterOnStay(string zone, OnZoneEvent e)
		{
			var zoneName = zone + "_stay";
			if (_onZoneStay.ContainsKey(zoneName))
			{
				_onZoneStay[zoneName] += e;
			}
			else
			{
				_onZoneStay.Add(zoneName, e);
			}
		}

		public void RegisterOnExit(string zone, OnZoneEvent e)
		{
			var zoneName = zone + "_exit";
			if (_onZoneExit.ContainsKey(zoneName))
			{
				_onZoneExit[zoneName] += e;
			}
			else
			{
				_onZoneExit.Add(zoneName, e);
			}
		}

		public void OnEntered(string zone, GameObject other)
		{
			var zoneName = zone + "_entered";
			var collisionName = zoneName + other.name;
			if (!_onZoneEntered.ContainsKey(zoneName))
				return;

			if (_nameToLastActivationTime.TryGetValue(collisionName, out float lastTime))
			{
				if (lastTime == Time.fixedTime)
				{
					return;
				}

				_nameToLastActivationTime[collisionName] = Time.fixedTime;
			}
			else
			{
				_nameToLastActivationTime.Add(collisionName, Time.fixedTime);
			}

			_onZoneEntered[zoneName](other);
		}

		public void OnStay(string zone, GameObject other)
		{
			var zoneName = zone + "_stay";
			var collisionName = zoneName + other.name;
			if (!_onZoneStay.ContainsKey(zoneName))
				return;
			if (_nameToLastActivationTime.TryGetValue(collisionName, out float lastTime))
			{
				if (lastTime == Time.fixedTime)
				{
					return;
				}

				_nameToLastActivationTime[collisionName] = Time.fixedTime;
			}
			else
			{
				_nameToLastActivationTime.Add(collisionName, Time.fixedTime);
			}

			_onZoneStay[zoneName](other);
		}

		public void OnExit(string zone, GameObject other)
		{
			var zoneName = zone + "_exit";
			var collisionName = zoneName + other.name;
			if (!_onZoneExit.ContainsKey(zoneName))
				return;
			if (_nameToLastActivationTime.TryGetValue(collisionName, out float lastTime))
			{
				if (lastTime == Time.fixedTime)
				{
					return;
				}

				_nameToLastActivationTime[collisionName] = Time.fixedTime;
			}
			else
			{
				_nameToLastActivationTime.Add(collisionName, Time.fixedTime);
			}

			_onZoneExit[zoneName](other);
		}

		public void CleanupPreviousZoneCollisions()
		{
			float currTime = Time.fixedTime;
			float timestep = Time.fixedDeltaTime * 2;
			foreach ((string collisionName, float time) in _nameToLastActivationTime.ToList())
			{
				if (currTime - time > timestep)
				{
					_nameToLastActivationTime.Remove(collisionName);
				}
			}
		}
	}

	public class ZoneCleaner : MonoBehaviour
	{
		public static void Instantiate()
		{
			if(_instance)
				return;

			var gobj = new GameObject();
			gobj.name = ObjectNames.NicifyVariableName(nameof(ZoneCleaner));
			_instance = gobj.AddComponent<ZoneCleaner>();
			DontDestroyOnLoad(gobj);
		}

		private static ZoneCleaner _instance;
		private        float       _lastClean = 0;

		private const float CleanEvery = 4;
		
		private void FixedUpdate()
		{
			if (Time.time - _lastClean > CleanEvery)
			{
				_lastClean = Time.time;
				ZoneManager.Instance.CleanupPreviousZoneCollisions();
			}
		}
	}
}