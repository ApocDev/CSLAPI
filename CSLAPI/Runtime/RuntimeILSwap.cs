//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Security;

//namespace CSLAPI.Runtime
//{
//	internal unsafe class RuntimeILSwap
//	{
//		private static _mono_runtime_free_method_Delegate mono_runtime_free_method;

//		public RuntimeILSwap()
//		{
//			var hMono = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().FirstOrDefault(m => m.ModuleName == "mono.dll").BaseAddress;
//			var baseAddr = 0x76FAC;

//			mono_runtime_free_method =
//				Marshal.GetDelegateForFunctionPointer(new IntPtr(hMono.ToInt64() + baseAddr), typeof(_mono_runtime_free_method_Delegate)) as _mono_runtime_free_method_Delegate;
//		}

//		[DllImport("mono.dll", CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.FastCall)]
//		[SuppressUnmanagedCodeSecurity]
//		private static extern IntPtr mono_method_get_header(IntPtr method);

//		[DllImport("mono.dll", CallingConvention = CallingConvention.FastCall, ExactSpelling = true)]
//		[SuppressUnmanagedCodeSecurity]
//		private static extern IntPtr mono_domain_get();

//		#region Nested type: _mono_runtime_free_method_Delegate

//		[UnmanagedFunctionPointer(CallingConvention.FastCall)]
//		internal delegate void _mono_runtime_free_method_Delegate(IntPtr domain, IntPtr method);

//		#endregion

//		#region Nested type: MonoMethodHeader

//		[StructLayout(LayoutKind.Sequential)]
//		private struct MonoMethodHeader
//		{
//			public readonly byte* code;
//			public readonly uint code_size;
//			public readonly int bitvector1;
//		}

//		#endregion

//		public void ReplaceIL(MethodInfo targetMethod, byte[] newiLBytes, int newMaxStack = -1)
//		{
//			// TODO: Some IL parsing to make it easier to deal with the locals count maybe?
//			// Just offers mainly debugging support

//			// get the old header
//			var targetHeader = (MonoMethodHeader*)mono_method_get_header(targetMethod.MethodHandle.Value);

//			// TODO: Free the original code bytes
//			// This will result in a mem leak if we do a ton of replacements
//			var newCodePtr = Marshal.AllocHGlobal(newiLBytes.Length + 5);
//			Marshal.Copy(newiLBytes, 0, newCodePtr, newiLBytes.Length);
//			targetHeader->code = (byte*)newCodePtr;
//			targetHeader->code_size = (uint)newiLBytes.Length;
//			if (newMaxStack != -1)
//			{
//				targetHeader->bitvector1 = newMaxStack & 0x7FFF;
//			}

//			// Free the target method, so we can recompile with new IL
//			mono_runtime_free_method(mono_domain_get(), targetMethod.MethodHandle.GetFunctionPointer());

//			// Force-compile the method
//			// Plain old .NET support here.
//			RuntimeHelpers.PrepareMethod(targetMethod.MethodHandle);
//			targetMethod.MethodHandle.GetFunctionPointer();
//		}
//	}
//}