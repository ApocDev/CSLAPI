using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace CSLAPI.Utils.Finders
{
	[AttributeUsage(AttributeTargets.All, Inherited = false)]
	public sealed class TypeFinderIgnoredAttribute : Attribute
	{
	}

	/// <summary>
	///     Finds all types inheriting from the specified type. This class will attempt to instantiate all classes
	///     if told to do so.
	/// </summary>
	public class TypeFinder : List<object>
	{
		public TypeFinder(Type type, bool includeGacAssemblies = false, bool createInstances = true)
		{
			Type = type;
			IncludeGacAssemblies = includeGacAssemblies;
			CreateInstances = createInstances;
			InstanceTypes = new List<Type>();

			Refresh();

			// When we load a new assembly, refresh this list. This allows us to "refresh" displays.
			AppDomain.CurrentDomain.AssemblyLoad += (s, e) => LoadTypesFromAssembly(e.LoadedAssembly);
		}

		public bool IncludeGacAssemblies { get; set; }
		public bool CreateInstances { get; set; }
		public Type Type { get; private set; }
		public List<Type> InstanceTypes { get; set; }

		private void LoadTypesFromAssembly(Assembly asm)
		{
			// Skip assemblies in the GAC. We should never be installing them.
			if (!IncludeGacAssemblies && asm.GlobalAssemblyCache)
			{
				return;
			}

			//Log.Debug("Loading types of " + Type.Name + " from assembly " + asm.GetName().Name);
			foreach (Type type in asm.GetTypes())
			{
				if (!type.IsClass)
				{
					continue;
				}

				// If we already included this type, go ahead and skip it.
				if (this.Any(o => o.GetType() == type))
				{
					continue;
				}

				// Type is ignored, so don't process it.
				if (type.GetCustomAttributes(typeof(TypeFinderIgnoredAttribute), false).Any())
				{
					continue;
				}

				if (Type.IsInterface)
				{
					if (type.GetInterfaces().Any(interfaceType => interfaceType == Type))
					{
						try
						{
							InstanceTypes.Add(type);
							if (CreateInstances)
							{
								Add(Activator.CreateInstance(type));
							}
						}
						catch (MissingMethodException ex)
						{
							Debug.LogError("Could not instantiate class " + type +
							               ". It does not have a public, parameterless constructor.");
							Debug.LogException(ex);
						}
						catch (Exception ex)
						{
							Debug.LogError("Exception instantiating type " + type);
							Debug.LogException(ex);
						}
					}
				}
				else
				{
					if (type.IsSubclassOf(Type))
					{
						try
						{
							InstanceTypes.Add(type);
							if (CreateInstances)
							{
								Add(Activator.CreateInstance(type));
							}
						}
						catch (MissingMethodException ex)
						{
							Debug.LogError("Could not instantiate class " + type +
							               ". It does not have a public, parameterless constructor.");
							Debug.LogException(ex);
						}
						catch (Exception ex)
						{
							Debug.LogError("Exception instantiating type " + type);
							Debug.LogException(ex);
						}
					}
				}
			}
		}

		public void Refresh()
		{
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					LoadTypesFromAssembly(asm);
				}
				catch (ReflectionTypeLoadException)
				{
				}
				catch (TypeLoadException)
				{
				}
			}
		}
	}

	/// <summary>
	///     Finds all types inheriting from the specified generic parameter. This class will attempt to instantiate all classes
	///     if told to do so.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TypeFinder<T> : List<T>
	{
		public TypeFinder(bool includeGacAssemblies, bool createInstances = true)
		{
			Type = typeof(T);
			IncludeGacAssemblies = includeGacAssemblies;
			CreateInstances = createInstances;
			InstanceTypes = new List<Type>();

			Refresh();

			// When we load a new assembly, refresh this list. This allows us to "refresh" displays.
			AppDomain.CurrentDomain.AssemblyLoad += (s, e) => LoadTypesFromAssembly(e.LoadedAssembly);
		}

		public bool IncludeGacAssemblies { get; set; }
		public bool CreateInstances { get; set; }
		public Type Type { get; private set; }
		public List<Type> InstanceTypes { get; set; }

		private void LoadTypesFromAssembly(Assembly asm)
		{
			// Skip assemblies in the GAC. We should never be installing them.
			if (!IncludeGacAssemblies && asm.GlobalAssemblyCache)
			{
				return;
			}

			//Log.Debug("Loading types of " + Type.Name + " from assembly " + asm.GetName().Name);
			foreach (Type type in asm.GetTypes())
			{
				if (!type.IsClass)
				{
					continue;
				}

				// If we already included this type, go ahead and skip it.
				if (this.Any(o => o.GetType() == type))
				{
					continue;
				}

				// Type is ignored, so don't process it.
				if (type.GetCustomAttributes(typeof(TypeFinderIgnoredAttribute), false).Any())
				{
					continue;
				}

				if (Type.IsInterface)
				{
					if (type.GetInterfaces().Any(interfaceType => interfaceType == Type))
					{
						try
						{
							InstanceTypes.Add(type);
							if (CreateInstances)
							{
								Add((T) Activator.CreateInstance(type));
							}
						}
						catch (MissingMethodException ex)
						{
							Debug.LogError("Could not instantiate class " + type +
							               ". It does not have a public, parameterless constructor.");
							Debug.LogException(ex);
						}
						catch (Exception ex)
						{
							Debug.LogError("Exception instantiating type " + type);
							Debug.LogException(ex);
						}
					}
				}
				else
				{
					if (type.IsSubclassOf(Type))
					{
						try
						{
							InstanceTypes.Add(type);
							if (CreateInstances)
							{
								Add((T) Activator.CreateInstance(type));
							}
						}
						catch (MissingMethodException ex)
						{
							Debug.LogError("Could not instantiate class " + type +
							               ". It does not have a public, parameterless constructor.");
							Debug.LogException(ex);
						}
						catch (Exception ex)
						{
							Debug.LogError("Exception instantiating type " + type);
							Debug.LogException(ex);
						}
					}
				}
			}
		}

		public void Refresh()
		{
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					LoadTypesFromAssembly(asm);
				}
				catch (ReflectionTypeLoadException)
				{
				}
				catch (TypeLoadException)
				{
				}
			}
		}
	}
}