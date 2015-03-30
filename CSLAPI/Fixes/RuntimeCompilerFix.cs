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
		}
	}
}
