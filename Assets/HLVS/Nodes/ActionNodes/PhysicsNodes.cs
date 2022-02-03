using System;
using GraphProcessor;
using UnityEngine;

namespace HLVS.Nodes.ActionNodes
{
	[Serializable, NodeMenuItem("Physics/Create Explosive Force")]
	public class ExplosiveForceNode : HlvsActionNode
	{
		public override string name => "Add Explosive Force";
		
		[Input("Target")]
		public Rigidbody target;

		[Input("Explosion Strength")]
		public float strength;

		[Input("Force Origin")]
		public Vector3 origin;

		[Input("Explosion Radius")]
		public float radius;

		public override ProcessingStatus Evaluate()
		{
			target.AddExplosionForce(strength, origin, radius);
			
			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Physics/Push Body")]
	public class AddForceNode : HlvsActionNode
	{
		public override string name => "Push Body";
		
		[Input("Target")]
		public Rigidbody target;

		[Input("Force")]
		public Vector3 force;

		public override ProcessingStatus Evaluate()
		{
			target.AddForce(force);
			
			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Physics/Use Gravity")]
	public class UseGravityNode : HlvsActionNode
	{
		public override string name => "Use Gravity";
		
		[Input("Target")]
		public Rigidbody target;

		[Input("Use Gravity")]
		public bool value;

		public override ProcessingStatus Evaluate()
		{
			target.useGravity = value;
			
			return ProcessingStatus.Finished;
		}
	}
	
	[Serializable, NodeMenuItem("Physics/Set Kinematic")]
	public class SetKinematicNode : HlvsActionNode
	{
		public override string name => "Set Kinematic";
		
		[Input("Target")]
		public GameObject target;

		[Input("Is Kinematic")]
		public bool value;

		public override ProcessingStatus Evaluate()
		{
			target.GetComponent<Rigidbody>().isKinematic = value;
			
			return ProcessingStatus.Finished;
		}
	}
}