using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Rendering/Change Light Color")]
	public class ChangeLightColorNode : HlvsActionNode
	{
		public override string name => "Change Light Color";

		[Input("Light")]
		public GameObject target;

		[Input("Color")]
		public Color lightColor = Color.white;


		[Input("Change Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;


		private float _endTime = 0;
		private Color _startColor;

		private Light _lightComponent;

		public override void Reset()
		{
			_endTime = 0;
		}

		public override ProcessingStatus Evaluate()
		{
			if (_endTime == 0)
			{
				Init();
			}

			if (IsFinished())
			{
				_lightComponent.color = lightColor;

				return ProcessingStatus.Finished;
			}
			else
			{
				float remainingTimeNormalized = 1 - (_endTime - Time.time) / duration;

				_lightComponent.color = Color.Lerp(_startColor, lightColor, remainingTimeNormalized);

				return ProcessingStatus.Unfinished;
			}
		}

		private bool IsFinished()
		{
			return Time.time >= _endTime;
		}

		private void Init()
		{
			_lightComponent = target.GetComponent<Light>();

			if (_lightComponent)
			{
				_endTime = Time.time + duration;
				_startColor = _lightComponent.color;
			}
			else
			{
				Debug.LogError("No light component was attached to the given gameobject");
			}
		}
	}

	[Serializable, NodeMenuItem("Rendering/Change Light Intensity")]
	public class ChangeLightIntensityNode : HlvsActionNode
	{
		public override string name => "Change Light Intensity";

		[Input("Light")]
		public GameObject target;

		[Input("Intensity")]
		[LargerThan(0.0f)]
		public float lightIntensity = 1;


		[Input("Change Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;


		private float _endTime = 0;
		private float _startIntensity;

		private Light _lightComponent;

		public override void Reset()
		{
			_endTime = 0;
		}

		public override ProcessingStatus Evaluate()
		{
			if (_endTime == 0)
			{
				Init();
			}

			if (IsFinished())
			{
				_lightComponent.intensity = lightIntensity;

				return ProcessingStatus.Finished;
			}
			else
			{
				float remainingTimeNormalized = 1 - (_endTime - Time.time) / duration;

				_lightComponent.intensity = Mathf.Lerp(_startIntensity, lightIntensity, remainingTimeNormalized);

				return ProcessingStatus.Unfinished;
			}
		}

		private bool IsFinished()
		{
			return Time.time >= _endTime;
		}

		private void Init()
		{
			_lightComponent = target.GetComponent<Light>();

			if (_lightComponent)
			{
				_endTime = Time.time + duration;
				_startIntensity = _lightComponent.intensity;
			}
			else
			{
				Debug.LogError("No light component was attached to the given gameobject");
			}
		}
	}
}