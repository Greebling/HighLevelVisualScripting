using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes.ActionNodes;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(SaveToVariableNode))]
	public class SaveToVariableNodeView : HlvsNodeView
	{
		private SaveToVariableNode _target;
		private DropdownField      _currentDropDown;

		public override void Enable()
		{
			base.Enable();
			
			_target = (SaveToVariableNode)nodeTarget;

			_currentDropDown = CreateDropDown();
			inputContainer.Add(_currentDropDown);
			
			controlsContainer.Clear();
			title = _target.name + " " + _target.variableName;
		}
		
		private void UpdateDropDown()
		{
			var choices = graph.GetParameters().Select(parameter => parameter.name).ToList();
			choices.AddRange(graph.GetBlackboardFields().Select(parameter => parameter.name));
			_currentDropDown.choices = choices;

			if (_currentDropDown.choices.Count == 0)
				_currentDropDown.value = "No variables found";
		}

		private DropdownField CreateDropDown()
		{
			var namesField = new DropdownField(new List<string>(), 0);
			namesField.name = "Name";
			namesField.value = _target.variableName;
			namesField.RegisterValueChangedCallback((v) =>
			{
				owner.RegisterCompleteObjectUndo("Updated variable name of SaveToVariableNode");
				_target.variableName = v.newValue;

				title = _target.name + " " + _target.variableName;
				ForceUpdatePorts();
			});

			namesField.RegisterCallback<FocusInEvent>(evt =>
				UpdateDropDown());

			return namesField;
		}
	}
}