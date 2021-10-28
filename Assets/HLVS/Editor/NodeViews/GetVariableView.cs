using GraphProcessor;
using HLVS.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(GetVariableNode))]
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

			OnNameChanged();
		}

		private void UpdateVisuals()
		{
			if (_parameter == null)
			{
				Debug.LogError($"No variable called '{target.variableName}' found");
			}
			else
			{
				// TODO: Add output port
				
				outputContainer.Clear();
				outputContainer.Add(new Label($"Value = {_parameter.GetValueType().Name}"));
				RefreshPorts();
			}
		}

		private void OnNameChanged()
		{
			Undo.RecordObject(owner.graph, "Set GetVariable node");
			
			target.variableName = _nameField.value;
			_titleLabel.text = target.name;
			_parameter = graph.GetVariable(target.variableName);
			
			EditorUtility.SetDirty(owner.graph);

			UpdateVisuals();
		}
	}
}