using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLVS.Editor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class HlvsTypeAttribute : Attribute
	{
	}

	public static class HlvsTypes
	{
		public static readonly List<Type> BuiltInTypes = new List<Type>()
		{
			typeof(bool), typeof(float), typeof(int), typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(string),
			typeof(Color), typeof(GameObject), typeof(Collider), typeof(Rigidbody), typeof(Material), typeof(Scene)
		};

		public static TypeCache.TypeCollection GetUserTypes()
		{
			return TypeCache.GetTypesWithAttribute<HlvsTypeAttribute>();
		}
	}
}