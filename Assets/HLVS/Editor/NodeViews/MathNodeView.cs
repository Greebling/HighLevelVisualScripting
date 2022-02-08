using GraphProcessor;
using HLVS.Nodes.DataNodes;
using UnityEngine;
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
				
				if (Application.isPlaying)
				{
					var error = new Label("Cannot edit math expression while playing");
					_errorBox.Add(error);
				} else
				{
					Target.formula.Expression = evt.newValue;
					Target.RecompileExpression();

					// add errors
					if (!string.IsNullOrEmpty(Target.errors))
					{
						var errors = new Label(Target.errors);
						_errorBox.Add(errors);
					}

					Target.Evaluate();

					ForceUpdatePorts();
				}
			});
			formulaInput.style.minWidth = 200;
			
			controlsContainer.Add(formulaInput);
			controlsContainer.Add(_errorBox);
		}
	}
}