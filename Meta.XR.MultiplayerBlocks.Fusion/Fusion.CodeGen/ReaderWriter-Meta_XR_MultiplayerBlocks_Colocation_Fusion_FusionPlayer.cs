using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Meta.XR.MultiplayerBlocks.Colocation.Fusion;

namespace Fusion.CodeGen;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[WeaverGenerated]
internal struct ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionPlayer : IElementReaderWriter<FusionPlayer>
{
	[WeaverGenerated]
	public static IElementReaderWriter<FusionPlayer> Instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public unsafe FusionPlayer Read(byte* data, int index)
	{
		return *(FusionPlayer*)(data + index * 20);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public unsafe ref FusionPlayer ReadRef(byte* data, int index)
	{
		return ref *(FusionPlayer*)(data + index * 20);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public unsafe void Write(byte* data, int index, FusionPlayer val)
	{
		*(FusionPlayer*)(data + index * 20) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public int GetElementWordCount()
	{
		return 5;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public int GetElementHashCode(FusionPlayer val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public static IElementReaderWriter<FusionPlayer> GetInstance()
	{
		if (Instance == null)
		{
			Instance = default(ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionPlayer);
		}
		return Instance;
	}
}
