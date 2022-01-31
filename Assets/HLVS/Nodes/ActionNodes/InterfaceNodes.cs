using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UI;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Interface/Set Text")]
	public class SetTextNode : HlvsActionNode
	{
		public override string name => "Set Text";

		[Input("Textbox")]
		public GameObject textbox;

		[Input("Text")]
		public string textToSet = "";


		public override ProcessingStatus Evaluate()
		{
			if (!textbox)
			{
				Debug.LogWarning($"No gameobject called '{textbox}' found");
			}

			Text oldText = textbox.GetComponent<Text>();
			if (oldText)
			{
				oldText.text = textToSet;
			}
			else
			{
				Debug.LogWarning($"{textbox} has not Text component");
			}

			return ProcessingStatus.Finished;
		}
	}
}