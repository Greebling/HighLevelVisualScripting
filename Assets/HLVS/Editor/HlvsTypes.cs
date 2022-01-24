using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLVS.Editor
{
	/// <summary>
	/// When applied the given class pops up as an addable type in graph blackboards and parameter lists
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class HlvsTypeAttribute : Attribute
	{
	}

	public static class HlvsTypes
	{
		public static readonly List<Type> BuiltInTypes = new List<Type>()
		{
			typeof(bool), typeof(float), typeof(Vector3), typeof(string),
			typeof(GameObject), typeof(Scene)
		};

		/// <summary>
		/// Beautifies a given types name
		/// </summary>
		public static string GetTypeName(Type type)
		{
			string result = type.Name switch
			                {
				                "Single"  => "Number",
				                "String"  => "Text",
				                "Vector3" => "Position",
				                _         => ObjectNames.NicifyVariableName(type.Name)
			                };
			return result;
		}

		public static TypeCache.TypeCollection GetUserTypes()
		{
			return TypeCache.GetTypesWithAttribute<HlvsTypeAttribute>();
		}
	}
}