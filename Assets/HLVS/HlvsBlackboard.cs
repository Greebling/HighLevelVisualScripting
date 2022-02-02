using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS
{
	public class HlvsBlackboard : ScriptableObject
	{
		[SerializeReference] public List<ExposedParameter> fields = new List<ExposedParameter>();

		public bool saveToDisk = false;

		/// <summary>
		/// Used for runtime uses of the variables. Done to reset values after playing
		/// </summary>
		public HlvsBlackboard RuntimeInstance
		{
			get
			{
				if (saveToDisk)
					return this;
				
				if (_instance)
				{
					return _instance;
				}
				else
				{
					_instance = Instantiate(this);
					return _instance;
				}
			}
		}

		public bool HasRuntimeInstance => _instance;

		[NonSerialized]
		private HlvsBlackboard _instance;
	}
}