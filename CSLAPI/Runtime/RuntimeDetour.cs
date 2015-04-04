using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSLAPI.Runtime
{
	/// <summary>
	/// Enables detouring of pure .NET functions at runtime.
	/// </summary>
	/// <remarks>
	/// When detouring methods, the method signatures must match fully. Generic methods are not fully supported yet.
	/// </remarks>
	public class RuntimeDetour : IDisposable
	{
		/*
		 TODO:
		 * Add proper disasm so we can insert detours at the start/end/middle/wherever of the function
		 * Maybe add some simple pattern-based detouring. (Steal PatternFinder from GreyMagic and use that)		 
		 */


		/// <summary>
		///     Initializes a new instance of RuntimeDetour
		/// </summary>
		/// <param name="target">The method that will be detoured</param>
		/// <param name="detour">
		///     The method that will be called in place of the
		///     <param name="target"> method.</param>
		/// </param>
		public RuntimeDetour(MethodInfo target, MethodInfo detour)
		{
			if (!ValidateMethodSignatures(target, detour))
			{
				throw new ArgumentException("Target and Detour method signatures must match.");
			}

			ForceCompileMethod(target);
			ForceCompileMethod(detour);

			TargetMethod = target;
			DetourMethod = detour;
		}

		public bool IsApplied { get; protected set; }
		protected byte[] OriginalBytes { get; set; }
		public MethodInfo TargetMethod { get; protected set; }
		public MethodInfo DetourMethod { get; protected set; }
		protected IntPtr TargetPointer { get { return TargetMethod.MethodHandle.GetFunctionPointer(); } }
		protected IntPtr DetourPointer { get { return DetourMethod.MethodHandle.GetFunctionPointer(); } }
		public bool IsDisposed { get; protected set; }

		#region IDisposable Members

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		#endregion

		protected bool ValidateMethodSignatures(MethodInfo m1, MethodInfo m2)
		{
			// Generic method must be detoured by a similar generic method.
			if (m1.IsGenericMethod != m2.IsGenericMethod)
			{
				return false;
			}

			// Must return the same types
			if (m1.ReturnType != m2.ReturnType)
			{
				return false;
			}

			// Parameters *must* match
			var m1Params = m1.GetParameters().ToList();
			var m2Params = m2.GetParameters().ToList();

			if (!m1Params.SequenceEqual(m2Params))
			{
				return false;
			}

			// TODO: More validation

			return true;
		}

		protected void ForceCompileMethod(MethodInfo method)
		{
			// .NET Support (Mono nops this call)
			RuntimeHelpers.PrepareMethod(method.MethodHandle);
			// Mono Support (Mono actually does JIT here)
			method.MethodHandle.GetFunctionPointer();
		}

		/// <summary>
		///     Applies this runtime detour.
		/// </summary>
		public virtual unsafe void Apply()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("RuntimeDetour");
			}
			if (IsApplied)
			{
				return;
			}

			byte[] origBytes = new byte[13];
			// Note: Marshal is incredibly slow in general. For full-blown production code
			// This should be avoided at all costs.
			Marshal.Copy(TargetPointer, origBytes, 0, origBytes.Length);
			OriginalBytes = origBytes;

			// TODO: jmp qword [funcPtr]
			// 5 byte patch to just far jump
			// Assume x64 shadow space stack is alloc'd correctly for locals
			// Also assume that the incoming calling convention is proper.
			//target[0] = 0xEC; // 0xEA maybe?
			//*((IntPtr*)&target[1]) = detourMethod.MethodHandle.GetFunctionPointer();

			// mono itself uses the r11 register (which isn't good on Windows due to syscall stuff)
			// So we'll just move to r11, and jmp to r11
			// TODO: MonoMethod code pointer swap so we can do proper detours
			// Use the hooking system otherwise.
			// TODO: Generate code caves so we can do rel jumps back and forth for proper hooks.

			byte* target = (byte*) TargetPointer;
			// mov r11, funcptr
			target[0] = 0x49;
			target[1] = 0xBB;
			*((IntPtr*) &target[2]) = DetourPointer;

			// jmp r11
			target[10] = 0x41;
			target[11] = 0xFF;
			target[12] = 0xE3;

			IsApplied = true;
		}

		/// <summary>
		///     Removes this runtime detour.
		/// </summary>
		public virtual void Remove()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("RuntimeDetour");
			}

			if (!IsApplied)
			{
				return;
			}

			// Note: Marshal is incredibly slow in general. For full-blown production code
			// This should be avoided at all costs.
			Marshal.Copy(OriginalBytes, 0, TargetPointer, OriginalBytes.Length);
			IsApplied = false;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (IsApplied)
				Remove();

			if (disposing)
			{
				GC.SuppressFinalize(this);
				IsDisposed = true;
			}
			else
			{
#if DEBUG
				if (!IsDisposed)
				{
					throw new InvalidOperationException("A RuntimeDetour is being finalized before it has been disposed correctly.");
				}
#endif
			}
		}

		~RuntimeDetour()
		{
			Dispose(false);
		}
	}
}