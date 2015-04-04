using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSLAPI.Utils
{
	// The copy/pasta is strong in this one. No real way to do simple static/nonstatic methods without copying code.
	// This entire class wouldn't be needed if Unity would update it's version of Mono to at least .NET 4, where we can use the "dynamic" object
	// syntax and just make life easy.
	internal class ReflectionUtils
	{
		private static Dictionary<Type, Dictionary<string, MethodInfo>> _methodCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
		private static Dictionary<Type, Dictionary<string, FieldInfo>> _fieldCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
		#region Invoke

		public static MethodInfo FindMethod(Type type, string methodName, params Type[] args)
		{
			Dictionary<string, MethodInfo> typeCache;
			if (_methodCache.TryGetValue(type, out typeCache))
			{
				// Build the method sig maybe?
				string methodSig = methodName + "@" + string.Join(",", args.Select(t => t.Name).ToArray());
				MethodInfo info;
				if (typeCache.TryGetValue(methodSig, out info))
				{
					return info;
				}
			}

			// Pass a null array to GetMethod as it shortcuts early instead of doing some sanity checks inside GetMethod itself.
			if (args.Length == 0)
			{
				args = null;
			}

			var methodInfo = type.GetMethod(methodName,
				BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
				null,
				args,
				null);

			if (methodInfo == null)
			{
				throw new ArgumentException(
					string.Format("Method '{0}({1})' could not be found on object of type {2}",
						methodName,
						args != null ? string.Join(", ", args.Select(t => t.Name).ToArray()) : string.Empty,
						type.FullName),
					"methodName");
			}

			if (typeCache == null)
			{
				typeCache = new Dictionary<string, MethodInfo>
				{
					{methodName + "@" + string.Join(",", args.Select(t => t.Name).ToArray()), methodInfo}
				};
				_methodCache.Add(type, typeCache);
			}

			return methodInfo;
		}

		/// <summary>
		///     Invokes a static method on the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static void Invoke(Type type, string methodName, params object[] args)
		{
			Invoke<int>(type, methodName, args);
		}

		/// <summary>
		///     Invokes a static method on the specified type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static T Invoke<T>(Type type, string methodName, params object[] args)
		{
			// Try and find the method via the arguments passed in.
			var methodArgumentTypes = args.Select(a => a.GetType()).ToArray();

			Dictionary<string, MethodInfo> typeCache;
			if (_methodCache.TryGetValue(type, out typeCache))
			{
				// Build the method sig maybe?
				string methodSig = methodName + "@" + string.Join(",", methodArgumentTypes.Select(t => t.Name).ToArray());
				MethodInfo info;
				if (typeCache.TryGetValue(methodSig, out info))
				{
					if (info.ReturnType == typeof(void))
					{
						info.Invoke(null, args);
						return default(T);
					}
					return (T)info.Invoke(null, args);
				}
			}

			// Pass a null array to GetMethod as it shortcuts early instead of doing some sanity checks inside GetMethod itself.
			if (methodArgumentTypes.Length == 0)
			{
				methodArgumentTypes = null;
			}

			var methodInfo = type.GetMethod(methodName,
				BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
				null,
				methodArgumentTypes,
				null);

			if (methodInfo == null)
			{
				throw new ArgumentException(
					string.Format("Method '{0}({1})' could not be found on object of type {2}",
						methodName,
						methodArgumentTypes != null ? string.Join(", ", methodArgumentTypes.Select(t => t.Name).ToArray()) : string.Empty,
						type.FullName),
					"methodName");
			}

			if (typeCache == null)
			{
				typeCache = new Dictionary<string, MethodInfo>
				{
					{methodName + "@" + string.Join(",", methodArgumentTypes.Select(t => t.Name).ToArray()), methodInfo}
				};
				_methodCache.Add(type, typeCache);
			}

			// Note: The invokes here are specifically not in a try/catch. The exception will bubble up to the caller so it can be handled there properly,
			// rather than suppressing anything we'd do here.
			if (methodInfo.ReturnType == typeof(void))
			{
				methodInfo.Invoke(null, args);
				return default(T);
			}
			return (T) methodInfo.Invoke(null, args);
		}

		/// <summary>
		///     Invokes an instance method on the specified object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static void Invoke(object instance, string methodName, params object[] args)
		{
			Invoke<int>(instance, methodName, args);
		}

		/// <summary>
		///     Invokes an instance method on the specified object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static T Invoke<T>(object instance, string methodName, params object[] args)
		{
			// Try and find the method via the arguments passed in.
			var methodArgumentTypes = args.Select(a => a.GetType()).ToArray();

			Dictionary<string, MethodInfo> typeCache;
			if (_methodCache.TryGetValue(instance.GetType(), out typeCache))
			{
				// Build the method sig maybe?
				string methodSig = methodName + "@" + string.Join(",", methodArgumentTypes.Select(t => t.Name).ToArray());
				MethodInfo info;
				if (typeCache.TryGetValue(methodSig, out info))
				{
					if (info.ReturnType == typeof(void))
					{
						info.Invoke(instance, args);
						return default(T);
					}
					return (T)info.Invoke(instance, args);
				}
			}

			// Pass a null array to GetMethod as it shortcuts early instead of doing some sanity checks inside GetMethod itself.
			if (methodArgumentTypes.Length == 0)
			{
				methodArgumentTypes = null;
			}

			var methodInfo = instance.GetType().GetMethod(methodName,
				BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
				null,
				methodArgumentTypes,
				null);

			if (methodInfo == null)
			{
				throw new ArgumentException(
					string.Format("Method '{0}({1})' could not be found on object of type {2}",
						methodName,
						methodArgumentTypes != null ? string.Join(", ", methodArgumentTypes.Select(t => t.Name).ToArray()) : string.Empty,
						instance.GetType().FullName),
					"methodName");
			}


			if (typeCache == null)
			{
				typeCache = new Dictionary<string, MethodInfo>
				{
					{methodName + "@" + string.Join(",", methodArgumentTypes.Select(t => t.Name).ToArray()), methodInfo}
				};
				_methodCache.Add(instance.GetType(), typeCache);
			}

			// Note: The invokes here are specifically not in a try/catch. The exception will bubble up to the caller so it can be handled there properly,
			// rather than suppressing anything we'd do here.
			if (methodInfo.ReturnType == typeof(void))
			{
				methodInfo.Invoke(instance, args);
				return default(T);
			}
			return (T) methodInfo.Invoke(instance, args);
		}

		#endregion

		#region Get/SetField

		public static T GetField<T>(Type type, string fieldName)
		{
			Dictionary<string, FieldInfo> typeCache;
			if (_fieldCache.TryGetValue(type, out typeCache))
			{
				FieldInfo info;
				if (typeCache.TryGetValue(fieldName, out info))
				{
					return (T)info.GetValue(null);
				}
			}
			var field = type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (field == null)
			{
				throw new ArgumentException("Field '" + fieldName + "' could not be found on object of type " + type.FullName, "fieldName");
			}


			if (typeCache == null)
			{
				typeCache = new Dictionary<string, FieldInfo>
				{
					{fieldName, field}
				};
				_fieldCache.Add(type, typeCache);
			}

			return (T) field.GetValue(null);
		}

		public static void SetField(Type type, string fieldName, object value)
		{
			Dictionary<string, FieldInfo> typeCache;
			if (_fieldCache.TryGetValue(type, out typeCache))
			{
				FieldInfo info;
				if (typeCache.TryGetValue(fieldName, out info))
				{
					info.SetValue(null, value);
				}
			}
			var field = type.GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (field == null)
			{
				throw new ArgumentException("Field '" + fieldName + "' could not be found on object of type " + type.FullName, "fieldName");
			}


			if (typeCache == null)
			{
				typeCache = new Dictionary<string, FieldInfo>
				{
					{fieldName, field}
				};
				_fieldCache.Add(type, typeCache);
			}

			field.SetValue(null, value);
		}

		public static T GetField<T>(object instance, string fieldName)
		{
			Dictionary<string, FieldInfo> typeCache;
			if (_fieldCache.TryGetValue(instance.GetType(), out typeCache))
			{
				FieldInfo info;
				if (typeCache.TryGetValue(fieldName, out info))
				{
					return (T)info.GetValue(instance);
				}
			}
			var field = instance.GetType().GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (field == null)
			{
				throw new ArgumentException("Field '" + fieldName + "' could not be found on object of type " + instance.GetType().FullName, "fieldName");
			}



			if (typeCache == null)
			{
				typeCache = new Dictionary<string, FieldInfo>
				{
					{fieldName, field}
				};
				_fieldCache.Add(instance.GetType(), typeCache);
			}

			return (T) field.GetValue(instance);
		}

		public static void SetField(object instance, string fieldName, object value)
		{
			Dictionary<string, FieldInfo> typeCache;
			if (_fieldCache.TryGetValue(instance.GetType(), out typeCache))
			{
				FieldInfo info;
				if (typeCache.TryGetValue(fieldName, out info))
				{
					info.SetValue(instance, value);
				}
			}
			var field = instance.GetType().GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (field == null)
			{
				throw new ArgumentException("Field '" + fieldName + "' could not be found on object of type " + instance.GetType().FullName, "fieldName");
			}

			if (typeCache == null)
			{
				typeCache = new Dictionary<string, FieldInfo>
				{
					{fieldName, field}
				};
				_fieldCache.Add(instance.GetType(), typeCache);
			}
			field.SetValue(instance, value);
		}

		#endregion
	}
}