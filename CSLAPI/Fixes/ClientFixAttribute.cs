using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using CSLAPI.Utils;

namespace CSLAPI.Fixes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public sealed class ClientFixAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
		/// </summary>
		public ClientFixAttribute(string fixName)
		{
			FixName = fixName;
		}

		public string FixName { get; set; }
		public bool IsApplied { get; set; }
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public sealed class MethodDetourAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
		/// </summary>
		public MethodDetourAttribute(Type targetType, string methodName, string comment = null)
		{
			TargetType = targetType; 
			TargetMethod = methodName;
			Comment = comment;
		}

		public Type TargetType { get; set; }
		public string TargetMethod { get; set; }
		public bool IsApplied { get; set; }
		public Type[] MethodParameters { get; set; }
		public string Comment { get; set; }

		public MethodInfo GetTargetMethod()
		{
			return ReflectionUtils.FindMethod(TargetType, TargetMethod, MethodParameters);
		}
	}
}
