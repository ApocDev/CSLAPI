using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CSLAPI.Fixes;
using CSLAPI.Runtime;
using CSLAPI.Utils.Finders;

namespace CSLAPI.Registry
{
	public static class DetourRegistry
	{
		private static bool _initialized;
		private static AttributeFinder<MethodDetourAttribute> _detourMethods;

		/// <summary>
		///     The currently registered <seealso cref="RuntimeDetour" />s, indexed by the target method metadata token.
		/// </summary>
		public static Dictionary<int, RuntimeDetour> Detours { get; private set; }

		internal static void Initialize()
		{
			if (_initialized)
			{
				return;
			}

			Detours = new Dictionary<int, RuntimeDetour>();

			_detourMethods = new AttributeFinder<MethodDetourAttribute>(false);
			_detourMethods.Updated += UpdateDetours;

			UpdateDetours();

			_initialized = true;
		}

		/// <summary>
		///     Invokes the specified detoured method, in it's default state.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="method"></param>
		/// <param name="instance"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		/// <remarks>This method will remove the detour, call the original, then replace the detour.</remarks>
		public static T InvokeOriginal<T>(MethodInfo method, object instance, params object[] args)
		{
			RuntimeDetour detour;
			if (Detours.TryGetValue(method.MetadataToken, out detour))
			{
				detour.Remove();
				T ret = (T) method.Invoke(instance, args);
				detour.Apply();
				return ret;
			}
			throw new ArgumentException("Specified method is not currently registered.", "method");
		}

		/// <summary>
		///     Invokes the specified detoured method, in it's default state.
		/// </summary>
		/// <param name="method"></param>
		/// <param name="instance"></param>
		/// <param name="args"></param>
		/// <remarks>This method will remove the detour, call the original, then replace the detour.</remarks>
		public static void InvokeOriginal(MethodInfo method, object instance, params object[] args)
		{
			RuntimeDetour detour;
			if (Detours.TryGetValue(method.MetadataToken, out detour))
			{
				detour.Remove();
				method.Invoke(instance, args);
				detour.Apply();
				return;
			}
			throw new ArgumentException("Specified method is not currently registered.", "method");
		}

		/// <summary>
		///     Returns the original method that the calling method is detouring. Pass <see cref="MethodBase.GetCurrentMethod" />()
		///     for easiest usage.
		/// </summary>
		/// <param name="detouredCaller"></param>
		/// <returns></returns>
		public static MethodInfo GetOriginalMethod(MethodBase detouredCaller)
		{
			var attrib = detouredCaller.GetCustomAttributes(typeof(MethodDetourAttribute), false).FirstOrDefault() as MethodDetourAttribute;
			if (attrib == null)
			{
				return null;
			}

			return attrib.GetTargetMethod();
		}

		private static void UpdateDetours()
		{
			foreach (var member in _detourMethods.AttributedMembers)
			{
				if (member.Item2[0].IsApplied)
				{
					continue;
				}

				var detourMethod = member.Item1 as MethodInfo;
				var targetMethod = member.Item2[0].GetTargetMethod();

				if (Detours.ContainsKey(targetMethod.MetadataToken))
				{
					throw new ArgumentException("Could not create a second detour on " + targetMethod.DeclaringType + "." + targetMethod.Name);
				}

				var dt = new RuntimeDetour(targetMethod, detourMethod);
				dt.Apply();
				member.Item2[0].IsApplied = true;
				Detours.Add(targetMethod.MetadataToken, dt);
			}
		}
	}
}