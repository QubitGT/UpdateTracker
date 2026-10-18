using System;
using System.Runtime.InteropServices;
using Liv.Lck.Encoding;
using Liv.NGFX;
using UnityEngine.Scripting;

namespace Liv.Lck.Recorder;

internal class LckNativeRecordingService : ILckNativeRecordingService
{
	private const string RecordingLib = "lck_rs";

	private IntPtr _nativeMuxerContext = IntPtr.Zero;

	private Liv.NGFX.LogLevel _logLevel = Liv.NGFX.LogLevel.Error;

	[DllImport("lck_rs")]
	private static extern IntPtr GetMuxerCallbackFunction();

	[DllImport("lck_rs")]
	private static extern IntPtr CreateMuxer();

	[DllImport("lck_rs")]
	private static extern void DestroyMuxer(IntPtr muxerContext);

	[DllImport("lck_rs")]
	private static extern bool StartMuxer(IntPtr muxerContext, ref MuxerConfig config);

	[DllImport("lck_rs")]
	private static extern void StopMuxer(IntPtr muxerContext);

	[DllImport("lck_rs")]
	private static extern void SetMuxerLogLevel(IntPtr muxerContext, uint level);

	[Preserve]
	public LckNativeRecordingService()
	{
	}

	public bool CreateNativeMuxer()
	{
		_nativeMuxerContext = CreateMuxer();
		if (!HasNativeMuxer())
		{
			return false;
		}
		UpdateNativeMuxerLogLevel();
		return true;
	}

	public void DestroyNativeMuxer()
	{
		if (HasNativeMuxer())
		{
			DestroyMuxer(_nativeMuxerContext);
			_nativeMuxerContext = IntPtr.Zero;
		}
	}

	public bool HasNativeMuxer()
	{
		return _nativeMuxerContext != IntPtr.Zero;
	}

	public bool StartNativeMuxer(ref MuxerConfig config)
	{
		return StartMuxer(_nativeMuxerContext, ref config);
	}

	public bool StopNativeMuxer()
	{
		StopMuxer(_nativeMuxerContext);
		return true;
	}

	public void SetNativeMuxerLogLevel(Liv.NGFX.LogLevel logLevel)
	{
		_logLevel = logLevel;
		if (HasNativeMuxer())
		{
			UpdateNativeMuxerLogLevel();
		}
	}

	public LckEncodedPacketCallback GetMuxPacketCallback()
	{
		return new LckEncodedPacketCallback(_nativeMuxerContext, GetMuxerCallbackFunction());
	}

	private void UpdateNativeMuxerLogLevel()
	{
		SetMuxerLogLevel(_nativeMuxerContext, (uint)_logLevel);
	}
}
