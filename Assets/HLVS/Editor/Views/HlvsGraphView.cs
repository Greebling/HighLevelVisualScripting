using System;
using System.Collections.Generic;
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
		public          List<BlackboardView> blackboardView;
		public          ParameterView        paramView;
		public readonly VisualElement        boardContainer;
		public readonly HlvsToolbarView      toolbarView;
		
		private HlvsGraph _graph => graph as HlvsGraph;

		public HlvsGraphView(EditorWindow window, HlvsGraph graph) : base(window)
		{
			this.graph = graph;
			paramView = new ParameterView(this);

			toolbarView = new HlvsToolbarView(this);
			Add(toolbarView);


			boardContainer = new VisualElement();
			boardContainer.name = "boards-container";
			boardContainer.style.alignSelf = new StyleEnum<Align>(Align.FlexStart);
			boardContainer.style.width = 300;
			boardContainer.style.marginTop = 1;

			boardContainer.Add(paramView.blackboard);
			
			Debug.Assert(_graph);
			blackboardView = new List<BlackboardView>();
			foreach (HlvsBlackboard graphBlackboard in _graph.blackboards)
			{
				AddBlackboardView(graphBlackboard);
			}

			_graph.onBlackboardAdded += AddBlackboardView;


			Add(boardContainer);
		}

		void AddBlackboardView(HlvsBlackboard board)
		{
			var view = new BlackboardView(this, board);
			blackboardView.Add(view);
			boardContainer.Add(view.blackboard);
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

			evt.menu.AppendSeparator();
			/*
			if (evt.target is GraphView || evt.target is Node)
				evt.menu.AppendAction("Copy", action => CopySelectionCallback(),
					action => canCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			if (evt.target is GraphView)
				evt.menu.AppendAction("Paste", action => this.PasteCallback(),
					action => canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
			*/ 
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
				blackboardView.ForEach(view => view.DisplayExistingBlackboardEntries());
				paramView.DisplayExistingParameterEntries();
				_graph.onParameterListChanged.Invoke();
			});

			evt.menu.AppendAction("Redo", action =>
			{
				Undo.PerformRedo();
				blackboardView.ForEach(view => view.DisplayExistingBlackboardEntries());
				paramView.DisplayExistingParameterEntries();
				_graph.onParameterListChanged.Invoke();
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