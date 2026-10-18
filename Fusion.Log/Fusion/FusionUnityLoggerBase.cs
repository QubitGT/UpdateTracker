using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Fusion;

public abstract class FusionUnityLoggerBase : IDisposable
{
	protected internal readonly struct LogContext
	{
		public readonly string Message;

		public readonly ILogSource Source;

		public readonly string Prefix;

		public readonly LogFlags Flags;

		public LogContext(string message, string prefix, ILogSource source, LogFlags flags)
		{
			Message = message;
			Source = source;
			Prefix = prefix;
			Flags = flags;
		}
	}

	private readonly Thread _mainThread;

	private readonly StringBuilder _mainThreadBuilder = new StringBuilder();

	private readonly ThreadLocal<StringBuilder> _threadedStringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());

	public bool AddHashCodePrefix;

	public string GlobalPrefix = "Fusion";

	public string GlobalPrefixColor;

	public Color32 MaxRandomColor;

	public Color32 MinRandomColor;

	public string NameUnavailableInWorkerThreadLabel = "";

	public string NameUnavailableObjectDestroyedLabel = "(destroyed)";

	public bool UseColorTags;

	public bool UseGlobalPrefix;

	public string DebugPrefix = "[DEBUG] ";

	public string TracePrefix = "[TRACE] ";

	private bool IsInMainThread => _mainThread == Thread.CurrentThread;

	public static Color DefaultLightPrefixColor => new Color32(20, 64, 120, byte.MaxValue);

	public static Color DefaultDarkPrefixColor => new Color32(115, 172, 229, byte.MaxValue);

	public FusionUnityLoggerBase(Thread mainThread = null, bool isDarkMode = false)
	{
		_mainThread = mainThread ?? Thread.CurrentThread;
		MinRandomColor = (isDarkMode ? new Color32(158, 158, 158, byte.MaxValue) : new Color32(30, 30, 30, byte.MaxValue));
		MaxRandomColor = (isDarkMode ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(90, 90, 90, byte.MaxValue));
		UseColorTags = true;
		UseGlobalPrefix = true;
		GlobalPrefixColor = Color32ToRGBString(isDarkMode ? DefaultDarkPrefixColor : DefaultLightPrefixColor);
	}

	public void Dispose()
	{
		_threadedStringBuilder.Dispose();
	}

	public LogStream CreateLogStream(LogLevel logLevel, LogFlags flags, TraceChannels channel)
	{
		return new UnityLogStream(this, logLevel, channel, flags);
	}

	protected internal virtual (string, UnityEngine.Object) CreateMessage(in LogContext context)
	{
		bool isMainThread;
		StringBuilder threadSafeStringBuilder = GetThreadSafeStringBuilder(out isMainThread);
		UnityEngine.Object @object = context.Source?.GetUnityObject();
		try
		{
			AppendPrefix(threadSafeStringBuilder, context.Flags, context.Prefix);
			if (@object != null)
			{
				int length = threadSafeStringBuilder.Length;
				AppendNameThreadSafe(threadSafeStringBuilder, @object);
				if (threadSafeStringBuilder.Length > length)
				{
					threadSafeStringBuilder.Append(": ");
				}
			}
			threadSafeStringBuilder.Append(context.Message);
			return (threadSafeStringBuilder.ToString(), isMainThread ? @object : null);
		}
		finally
		{
			threadSafeStringBuilder.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected StringBuilder GetThreadSafeStringBuilder(out bool isMainThread)
	{
		isMainThread = IsInMainThread;
		if (!IsInMainThread)
		{
			return _threadedStringBuilder.Value;
		}
		return _mainThreadBuilder;
	}

	protected void AppendPrefix(StringBuilder sb, LogFlags flags, string prefix)
	{
		if (UseGlobalPrefix)
		{
			if (UseColorTags)
			{
				sb.Append("<color=");
				sb.Append(GlobalPrefixColor);
				sb.Append(">");
			}
			sb.Append("[");
			sb.Append(GlobalPrefix);
			if (!string.IsNullOrEmpty(prefix))
			{
				sb.Append("/");
				sb.Append(prefix);
			}
			sb.Append("]");
			if (UseColorTags)
			{
				sb.Append("</color>");
			}
			sb.Append(" ");
		}
		else if (!string.IsNullOrEmpty(prefix))
		{
			sb.Append(prefix);
			sb.Append(": ");
		}
		if ((flags & LogFlags.Debug) == LogFlags.Debug)
		{
			sb.Append(DebugPrefix);
		}
		if ((flags & LogFlags.Trace) == LogFlags.Trace)
		{
			sb.Append(TracePrefix);
		}
	}

	public void AppendNameThreadSafe(StringBuilder builder, UnityEngine.Object obj)
	{
		if ((object)obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		string text = ((obj == null) ? NameUnavailableObjectDestroyedLabel : (IsInMainThread ? obj.name : NameUnavailableInWorkerThreadLabel));
		if (UseColorTags)
		{
			builder.AppendFormat("<color=#{0:X6}>", GetColorFromHash(text));
		}
		if (AddHashCodePrefix)
		{
			builder.AppendFormat("{0:X8}", obj.GetHashCode());
		}
		if (text != null && text.Length > 0)
		{
			if (AddHashCodePrefix)
			{
				builder.Append(" ");
			}
			builder.Append(text);
		}
		if (UseColorTags)
		{
			builder.Append("</color>");
		}
	}

	private int GetColorFromHash(string name)
	{
		int num = 0;
		for (int i = 0; i < name.Length; i++)
		{
			num = num * 31 + name[i];
		}
		return GetRandomColor(num, MinRandomColor, MaxRandomColor);
	}

	private static int GetRandomColor(int seed, Color32 min, Color32 max)
	{
		ulong x2 = (ulong)seed;
		ulong num = NextSplitMix64(ref x2);
		uint num2 = (uint)(max.r - min.r + 1);
		uint num3 = (uint)(max.g - min.g + 1);
		uint num4 = (uint)(max.b - min.b + 1);
		uint num5 = (uint)(num % num2);
		ulong num6 = num / num2;
		uint num7 = (uint)(num6 % num3);
		uint num8 = (uint)(num6 / num3 % num4);
		return (int)((num5 << 16) | (num7 << 8) | num8);
		static ulong NextSplitMix64(ref ulong x)
		{
			ulong num9 = (x += 11400714819323198485uL);
			long num10 = (long)(num9 ^ (num9 >> 30)) * -4658895280553007687L;
			long num11 = (num10 ^ (num10 >>> 27)) * -7723592293110705685L;
			return (ulong)num11 ^ ((ulong)num11 >> 31);
		}
	}

	private static int Color32ToRGB24(Color32 c)
	{
		return (c.r << 16) | (c.g << 8) | c.b;
	}

	private static string Color32ToRGBString(Color32 c)
	{
		return $"#{Color32ToRGB24(c):X6}";
	}
}
