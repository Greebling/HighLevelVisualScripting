using System;
using System.Globalization;
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

		public override void CheckFieldInputs()
		{
			base.CheckFieldInputs();
			if (amount <= 0)
			{
				amount = 1;
				Debug.LogError("Amount cant be lower than 1");
			}
		}
	}
}