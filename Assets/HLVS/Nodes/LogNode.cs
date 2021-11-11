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
		}

		public override ProcessingStatus Evaluate()
		{
			_currAmount++;
			if (_currAmount >= amount)
			{
				_currAmount = -1;
				return ProcessingStatus.Finished;
			}

			if (!string.IsNullOrEmpty(textToLog))
				Debug.Log(textToLog);
			
			return ProcessingStatus.Unfinished;
		}
	}
}