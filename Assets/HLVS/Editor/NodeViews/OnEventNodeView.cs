using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(OnEventNode))]
	public class OnEventNodeView : HlvsNodeView
	{
		private const string addText = "New Event...";

		private OnEventNode   _target;
		private DropdownField _currentDropDown;

		public override void Select(VisualElement selectionContainer, bool additive)
		{
			base.Select(selectionContainer, additive);
			
			
			var choices = EventManager.instance.GetEventNames().ToList();
			choices.Add(addText);
			controlsContainer.Remove(_currentDropDown);
			
			_currentDropDown = CreateDropDown(choices);
			controlsContainer.Add(_currentDropDown);
		}

		public override void Enable()
		{
			_target = (OnEventNode)nodeTarget;

			EventManager.instance.AddEventDefinition(new HlvsEvent()
			{
				parameters = new List<ExposedParameter>
				{
					new BoolParameter() { name = "bool", value = true},
					new FloatParameter() { name = "float" , value = 2.5f}
				},
				name = "bla"
			});
			EventManager.instance.AddEventDefinition(
				new HlvsEvent()
				{
					parameters = new List<ExposedParameter>
					{
						new BoolParameter()
					},
					name = "other"
				});
			var choices = EventManager.instance.GetEventNames().ToList();
			choices.Add(addText);

			_currentDropDown = CreateDropDown(choices);
			controlsContainer.Add(_currentDropDown);
			
			
			controlsContainer.RegisterCallback<FocusOutEvent>(evt => { UpdateDropDown(); });
		}

		private void UpdateDropDown()
		{
			var choices = EventManager.instance.GetEventNames().ToList();
			choices.Add(addText);
			_currentDropDown.choices = choices;
		}

		private DropdownField CreateDropDown(List<string> choices)
		{
			var namesField = new DropdownField(choices, 0);
			namesField.name = "Name";
			namesField.value = _target.eventName;
			namesField.RegisterValueChangedCallback((v) =>
			{
				if (v.newValue == addText)
				{
					var popup = new TextField();
					popup.name = "Popup";
					owner.Add(popup);
					_target.eventName = "";

					popup.RegisterCallback<FocusOutEvent>(evt =>
					{
						_target.eventName = popup.value;
						EventManager.instance.AddEventDefinition(new HlvsEvent() { name = popup.value });

						namesField.value = popup.value;
						owner.Remove(popup);
						owner.serializedGraph.ApplyModifiedProperties();
						owner.serializedGraph.Update();
					});
				}
				else
				{
					owner.RegisterCompleteObjectUndo("Updated event name of OnEventNode");
					_target.eventName = namesField.value;
				}
			});

			namesField.RegisterCallback<FocusInEvent>(evt =>
				UpdateDropDown());
			
			return namesField;
		}
	}
}