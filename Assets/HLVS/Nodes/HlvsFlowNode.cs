namespace HLVS.Nodes
{
	public abstract class HlvsFlowNode : HlvsNode
	{
		public abstract string[] GetNextExecutionLinks();
	}
}