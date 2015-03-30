using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using ColossalFramework.Plugins;

namespace CSLAPI.Fixes
{
	class RuntimeCompilerFix
	{
		[ClientFix("PluginManager Additional Assemblies Fix")]
		public static void PluginManagerFix()
		{
			// Should really just get a list of all assemblies loaded in the domain and be done with it.
			// Should also do this whenever a new assembly is loaded, so cross-mod compat works.

			PluginManager.SetAdditionalAssemblies(
				"ICities.dll", // Original
				"ColossalManaged.dll", // Add the CO lib
				"Assembly-CSharp.dll", // The Assembly-CSharp lib (which contains 99% of the game code)
				// "UnityEngine.dll", // Note: UnitEngine gets added by PluginManager when it's actually compiled (options.ReferencedAssemblies.Add(typeof(GameObject).Assembly.Location);)
				"UnityEngine.UI.dll", // UI stuff
				// Include ourselves in the compiler so we can do runtime hooks, etc, from other mods
				Path.GetFileName(Assembly.GetExecutingAssembly().Location)
				);

			// Compile scripts again here?
			PluginManager.CompileScripts();
			PluginManager.instance.LoadPlugins();
			PluginHelper.ValidatePlugins();
		}
	}
}
