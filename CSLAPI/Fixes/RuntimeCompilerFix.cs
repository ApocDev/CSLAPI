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
			PluginManager.SetAdditionalAssemblies(
				"ICities.dll", // Original
				"ColossalManaged.dll", // Add the CO lib
				"Assembly-CSharp.dll", // The Assembly-CSharp lib (which contains 99% of the game code)
				"UnityEngine.dll", // Unity, just for kicks
				"UnityEngine.UI.dll", // UI stuff
				// Include ourselves in the compiler.
				Path.GetFileName(Assembly.GetExecutingAssembly().Location)
				);
		}
	}
}
