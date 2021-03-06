﻿using System;
using System.Collections.Generic;
using System.Reflection;

using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;

using CSLAPI.Utils;

using ICities;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace CSLAPI.Fixes
{
	internal class CustomContentPanelFixes
	{
		[MethodDetour(typeof(CustomContentPanel), "RefreshPlugins", "CustomContentPanel.RefreshPlugins fix for missing IUserMod")]
		private static void RefreshPluginsDetour()
		{
			var instance = UIView.library.Get<CustomContentPanel>("CustomContentPanel");
			UITemplateManager.ClearInstances(ReflectionUtils.GetField<string>(typeof(CustomContentPanel), "kModEntryTemplate"));
			UIComponent component = instance.Find("ModsList");
			foreach (var current in PluginManager.instance.GetPluginsInfo())
			{
				string name = current.name;
				IUserMod[] instances = current.GetInstances<IUserMod>();
				if (instances.Length == 1)
				{
					name = instances[0].Name + " - " + instances[0].Description;
				}
				else if (instances.Length == 0)
				{
					// No IUserMod, so no plugin!
					DebugOutputPanel.AddMessage(PluginManager.MessageType.Error,
						"No IUserMod was found in the plugin. Each plugin MUST contain a public IUserMod implementation. " + current);
					continue;
				}
				else
				{
					name = instances[0].Name + " - " + instances[0].Description;
					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Multiple IUserMod implemented for the same mod. Only one IUserMod is accepted per mod. " + current);
				}
				PackageEntry entry = UITemplateManager.Get<PackageEntry>(ReflectionUtils.GetField<string>(typeof(CustomContentPanel), "kModEntryTemplate"));
				component.AttachUIComponent(entry.gameObject);
				entry.entryName = name;
				entry.entryActive = current.isEnabled;
				entry.pluginInfo = current;
				entry.publishedFileId = current.publishedFileID;
				entry.RequestDetails();
			}
		}
	}

	internal class PluginManagerFixes
	{
		[MethodDetour(typeof(PluginManager), "LoadPlugin", "PluginManager.LoadPlugin fix for assembly file path information.")]
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
		public static void AdditionalAssembliesFix()
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