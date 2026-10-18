using System;

namespace Liv.Lck.Core.FFI;

internal struct GameInfo
{
	public IntPtr GameName;

	public IntPtr GameVersion;

	public IntPtr ProjectName;

	public IntPtr CompanyName;

	public IntPtr EngineVersion;

	public IntPtr RenderPipeline;

	public IntPtr GraphicsAPI;

	public IntPtr Platform;

	public IntPtr PersistentDataPath;

	public IntPtr InteractionSystems;

	public static GameInfo AllocateFromGameInfo(Liv.Lck.Core.GameInfo gameInfo)
	{
		return new GameInfo(gameInfo);
	}

	public void Free()
	{
		InteropUtilities.Free(GameName);
		InteropUtilities.Free(GameVersion);
		InteropUtilities.Free(ProjectName);
		InteropUtilities.Free(CompanyName);
		InteropUtilities.Free(EngineVersion);
		InteropUtilities.Free(RenderPipeline);
		InteropUtilities.Free(GraphicsAPI);
		InteropUtilities.Free(Platform);
		InteropUtilities.Free(PersistentDataPath);
		InteropUtilities.Free(InteractionSystems);
	}

	private GameInfo(Liv.Lck.Core.GameInfo gameInfo)
	{
		GameName = InteropUtilities.StringToUTF8Pointer(gameInfo.GameName);
		GameVersion = InteropUtilities.StringToUTF8Pointer(gameInfo.GameVersion);
		ProjectName = InteropUtilities.StringToUTF8Pointer(gameInfo.ProjectName);
		CompanyName = InteropUtilities.StringToUTF8Pointer(gameInfo.CompanyName);
		EngineVersion = InteropUtilities.StringToUTF8Pointer(gameInfo.EngineVersion);
		RenderPipeline = InteropUtilities.StringToUTF8Pointer(gameInfo.RenderPipeline);
		GraphicsAPI = InteropUtilities.StringToUTF8Pointer(gameInfo.GraphicsAPI);
		Platform = InteropUtilities.StringToUTF8Pointer(gameInfo.Platform);
		PersistentDataPath = InteropUtilities.StringToUTF8Pointer(gameInfo.PersistentDataPath);
		InteractionSystems = InteropUtilities.StringToUTF8Pointer(gameInfo.InteractionSystems);
	}
}
