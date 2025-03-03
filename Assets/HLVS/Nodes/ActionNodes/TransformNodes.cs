﻿using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Transform/Move Towards")]
	public class MoveTowardNode : HlvsActionNode
	{
		public override string name => "Move Towards";

		[Input("Object")]
		public GameObject target;

		[Input("Destination")]
		public GameObject destination;

		[Input("Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;

		private float _endTime = 0;

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
				target.transform.position = destination.transform.position;

				return ProcessingStatus.Finished;
			}
			else
			{
				Vector3 destinationPos = destination.transform.position;
				Vector3 currPos = target.transform.position;

				float distance = Vector3.Distance(destinationPos, currPos);
				float remainingTime = _endTime - Time.time;

				float neededSpeed = distance / remainingTime;
				Vector3 direction = (destinationPos - currPos).normalized;

				target.transform.position += direction * neededSpeed * Time.deltaTime;

				return ProcessingStatus.Unfinished;
			}
		}

		private bool IsFinished()
		{
			return Time.time >= _endTime;
		}

		private void Init()
		{
			_endTime = Time.time + duration;
		}
	}

	[Serializable, NodeMenuItem("Transform/Rotate")]
	public class RotateNode : HlvsActionNode
	{
		public override string name => "Rotate";

		[Input("Object")]
		public GameObject target;

		[Input("Angles")]
		public Vector3 angles;

		[Input("Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;

		private float   _endTime = 0;
		private Vector3 _startEuler;

		public enum Axis
		{
			X,
			Y,
			Z,
		}

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
				target.transform.rotation = Quaternion.Euler(angles);
				;

				return ProcessingStatus.Finished;
			}
			else
			{
				float remainingTimeNormalized = 1 - (_endTime - Time.time) / duration;

				Vector3 lerpResult = Vector3.Lerp(_startEuler, angles, remainingTimeNormalized);
				target.transform.rotation = Quaternion.Euler(lerpResult);

				return ProcessingStatus.Unfinished;
			}
		}

		private bool IsFinished()
		{
			return Time.time >= _endTime;
		}

		private void Init()
		{
			_endTime = Time.time + duration;

			_startEuler = target.transform.rotation.eulerAngles;
		}
	}

	[Serializable, NodeMenuItem("Transform/Scale")]
	public class ScaleNode : HlvsActionNode
	{
		public override string name => "Scale";

		[Input("Object")]
		public GameObject target;

		[Input("Scaling")]
		public float scaling = 1;

		[Input("Duration")]
		[LargerThan(0.0f)]
		public float duration = 1;

		private float   _endTime = 0;
		private Vector3 _startScale, _targetScale;

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
				target.transform.localScale = _targetScale;

				return ProcessingStatus.Finished;
			}
			else
			{
				float remainingTimeNormalized = 1 - (_endTime - Time.time) / duration;

				Vector3 lerpResult = Vector3.Lerp(_startScale, _targetScale, remainingTimeNormalized);
				target.transform.localScale = lerpResult;

				return ProcessingStatus.Unfinished;
			}
		}

		private bool IsFinished()
		{
			return Time.time >= _endTime;
		}

		private void Init()
		{
			_endTime = Time.time + duration;
			_startScale = target.transform.localScale;
			_targetScale = _startScale * scaling;
		}
	}

	[Serializable, NodeMenuItem("Transform/Move To Direction")]
	public class MoveDirectionNode : HlvsActionNode
	{
		public override string name => "Move To Direction";

		[Input("Object")]
		public GameObject target;

		[Input("Direction")]
		public Vector3 direction;

		[Input("Orientation")]
		public CoordSystem orientation;

		public enum CoordSystem
		{
			Global, Local,
		}
		
		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogError("No gameobject to move was given");
				return ProcessingStatus.Finished;
			}

			var rb = target.GetComponent<Rigidbody>();
			if (rb)
			{
				switch (orientation)
				{
					case CoordSystem.Global:
						rb.MovePosition(target.transform.position + Time.deltaTime * direction);
						break;
					case CoordSystem.Local:
						var movement = target.transform.rotation * direction;
						rb.MovePosition(target.transform.position + Time.deltaTime * movement);
						break;
				}
			} else
			{
				switch (orientation)
				{
					case CoordSystem.Global:
						target.transform.position += Time.deltaTime * direction;
						break;
					case CoordSystem.Local:
						var movement = target.transform.rotation * direction;
						target.transform.position += Time.deltaTime * movement;
						break;
				}
			}
			
			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Transform/Rotate To Direction")]
	public class RotateToDirectionNode : HlvsActionNode
	{
		public override string name => "Rotate To Direction";

		[Input("Object")]
		public GameObject target;

		[Input("Direction")]
		public Vector3 direction;

		[Input("Speed")] [LargerOrEqual(0)]
		public float smoothAmount = 120.0f;

		
		public override ProcessingStatus Evaluate()
		{
			if (!target)
			{
				Debug.LogError("No gameobject to move was given");
				return ProcessingStatus.Finished;
			}

			if(direction.sqrMagnitude != 0)
			{
				var rot = Quaternion.LookRotation(direction);
				rot = Quaternion.RotateTowards(target.transform.rotation, rot, smoothAmount * Time.deltaTime);
				target.transform.rotation = rot;
			}

			return ProcessingStatus.Finished;
		}
	}

	[Serializable, NodeMenuItem("Transform/Parent To")]
	public class ParentNode : HlvsActionNode
	{
		public override string name => "Parent To";

		[Input("Parent")]
		public GameObject parent;

		[Input("Child")]
		public GameObject child;

		public override ProcessingStatus Evaluate()
		{
			child.transform.SetParent(parent.transform, true);

			return ProcessingStatus.Finished;
		}
	}
}