using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
