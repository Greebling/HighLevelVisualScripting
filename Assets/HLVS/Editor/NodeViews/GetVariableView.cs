using System;
using GraphProcessor;
using HLVS.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(GetVariableNode))]
	public class GetVariableView : HlvsNodeView
	{
		private GetVariableNode target => nodeTarget as GetVariableNode;

		private TextField _nameField;
		private bool _isRegisteredKeyDown = false;
		private Label _titleLabel;

		private ExposedParameter _parameter;

		public override void Enable()
		{
			_titleLabel = titleContainer.Q<Label>("title-label");

			_nameField = new TextField("Name");
			_nameField.RegisterValueChangedCallback(evt =>
			{
				target.variableName = evt.newValue;
				_titleLabel.text = target.name;
				
				
				if (!_isRegisteredKeyDown)
				{
					_nameField.RegisterCallback<KeyDownEvent>(OnKeyPressed);
					_isRegisteredKeyDown = true;
				}
			});

			inputContainer.Add(_nameField);
		}

		void OnKeyPressed(KeyDownEvent e)
		{
			if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
			{
				OnNameChanged();
				_isRegisteredKeyDown = false;
				_nameField.UnregisterCallback<KeyDownEvent>(OnKeyPressed);
			}
		}

		private void OnNameChanged()
		{
			_parameter = graph.GetVariable(target.variableName);
			if (_parameter == null)
			{
				Debug.LogError($"No variable called '{target.variableName}' found");
			}
			else
			{
				// TODO: Add output port
				mainContainer.Add(new Label($"Value = {_parameter.GetValueType().Name}"));
			}
		}
	}
}