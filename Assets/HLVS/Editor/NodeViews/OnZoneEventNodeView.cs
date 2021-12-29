using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(OnZoneEventNode))]
	public class OnZoneEventNodeView : HlvsNodeView
	{
		private OnZoneEventNode _target;
		private DropdownField   _currentDropDown;
		public override void Enable()
		{
			base.Enable();
			_target = (OnZoneEventNode)nodeTarget;

			// add callback to change node title
			var dropdown = contentContainer.Q<PropertyField>(nameof(OnZoneEventNode.activationType));
			dropdown.RegisterValueChangeCallback(evt =>
			{
				title = _target.name;
			});

			{
				var field = controlsContainer.Q<PropertyField>(nameof(OnZoneEventNode.zoneName));
				controlsContainer.Remove(field);
			}

			_currentDropDown = CreateDropDown();
			controlsContainer.Insert(0, _currentDropDown);
		}
		
		private void UpdateDropDown()
		{
			_currentDropDown.choices = ZoneNameProvider.instance.GetAllZoneNames().ToList();

			if (_currentDropDown.choices.Count == 0)
				_currentDropDown.value = "No variables found";
		}

		private DropdownField CreateDropDown()
		{
			var namesField = new DropdownField(new List<string>(), 0);
			namesField.name = "Name";
			namesField.value = _target.zoneName;

			if (namesField.value == "" &&  ZoneNameProvider.instance.GetAllZoneNames().FirstOrDefault() != null)
			{
				namesField.value = ZoneNameProvider.instance.GetAllZoneNames().FirstOrDefault();
			}
			
			namesField.RegisterValueChangedCallback((v) =>
			{
				owner.RegisterCompleteObjectUndo("Updated zone of OnZoneEventNode");
				_target.zoneName = namesField.value;
			});

			namesField.RegisterCallback<FocusInEvent>(evt => UpdateDropDown());

			return namesField;
		}
	}
}