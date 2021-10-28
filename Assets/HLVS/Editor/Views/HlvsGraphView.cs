using System;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Group = GraphProcessor.Group;

namespace HLVS.Editor.Views
{
	public class HlvsGraphView : BaseGraphView
	{
		public BlackboardView blackboardView;
		public ParameterView paramView;
		public readonly VisualElement boardContainer;
		public readonly HlvsToolbarView toolbarView;

		public HlvsGraphView(EditorWindow window) : base(window)
		{
			blackboardView = new BlackboardView(this);
			paramView = new ParameterView(this);

			toolbarView = new HlvsToolbarView(this);
			Add(toolbarView);


			boardContainer = new VisualElement();
			boardContainer.name = "boards-container";
			boardContainer.style.alignSelf = new StyleEnum<Align>(Align.FlexStart);
			boardContainer.style.width = 300;
			boardContainer.style.marginTop = 1;

			boardContainer.Add(blackboardView.blackboard);
			boardContainer.Add(paramView.blackboard);

			Add(boardContainer);
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			//base.BuildContextualMenu(evt);

			if (evt.target is GraphView && nodeCreationRequest != null)
			{
				evt.menu.AppendAction("Create Node", action => RequestNodeCreation(action.eventInfo.mousePosition),
					DropdownMenuAction.AlwaysEnabled);
				evt.menu.AppendSeparator();
			}

			foreach (var parameter in (graph as HlvsGraph).GetParameters())
			{
				evt.menu.AppendAction($"Add Parameters/{parameter.name}",
					action => Debug.Log($"Adding {parameter.name}")); // TODO: Actually add param as node
			}

			evt.menu.AppendSeparator();
			if (evt.target is GraphView || evt.target is Node)
				evt.menu.AppendAction("Copy", action => CopySelectionCallback(),
					action => canCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			if (evt.target is GraphView)
				evt.menu.AppendAction("Paste", action => this.PasteCallback(),
					action => canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			if (evt.target is GraphView || evt.target is Node || evt.target is Edge)
			{
				evt.menu.AppendAction("Delete",
					action => DeleteSelectionCallback(AskUser.DontAskUser),
					action => canDeleteSelection
						? DropdownMenuAction.Status.Normal
						: DropdownMenuAction.Status.Disabled);
			}

			evt.menu.AppendSeparator();
			evt.menu.AppendAction("Undo", action =>
			{
				Undo.PerformUndo();
				blackboardView.DisplayExistingBlackboardEntries();
				paramView.DisplayExistingParameterEntries();
				(graph as HlvsGraph).onParameterListChanged.Invoke();
			});

			evt.menu.AppendAction("Redo", action =>
			{
				Undo.PerformRedo();
				blackboardView.DisplayExistingBlackboardEntries();
				paramView.DisplayExistingParameterEntries();
				(graph as HlvsGraph).onParameterListChanged.Invoke();
			});
		}

		private void RequestNodeCreation(Vector2 position)
		{
			nodeCreationRequest(new NodeCreationContext()
			{
				screenMousePosition = position + new Vector2(300, 30),
				target = null,
				index = -1
			});
		}
	}
}