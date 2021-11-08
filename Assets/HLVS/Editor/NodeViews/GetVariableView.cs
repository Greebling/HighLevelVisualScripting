using System;
using GraphProcessor;
using HLVS.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(GetVariableNode))] [DefaultExecutionOrder(512)]
	public class GetVariableView : HlvsNodeView
	{
		private GetVariableNode target => nodeTarget as GetVariableNode;

		private TextField _nameField;
		private Label _titleLabel;

		private ExposedParameter _parameter;

		public override void Enable()
		{
			_titleLabel = titleContainer.Q<Label>("title-label");

			_nameField = new TextField("Name");
			_nameField.value = target.variableName;
			_nameField.Q<Label>().style.minWidth = 10;
			_nameField.Q("unity-text-input").style.minWidth = 75;
			_nameField.RegisterCallback<FocusOutEvent>(e => OnNameChanged());
			inputContainer.Add(_nameField);
			GetVariableData();
		}

		private void UpdateVisuals()
		{
			if (_parameter == null)
			{
				Debug.LogError($"No variable called '{target.variableName}' found");
			}
			else
			{
				(outputPortViews[0] as HlvsPortView).SetPortType(_parameter.GetValueType());
				RefreshPorts();
			}
		}

		private void OnNameChanged()
		{
			Undo.RecordObject(owner.graph, "Set GetVariable node");
			
			target.variableName = _nameField.value;
			
			GetVariableData();
			
			EditorUtility.SetDirty(owner.graph);
		}

		private void GetVariableData()
		{
			if (target.variableName != String.Empty)
			{
				_titleLabel.text = target.name;
				_parameter = graph.GetVariable(target.variableName);
				UpdateVisuals();
			}
		}
	}
}