using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes.ActionNodes;
using HLVS.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(RaiseEventNode))]
	public class RaiseEventNodeView : HlvsNodeView
	{
		private RaiseEventNode _target;
		private DropdownField  _currentDropDown;
		
		public override void Enable()
		{
			base.Enable();

			_target = (RaiseEventNode)nodeTarget;

			_currentDropDown = CreateDropDown();
			inputContainer.Add(_currentDropDown);

			controlsContainer.Clear();
			
			foreach (PortView portView in inputPortViews.ToList())
			{
				var button = portView.Q<Button>();
				if(button != null)
					button.parent.Remove(button);
			}
		}

		private void UpdateDropDown()
		{
			_currentDropDown.choices = EventManager.instance.GetAllEvents().Select(hlvsEvent => hlvsEvent.name).ToList();

			if (_currentDropDown.choices.Count == 0)
				_currentDropDown.value = "No events defined";
		}

		private DropdownField CreateDropDown()
		{
			var namesField = new DropdownField(new List<string>(), 0);
			namesField.name = "Name";
			namesField.value = _target.eventName;
			namesField.RegisterValueChangedCallback((v) =>
			{
				owner.RegisterCompleteObjectUndo("Updated event name of RaiseEventNode");
				_target.eventName = v.newValue;

				ForceUpdatePorts();

				foreach (PortView portView in inputPortViews.ToList())
				{
					var button = portView.Q<Button>();
					if(button != null)
						button.parent.Remove(button);
				}
			});

			namesField.RegisterCallback<FocusInEvent>(evt =>
				UpdateDropDown());

			return namesField;
		}
	}
}