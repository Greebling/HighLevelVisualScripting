using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(OnEventNode))]
	public class OnEventNodeView : HlvsNodeView
	{
		private OnEventNode   _target;
		private DropdownField _currentDropDown;

		public override void Select(VisualElement selectionContainer, bool additive)
		{
			base.Select(selectionContainer, additive);


			var choices = EventManager.instance.GetEventNames().ToList();
			controlsContainer.Remove(_currentDropDown);

			_currentDropDown = CreateDropDown(choices);
			controlsContainer.Add(_currentDropDown);
		}

		public override void Enable()
		{
			_target = (OnEventNode)nodeTarget;

			if (_target.eventName == "" && EventManager.instance.GetEventNames().FirstOrDefault() != null)
			{
				_target.eventName = EventManager.instance.GetEventNames().FirstOrDefault();
				OnEventSelected();
			}


			var choices = EventManager.instance.GetEventNames().ToList();

			_currentDropDown = CreateDropDown(choices);
			controlsContainer.Add(_currentDropDown);


			controlsContainer.RegisterCallback<FocusOutEvent>(evt => { UpdateDropDown(); });
		}

		private void UpdateDropDown()
		{
			var choices = EventManager.instance.GetEventNames().ToList();
			_currentDropDown.choices = choices;
		}

		private void OnEventSelected()
		{
			_target.eventData = EventManager.instance.GetEventDefinition(_target.eventName).parameters;
			ForceUpdatePorts();
		}

		private DropdownField CreateDropDown(List<string> choices)
		{
			var namesField = new DropdownField(choices, 0);
			namesField.name = "Name";
			namesField.value = _target.eventName;

			namesField.RegisterValueChangedCallback((v) =>
			{
				owner.RegisterCompleteObjectUndo("Updated event name of OnEventNode");
				_target.eventName = namesField.value;
				OnEventSelected();
			});

			namesField.RegisterCallback<FocusInEvent>(evt =>
				UpdateDropDown());

			return namesField;
		}
	}
}