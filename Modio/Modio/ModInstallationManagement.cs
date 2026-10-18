using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Caching;
using Modio.Errors;
using Modio.FileIO;
using Modio.Mods;
using Modio.Settings;
using Modio.Users;
using UnityEngine;

namespace Modio;

public static class ModInstallationManagement
{
	public delegate void InstallationManagementEventDelegate(Mod mod, Modfile modfile, OperationType jobType, OperationPhase jobPhase);

	private abstract class Job
	{
		private readonly CancellationTokenSource _cancellationTokenSource;

		public Func<Task<Error>> Operation;

		public readonly OperationType Type;

		private Mod _mod;

		protected readonly CancellationToken CancellationToken;

		protected OperationPhase Phase;

		public Mod Mod
		{
			get
			{
				return _mod;
			}
			private set
			{
				_mod = value;
			}
		}

		protected Job(Mod mod, OperationType type)
		{
			Type = type;
			Mod = mod;
			_cancellationTokenSource = new CancellationTokenSource();
			CancellationToken = _cancellationTokenSource.Token;
		}

		public abstract Task<Error> Run();

		protected void PostEvent(OperationPhase jobPhase, ModFileState modState, Error errorCause = null)
		{
			if (Mod.File.State != modState)
			{
				Mod.File.State = modState;
				Mod.File.FileStateErrorCause = errorCause ?? Error.None;
				Mod.InvokeModUpdated(ModChangeType.FileState);
			}
			Phase = jobPhase;
			ModInstallationManagement.ManagementEvents?.Invoke(Mod, Mod.File, Type, jobPhase);
		}

		internal void Cancel()
		{
			_cancellationTokenSource.Cancel();
		}

		internal abstract void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired);

		protected internal void ClearMod()
		{
			Mod = null;
		}
	}

	private class DownloadJob : Job
	{
		public DownloadJob(Mod mod)
			: base(mod, OperationType.Download)
		{
		}

		public override async Task<Error> Run()
		{
			PostEvent(OperationPhase.Checking, ModFileState.Downloading);
			Error error4;
			if (base.Mod.File.Download.ExpiresAfter < DateTime.Now.ToUniversalTime())
			{
				ModioLog.Verbose?.Log("Cached Modfile download for Mod " + base.Mod.Name + " has expired, getting new download");
				ModfileObject? modfileObject;
				(error4, modfileObject) = await ModioAPI.Files.GetModfile(base.Mod.Id, base.Mod.File.Id);
				if ((bool)error4)
				{
					PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error4);
					return error4;
				}
				if (!modfileObject.HasValue)
				{
					return new Error(ErrorCode.NO_DATA_AVAILABLE);
				}
				ModfileObject value = modfileObject.Value;
				base.Mod.UpdateModfile(new Modfile(value));
			}
			if (ModioClient.DataStorage.DoesModfileExist(base.Mod.Id, base.Mod.File.Id))
			{
				PostEvent(OperationPhase.Completed, base.Mod.File.State);
				return Error.None;
			}
			_ = base.Mod.File.Download.BinaryUrl;
			long modfileId = base.Mod.File.Id;
			_ = base.Mod.File.Md5Hash;
			if (!(await ModioClient.DataStorage.IsThereAvailableFreeSpaceFor(base.Mod.File.ArchiveFileSize, base.Mod.File.FileSize)))
			{
				error4 = new Error(ErrorCode.INSUFFICIENT_SPACE);
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error4);
				return error4;
			}
			base.Mod.File.FileStateProgress = 0f;
			base.Mod.File.DownloadingBytesPerSecond = 0L;
			base.Mod.InvokeModUpdated(ModChangeType.DownloadProgress);
			PostEvent(OperationPhase.Started, ModFileState.Downloading);
			error4 = Error.None;
			int attempt = 1;
			while (attempt <= 2)
			{
				string binaryUrl = base.Mod.File.Download.BinaryUrl;
				modfileId = base.Mod.File.Id;
				string filehashMd5 = base.Mod.File.Md5Hash;
				string text = $"{base.Mod.Id}_{modfileId}";
				_downloadAttemptsThisSession.TryGetValue(text, out var value2);
				value2 = (_downloadAttemptsThisSession[text] = value2 + 1);
				ModioLog.Message?.Log($"Starting download for {text} (attempt {value2} this session)");
				Stream stream;
				(error4, stream) = await ModioClient.Api.DownloadFile(binaryUrl, CancellationToken);
				if ((bool)error4)
				{
					PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error4);
					return error4;
				}
				long totalSize = ((base.Mod.File.ArchiveFileSize > 0) ? base.Mod.File.ArchiveFileSize : base.Mod.File.FileSize);
				ModInstallProgressTracker progressTracker = new ModInstallProgressTracker(base.Mod, totalSize);
				Task<Error> task = ModioClient.DataStorage.DownloadModFileFromStream(base.Mod.Id, modfileId, stream, filehashMd5, CancellationToken, progressTracker);
				if (CancellationToken.IsCancellationRequested)
				{
					await stream.DisposeAsync();
					PostEvent(OperationPhase.Cancelled, ModFileState.None);
					return new Error(ErrorCode.OPERATION_CANCELLED);
				}
				error4 = await task;
				if (!error4)
				{
					break;
				}
				if (error4.Code == ErrorCode.MD5DOES_NOT_MATCH && attempt < 2)
				{
					ModioLog.Warning?.Log("MD5 validation failed downloading Mod " + base.Mod.Name + "; refreshing modfile metadata and retrying...");
					var (error5, modfileObject2) = await ModioAPI.Files.GetModfile(base.Mod.Id, base.Mod.File.Id);
					if (!error5 && modfileObject2.HasValue)
					{
						base.Mod.UpdateModfile(new Modfile(modfileObject2.Value));
						base.Mod.File.State = ModFileState.Downloading;
						base.Mod.File.FileStateProgress = 0f;
						base.Mod.File.DownloadingBytesPerSecond = 0L;
						base.Mod.InvokeModUpdated(ModChangeType.FileState);
						base.Mod.InvokeModUpdated(ModChangeType.DownloadProgress);
						attempt++;
						continue;
					}
				}
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error4);
				return error4;
			}
			_index.GetEntry(base.Mod).FileState = ModFileState.Downloaded;
			_index.GetEntry(base.Mod).DownloadedModfileId = modfileId;
			await SaveIndex();
			PostEvent(OperationPhase.Completed, ModFileState.Downloaded);
			return error4;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
			switch (Phase)
			{
			case OperationPhase.Checking:
				tempSpaceRequired += base.Mod.File.ArchiveFileSize;
				break;
			case OperationPhase.Started:
				tempSpaceRequired += (long)((float)base.Mod.File.ArchiveFileSize * (1f - base.Mod.File.FileStateProgress));
				break;
			}
		}
	}

	private class InstallJob : Job
	{
		private readonly bool _isUpdateJob;

		public InstallJob(Mod mod, bool isUpdateJob)
			: base(mod, (!isUpdateJob) ? OperationType.Install : OperationType.Update)
		{
			_isUpdateJob = isUpdateJob;
		}

		public override async Task<Error> Run()
		{
			if (base.Mod.File.State == ModFileState.FileOperationFailed)
			{
				return new Error(ErrorCode.OPERATION_CANCELLED);
			}
			ModFileState progressFileState = (_isUpdateJob ? ModFileState.Updating : ModFileState.Installing);
			PostEvent(OperationPhase.Checking, progressFileState);
			base.Mod.File.FileStateProgress = 0f;
			base.Mod.File.DownloadingBytesPerSecond = 0L;
			base.Mod.InvokeModUpdated(ModChangeType.DownloadProgress);
			if (ModioClient.DataStorage.DoesInstallExist(base.Mod.Id, base.Mod.File.Id))
			{
				_index.GetEntry(base.Mod).FileState = ModFileState.Installed;
				_index.GetEntry(base.Mod).InstalledModfileId = base.Mod.File.Id;
				await SaveIndex();
				PostEvent(OperationPhase.Completed, ModFileState.Installed);
				return Error.None;
			}
			if (!ModioClient.DataStorage.DoesModfileExist(base.Mod.Id, base.Mod.File.Id))
			{
				if (_index.GetEntry(base.Mod).DownloadedModfileId == base.Mod.File.Id)
				{
					_index.GetEntry(base.Mod).DownloadedModfileId = -1L;
					_index.IsDirty = true;
				}
				string key = $"{base.Mod.Id}_{base.Mod.File.Id}";
				_missingModfileReenqueues.TryGetValue(key, out var value);
				if (value < 1)
				{
					_missingModfileReenqueues[key] = value + 1;
					_operationQueue.Enqueue(new DownloadJob(base.Mod));
					_operationQueue.Enqueue(new InstallJob(base.Mod, _isUpdateJob));
					ModioLog.Error?.Log($"Unable to install mod {base.Mod.Id}_{base.Mod.File.Id} as Modfile could not be found!");
					PostEvent(OperationPhase.Failed, ModFileState.Queued);
					return new Error(ErrorCode.FILE_NOT_FOUND);
				}
				ModioLog.Error?.Log($"Unable to install mod {base.Mod.Id}_{base.Mod.File.Id} as Modfile could not be found and a " + "re-download was already attempted! Marking as failed.");
				Error error = new Error(ErrorCode.FILE_NOT_FOUND);
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error);
				return error;
			}
			long num = base.Mod.File.FileSize;
			if (_isUpdateJob)
			{
				long installationSize = _index.GetEntry(base.Mod).InstallationSize;
				num -= installationSize;
			}
			bool flag = num > 0;
			if (flag)
			{
				flag = await ModioClient.DataStorage.IsThereAvailableFreeSpaceForModInstall(num);
			}
			Error error2;
			if (!flag)
			{
				error2 = new Error(ErrorCode.INSUFFICIENT_SPACE);
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error2);
				return error2;
			}
			PostEvent(OperationPhase.Started, progressFileState);
			base.Mod.File.State = progressFileState;
			base.Mod.InvokeModUpdated(ModChangeType.FileState);
			if (_isUpdateJob)
			{
				error2 = await ModioClient.DataStorage.DeleteInstalledMod(base.Mod, _index.GetEntry(base.Mod).InstalledModfileId);
				if ((bool)error2)
				{
					PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error2);
					return error2;
				}
			}
			error2 = await ModioClient.DataStorage.InstallMod(base.Mod, base.Mod.File.Id, CancellationToken);
			if (error2.Code == ErrorCode.OPERATION_CANCELLED)
			{
				PostEvent(OperationPhase.Cancelled, ModFileState.None);
				return new Error(ErrorCode.OPERATION_CANCELLED);
			}
			if ((bool)error2)
			{
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error2);
				return error2;
			}
			base.Mod.File.InstallLocation = ModioClient.DataStorage.GetInstallPath(base.Mod.Id, base.Mod.File.Id);
			_index.GetEntry(base.Mod).FileState = ModFileState.Installed;
			_index.GetEntry(base.Mod).InstalledModfileId = base.Mod.File.Id;
			await SaveIndex();
			PostEvent(OperationPhase.Completed, ModFileState.Installed);
			return Error.None;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
			tempSpaceRequired -= base.Mod.File.ArchiveFileSize;
			switch (Phase)
			{
			case OperationPhase.Checking:
				spaceRequired += base.Mod.File.FileSize;
				if (_isUpdateJob)
				{
					spaceRequired -= _index.GetEntry(base.Mod).InstallationSize;
				}
				break;
			case OperationPhase.Started:
				spaceRequired += (long)((double)base.Mod.File.FileSize * (1.0 - (double)base.Mod.File.FileStateProgress));
				if (_isUpdateJob && base.Mod.File.FileStateProgress == 0f)
				{
					spaceRequired -= _index.GetEntry(base.Mod).InstallationSize;
				}
				break;
			}
		}
	}

	private class DownloadAndExtractJob : Job
	{
		private bool IsUpdateJob => Type == OperationType.Update;

		public DownloadAndExtractJob(Mod mod, bool isUpdateJob)
			: base(mod, isUpdateJob ? OperationType.Update : OperationType.Download)
		{
		}

		public override async Task<Error> Run()
		{
			ModFileState progressFileState = (IsUpdateJob ? ModFileState.Updating : ModFileState.Downloading);
			PostEvent(OperationPhase.Checking, progressFileState);
			Error error;
			if (base.Mod.File.Download.ExpiresAfter < DateTime.Now.ToUniversalTime())
			{
				ModioLog.Verbose?.Log("Cached Modfile download for Mod " + base.Mod.Name + " has expired, getting new download");
				ModfileObject? modfileObject;
				(error, modfileObject) = await ModioAPI.Files.GetModfile(base.Mod.Id, base.Mod.File.Id);
				if ((bool)error)
				{
					PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error);
					return error;
				}
				if (!modfileObject.HasValue)
				{
					return new Error(ErrorCode.NO_DATA_AVAILABLE);
				}
				ModfileObject value = modfileObject.Value;
				base.Mod.UpdateModfile(new Modfile(value));
			}
			if (ModioClient.DataStorage.DoesInstallExist(base.Mod.Id, base.Mod.File.Id))
			{
				_index.GetEntry(base.Mod).FileState = ModFileState.Installed;
				_index.GetEntry(base.Mod).InstalledModfileId = base.Mod.File.Id;
				await SaveIndex();
				base.Mod.File.InstallLocation = ModioClient.DataStorage.GetInstallPath(base.Mod.Id, base.Mod.File.Id);
				PostEvent(OperationPhase.Completed, ModFileState.Installed);
				return Error.None;
			}
			long num = base.Mod.File.FileSize;
			if (IsUpdateJob)
			{
				long installationSize = _index.GetEntry(base.Mod).InstallationSize;
				num -= installationSize;
			}
			bool flag = num > 0;
			if (flag)
			{
				flag = await ModioClient.DataStorage.IsThereAvailableFreeSpaceForModInstall(num);
			}
			if (!flag)
			{
				error = new Error(ErrorCode.INSUFFICIENT_SPACE);
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error);
				return error;
			}
			base.Mod.File.FileStateProgress = 0f;
			base.Mod.File.DownloadingBytesPerSecond = 0L;
			base.Mod.InvokeModUpdated(ModChangeType.DownloadProgress);
			PostEvent(OperationPhase.Started, progressFileState);
			if (IsUpdateJob)
			{
				Error error2 = await ModioClient.DataStorage.DeleteInstalledMod(base.Mod, _index.GetEntry(base.Mod).InstalledModfileId);
				if ((bool)error2)
				{
					PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error2);
					return error2;
				}
			}
			var (error3, stream) = await ModioClient.Api.DownloadFile(base.Mod.File.Download.BinaryUrl, CancellationToken);
			if ((bool)error3)
			{
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error3);
				return error3;
			}
			PostEvent(OperationPhase.Completed, ModFileState.Downloaded);
			PostEvent(OperationPhase.Started, ModFileState.Installing);
			error = await ModioClient.DataStorage.InstallModFromStream(base.Mod, base.Mod.File.Id, stream, base.Mod.File.Md5Hash, CancellationToken);
			if ((bool)error)
			{
				PostEvent(OperationPhase.Failed, ModFileState.FileOperationFailed, error);
				return error;
			}
			base.Mod.File.InstallLocation = ModioClient.DataStorage.GetInstallPath(base.Mod.Id, base.Mod.File.Id);
			_index.GetEntry(base.Mod).FileState = ModFileState.Installed;
			_index.GetEntry(base.Mod).InstalledModfileId = base.Mod.File.Id;
			await SaveIndex();
			PostEvent(OperationPhase.Completed, ModFileState.Installed);
			return Error.None;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
			switch (Phase)
			{
			case OperationPhase.Checking:
				spaceRequired += base.Mod.File.FileSize;
				if (IsUpdateJob)
				{
					spaceRequired -= _index.GetEntry(base.Mod).InstallationSize;
				}
				break;
			case OperationPhase.Started:
				spaceRequired += (long)((double)base.Mod.File.FileSize * (1.0 - (double)base.Mod.File.FileStateProgress));
				if (IsUpdateJob && base.Mod.File.FileStateProgress == 0f)
				{
					spaceRequired -= _index.GetEntry(base.Mod).InstallationSize;
				}
				break;
			}
		}
	}

	private class UninstallJob : Job
	{
		public UninstallJob(Mod mod)
			: base(mod, OperationType.Uninstall)
		{
		}

		public override async Task<Error> Run()
		{
			PostEvent(OperationPhase.Started, ModFileState.Uninstalling);
			ModIndex.IndexEntry entry = _index.GetEntry(base.Mod);
			Error error = await ModioClient.DataStorage.DeleteInstalledMod(base.Mod, entry.InstalledModfileId);
			base.Mod.File.InstallLocation = null;
			_index.RemoveEntry(base.Mod);
			await SaveIndex();
			PostEvent(OperationPhase.Completed, ModFileState.None);
			_unverifiedMods.Remove(base.Mod);
			if (_modsToUninstall.Contains(base.Mod))
			{
				_modsToUninstall.Remove(base.Mod);
			}
			ClearMod();
			RetryInstallingTaintedMods();
			return error;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
			spaceRequired -= base.Mod.File.FileSize;
		}
	}

	private class ValidateJob : Job
	{
		public ValidateJob(Mod mod)
			: base(mod, OperationType.Validate)
		{
		}

		public override async Task<Error> Run()
		{
			PostEvent(OperationPhase.Started, base.Mod.File.State);
			ModIndex.IndexEntry entry = _index.GetEntry(base.Mod);
			if (!ModioClient.DataStorage.DoesInstallExist(base.Mod.Id, entry.InstalledModfileId))
			{
				ModioLog.Warning?.Log($"Mod {base.Mod}: Failed to validate installed mod directory. It will be re-downloaded");
				PostEvent(OperationPhase.Completed, ModFileState.None);
				if (!base.Mod.IsSubscribed)
				{
					_index.RemoveEntry(base.Mod);
				}
				else
				{
					entry.FileState = ModFileState.None;
					entry.InstalledModfileId = -1L;
					entry.InstallationSize = 0L;
				}
				await SaveIndex();
				_unverifiedMods.Remove(base.Mod);
				return Error.None;
			}
			_unverifiedMods.Remove(base.Mod);
			PostEvent(OperationPhase.Completed, ModFileState.Installed);
			return Error.None;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
		}
	}

	private class ScanMissingInstallsJob : Job
	{
		public ScanMissingInstallsJob()
			: base(null, OperationType.Scan)
		{
		}

		public override async Task<Error> Run()
		{
			Phase = OperationPhase.Started;
			if (await _index.UpdateIndexWithMissingEntriesFromScan())
			{
				await SaveIndex();
			}
			_hasScannedMissingMods = true;
			Phase = OperationPhase.Completed;
			return Error.None;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
		}
	}

	private class ScanForInstalledModJob : Job
	{
		public ScanForInstalledModJob(Mod mod)
			: base(mod, OperationType.Scan)
		{
		}

		public override async Task<Error> Run()
		{
			Phase = OperationPhase.Started;
			var (flag, modfileId) = await _index.UpdateIndexForMod(base.Mod);
			if (flag)
			{
				base.Mod.File.InstallLocation = ModioClient.DataStorage.GetInstallPath(base.Mod.Id, modfileId);
				await SaveIndex();
			}
			Phase = OperationPhase.Completed;
			return Error.None;
		}

		internal override void GetPendingSpaceChange(ref long spaceRequired, ref long tempSpaceRequired)
		{
		}
	}

	public enum OperationType
	{
		Download,
		Install,
		Update,
		Uninstall,
		Validate,
		Scan
	}

	public enum OperationPhase
	{
		Checking,
		Started,
		Completed,
		Cancelled,
		Failed
	}

	private static bool _uninstallUnsubscribedMods = true;

	private static ModIndex _index;

	private static Job _currentOperation;

	private static bool _isRunning;

	private static Queue<Job> _operationQueue;

	private static HashSet<ModId> _requestedModDownloads = new HashSet<ModId>();

	private static readonly Dictionary<string, int> _downloadAttemptsThisSession = new Dictionary<string, int>();

	private static readonly Dictionary<string, int> _missingModfileReenqueues = new Dictionary<string, int>();

	private static HashSet<Mod> _modsToRefresh = new HashSet<Mod>();

	private static HashSet<ModId> _currentSessionMods = new HashSet<ModId>();

	private static HashSet<Mod> _modsToUninstall = new HashSet<Mod>();

	private static HashSet<Mod> _unverifiedMods = new HashSet<Mod>();

	private static bool _hasScannedMissingMods;

	private static bool _isDeactivated;

	public static bool DownloadAndExtractAsSingleJob { get; set; } = true;


	public static Mod CurrentOperationOnMod => _currentOperation?.Mod;

	public static bool IsInitialized => _index != null;

	public static int PendingModOperationCount => _operationQueue?.Count ?? 0;

	public static bool IsRunning => _isRunning;

	public static event InstallationManagementEventDelegate ManagementEvents;

	internal static async Task<Error> Init()
	{
		var (error, index) = await ModioClient.DataStorage.ReadIndexData();
		if ((bool)error)
		{
			(error, index) = await ModIndex.CreateIndexFromScan();
		}
		_index = ((!error) ? index : new ModIndex());
		_operationQueue = new Queue<Job>();
		_unverifiedMods.Clear();
		_hasScannedMissingMods = false;
		_downloadAttemptsThisSession.Clear();
		_missingModfileReenqueues.Clear();
		await Mod.GetMods(_index.Index.Keys);
		try
		{
			foreach (KeyValuePair<long, ModIndex.IndexEntry> item in index.Index)
			{
				Mod modRespectingIndexCache = GetModRespectingIndexCache(item.Key);
				ModIndex.IndexEntry value = item.Value;
				if (modRespectingIndexCache.File != null)
				{
					modRespectingIndexCache.File.State = value.FileState;
					if (modRespectingIndexCache.File.State == ModFileState.Installed)
					{
						modRespectingIndexCache.File.InstallLocation = ModioClient.DataStorage.GetInstallPath(modRespectingIndexCache.Id, value.InstalledModfileId);
					}
					modRespectingIndexCache.InvokeModUpdated(ModChangeType.FileState);
				}
				modRespectingIndexCache.Logo?.CacheLowestResolutionOnDisk(shouldCache: true);
				if (User.Current.IsAuthenticated && !User.Current.ModRepository.HasGotSubscriptions)
				{
					modRespectingIndexCache.UpdateLocalSubscriptionStatus(value.Subscribers.Contains(User.Current.Profile.UserId));
				}
				_unverifiedMods.Add(modRespectingIndexCache);
			}
			Mod.AddChangeListener(ModChangeType.IsSubscribed, OnModSubscriptionChange);
			if (!ModioServices.Resolve<ModioSettings>().TryGetPlatformSettings<ModInstallationManagementSettings>(out var settings))
			{
				Activate();
			}
			else
			{
				_uninstallUnsubscribedMods = settings.UninstallIfNoSubscriptions;
				if (settings.AutoActivate)
				{
					Activate();
				}
				else
				{
					_isDeactivated = true;
				}
			}
			return error;
		}
		catch (Exception arg)
		{
			Debug.LogError($"[mod.io] Exception during ModInstallationManagement Init: {arg}");
			return Error.Unknown;
		}
	}

	internal static async Task Shutdown()
	{
		Mod.RemoveChangeListener(ModChangeType.IsSubscribed, OnModSubscriptionChange);
		_index?.Shutdown();
		_index = null;
		while (_currentOperation != null)
		{
			await Task.Yield();
		}
	}

	internal static void WakeUp()
	{
		if (_index == null)
		{
			ModioLog.Warning?.Log("[WakeUp] _index is null.");
		}
		else
		{
			ExecuteJobs();
		}
	}

	public static void Activate()
	{
		_isDeactivated = false;
		WakeUp();
	}

	public static void Deactivate(bool cancelCurrentJob)
	{
		_isDeactivated = true;
		_operationQueue.Clear();
		if (cancelCurrentJob)
		{
			CancelInstallOperation(CurrentOperationOnMod);
		}
	}

	public static bool IsModSubscribed(long modId, long userId)
	{
		if (IsInitialized && _index.Index.TryGetValue(modId, out var value))
		{
			return value.Subscribers.Contains(userId);
		}
		return false;
	}

	public static async Task<ICollection<Mod>> GetAllInstalledMods(bool forceRefresh = false)
	{
		try
		{
			if (IsInitialized)
			{
				if (forceRefresh)
				{
					await _index.RefreshIndexModObjects();
				}
				return _index.ModObjectCache.Values.ToArray().Select(ModCache.GetMod).ToArray();
			}
			return Array.Empty<Mod>();
		}
		catch (Exception arg)
		{
			Debug.LogError($"Exception while getting installed mods: {arg}");
			return Array.Empty<Mod>();
		}
	}

	public static long GetTotalDiskUsage(bool includeQueued)
	{
		if (_index?.Index == null)
		{
			return 0L;
		}
		long num = 0L;
		foreach (KeyValuePair<long, ModIndex.IndexEntry> item in _index.Index)
		{
			item.Deconstruct(out var key, out var _);
			Mod modRespectingIndexCache = GetModRespectingIndexCache(key);
			if (modRespectingIndexCache?.File == null || modRespectingIndexCache.File.State == ModFileState.FileOperationFailed)
			{
				continue;
			}
			long num2 = modRespectingIndexCache.File.FileSize;
			if (!includeQueued)
			{
				if (modRespectingIndexCache.File.State == ModFileState.Downloading || modRespectingIndexCache.File.State == ModFileState.Installing)
				{
					num2 = (long)((float)num2 * modRespectingIndexCache.File.FileStateProgress);
				}
				else if (modRespectingIndexCache.File.State != ModFileState.Installed)
				{
					continue;
				}
			}
			num += num2;
		}
		return num;
	}

	private static Mod GetModRespectingIndexCache(long modId)
	{
		if (ModCache.TryGetMod(modId, out var mod))
		{
			_index.ModObjectCache[modId] = mod.LastModObject;
			return mod;
		}
		if (_index.ModObjectCache.TryGetValue(modId, out var value))
		{
			if (value.Id != 0L)
			{
				return ModCache.GetMod(value);
			}
			ModioLog.Message?.Log($"Removing invalid cached mod object for mod: {modId}");
			_index.ModObjectCache.Remove(modId);
		}
		return Mod.Get(modId);
	}

	private static async Task<Error> SaveIndex()
	{
		_index.IsDirty = false;
		return await ModioClient.DataStorage.WriteIndexData(_index);
	}

	private static async void ExecuteJobs()
	{
		if (_isRunning)
		{
			await EnqueueJobs();
			return;
		}
		_isRunning = true;
		Job lastStartedJob = null;
		try
		{
			while (true)
			{
				await EnqueueJobs();
				if (_index.IsDirty)
				{
					await SaveIndex();
				}
				else if (!_operationQueue.Any())
				{
					break;
				}
				try
				{
					while (_operationQueue.Any())
					{
						Job currentOperation;
						lastStartedJob = (currentOperation = _operationQueue.Dequeue());
						_currentOperation = currentOperation;
						Error error = await _currentOperation.Run();
						if ((bool)error)
						{
							if (error.IsSilent)
							{
								ModioLog.Verbose?.Log($"Cancelled performing {_currentOperation.Type} job for mod {_currentOperation.Mod}: {error}");
							}
							else
							{
								ModioLog.Error?.Log($"Error performing {_currentOperation.Type} job for mod {_currentOperation.Mod}: {error}");
							}
						}
						if (!ModioClient.IsInitialized && !ModioClient.IsCurrentlyInitializing)
						{
							return;
						}
						if (_index.IsDirty)
						{
							await SaveIndex();
						}
					}
				}
				finally
				{
					_currentOperation = null;
				}
				await Task.Yield();
			}
		}
		catch (Exception arg)
		{
			ModioLog.Error?.Log("Unhandled exception performing " + (lastStartedJob?.Type.ToString() ?? "unknown") + " " + string.Format("job for mod {0}: {1}", lastStartedJob?.Mod?.ToString() ?? "unknown", arg));
			if (lastStartedJob?.Mod?.File != null)
			{
				ModFileState state = lastStartedJob.Mod.File.State;
				if (state == ModFileState.Downloading || state == ModFileState.Installing || state == ModFileState.Updating)
				{
					lastStartedJob.Mod.File.State = ModFileState.FileOperationFailed;
					lastStartedJob.Mod.InvokeModUpdated(ModChangeType.FileState);
				}
			}
		}
		finally
		{
			_currentOperation = null;
			_isRunning = false;
		}
	}

	private static async Task EnqueueJobs()
	{
		_operationQueue.Clear();
		foreach (Mod item in _modsToRefresh)
		{
			_operationQueue.Enqueue(new ScanForInstalledModJob(item));
		}
		_modsToRefresh.Clear();
		foreach (Mod item2 in User.Current.ModRepository.GetSubscribed())
		{
			if (item2.File != null && item2.File.State != ModFileState.FileOperationFailed)
			{
				_index.GetEntry(item2);
			}
		}
		foreach (KeyValuePair<long, ModIndex.IndexEntry> item3 in _index.Index)
		{
			ModId modId = new ModId(item3.Key);
			bool flag = User.Current.ModRepository.IsSubscribed(modId);
			List<long> subscribers = item3.Value.Subscribers;
			if (flag && !subscribers.Contains(User.Current.UserId))
			{
				subscribers.Add(User.Current.Profile.UserId);
				_index.IsDirty = true;
			}
			else if (!flag && subscribers.Contains(User.Current.UserId))
			{
				subscribers.Remove(User.Current.Profile.UserId);
				_index.IsDirty = true;
			}
			bool flag2 = flag || subscribers.Count > 0;
			bool flag3 = item3.Value.ExpiresAfter > DateTime.Today.ToUniversalTime() || _currentSessionMods.Contains(modId);
			Mod modRespectingIndexCache = GetModRespectingIndexCache(modId);
			if (_isDeactivated || modRespectingIndexCache.File == null)
			{
				continue;
			}
			if (_modsToUninstall.Contains(modRespectingIndexCache))
			{
				_operationQueue.Enqueue(new UninstallJob(modRespectingIndexCache));
				continue;
			}
			if (_uninstallUnsubscribedMods && !flag3)
			{
				if (!flag2)
				{
					goto IL_022f;
				}
				if (!flag)
				{
					ModFileState state = modRespectingIndexCache.File.State;
					if (state != ModFileState.Installed && state != ModFileState.Uninstalling)
					{
						goto IL_022f;
					}
				}
			}
			EnqueueJobsIfNeeded(item3.Value, modRespectingIndexCache);
			continue;
			IL_022f:
			_operationQueue.Enqueue(new UninstallJob(modRespectingIndexCache));
		}
		ModId[] array = _requestedModDownloads.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			var (error, mod2) = await Mod.GetMod(array[i]);
			if ((bool)error || mod2.File == null)
			{
				continue;
			}
			ModFileState state = mod2.File.State;
			if (state == ModFileState.None || state == ModFileState.Queued || state == ModFileState.FileOperationFailed || state == ModFileState.Downloaded)
			{
				if (DownloadAndExtractAsSingleJob)
				{
					_operationQueue.Enqueue(new DownloadAndExtractJob(mod2, isUpdateJob: false));
					continue;
				}
				_operationQueue.Enqueue(new DownloadJob(mod2));
				_operationQueue.Enqueue(new InstallJob(mod2, isUpdateJob: false));
			}
		}
		_requestedModDownloads.Clear();
		if (!_hasScannedMissingMods)
		{
			_operationQueue.Enqueue(new ScanMissingInstallsJob());
		}
		static void EnqueueJobsIfNeeded(ModIndex.IndexEntry entry, Mod mod)
		{
			if (_unverifiedMods.Contains(mod))
			{
				_operationQueue.Enqueue(new ValidateJob(mod));
			}
			if (mod.File != null && mod.File.State != ModFileState.Installing && mod.File.State != ModFileState.Updating && (mod.File.State != ModFileState.FileOperationFailed || _requestedModDownloads.Contains(mod.Id)))
			{
				if (entry.InstalledModfileId == mod.File.Id && mod.File.Id > 0)
				{
					if (mod.File.State != ModFileState.Installed)
					{
						_operationQueue.Enqueue(new ValidateJob(mod));
					}
				}
				else
				{
					if (_requestedModDownloads.Contains(mod.Id))
					{
						if (mod.File.State == ModFileState.FileOperationFailed)
						{
							mod.File.State = ModFileState.Queued;
							mod.InvokeModUpdated(ModChangeType.FileState);
						}
						bool isUpdateJob = entry.InstalledModfileId != -1;
						if (DownloadAndExtractAsSingleJob)
						{
							if (mod.File.State != ModFileState.Downloading)
							{
								_operationQueue.Enqueue(new DownloadAndExtractJob(mod, isUpdateJob));
							}
						}
						else
						{
							if (entry.DownloadedModfileId != mod.File.Id && mod.File.State != ModFileState.Downloading)
							{
								_operationQueue.Enqueue(new DownloadJob(mod));
							}
							_operationQueue.Enqueue(new InstallJob(mod, isUpdateJob));
						}
						_requestedModDownloads.Remove(mod.Id);
					}
					else if (mod.File.State == ModFileState.Downloaded && entry.DownloadedModfileId == mod.File.Id)
					{
						bool isUpdateJob2 = entry.InstalledModfileId != -1;
						_operationQueue.Enqueue(new InstallJob(mod, isUpdateJob2));
					}
					if (mod.File.State == ModFileState.None || mod.File.State == ModFileState.Installed)
					{
						mod.File.State = ModFileState.Queued;
						mod.InvokeModUpdated(ModChangeType.FileState);
						ModInstallationManagement.ManagementEvents?.Invoke(mod, mod.File, OperationType.Validate, OperationPhase.Completed);
					}
				}
			}
		}
	}

	public static async Task<Error> StartTempModSession(List<ModId> tempMods, bool appendCurrentSession = false)
	{
		if (_currentSessionMods.Count == 0 && !appendCurrentSession)
		{
			ModioLog.Message?.Log("Attempting to start new Temp Mod Session while one is active! Ending previous session. If you want to append mods onto the current session, set appendCurrentSession to true.");
			EndCurrentTempModSession();
		}
		foreach (ModId tempMod in tempMods)
		{
			_currentSessionMods.Add(tempMod);
		}
		return await AddTemporaryMods(tempMods, 0);
	}

	public static void EndCurrentTempModSession()
	{
		_currentSessionMods.Clear();
		ExecuteJobs();
	}

	public static async Task<Error> AddTemporaryMods(List<ModId> tempMods, int lifeTimeDaysOverride = -1)
	{
		int lifeTimeDays = ((lifeTimeDaysOverride > -1) ? lifeTimeDaysOverride : ModioClient.Settings.GetPlatformSettings<TempModInstallationSettings>().LifeTimeDays);
		var (error, collection) = await Mod.GetMods(((IEnumerable<ModId>)tempMods).Select((Func<ModId, long>)((ModId modId) => modId)).ToList());
		if ((bool)error)
		{
			return error;
		}
		if (tempMods.Any((ModId modId) => _index.TryGetEntry(modId, out var entry) && entry.FileState == ModFileState.FileOperationFailed))
		{
			return new Error(ErrorCode.CANT_INSTALL_TAINTED_MOD);
		}
		foreach (Mod item in collection)
		{
			AddTemporaryMod(item, lifeTimeDays);
		}
		return Error.None;
	}

	private static void AddTemporaryMod(Mod modId, int lifetime)
	{
		ModIndex.IndexEntry entry = _index.GetEntry(modId);
		switch (entry.FileState)
		{
		case ModFileState.None:
		case ModFileState.Uninstalling:
			entry.ExpiresAfter = ((lifetime == 0) ? DateTime.UnixEpoch : DateTime.Today.ToUniversalTime().AddDays(lifetime));
			WakeUp();
			break;
		case ModFileState.Queued:
		case ModFileState.Downloading:
		case ModFileState.Downloaded:
		case ModFileState.Installing:
		case ModFileState.Installed:
		case ModFileState.Updating:
			entry.ExpiresAfter = ((lifetime == 0) ? entry.ExpiresAfter : DateTime.Today.ToUniversalTime().AddDays(lifetime));
			break;
		case ModFileState.FileOperationFailed:
			break;
		}
	}

	public static void ClearExpiredTempMods()
	{
		ExecuteJobs();
	}

	private static void RetryInstallingTaintedMods()
	{
		foreach (KeyValuePair<long, ModIndex.IndexEntry> item in _index.Index)
		{
			if (item.Value.FileState == ModFileState.FileOperationFailed)
			{
				item.Value.FileState = ModFileState.None;
				Mod modRespectingIndexCache = GetModRespectingIndexCache(item.Key);
				modRespectingIndexCache.File.State = ModFileState.None;
				modRespectingIndexCache.InvokeModUpdated(ModChangeType.FileState);
			}
		}
		WakeUp();
	}

	public static async Task<Error> RetryInstallingMod(Mod mod)
	{
		if (mod.File.State != ModFileState.FileOperationFailed)
		{
			return Error.None;
		}
		_index.GetEntry(mod).FileState = ModFileState.None;
		mod.File.State = ModFileState.None;
		mod.InvokeModUpdated(ModChangeType.FileState);
		if (!(await IsThereAvailableSpaceFor(mod)))
		{
			FilesystemError filesystemError = new FilesystemError(FilesystemErrorCode.INSUFFICIENT_SPACE);
			mod.File.FileStateErrorCause = filesystemError;
			mod.File.State = ModFileState.FileOperationFailed;
			mod.InvokeModUpdated(ModChangeType.FileState);
			return filesystemError;
		}
		WakeUp();
		return Error.None;
	}

	public static void MarkModForUninstallation(Mod mod)
	{
		_modsToUninstall.Add(mod);
		WakeUp();
	}

	private static void OnModSubscriptionChange(Mod mod, ModChangeType changeType)
	{
		if (ModChangeType.IsSubscribed == changeType && !mod.IsSubscribed)
		{
			CancelInstallOperation(mod);
		}
		if (mod.IsSubscribed && _modsToUninstall.Contains(mod))
		{
			_modsToUninstall.Remove(mod);
			WakeUp();
		}
	}

	private static void CancelInstallOperation(Mod mod)
	{
		if (_currentOperation != null && _currentOperation.Mod == mod && (_currentOperation.Type == OperationType.Download || _currentOperation.Type == OperationType.Install))
		{
			_currentOperation.Cancel();
		}
	}

	public static bool DoesModNeedUpdate(Mod mod)
	{
		if (mod.File.State == ModFileState.Queued)
		{
			return _index.GetEntry(mod).InstalledModfileId != -1;
		}
		return false;
	}

	public static long GetInstalledModfileId(Mod mod)
	{
		if (_index == null || mod == null)
		{
			return -1L;
		}
		if (!_index.TryGetEntry(mod.Id, out var entry))
		{
			return -1L;
		}
		return entry.InstalledModfileId;
	}

	public static async Task<bool> DownloadAndInstallMod(ModId modId)
	{
		if (_requestedModDownloads.Contains(modId))
		{
			ModioLog.Verbose?.Log($"[DownloadAndInstallMod] Mod {modId} download already requested.");
			return true;
		}
		var (error, mod) = await Mod.GetMod(modId);
		if ((bool)error)
		{
			ModioLog.Error?.Log($"[DownloadAndInstallMod] Failed to retrieve details for Mod {modId}");
			return false;
		}
		ModFileState state = mod.File.State;
		if (state == ModFileState.Downloading || (uint)(state - 4) <= 3u)
		{
			ModioLog.Message?.Log($"[DownloadAndInstallMod] Mod Operation already in progress: {mod.File.State}");
			return false;
		}
		ModioLog.Verbose?.Log($"[DownloadAndInstallMod] Mod {modId} download queued.");
		_requestedModDownloads.Add(modId);
		WakeUp();
		return true;
	}

	public static async Task<bool> IsThereAvailableSpaceFor(Mod mod)
	{
		long spaceRequired = 0L;
		long tempSpaceRequired = 0L;
		_currentOperation?.GetPendingSpaceChange(ref spaceRequired, ref tempSpaceRequired);
		foreach (Job item in _operationQueue)
		{
			item.GetPendingSpaceChange(ref spaceRequired, ref tempSpaceRequired);
		}
		if (DownloadAndExtractAsSingleJob)
		{
			return await ModioClient.DataStorage.IsThereAvailableFreeSpaceFor(tempSpaceRequired, spaceRequired + mod.File.FileSize);
		}
		return await ModioClient.DataStorage.IsThereAvailableFreeSpaceFor(tempSpaceRequired + mod.File.ArchiveFileSize, spaceRequired + mod.File.FileSize);
	}

	public static async Task UninstallAllMods()
	{
		if (!IsInitialized || _isDeactivated)
		{
			return;
		}
		foreach (Mod item in await GetAllInstalledMods())
		{
			_modsToUninstall.Add(item);
		}
		WakeUp();
	}

	public static void RefreshMod(Mod mod)
	{
		if (IsInitialized && !_isDeactivated)
		{
			_modsToRefresh.Add(mod);
			WakeUp();
		}
	}

	public static void RefreshMods(List<Mod> mods)
	{
		if (!IsInitialized || _isDeactivated)
		{
			return;
		}
		foreach (Mod mod in mods)
		{
			_modsToRefresh.Add(mod);
		}
		WakeUp();
	}

	public static void NotifyLoggingOut()
	{
		if (!IsInitialized || _isDeactivated)
		{
			return;
		}
		foreach (KeyValuePair<long, ModIndex.IndexEntry> item in _index.Index)
		{
			ModId modId = new ModId(item.Key);
			bool num = User.Current.ModRepository.IsSubscribed(modId);
			List<long> subscribers = item.Value.Subscribers;
			if (num && subscribers.Contains(User.Current.UserId))
			{
				subscribers.Remove(User.Current.Profile.UserId);
				_index.IsDirty = true;
			}
			if (subscribers.Count <= 0)
			{
				Mod modRespectingIndexCache = GetModRespectingIndexCache(modId);
				if (modRespectingIndexCache.File.State == ModFileState.Queued && modRespectingIndexCache.File.InstallLocation == null)
				{
					modRespectingIndexCache.File.State = ModFileState.None;
					modRespectingIndexCache.InvokeModUpdated(ModChangeType.FileState);
					ModInstallationManagement.ManagementEvents?.Invoke(modRespectingIndexCache, modRespectingIndexCache.File, OperationType.Validate, OperationPhase.Completed);
				}
			}
		}
	}

	public static bool ValidateInstalledMod(Mod mod)
	{
		if (mod.File.State != ModFileState.Installed)
		{
			return false;
		}
		if (ModioClient.DataStorage.DoesInstallExist(mod.Id, mod.File.Id))
		{
			return true;
		}
		mod.File.State = ModFileState.Queued;
		ModioLog.Message?.Log("[ValidateInstalledMod] Validate failed, refreshing mod " + mod.Name);
		RefreshMod(mod);
		return false;
	}

	internal static ModObject GetHiddenModObjectFromIndex(ModId modId, ModIndex tempIndex = null)
	{
		ModIndex modIndex = tempIndex ?? _index;
		if (modIndex == null)
		{
			return default(ModObject);
		}
		long modFileId = -1L;
		if (modIndex.TryGetEntry(modId, out var entry))
		{
			modFileId = entry.InstalledModfileId;
		}
		modIndex.ModObjectCache.Remove(modId);
		ModObject hiddenModObject = ModObject.GetHiddenModObject(modId, modFileId);
		modIndex.ModObjectCache[modId] = hiddenModObject;
		return hiddenModObject;
	}
}
