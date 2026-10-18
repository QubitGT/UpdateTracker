using System;

namespace Unity.Collections;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public class GenerateTestsForBurstCompatibilityAttribute : Attribute
{
	public enum BurstCompatibleCompileTarget
	{
		Player,
		Editor,
		PlayerAndEditor
	}

	public string RequiredUnityDefine;

	public BurstCompatibleCompileTarget CompileTarget;

	public Type[] GenericTypeArguments { get; set; }
}
