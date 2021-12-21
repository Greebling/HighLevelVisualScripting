using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;
using UnityEngine.Audio;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Audio/Play Sound")]
	public class PlaySoundNode : HlvsActionNode
	{
		public override string name => "Play Sound";

		[Input("Sound")]
		public AudioClip sound;

		[Input("Mixer")]
		public AudioMixerGroup mixGroup;
		
		[Input("Volume")] [LargerOrEqual(0.0f)] [SmallerOrEqual(1.0f)]
		public float volume = 1;
		
		[Input("Pitch")] [LargerOrEqual(0.0f)]
		public float pitch = 1;
		

		[Input("On Target")]
		public GameObject soundOwner;

		public override ProcessingStatus Evaluate()
		{
			var source = AudioManager.GetSoundPlayer();

			source.clip = sound;
			source.volume = volume;
			source.pitch = pitch;
			source.outputAudioMixerGroup = mixGroup;
			source.Play();

			if (soundOwner)
				source.transform.SetParent(soundOwner.transform);
			return ProcessingStatus.Finished;
		}
	}
}