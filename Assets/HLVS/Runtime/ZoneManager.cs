using System.Collections.Generic;
using UnityEngine;

namespace HLVS.Runtime
{
	public enum ZoneNotificationType
	{
		Enter, Stay, Exit
	}

	public class ZoneManager
	{
		private static ZoneManager _instance;
		public static  ZoneManager Instance => _instance ??= new ZoneManager();

		public delegate void OnZoneEvent(GameObject other);

		private readonly Dictionary<string, OnZoneEvent> _onZoneEntered = new Dictionary<string, OnZoneEvent>();
		private readonly Dictionary<string, OnZoneEvent> _onZoneStay    = new Dictionary<string, OnZoneEvent>();
		private readonly Dictionary<string, OnZoneEvent> _onZoneExit    = new Dictionary<string, OnZoneEvent>();

		private readonly Dictionary<string, float> _lastActivationTime = new Dictionary<string, float>();

		public void RegisterOnEnter(string zone, OnZoneEvent e)
		{
			var zoneName = zone + "_entered";
			if (_onZoneEntered.TryGetValue(zoneName, out OnZoneEvent events))
			{
				events += e;
			}
			else
			{
				_onZoneEntered.Add(zoneName, e);
			}
		}

		public void RegisterOnStay(string zone, OnZoneEvent e)
		{
			var zoneName = zone + "_stay";
			if (_onZoneStay.TryGetValue(zoneName, out OnZoneEvent events))
			{
				events += e;
			}
			else
			{
				_onZoneStay.Add(zoneName, e);
			}
		}

		public void RegisterOnExit(string zone, OnZoneEvent e)
		{
			var zoneName = zone + "_exit";
			if (_onZoneExit.TryGetValue(zoneName, out OnZoneEvent events))
			{
				events += e;
			}
			else
			{
				_onZoneExit.Add(zoneName, e);
			}
		}

		public void OnEntered(string zone, GameObject other)
		{
			var zoneName = zone + "_entered";
			if(!_onZoneEntered.ContainsKey(zoneName))
				return;
			
			if (_lastActivationTime.TryGetValue(zoneName, out float lastTime))
			{
				if (lastTime == Time.fixedTime)
				{
					return;
				}

				_lastActivationTime[zoneName] = Time.fixedTime;
			}
			else
			{
				_lastActivationTime.Add(zoneName, Time.fixedTime);
			}

			_onZoneEntered[zoneName](other);
		}

		public void OnStay(string zone, GameObject other)
		{
			var zoneName = zone + "_stay";
			if(!_onZoneEntered.ContainsKey(zoneName))
				return;
			if (_lastActivationTime.TryGetValue(zoneName, out float lastTime))
			{
				if (lastTime == Time.fixedTime)
				{
					return;
				}

				_lastActivationTime[zoneName] = Time.fixedTime;
			}
			else
			{
				_lastActivationTime.Add(zoneName, Time.fixedTime);
			}

			_onZoneStay[zone](other);
		}

		public void OnExit(string zone, GameObject other)
		{
			var zoneName = zone + "_exit";
			if(!_onZoneEntered.ContainsKey(zoneName))
				return;
			if (_lastActivationTime.TryGetValue(zoneName, out float lastTime))
			{
				if (lastTime == Time.fixedTime)
				{
					return;
				}

				_lastActivationTime[zoneName] = Time.fixedTime;
			}
			else
			{
				_lastActivationTime.Add(zoneName, Time.fixedTime);
			}

			_onZoneExit[zone](other);
		}
	}
}