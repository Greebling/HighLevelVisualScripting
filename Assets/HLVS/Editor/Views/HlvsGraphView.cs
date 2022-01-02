using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using HLVS.Nodes;
using HLVS.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace HLVS.Editor.Views
{
	public class HlvsGraphView : BaseGraphView
	{
		public List<BlackboardView> blackboardViews;
		public ParameterView paramView;
		public readonly VisualElement boardContainer;
		public readonly HlvsToolbarView toolbarView;

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
			boardContainer.style.flexGrow = 1;

			// handle adding of blackboards via drag n drop TODO: Set drag indicator on blackboards correct
			RegisterCallback<DragExitedEvent>(evt =>
			{
				if (DragAndDrop.objectReferences.Length > 0 &&
					DragAndDrop.objectReferences.All(o => o is HlvsBlackboard))
				{
					foreach (var draggedObject in DragAndDrop.objectReferences)
					{
						_graph.AddBlackboard(draggedObject as HlvsBlackboard);
					}
				}
			});


			boardContainer.Add(paramView.blackboard);


			blackboardViews = new List<BlackboardView>();
			foreach (HlvsBlackboard graphBlackboard in _graph.blackboards)
			{
				AddBlackboardView(graphBlackboard);
			}

			_graph.onBlackboardAdded += AddBlackboardView;
			_graph.onBlackboardRemoved += RemoveBlackboardView;

			Add(boardContainer);
		}
		
		public override List< Port > GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List< Port >();

			var startNode = (HlvsNode) (startPort as PortView).owner.nodeTarget;
			var startView = ((PortView)startPort).owner;
			var previousNodes = new HashSet<BaseNode>();
			foreach (var previousNode in startNode.GetAllPreviousNodes())
			{
				previousNodes.Add(previousNode);
			}
			
			foreach (var p in ports)
			{
				var portView = (PortView) p;
				if (p.direction == startPort.direction)
					continue;

				//Check for type assignability

				if (startPort.direction == Direction.Input)
				{
					if (!BaseGraph.TypesAreConnectable(p.portType, startPort.portType))
						continue;
				}
				else
				{
					if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))
						continue;
				}

				if (portView.owner == startView)
					continue;
				
				var nextNode = (p as PortView).owner.nodeTarget;
				if(!(startNode is HlvsDataNode || nextNode is HlvsDataNode) && previousNodes.Contains(nextNode))
					continue;

				//Check if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
					continue;

				compatiblePorts.Add(p);
			}

			return compatiblePorts;
		}

		void AddBlackboardView(HlvsBlackboard board)
		{
			var view = new BlackboardView(this, board);
			blackboardViews.Add(view);
			boardContainer.Add(view.blackboard);
		}

		void RemoveBlackboardView(HlvsBlackboard board)
		{
			var view = blackboardViews.Find(view1 => view1.target == board);
			if (view != null)
			{
				boardContainer.Remove(view.blackboard);
				blackboardViews.Remove(view);
			}
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
				blackboardViews.ForEach(view => view.DisplayExistingBlackboardEntries());
				paramView.DisplayExistingParameterEntries();
				_graph.onParameterListChanged.Invoke();
			});

			evt.menu.AppendAction("Redo", action =>
			{
				Undo.PerformRedo();
				blackboardViews.ForEach(view => view.DisplayExistingBlackboardEntries());
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