using GraphProcessor;
using HLVS.Nodes.DataNodes;
using UnityEngine.UIElements;

namespace HLVS.Editor.NodeViews
{
	[NodeCustomEditor(typeof(MathNode))]
	public class MathNodeView : HlvsNodeView
	{
		private MathNode Target => (MathNode) nodeTarget;

		private VisualElement _errorBox;
		public override void Enable()
		{
			base.Enable();

			_errorBox = new VisualElement();

			TextField formulaInput = new();
			formulaInput.value = Target.formula.Expression;
			formulaInput.RegisterValueChangedCallback(evt =>
			{
				_errorBox.Clear();
				
				Target.formula.Expression = evt.newValue;
				Target.RecompileExpression();

				if (!string.IsNullOrEmpty(Target.errors))
				{
					var errors = new Label(Target.errors);
					_errorBox.Add(errors);
				}

				Target.Evaluate();
				
				ForceUpdatePorts();
			});
			formulaInput.style.minWidth = 200;
			
			controlsContainer.Add(formulaInput);
			controlsContainer.Add(_errorBox);
		}
	}
}