using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace HLVS
{
	public class HlvsBlackboard : ScriptableObject
	{
		[SerializeReference] public List<ExposedParameter> fields = new List<ExposedParameter>();
	}
}