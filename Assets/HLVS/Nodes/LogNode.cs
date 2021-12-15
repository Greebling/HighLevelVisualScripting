using System;
using System.Collections.Generic;
using System.Globalization;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes
{
	[Serializable, NodeMenuItem("HLVS/Log")]
	public class LogNode : HlvsActionNode
	{
		[Input("Text")]
		public string textToLog = "";

		[Input("Amount")]
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

		public override List<(string fieldName, string errorMessage)> CheckFieldInputs()
		{
			List<(string fieldName, string errorMessage)> errors = new List<(string fieldName, string errorMessage)>();
			if (amount <= 0)
			{
				amount = 1;
				errors.Add((nameof(amount), "Cannot be lower than 1"));
			}

			return errors;
		}
	}
}