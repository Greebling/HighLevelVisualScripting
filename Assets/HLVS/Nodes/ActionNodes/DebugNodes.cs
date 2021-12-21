using System;
using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Debug/Pause")]
	public class PauseGameNode : HlvsActionNode
	{
		public override string name => "Pause Game";

		public override ProcessingStatus Evaluate()
		{
#if UNITY_EDITOR
			EditorApplication.isPaused = true;
			Debug.LogWarning("Pausing Game"); // give users a message so they wont wonder why their editor suddenly stopped
#endif
			return ProcessingStatus.Finished;
		}
	}
	
	
	[Serializable, NodeMenuItem("Debug/Log to Console")]
	public class ConsoleLogNode : HlvsActionNode
	{
		public override string name => "Log to Console";

		[Input("Text")]
		public string text;

		[Input("Log Type")]
		public LogType logType = LogType.Default;

		public enum LogType
		{
			Default, Warning, Error,
		}

		public override ProcessingStatus Evaluate()
		{
			switch (logType)
			{
				case LogType.Default:
					Debug.Log(text);
					break;
				case LogType.Warning:
					Debug.LogWarning(text);
					break;
				case LogType.Error:
					Debug.LogError(text);
					break;
			}
			
			return ProcessingStatus.Finished;
		}
	}
}