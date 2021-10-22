using System;
using UnityEngine;

namespace HLVS
{
	[Serializable]
	public class BlackboardEntry : ISerializationCallbackReceiver
	{
		public object value
		{
			get => val;
			set
			{
				val = value;
				if (value != null)
				{
					type = value.GetType();
				}
			}
		}

		public string name = "";
		public string guid;
		[NonSerialized] public Type type;

		[SerializeField] private object val;

		/// <summary>
		/// Only used for serialization purposes, not frequently updated!
		/// </summary>
		[SerializeField] private string typeName;

		public void OnBeforeSerialize()
		{
			typeName = type.AssemblyQualifiedName;
		}

		public void OnAfterDeserialize()
		{
			type = Type.GetType(typeName);
		}
	}
}