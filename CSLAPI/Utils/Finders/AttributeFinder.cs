using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSLAPI.Utils.Finders
{
	public class AttributeFinder<T> where T : Attribute
	{
		public AttributeFinder(bool publicOnly = true, bool includeGacAssemblies = false)
		{
			AttributedMembers = new List<Tuple<MemberInfo, List<T>>>();
			AttributedObjects = new List<Tuple<Type, List<T>>>();

			//[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
			var usage = typeof(T).GetCustomAttributes(typeof(AttributeUsageAttribute), false).FirstOrDefault() as AttributeUsageAttribute;
			var classes = (usage.ValidOn & (AttributeTargets.Class)) != 0;
			var structs = (usage.ValidOn & (AttributeTargets.Struct)) != 0;
			// probably not right
			var enums = (usage.ValidOn & (AttributeTargets.Enum)) != 0;

			var members = (usage.ValidOn & (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Constructor | AttributeTargets.Method)) != 0;
			//var methods = (usage.ValidOn & (AttributeTargets.Constructor | AttributeTargets.Method)) != 0;
			
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				LoadAttribsFromAssembly(publicOnly, includeGacAssemblies, asm, classes, enums, structs, members);
			}

			// When we load a new assembly, refresh this list. This allows us to "refresh" displays.
			AppDomain.CurrentDomain.AssemblyLoad += (s, e) => LoadAttribsFromAssembly(
				publicOnly,
				includeGacAssemblies,
				e.LoadedAssembly,
				classes,
				enums,
				structs,
				members);
		}

		public event Action Updated;

		private void LoadAttribsFromAssembly(bool publicOnly, bool includeGacAssemblies, Assembly asm, bool classes, bool enums, bool structs, bool members)
		{
			// Exclude GAC types.
			if (!includeGacAssemblies && asm.GlobalAssemblyCache)
			{
				return;
			}

			foreach (var t in asm.GetTypes())
			{
				if (classes && !t.IsClass)
				{
					continue;
				}

				if (enums && !t.IsEnum)
				{
					continue;
				}

				if (structs && !t.IsValueType)
				{
					continue;
				}

				if (!publicOnly && !t.IsPublic)
				{
					continue;
				}

				// check for the attribute on the class
				var typeAttribs = t.GetCustomAttributes(typeof(T), false).Cast<T>().ToList();
				if (typeAttribs.Count != 0)
				{
					AttributedObjects.Add(new Tuple<Type, List<T>>(t, typeAttribs));
				}

				// now check members
				if (members)
				{
					foreach (var mi in t.GetMembers())
					{
						var mia = mi.GetCustomAttributes(typeof(T), false).Cast<T>().ToList();
						if (mia.Count != 0)
						{
							AttributedMembers.Add(new Tuple<MemberInfo, List<T>>(mi, mia));
						}
					}

					//foreach (MethodInfo mi in t.GetMethods())
					//{
					//	var mia = mi.GetCustomAttributes<T>().ToList();
					//	if (mia.Count != 0)
					//	{
					//		AttributedMembers.Add(new Tuple<MemberInfo, List<T>>(mi, mia));
					//	}
					//}
				}
			}

			if (Updated != null)
				Updated();
		}

		public List<Tuple<MemberInfo, List<T>>> AttributedMembers { get; set; }
		public List<Tuple<Type, List<T>>> AttributedObjects { get; set; }
	}
}