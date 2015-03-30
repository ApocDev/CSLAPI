using System;
using System.Reflection;

using CSLAPI.Fixes;
using CSLAPI.Utils.Finders;

using ICities;

using UnityEngine;

namespace CSLAPI
{
	public class Bootstrap : IUserMod
	{
		public static bool IsInitialized { get; set; }
		private static AttributeFinder<ClientFixAttribute> ClientFixes { get; set; }

		public static void Initialize()
		{
			if (IsInitialized)
			{
				return;
			}

			ClientFixes = new AttributeFinder<ClientFixAttribute>(false);
			ClientFixes.Updated += ApplyClientFixes;
			ApplyClientFixes();
		}

		private static void ApplyClientFixes()
		{
			foreach (var mem in ClientFixes.AttributedMembers)
			{
				var method = mem.Item1 as MethodInfo;
				if (mem.Item2[0].IsApplied)
				{
					continue;
				}
				try
				{
					Debug.Log("Applying client fix '" + mem.Item2[0].FixName + "'");
					method.Invoke(null, null);
					mem.Item2[0].IsApplied = true;
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to apply client fix '" + mem.Item2[0].FixName + "'");
					Debug.LogException(ex);
				}
			}
		}

		#region Implementation of IUserMod

		public string Name
		{
			get
			{
				// This is a hackish way to force the bootstrap to run.
				Initialize();
				return "CSLAPI";
			}
		}

		public string Description { get { return "Provides API for Modders on Cities: Skylines"; } }

		#endregion
	}
}