using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Modio.Mods;
using Modio.Users;

namespace Modio.FileIO;

public interface IModioDataStorage
{
	Task<Error> Init();

	Task Shutdown();

	Task<Error> DeleteAllGameData();

	Task<(Error error, GameData result)> ReadGameData();

	Task<Error> WriteGameData(GameData gameData);

	Task<Error> DeleteGameData();

	Task<(Error error, ModIndex index)> ReadIndexData();

	Task<Error> WriteIndexData(ModIndex index);

	Task<Error> DeleteIndexData();

	Task<(Error error, UserSaveObject[] results)> ReadAllSavedUserData();

	Task<(Error error, UserSaveObject result)> ReadUserData(string localUserId);

	Task<(Error error, LegacyUserSaveObject result)> ReadLegacyUserData(string localUserId);

	Task<Error> WriteUserData(UserSaveObject userObject);

	Task<Error> DeleteUserData(string localUserId);

	Error DeleteLegacyUserData();

	Task<Error> DownloadModFileFromStream(long modId, long modfileId, Stream downloadStream, string md5Hash, CancellationToken token, ModInstallProgressTracker progressTracker = null);

	Task<Error> DeleteModfile(long modId, long modfileId);

	Task<(Error error, List<(long modId, long modfileId)> results)> ScanForModfiles();

	Task<Error> InstallMod(Mod mod, long modfileId, CancellationToken token);

	Task<Error> InstallModFromStream(Mod mod, long modfileId, Stream stream, string md5Hash, CancellationToken token);

	Task<Error> DeleteInstalledMod(Mod mod, long modfileId);

	Task<(Error error, List<(long modId, long modfileId)> results)> ScanForInstalledMods();

	Task<(Error error, bool installed, long modfileId)> ScanForInstalledMod(Mod mod);

	Task<(Error error, byte[] result)> ReadCachedImage(Uri serverPath);

	Task<Error> WriteCachedImage(Uri serverPath, byte[] data);

	Task<Error> DeleteCachedImage(Uri serverPath);

	Task<bool> IsThereAvailableFreeSpaceFor(long tempBytes, long persistentBytes);

	Task<bool> IsThereAvailableFreeSpaceForModfile(long bytes);

	Task<long> GetAvailableFreeSpaceForModfile();

	Task<bool> IsThereAvailableFreeSpaceForModInstall(long bytes);

	Task<long> GetAvailableFreeSpaceForModInstall();

	string GetModfilePath(long modId, long modfileId);

	string GetInstallPath(long modId, long modfileId);

	bool DoesModfileExist(long modId, long modfileId);

	bool DoesInstallExist(long modId, long modfileId);

	Task<Error> CompressToZip(string filePath, Stream outputTo);
}
