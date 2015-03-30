using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using ColossalFramework;
using ColossalFramework.Plugins;

using CSLAPI.Runtime;

namespace CSLAPI.Fixes
{
	class PluginManagerFixes
	{
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
				CODebugBase<InternalLogChannel>.Error(InternalLogChannel.Mods, "Assembly at " + dllPath + " failed to load.\n" + exception.ToString());
				return null;
			}

		}

		[ClientFix("PluginManager Additional Assemblies Fix")]
		// ReSharper disable once UnusedMember.Global
		public static void PluginManagerFix()
		{
			// Should really just get a list of all assemblies loaded in the domain and be done with it.
			// Should also do this whenever a new assembly is loaded, so cross-mod compat works.

			List<string> assemblies = new List<string>
			{
				"ICities.dll",
				"ColossalManaged.dll",
				"UnityEngine.UI.dll"
			};

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (asm.GlobalAssemblyCache)
					continue;

				var dllName = Path.GetFileName(asm.Location);

				// This gets added automatically in their compiler
				if (dllName == "UnityEngine.dll")
					continue;

				assemblies.Add(dllName);
			}

			PluginManager.SetAdditionalAssemblies(assemblies.ToArray());

			// Compile scripts again here?
			PluginManager.CompileScripts();
			PluginManager.instance.LoadPlugins();
			PluginHelper.ValidatePlugins();

			Assembly.LoadFrom()
		}
	}
}
