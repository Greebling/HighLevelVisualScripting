using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text")]
		public string textToLog;

		[Input("Amount")]
		public int amount;

		public override string name => "Log in Console";
		
		[NonSerialized]
		private int _currAmount = -1;

		public override void Reset()
		{
			_currAmount = -1;
			status = ProcessingStatus.Unfinished;
		}

		public override void Evaluate()
		{
			_currAmount++;
			if (_currAmount >= amount)
			{
				_currAmount = -1;
				status = ProcessingStatus.Finished;
				return;
			}

			status = ProcessingStatus.Unfinished;

			if (!string.IsNullOrEmpty(textToLog))
				Debug.Log(textToLog);
		}
	}
}