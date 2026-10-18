using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Unity.Burst.LowLevel;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Burst;

public static class BurstCompiler
{
	private class CommandBuilder
	{
		private StringBuilder _builder;

		private bool _hasArgs;

		public CommandBuilder()
		{
			_builder = new StringBuilder();
			_hasArgs = false;
		}

		public CommandBuilder Begin(string cmd)
		{
			_builder.Clear();
			_hasArgs = false;
			_builder.Append(cmd);
			return this;
		}

		public CommandBuilder With(string arg)
		{
			if (!_hasArgs)
			{
				_builder.Append(' ');
			}
			_hasArgs = true;
			_builder.Append(arg);
			return this;
		}

		public CommandBuilder With(IntPtr arg)
		{
			if (!_hasArgs)
			{
				_builder.Append(' ');
			}
			_hasArgs = true;
			_builder.AppendFormat("0x{0:X16}", arg.ToInt64());
			return this;
		}

		public CommandBuilder And(char sep = '|')
		{
			_builder.Append(sep);
			return this;
		}

		public string SendToCompiler()
		{
			return SendRawCommandToCompiler(_builder.ToString());
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal class StaticTypeReinitAttribute : Attribute
	{
		public readonly Type reinitType;

		public StaticTypeReinitAttribute(Type toReinit)
		{
			reinitType = toReinit;
		}
	}

	[BurstCompile]
	internal static class BurstCompilerHelper
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate bool IsBurstEnabledDelegate();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate bool IsBurstEnabled_00000145_0024PostfixBurstDelegate();

		internal static class IsBurstEnabled_00000145_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = CompileFunctionPointer<IsBurstEnabled_00000145_0024PostfixBurstDelegate>(IsBurstEnabled).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static bool Invoke()
			{
				if (IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						return ((delegate* unmanaged[Cdecl]<bool>)functionPointer)();
					}
				}
				return IsBurstEnabled_0024BurstManaged();
			}
		}

		private static readonly IsBurstEnabledDelegate IsBurstEnabledImpl = IsBurstEnabled;

		public static readonly bool IsBurstGenerated = IsCompiledByBurst(IsBurstEnabledImpl);

		[BurstCompile]
		[MonoPInvokeCallback(typeof(IsBurstEnabledDelegate))]
		private static bool IsBurstEnabled()
		{
			return IsBurstEnabled_00000145_0024BurstDirectCall.Invoke();
		}

		[BurstDiscard]
		private static void DiscardedMethod(ref bool value)
		{
			value = false;
		}

		private unsafe static bool IsCompiledByBurst(Delegate del)
		{
			return BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(BurstCompilerService.CompileAsyncDelegateMethod(del, string.Empty)) != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(IsBurstEnabledDelegate))]
		internal static bool IsBurstEnabled_0024BurstManaged()
		{
			bool value = true;
			DiscardedMethod(ref value);
			return value;
		}
	}

	private class FakeDelegate
	{
		[Preserve]
		public MethodInfo Method { get; }

		public FakeDelegate(MethodInfo method)
		{
			Method = method;
		}
	}

	[ThreadStatic]
	private static CommandBuilder _cmdBuilder;

	internal static bool _IsEnabled;

	public static readonly BurstCompilerOptions Options = new BurstCompilerOptions(isGlobal: true);

	internal static Action OnCompileILPPMethod2;

	private static readonly MethodInfo DummyMethodInfo = typeof(BurstCompiler).GetMethod("DummyMethod", BindingFlags.Static | BindingFlags.NonPublic);

	public static bool IsEnabled
	{
		get
		{
			if (_IsEnabled)
			{
				return BurstCompilerHelper.IsBurstGenerated;
			}
			return false;
		}
	}

	public static bool IsLoadAdditionalLibrarySupported()
	{
		return IsApiAvailable("LoadBurstLibrary");
	}

	private static CommandBuilder BeginCompilerCommand(string cmd)
	{
		if (_cmdBuilder == null)
		{
			_cmdBuilder = new CommandBuilder();
		}
		return _cmdBuilder.Begin(cmd);
	}

	public static void SetExecutionMode(BurstExecutionEnvironment mode)
	{
		BurstCompilerService.SetCurrentExecutionMode((uint)mode);
	}

	public static BurstExecutionEnvironment GetExecutionMode()
	{
		return (BurstExecutionEnvironment)BurstCompilerService.GetCurrentExecutionMode();
	}

	internal unsafe static T CompileDelegate<T>(T delegateMethod, bool deterministicCompilation = false) where T : class
	{
		return (T)(object)Marshal.GetDelegateForFunctionPointer((IntPtr)Compile(delegateMethod, isFunctionPointer: false, deterministicCompilation), delegateMethod.GetType());
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void VerifyDelegateIsNotMulticast<T>(T delegateMethod) where T : class
	{
		if ((delegateMethod as Delegate).GetInvocationList().Length > 1)
		{
			throw new InvalidOperationException($"Burst does not support multicast delegates, please use a regular delegate for `{delegateMethod}'");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void VerifyDelegateHasCorrectUnmanagedFunctionPointerAttribute<T>(T delegateMethod) where T : class
	{
		UnmanagedFunctionPointerAttribute customAttribute = delegateMethod.GetType().GetCustomAttribute<UnmanagedFunctionPointerAttribute>();
		if (customAttribute == null || customAttribute.CallingConvention != CallingConvention.Cdecl)
		{
			UnityEngine.Debug.LogWarning("The delegate type " + delegateMethod.GetType().FullName + " should be decorated with [UnmanagedFunctionPointer(CallingConvention.Cdecl)] to ensure runtime interoperabilty between managed code and Burst-compiled code.");
		}
	}

	[Obsolete("This method will be removed in a future version of Burst")]
	public static IntPtr CompileILPPMethod(RuntimeMethodHandle burstMethodHandle, RuntimeMethodHandle managedMethodHandle, RuntimeTypeHandle delegateTypeHandle)
	{
		throw new NotImplementedException();
	}

	public unsafe static IntPtr CompileILPPMethod2(RuntimeMethodHandle burstMethodHandle)
	{
		if (burstMethodHandle.Value == IntPtr.Zero)
		{
			throw new ArgumentNullException("burstMethodHandle");
		}
		OnCompileILPPMethod2?.Invoke();
		MethodInfo methodInfo = (MethodInfo)MethodBase.GetMethodFromHandle(burstMethodHandle);
		return (IntPtr)Compile(new FakeDelegate(methodInfo), methodInfo, isFunctionPointer: true, isILPostProcessing: true);
	}

	[Obsolete("This method will be removed in a future version of Burst")]
	public unsafe static void* GetILPPMethodFunctionPointer(IntPtr ilppMethod)
	{
		throw new NotImplementedException();
	}

	public unsafe static void* GetILPPMethodFunctionPointer2(IntPtr ilppMethod, RuntimeMethodHandle managedMethodHandle, RuntimeTypeHandle delegateTypeHandle)
	{
		if (managedMethodHandle.Value == IntPtr.Zero)
		{
			throw new ArgumentNullException("managedMethodHandle");
		}
		if (delegateTypeHandle.Value == IntPtr.Zero)
		{
			throw new ArgumentNullException("delegateTypeHandle");
		}
		if (ilppMethod == IntPtr.Zero)
		{
			GetManagedFallbackDelegate(out var managedFallbackDelegate2, out var _);
			return (void*)Marshal.GetFunctionPointerForDelegate(managedFallbackDelegate2);
		}
		return ilppMethod.ToPointer();
		void GetManagedFallbackDelegate(out Delegate managedFallbackDelegate, out GCHandle gcHandle)
		{
			MethodInfo method = (MethodInfo)MethodBase.GetMethodFromHandle(managedMethodHandle);
			Type typeFromHandle = Type.GetTypeFromHandle(delegateTypeHandle);
			managedFallbackDelegate = Delegate.CreateDelegate(typeFromHandle, method);
			gcHandle = GCHandle.Alloc(managedFallbackDelegate);
		}
	}

	[Obsolete("This method will be removed in a future version of Burst")]
	public unsafe static void* CompileUnsafeStaticMethod(RuntimeMethodHandle handle)
	{
		throw new NotImplementedException();
	}

	public unsafe static FunctionPointer<T> CompileFunctionPointer<T>(T delegateMethod) where T : class
	{
		return new FunctionPointer<T>(new IntPtr(Compile(delegateMethod, isFunctionPointer: true)));
	}

	private unsafe static void* Compile(object delegateObj, bool isFunctionPointer, bool deterministicCompilation = false)
	{
		if (!(delegateObj is Delegate))
		{
			throw new ArgumentException("object instance must be a System.Delegate", "delegateObj");
		}
		Delegate @delegate = (Delegate)delegateObj;
		return Compile(@delegate, @delegate.Method, isFunctionPointer, isILPostProcessing: false, deterministicCompilation);
	}

	private unsafe static void* Compile(object delegateObj, MethodInfo methodInfo, bool isFunctionPointer, bool isILPostProcessing, bool deterministicCompilation = false)
	{
		if (delegateObj == null)
		{
			throw new ArgumentNullException("delegateObj");
		}
		if (delegateObj.GetType().IsGenericType)
		{
			throw new InvalidOperationException($"The delegate type `{delegateObj.GetType()}` must be a non-generic type");
		}
		if (!methodInfo.IsStatic)
		{
			throw new InvalidOperationException($"The method `{methodInfo}` must be static. Instance methods are not supported");
		}
		if (methodInfo.IsGenericMethod)
		{
			throw new InvalidOperationException($"The method `{methodInfo}` must be a non-generic method");
		}
		Delegate @delegate = null;
		if (!isILPostProcessing)
		{
			@delegate = delegateObj as Delegate;
		}
		Delegate delegate2 = delegateObj as Delegate;
		if (BurstCompilerOptions.HasBurstCompileAttribute(methodInfo))
		{
			void* ptr = null;
			if (Options.EnableBurstCompilation && BurstCompilerHelper.IsBurstGenerated)
			{
				if (isFunctionPointer && methodInfo.Name.EndsWith("$BurstManaged"))
				{
					delegateObj = methodInfo.DeclaringType.GetMethod(methodInfo.Name.Replace("$BurstManaged", ""), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).CreateDelegate(delegate2.GetType());
				}
				ptr = BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(BurstCompilerService.CompileAsyncDelegateMethod(delegateObj, string.Empty));
			}
			if (ptr == null)
			{
				if (isILPostProcessing)
				{
					return null;
				}
				GCHandle.Alloc(@delegate);
				ptr = (void*)Marshal.GetFunctionPointerForDelegate(@delegate);
			}
			if (ptr == null)
			{
				throw new InvalidOperationException($"Burst failed to compile the function pointer `{methodInfo}`");
			}
			return ptr;
		}
		throw new InvalidOperationException($"Burst cannot compile the function pointer `{methodInfo}` because the `[BurstCompile]` attribute is missing");
	}

	internal static void Shutdown()
	{
	}

	internal static void Cancel()
	{
	}

	internal static bool IsCurrentCompilationDone()
	{
		return true;
	}

	internal static void Enable()
	{
	}

	internal static void Disable()
	{
	}

	internal static bool IsHostEditorArm()
	{
		return false;
	}

	internal static void TriggerUnsafeStaticMethodRecompilation()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			foreach (Attribute item in from x in assemblies[i].GetCustomAttributes()
				where x.GetType().FullName == "Unity.Burst.BurstCompiler+StaticTypeReinitAttribute"
				select x)
			{
				(item as StaticTypeReinitAttribute).reinitType.GetMethod("Constructor", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[0]);
			}
		}
	}

	internal static void TriggerRecompilation()
	{
	}

	internal static void UnloadAdditionalLibraries()
	{
		SendCommandToCompiler("$unload_burst_natives");
	}

	internal static bool IsApiAvailable(string apiName)
	{
		return SendCommandToCompiler("$is_native_api_available", apiName) == "True";
	}

	internal static int RequestSetProtocolVersion(int version)
	{
		string text = SendCommandToCompiler("$request_set_protocol_version_editor", $"{version}");
		if (string.IsNullOrEmpty(text) || !int.TryParse(text, out var result))
		{
			result = 0;
		}
		SendCommandToCompiler("$set_protocol_version_burst", $"{result}");
		return result;
	}

	internal static void Initialize(string[] assemblyFolders, string[] ignoreAssemblies)
	{
	}

	internal static void NotifyCompilationStarted(string[] assemblyFolders, string[] ignoreAssemblies)
	{
	}

	internal static void NotifyAssemblyCompilationNotRequired(string assemblyName)
	{
	}

	internal static void NotifyAssemblyCompilationFinished(string assemblyName, string[] defines)
	{
	}

	internal static void NotifyCompilationFinished()
	{
	}

	internal static string AotCompilation(string[] assemblyFolders, string[] assemblyRoots, string options)
	{
		return "failed";
	}

	internal static void SetProfilerCallbacks()
	{
	}

	private static string SendRawCommandToCompiler(string command)
	{
		string disassembly = BurstCompilerService.GetDisassembly(DummyMethodInfo, command);
		if (!string.IsNullOrEmpty(disassembly))
		{
			return disassembly.TrimStart('\n');
		}
		return "";
	}

	private static string SendCommandToCompiler(string commandName, string commandArgs = null)
	{
		if (commandName == null)
		{
			throw new ArgumentNullException("commandName");
		}
		if (commandArgs == null)
		{
			return SendRawCommandToCompiler(commandName);
		}
		return BeginCompilerCommand(commandName).With(commandArgs).SendToCompiler();
	}

	private static void DummyMethod()
	{
	}
}
