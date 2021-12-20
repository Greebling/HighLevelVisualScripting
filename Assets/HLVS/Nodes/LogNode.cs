using System;
using GraphProcessor;
using HLVS.Runtime;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text")]
		public string textToLog = "";

		[Input("Amount Printed")] [LargerThan(0)]
		public int amount = 1;

		public override string name => "Log in Console";
		
		[NonSerialized]
		private int _amountPrinted = 0;

		public override void Reset()
		{
			_amountPrinted = 0;
		}

		public override ProcessingStatus Evaluate()
		{
			if (!string.IsNullOrEmpty(textToLog))
				Debug.Log(textToLog);
			
			_amountPrinted++;
			if (_amountPrinted >= amount)
			{
				Reset();
				return ProcessingStatus.Finished;
			} else
			{
				return ProcessingStatus.Unfinished;
			}
		}
	}
}