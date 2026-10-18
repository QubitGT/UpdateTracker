using System;
using System.Runtime.InteropServices;
using Liv.Lck.ErrorHandling;

namespace Liv.Lck.Encoding;

internal static class LckNativeEncodingApi
{
	public enum TrackType : uint
	{
		Video,
		Audio,
		Metadata
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void CaptureErrorCallback(CaptureErrorType errorType, [MarshalAs(UnmanagedType.LPStr)] string errorMessage);

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct TrackInfo
	{
		public TrackType type;

		public uint bitrate;

		public uint width;

		public uint height;

		public uint framerate;

		public uint samplerate;

		public uint channels;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct FrameTexture
	{
		public uint id;

		public uint trackIndex;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct AudioTrack
	{
		public uint trackIndex;

		public ulong timestampSamples;

		public uint dataSize;

		public IntPtr data;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct FrameSubmission
	{
		public IntPtr encoderContext;

		public IntPtr textureIDs;

		public uint textureIDsSize;

		public ulong videoTimestampMilli;

		public uint audioTracksSize;

		public IntPtr audioTracks;

		public uint readyFramesSize;

		public IntPtr readyFrames;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct ResourceData
	{
		public IntPtr encoderContext;
	}

	private const string EncodingLib = "lck_rs";

	[DllImport("lck_rs")]
	internal static extern IntPtr CreateEncoder();

	[DllImport("lck_rs")]
	internal static extern void DestroyEncoder(IntPtr encoderContext);

	[DllImport("lck_rs")]
	internal static extern bool StartEncoder(IntPtr encoderContext, [MarshalAs(UnmanagedType.LPArray)] TrackInfo[] tracks, uint tracksCount);

	[DllImport("lck_rs")]
	internal static extern void StopEncoder(IntPtr encoderContext);

	[DllImport("lck_rs")]
	internal static extern void AddEncoderPacketCallback(IntPtr encoderContext, IntPtr objectPtr, IntPtr functionPtr);

	[DllImport("lck_rs")]
	internal static extern void RemoveEncoderPacketCallback(IntPtr encoderContext, IntPtr objectPtr, IntPtr functionPtr);

	[DllImport("lck_rs")]
	internal static extern IntPtr GetResourceContext(IntPtr encoderContext);

	[DllImport("lck_rs")]
	internal static extern IntPtr AllocateFrameSubmission([MarshalAs(UnmanagedType.LPStruct)] FrameSubmission frame, [MarshalAs(UnmanagedType.LPArray)] AudioTrack[] audioTracks, [MarshalAs(UnmanagedType.LPArray)] bool[] readyFrames);

	[DllImport("lck_rs")]
	internal static extern IntPtr GetPluginUpdateFunction();

	[DllImport("lck_rs")]
	internal static extern IntPtr GetInitResourcesFunction();

	[DllImport("lck_rs")]
	internal static extern IntPtr GetReleaseResourcesFunction();

	[DllImport("lck_rs")]
	internal static extern uint GetAudioTrackFrameSize(IntPtr encoderContext, uint track_index);

	[DllImport("lck_rs")]
	internal static extern void SetEncoderLogLevel(IntPtr encoderContext, uint level);

	[DllImport("lck_rs")]
	internal static extern bool SetCaptureErrorCallback(IntPtr encoderContext, CaptureErrorCallback errorCallback);

	[DllImport("lck_rs")]
	internal static extern void SetAllowBFrames(IntPtr encoderContext, bool allowBFrames);
}
