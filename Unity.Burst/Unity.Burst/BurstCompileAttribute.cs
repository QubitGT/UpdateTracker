using System;

namespace Unity.Burst;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
public class BurstCompileAttribute : Attribute
{
	internal bool? _compileSynchronously;

	internal bool? _debug;

	internal bool? _disableSafetyChecks;

	internal bool? _disableDirectCall;

	public FloatMode FloatMode { get; set; }

	public FloatPrecision FloatPrecision { get; set; }

	public bool CompileSynchronously
	{
		get
		{
			if (!_compileSynchronously.HasValue)
			{
				return false;
			}
			return _compileSynchronously.Value;
		}
		set
		{
			_compileSynchronously = value;
		}
	}

	public bool Debug
	{
		get
		{
			if (!_debug.HasValue)
			{
				return false;
			}
			return _debug.Value;
		}
		set
		{
			_debug = value;
		}
	}

	public bool DisableSafetyChecks
	{
		get
		{
			if (!_disableSafetyChecks.HasValue)
			{
				return false;
			}
			return _disableSafetyChecks.Value;
		}
		set
		{
			_disableSafetyChecks = value;
		}
	}

	public bool DisableDirectCall
	{
		get
		{
			if (!_disableDirectCall.HasValue)
			{
				return false;
			}
			return _disableDirectCall.Value;
		}
		set
		{
			_disableDirectCall = value;
		}
	}

	public OptimizeFor OptimizeFor { get; set; }

	internal string[] Options { get; set; }

	public BurstCompileAttribute()
	{
	}

	public BurstCompileAttribute(FloatPrecision floatPrecision, FloatMode floatMode)
	{
		FloatMode = floatMode;
		FloatPrecision = floatPrecision;
	}

	internal BurstCompileAttribute(string[] options)
	{
		Options = options;
	}
}
