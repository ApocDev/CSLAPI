using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ColossalFramework;
using ColossalFramework.Plugins;

using CSLAPI.Runtime;

namespace CSLAPI.Fixes
{
	internal class PluginManagerFixes
	{
		// Static ref to keep it available "all the time" and not dispose of the detour unexepectedly
		// At least until this domain is unloaded, and it has to revert itself.
		private static RuntimeDetour _pluginManagerLoadPluginDetour;

		[ClientFix("PluginManager.LoadPlugin assembly file path info fix")]
		// ReSharper disable once UnusedMember.Global
		public static void PluginManagerLoadPluginFix()
		{
			_pluginManagerLoadPluginDetour = new RuntimeDetour(
				typeof(PluginManager).GetMethod("LoadPlugin", BindingFlags.Instance | BindingFlags.NonPublic),
				typeof(PluginManagerFixes).GetMethod("LoadPluginDetour", BindingFlags.Static | BindingFlags.NonPublic)
				);
			_pluginManagerLoadPluginDetour.Apply();
		}

		private static Assembly LoadPluginDetour(string dllPath)
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(dllPath);
				if (assembly != null)
				{
					CODebugBase<InternalLogChannel>.Log(InternalLogChannel.Mods, "Assembly " + assembly.FullName + " loaded.");
				}
				else
				{
					CODebugBase<InternalLogChannel>.Error(InternalLogChannel.Mods, "Assembly at " + dllPath + " failed to load.");
				}
				return assembly;
			}
			catch (Exception exception)
			{
				CODebugBase<InternalLogChannel>.Error(InternalLogChannel.Mods, "Assembly at " + dllPath + " failed to load.\n" + exception);
				return null;
			}
		}

		[ClientFix("PluginManager Additional Assemblies Fix")]
		// ReSharper disable once UnusedMember.Global
		public static void PluginManagerFix()
		{
			// Should really just get a list of all assemblies loaded in the domain and be done with it.
			// Should also do this whenever a new assembly is loaded, so cross-mod compat works.

			List<string> assemblies = new List<string>();

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (asm.GlobalAssemblyCache)
				{
					continue;
				}

				var dllName = asm.Location;
				assemblies.Add(dllName);
			}

			PluginManager.SetAdditionalAssemblies(assemblies.ToArray());

			// Compile scripts again here?
			PluginManager.CompileScripts();
			PluginManager.instance.LoadPlugins();
			PluginHelper.ValidatePlugins();
		}
	}
}