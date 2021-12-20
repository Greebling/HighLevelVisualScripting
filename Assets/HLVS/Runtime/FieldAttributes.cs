using System;
using UnityEngine;

namespace HLVS.Runtime
{
	public abstract class NodeFieldAttribute : Attribute
	{
		public abstract bool CheckField(object fieldValue);
		public abstract string GetErrorMessage();
	}
	
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class LargerThanAttribute : NodeFieldAttribute
	{
		public readonly float min;
		
		public LargerThanAttribute(float min)
		{
			this.min = min;
		}

		public override bool CheckField(object fieldValue)
		{
			var converted = Convert.ChangeType(fieldValue, typeof(float));
			Debug.Assert(converted != null, "Attribute must be attached to a field that is convertible to float");
			var field = (float)converted;
			return field > min;
		}

		public override string GetErrorMessage()
		{
			return "Value was smaller than " + min;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class LargerOrEqualAttribute : NodeFieldAttribute
	{
		public readonly float min;
		
		public LargerOrEqualAttribute(float min)
		{
			this.min = min;
		}

		public override bool CheckField(object fieldValue)
		{
			var converted = Convert.ChangeType(fieldValue, typeof(float));
			Debug.Assert(converted != null, "Attribute must be attached to a field that is convertible to float");
			var field = (float)converted;
			return field >= min;
		}

		public override string GetErrorMessage()
		{
			return "Value was smaller than " + min;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class SmallerThanAttribute : NodeFieldAttribute
	{
		public readonly float max;
		
		public SmallerThanAttribute(float max)
		{
			this.max = max;
		}

		public override bool CheckField(object fieldValue)
		{
			var converted = Convert.ChangeType(fieldValue, typeof(float));
			Debug.Assert(converted != null, "Attribute must be attached to a field that is convertible to float");
			var field = (float)converted;
			return field < max;
		}

		public override string GetErrorMessage()
		{
			return "Value was larger than " + max;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class SmallerOrEqualAttribute : NodeFieldAttribute
	{
		public readonly float max;
		
		public SmallerOrEqualAttribute(float max)
		{
			this.max = max;
		}

		public override bool CheckField(object fieldValue)
		{
			var converted = Convert.ChangeType(fieldValue, typeof(float));
			Debug.Assert(converted != null, "Attribute must be attached to a field that is convertible to float");
			var field = (float)converted;
			return field <= max;
		}

		public override string GetErrorMessage()
		{
			return "Value was larger than " + max;
		}
	}
}