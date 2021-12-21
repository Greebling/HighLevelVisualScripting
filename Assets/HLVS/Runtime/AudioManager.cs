using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HLVS.Runtime
{
	[AddComponentMenu("")]
	public class AudioManager : MonoBehaviour
	{
		public static AudioManager Instance
		{
			get
			{
				if (!_instance)
				{
					GameObject singleton = new GameObject();
					_instance = singleton.AddComponent<AudioManager>();

					string name = ObjectNames.NicifyVariableName(nameof(AudioManager));
					singleton.name = name + " (Singleton)";
				}

				return _instance;
			}
		}

		private static AudioManager _instance;


		private GameObject sounds;

		private void Awake()
		{
			sounds = new GameObject("Sounds");
		}

		/// <summary>
		/// Gets a new (or free) audio source to play a sound from
		/// </summary>
		public static AudioSource GetSoundPlayer()
		{
			var instance = Instance;

			var foundSource = instance.sounds.GetComponentsInChildren<AudioSource>().FirstOrDefault(source => !source.isPlaying);
			if (!foundSource)
			{
				var newSource = new GameObject("Sound Source");
				foundSource = newSource.AddComponent<AudioSource>();
				newSource.transform.SetParent(instance.sounds.transform);
			}
			
			return foundSource;
		}
	}
}