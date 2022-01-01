using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor
{
	public class EventsWindow : EditorWindow
	{
		private VisualElement _root;

		private List<HlvsEvent> _events = new()
		{
			new()
			{
				name = "My Event",
				parameters = new List<ExposedParameter>
					{ new BoolParameter { name = "Pog" }, new FloatParameter { name = "Champ" }, new FloatParameter { name = "Kappa" } }
			}
		};

		[MenuItem("HLVS/Event Editor")]
		public static EventsWindow Open()
		{
			var window = GetWindow<EventsWindow>();
			return !window ? CreateWindow<EventsWindow>() : window;
		}

		private void OnEnable()
		{
			_events = EventManager.instance.GetAllEvents();

			titleContent.text = "Events Manager";
			CreateGUI();
		}

		private void CreateGUI()
		{
			_root = new VisualElement();
			_root.styleSheets.Add(Resources.Load<StyleSheet>("EventWindow"));
			rootVisualElement.Clear();
			rootVisualElement.Add(_root);
			

			AddEventList();

			//AddClearButton(); // disabled, only for debugging purposes
		}

		private void AddEventList()
		{
			var list = new ListView(_events, 140,
				() => new EventDrawer(),
				(element, i) => (element as EventDrawer).SetRender(_events[i]));
			list.showAddRemoveFooter = true;
			list.headerTitle = "Parameters";
			list.style.flexGrow = 1;


			list.itemsAdded += ints =>
			{
				foreach (int i in ints)
				{
					var currentEventNames = EventManager.instance.GetEventNames().ToArray();
					var newEventName = ObjectNames.GetUniqueName(currentEventNames, "My Event");

					_events[i] = new HlvsEvent() { name = newEventName };
					EventManager.instance.AddEventDefinition(_events[i]);
					AssetDatabase.SaveAssetIfDirty(EventManager.instance);
				}
			};
			list.itemsRemoved += ints =>
			{
				var list = _events.ToList();
				foreach (int i in ints)
				{
					EventManager.instance.RemoveEventDefinition(list[i].name);
				}
			};

			_root.Add(list);
		}

		private void AddClearButton()
		{
			var button = new Button(() =>
			{
				EventManager.instance.ClearAllEvents();
				_events.Clear();
			});
			button.text = "Clear Manager";
			button.style.maxWidth = 300;
			_root.Add(button);
		}
	}

	class EventDrawer : VisualElement
	{
		private                 bool         isAddingItem = false;
		private static readonly List<string> typeChoices  = new() { "GameObject", "Position", "Number", "Text" };

		public void SetRender(HlvsEvent e)
		{
			Clear();

			if (e == null)
				return;

			style.flexDirection = FlexDirection.Row;

			var nameField = new TextField();
			nameField.value = e.name;
			nameField.style.minWidth = 200;
			nameField.style.height = 20;
			nameField.style.alignSelf = Align.Center;
			nameField.RegisterValueChangedCallback(evt =>
			{
				EventManager.instance.RemoveEventDefinition(e.name);
				e.name = evt.newValue;
				EventManager.instance.AddEventDefinition(e);
			});
			Add(nameField);


			var separatorBox = new Box();
			separatorBox.style.marginLeft = 3;
			separatorBox.style.marginRight = 3;
			separatorBox.style.marginTop = 3;
			separatorBox.style.marginBottom = 3;
			Add(separatorBox);


			var parameterList = new ListView(e.parameters, 22, () =>
				{
					var ele = new VisualElement();
					if (isAddingItem)
					{
						ShowAddParamMenu(e, ele);
						isAddingItem = false;
					}

					return ele;
				},
				(ve, i) => { UpdateParamView(e, ve, i); }
			);
			parameterList.itemsAdded += ints => { isAddingItem = true; };
			parameterList.headerTitle = "Parameters";
			parameterList.showAddRemoveFooter = true;
			parameterList.style.marginTop = 2;
			parameterList.style.minWidth = 350;
			parameterList.style.flexGrow = 1;
			parameterList.style.marginRight = 10;
			parameterList.style.marginBottom = 10;

			// make outline box around parameter list view
			var box = parameterList.Q("unity-content-and-vertical-scroll-container");
			box.AddToClassList("hlvs-event-list");


			Add(parameterList);
		}

		private static void UpdateParamView(HlvsEvent e, VisualElement ve, int i)
		{
			ve.Clear();

			if (e.parameters.Count <= i || e.parameters[i] == null)
				return;

			ve.style.flexDirection = FlexDirection.Row;


			var nameField = new TextField();
			nameField.value = e.parameters[i].name;
			nameField.RegisterValueChangedCallback(evt =>
			{
				EventManager.instance.RemoveEventDefinition(e.name);
				e.name = evt.newValue;
				EventManager.instance.AddEventDefinition(e);
			});
			nameField.style.width = 200;
			ve.Add(nameField);

			var isLabel = new Label(" : ");
			ve.Add(isLabel);

			var typeLabel = new Label(e.parameters[i].GetValueType().Name);
			typeLabel.style.minWidth = 100;


			var typeDropdown = new DropdownField(typeChoices, TypeToString(e.parameters[i].GetValueType()));
			typeDropdown.RegisterValueChangedCallback(evt =>
			{
				EventManager.instance.RemoveEventDefinition(e.name);
				var previousName = e.parameters[i].name;
				e.parameters[i] = StringToParam(evt.newValue);
				e.parameters[i].name = previousName;
				EventManager.instance.AddEventDefinition(e);
			});
			typeDropdown.style.minWidth = 150;
			typeDropdown.style.flexGrow = 1;
			ve.Add(typeDropdown);
		}

		private static string TypeToString(Type t)
		{
			switch (t)
			{
				case Type _ when t == typeof(GameObject):
					return "GameObject";
				case Type _ when t == typeof(Vector3):
					return "Position";
				case Type _ when t == typeof(float):
					return "Number";
				case Type _ when t == typeof(string):
					return "Text";
				case Type _ when t == typeof(bool):
					return "Boolean";
			}

			return "Unknown";
		}

		private static ExposedParameter StringToParam(string typeName)
		{
			switch (typeName)
			{
				case "GameObject":
					return new GameObjectParameter();
				case "Position":
				case "Vector3":
					return new Vector3Parameter();
				case "Number":
				case "Single":
				case "Float":
				case "single":
				case "float":
					return new FloatParameter();
				case "Text":
				case "String":
				case "string":
					return new StringParameter();
				case "Boolean":
				case "bool":
					return new BoolParameter();
				default:
					throw new NotImplementedException("Unknown type " + typeName);
			}
		}


		private static void ShowAddParamMenu(HlvsEvent e, VisualElement element)
		{
			var menu = new GenericMenu();

			AddItem("GameObject", new GameObjectParameter());
			AddItem("Position", new Vector3Parameter());
			AddItem("Number", new FloatParameter());
			AddItem("Text", new StringParameter());
			AddItem("Boolean", new BoolParameter());

			void AddItem(string name, ExposedParameter param)
			{
				menu.AddItem(new GUIContent(name), false, data =>
				{
					EventManager.instance.RemoveEventDefinition(e.name);
					var names = e.parameters.Where(p => p != null).Select(p => p.name).ToArray();
					var paramName = ObjectNames.GetUniqueName(names, "My " + name);
					param.name = paramName;
					e.parameters[^1] = param;
					EventManager.instance.AddEventDefinition(e);

					UpdateParamView(e, element, e.parameters.Count - 1);
				}, null);
			}

			menu.ShowAsContext();
		}
	}
}