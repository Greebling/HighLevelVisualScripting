using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Time/Wait")]
	public class WaitNode : HlvsActionNode
	{
		public override string name => "Wait";

		[Input("Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;

		private float _currentTime = 0, _endTime = 0;

		public override void Reset()
		{
			_currentTime = 0;
			_endTime = 0;
		}

		public override ProcessingStatus Evaluate()
		{
			if (_endTime == 0)
			{
				Init();
			}

			_currentTime = Time.time;

			return IsFinished() ? ProcessingStatus.Finished : ProcessingStatus.Unfinished;
		}

		private bool IsFinished()
		{
			return _currentTime >= _endTime;
		}

		private void Init()
		{
			_currentTime = Time.time;
			_endTime = _currentTime + duration;
		}
	}

	[Serializable, NodeMenuItem("Time/Change Time Speed")]
	public class SetTimescaleNode : HlvsActionNode
	{
		public override string name => "Change Time Speed";

		[Input("Time Speed")]
		[LargerOrEqual(0.0f)] 
		public float timeScale = 1;

		public override ProcessingStatus Evaluate()
		{
			Time.timeScale = timeScale;
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Time/Slow motion")]
	public class DoSlowMotionNode : HlvsActionNode
	{
		public override string name => "Slow motion";

		[Input("Amount")]
		[LargerThan(0.0f)]
		public float timeScale = 1;

		[Input("Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;

		private float _previousTimeScale = 1;
		private float _currentTime       = 0, _endTime = 0;
		private float _setTimeScale      = 1;

		public override void Reset()
		{
			_previousTimeScale = 1;
			_currentTime = 0;
			_endTime = 0;
			_setTimeScale = 1;
		}

		public override ProcessingStatus Evaluate()
		{
			_currentTime = Time.unscaledTime;
			if (_endTime == 0)
			{
				Init();
			}
			
			if (IsFinished())
			{
				if (Time.timeScale == _setTimeScale)
				{
					Time.timeScale = _previousTimeScale;
				}
				else
				{
					Debug.LogWarning("Did not reset time scale back from slow motion as the time scale was changed by another node");
				}

				return ProcessingStatus.Finished;
			}
			else
			{
				return ProcessingStatus.Unfinished;
			}
		}

		private bool IsFinished()
		{
			return _currentTime >= _endTime;
		}

		private void Init()
		{
			_endTime = _currentTime + duration;
			_previousTimeScale = Time.timeScale;
			_setTimeScale = 1 / timeScale;
			Time.timeScale = _setTimeScale;
		}
	}
}