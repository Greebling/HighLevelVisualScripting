using HLVS.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	[CustomEditor(typeof(Zone))]
	public class ZoneEditor : UnityEditor.Editor
	{
		private VisualElement       _root;
		private TextField           _zoneField;

		private Zone _target => (Zone)target;

		public override VisualElement CreateInspectorGUI()
		{
			_root = new VisualElement();
			CreateInspector();
			return _root;
		}

		private void CreateInspector()
		{
			_root.Clear();
			_root.style.flexDirection = FlexDirection.Row;

			_zoneField = new TextField("Zone");
			_zoneField.value = _target.zoneName;
			_zoneField.RegisterCallback<FocusOutEvent>(evt =>
			{
				ZoneNameProvider.instance.RemoveZone(_target.zoneName);
				
				_target.zoneName = _zoneField.value;
				ZoneNameProvider.instance.AddZone(_zoneField.value);
				
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			});
			_zoneField.style.flexGrow = 4;
			_zoneField.Q<Label>().style.minWidth = 80;
			_root.Add(_zoneField);


			// from previous zone
			var selectButton = new Button();
			selectButton.clicked += () =>
			{
				var dropDown = CreateDropDown();
				var pos = selectButton.worldTransform.GetPosition();
				float height = 400;
				dropDown.DropDown(new Rect(pos.x, pos.y- (height), 400, height), _zoneField);
			};
			selectButton.text = "Current";
			_root.Add(selectButton);
		}

		private GenericDropdownMenu CreateDropDown()
		{
			var dropdown = new GenericDropdownMenu();

			foreach (var zone in ZoneNameProvider.instance.GetAllZoneNames())
			{
				dropdown.AddItem(zone, false, action =>
				{
					_zoneField.value = zone;
					_target.zoneName = zone;
					ZoneNameProvider.instance.AddZone(_zoneField.value);
					
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
				}, this);
			}
			
			return dropdown;
		}
	}
}