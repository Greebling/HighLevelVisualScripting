using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HLVS.Editor.Views
{
	public class HlvsGraphView : BaseGraphView
	{
		public Blackboard blackboard;

		private HlvsGraph Graph => graph as HlvsGraph;

		public HlvsGraphView(EditorWindow window) : base(window)
		{
			blackboard = new Blackboard(this);
			blackboard.style.alignItems = Align.Stretch;
			blackboard.scrollable = true;
			blackboard.addItemRequested += blackboard => Debug.Log("Request add"); // TODO: Add blackboard variables
			blackboard.addItemRequested += OnAddClicked;
		}

		protected void OnAddClicked(Blackboard b)
		{
			var addMenu = new GenericMenu();

			// TODO: Add gameobject, floats, strings etc
			foreach (var paramType in GetAddableTypes())
			{
				var niceParamName = paramType.Name;
				addMenu.AddItem(new GUIContent("add " + niceParamName), false, () =>
				{
					AddBlackboardEntry(paramType);
				});
			}

			addMenu.ShowAsContext();
		}

		protected void AddBlackboardEntry(Type entryType)
		{
			ExposedParameter param = new ExposedParameter();
			Graph.blackboardFields.Add(param);

			// displays the name of the new entry
			var field = new BlackboardField();
			field.AddToClassList("hlvs-blackboard-field");
			field.text = ObjectNames.NicifyVariableName(entryType.Name);
			field.typeText = "";
			field.tooltip = "Entry name";
			var typeL = field.Q<Label>("typeLabel");
			field.Q("contentItem").Remove(typeL);
			field.Q("node-border").style.overflow = Overflow.Hidden;
			//field.Q<TemplateContainer>("").
			
			// displays the value of the field
			var objField = new ObjectField();
			objField.label = "";
			objField.tooltip = "";
			objField.objectType = entryType;
			objField.allowSceneObjects = false;
			objField.style.flexGrow = 1;
			objField.style.width = 0; //so it does not have such a long minimum width and is aligned with otjher fields
			field.Add(objField);
			
			// display remove option
			var removeButton = new Button(() =>
			{
				Graph.blackboardFields.Remove(param);
				blackboard.Remove(field);
			});
			removeButton.text = " - ";
			removeButton.tooltip = "Remove entry";
			removeButton.style.flexGrow = 0;
			field.Add(removeButton);

			blackboard.Add(field);
		}
		
		protected virtual IEnumerable<Type> GetAddableTypes()
		{
			foreach (var type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>().OrderBy(type => type.Name))
			{
				if (type.IsGenericType)
					continue;

				yield return type;
			}
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			BuildRunGraphMenu(evt);
			base.BuildContextualMenu(evt);
		}

		/// <summary>
		/// Add the View entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected void BuildRunGraphMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Run Graph", e => (graph as HlvsGraph)?.RunStartNodes());
			evt.menu.AppendSeparator();
		}
	}
}