using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLVS.Runtime
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("Physics/Zone")]
	[ExecuteInEditMode]
	public class Zone : MonoBehaviour
	{
		public string zoneName = "";

#if UNITY_EDITOR
		private void Awake()
		{
			if (!GetComponents<Collider>().Any(col => col.isTrigger))
			{
				Debug.LogError("Zone is attached to a gameobject without any trigger colliders");
			}
		}
		
		private void OnDestroy()
		{
			if (Application.isEditor && gameObject.scene.isLoaded) // only when the component is removed
			{
				ZoneNameProvider.instance.RemoveZone(zoneName);
				ZoneNameProvider.instance.RemoveZone(zoneName);
			}
		}
#endif

		private bool IsSameZone(GameObject other)
		{
			var otherZone = other.GetComponent<Zone>();
			if (otherZone && otherZone.zoneName == zoneName)
			{
				return true;
			}

			return false;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (IsSameZone(other.gameObject))
				return;

			ZoneManager.Instance.OnEntered(zoneName, other.gameObject);
		}

		private void OnTriggerStay(Collider other)
		{
			if (IsSameZone(other.gameObject))
				return;

			ZoneManager.Instance.OnStay(zoneName, other.gameObject);
		}

		private void OnTriggerExit(Collider other)
		{
			if (IsSameZone(other.gameObject))
				return;

			ZoneManager.Instance.OnExit(zoneName, other.gameObject);
		}
	}
}