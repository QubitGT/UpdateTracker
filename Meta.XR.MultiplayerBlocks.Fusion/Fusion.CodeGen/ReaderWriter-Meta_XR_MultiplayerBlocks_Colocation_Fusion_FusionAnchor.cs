using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Meta.XR.MultiplayerBlocks.Colocation.Fusion;

namespace Fusion.CodeGen;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[WeaverGenerated]
internal struct ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionAnchor : IElementReaderWriter<FusionAnchor>
{
	[WeaverGenerated]
	public static IElementReaderWriter<FusionAnchor> Instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public unsafe FusionAnchor Read(byte* data, int index)
	{
		return *(FusionAnchor*)(data + index * 280);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public unsafe ref FusionAnchor ReadRef(byte* data, int index)
	{
		return ref *(FusionAnchor*)(data + index * 280);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public unsafe void Write(byte* data, int index, FusionAnchor val)
	{
		*(FusionAnchor*)(data + index * 280) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public int GetElementWordCount()
	{
		return 70;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public int GetElementHashCode(FusionAnchor val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[WeaverGenerated]
	public static IElementReaderWriter<FusionAnchor> GetInstance()
	{
		if (Instance == null)
		{
			Instance = default(ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionAnchor);
		}
		return Instance;
	}
}
