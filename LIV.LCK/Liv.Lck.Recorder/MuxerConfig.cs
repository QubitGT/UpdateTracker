using System.Runtime.InteropServices;

namespace Liv.Lck.Recorder;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MuxerConfig
{
	[MarshalAs(UnmanagedType.LPStr)]
	public string outputPath;

	public uint videoBitrate;

	public uint audioBitrate;

	public uint width;

	public uint height;

	public uint framerate;

	public uint samplerate;

	public uint channels;

	public uint numberOfTracks;

	[MarshalAs(UnmanagedType.I1)]
	public bool realtimeOutput;
}
