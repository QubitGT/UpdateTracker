using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using Liv.Lck.Core.Serialization;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck.Core.Cosmetics;

[Preserve]
public class LckCoreCosmeticsCoordinator : ILckCosmeticsCoordinator
{
	private readonly ILckSerializer _serializer;

	private readonly ILckCosmeticsFeatureFlagManager _featureFlagManager;

	private CancellationTokenSource _reannounceCancellationTokenSource;

	private Task _requestLocalUserCosmeticsTask;

	private readonly object _lock = new object();

	private string _playerId;

	private string _sessionId;

	private static LckCoreCosmeticsCoordinator Instance { get; set; }

	public event Action<LckAvailableCosmeticInfo> OnCosmeticAvailable;

	[Preserve]
	public LckCoreCosmeticsCoordinator(ILckSerializer serializer, ILckCosmeticsFeatureFlagManager featureFlagManager)
	{
		Instance = this;
		_serializer = serializer;
		_featureFlagManager = featureFlagManager;
		InitializeLocalCosmeticsAsync();
	}

	public Task InitializeLocalCosmeticsAsync()
	{
		lock (_lock)
		{
			if (_requestLocalUserCosmeticsTask == null || _requestLocalUserCosmeticsTask.IsCompleted)
			{
				_requestLocalUserCosmeticsTask = RequestLocalUserCosmeticsAsyncDelayed();
			}
			return _requestLocalUserCosmeticsTask;
		}
	}

	private async Task RequestLocalUserCosmeticsAsyncDelayed()
	{
		_ = 2;
		try
		{
			await Task.Delay(3000);
			if (!(await _featureFlagManager.IsEnabledAsync()))
			{
				Debug.Log("LCK: Cosmetics feature is disabled by feature flag. Local cosmetics will not be loaded.");
				return;
			}
			Result<bool> result = await GetLocalUserCosmeticsAsync();
			if (!result.IsOk)
			{
				Debug.LogError("LCK: The initial, delayed fetch for local user cosmetics failed. Error: " + result.Message);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("LCK: RequestLocalUserCosmeticsAsyncDelayed failed with exception: " + ex.Message);
		}
	}

	private async Task<Result<bool>> GetLocalUserCosmeticsAsync()
	{
		if (!(await _featureFlagManager.IsEnabledAsync()))
		{
			return Result<bool>.NewSuccess(result: true);
		}
		return await Task.Run(delegate
		{
			CosmeticsReturnCode cosmeticsReturnCode = LckCoreCosmeticsNative.get_local_user_cosmetics(OnCosmeticAvailableStatic);
			switch (cosmeticsReturnCode)
			{
			case CosmeticsReturnCode.Ok:
				return Result<bool>.NewSuccess(result: true);
			case CosmeticsReturnCode.Unauthorized:
				return Result<bool>.NewSuccess(result: false);
			case CosmeticsReturnCode.FailedToCacheCosmetics:
				return Result<bool>.NewError(CoreError.FailedToCacheCosmetics, "Failed to cache one or more local user cosmetics");
			default:
			{
				string message = $"Failed to get local user cosmetics (return code = {cosmeticsReturnCode})";
				Debug.LogError(message);
				return Result<bool>.NewError(CoreError.InternalError, message);
			}
			}
		});
	}

	public async Task<Result<bool>> GetUserCosmeticsForSessionAsync(IEnumerable<string> playerIds, string sessionId)
	{
		if (!(await _featureFlagManager.IsEnabledAsync()))
		{
			return Result<bool>.NewSuccess(result: true);
		}
		IReadOnlyCollection<IntPtr> playerIdUtf8StringPtrs = InteropUtilities.AllocateUnmanagedStringPointers(playerIds, Encoding.UTF8);
		IntPtr playerIdsArrayPointer = InteropUtilities.AllocateUnmanagedArray(playerIdUtf8StringPtrs);
		return await Task.Run(delegate
		{
			try
			{
				IntPtr intPtr = InteropUtilities.StringToUTF8Pointer(sessionId);
				CosmeticsReturnCode cosmeticsReturnCode = LckCoreCosmeticsNative.get_user_cosmetics_for_session(playerIdsArrayPointer, (UIntPtr)(ulong)playerIdUtf8StringPtrs.Count, intPtr, OnCosmeticAvailableStatic);
				InteropUtilities.Free(intPtr);
				switch (cosmeticsReturnCode)
				{
				case CosmeticsReturnCode.Ok:
					return Result<bool>.NewSuccess(result: true);
				case CosmeticsReturnCode.FailedToCacheCosmetics:
					return Result<bool>.NewError(CoreError.FailedToCacheCosmetics, "Failed to cache one or more cosmetics");
				default:
				{
					string message = $"Failed to get user cosmetics for session (return code = {cosmeticsReturnCode})";
					Debug.LogError(message);
					return Result<bool>.NewError(CoreError.InternalError, message);
				}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				return Result<bool>.NewError(CoreError.InternalError, ex.Message);
			}
			finally
			{
				foreach (IntPtr item in playerIdUtf8StringPtrs)
				{
					Marshal.FreeHGlobal(item);
				}
				Marshal.FreeHGlobal(playerIdsArrayPointer);
			}
		});
	}

	public async Task<Result<bool>> AnnouncePlayerPresenceForSessionAsync(string playerId, string sessionId)
	{
		if (!(await _featureFlagManager.IsEnabledAsync()))
		{
			return Result<bool>.NewSuccess(result: true);
		}
		_sessionId = sessionId;
		return await Task.Run(delegate
		{
			IntPtr intPtr = InteropUtilities.StringToUTF8Pointer(playerId);
			IntPtr intPtr2 = InteropUtilities.StringToUTF8Pointer(sessionId);
			CosmeticsReturnCode cosmeticsReturnCode = LckCoreCosmeticsNative.announce_player_presence_for_session(intPtr, intPtr2, OnPresenceAnnouncementExpiryReceivedStatic);
			InteropUtilities.Free(intPtr);
			InteropUtilities.Free(intPtr2);
			switch (cosmeticsReturnCode)
			{
			case CosmeticsReturnCode.Ok:
				return Result<bool>.NewSuccess(result: true);
			case CosmeticsReturnCode.Unauthorized:
				return Result<bool>.NewSuccess(result: false);
			default:
			{
				string message = $"Failed to announce player presence for session (return code = {cosmeticsReturnCode})";
				Debug.LogError(message);
				return Result<bool>.NewError(CoreError.InternalError, message);
			}
			}
		});
	}

	[MonoPInvokeCallback(typeof(LckCoreCosmeticsNative.get_user_cosmetics_for_session_on_cosmetic_available_delegate))]
	private static void OnCosmeticAvailableStatic(IntPtr serializedCosmeticDataPtr, UIntPtr serializedDataLength, SerializationType serializationType)
	{
		if (Instance == null)
		{
			Debug.LogError("Cosmetic became available while LckCoreCosmeticsCoordinator is uninitialized");
		}
		else
		{
			Instance.HandleOnCosmeticAvailable(serializedCosmeticDataPtr, serializedDataLength, serializationType);
		}
	}

	private void HandleOnCosmeticAvailable(IntPtr serializedCosmeticDataPtr, UIntPtr serializedDataLength, SerializationType serializationType)
	{
		if (_serializer.SerializationType != serializationType)
		{
			Debug.LogError($"Received cosmetic data in unexpected serialization format: {serializationType} " + $"(expected {_serializer.SerializationType})");
			return;
		}
		byte[] data = InteropUtilities.CopyUnmanagedByteArray(serializedCosmeticDataPtr, (int)(uint)serializedDataLength);
		LckAvailableCosmeticInfo obj = _serializer.Deserialize<LckAvailableCosmeticInfo>(data);
		string seed = "Cosmetic available at " + obj.CosmeticInfo.CosmeticFilepath + " for players:\n";
		seed = obj.PlayerIds.Aggregate(seed, (string current, string playerId) => current + "  - " + playerId);
		Debug.Log(seed);
		this.OnCosmeticAvailable?.Invoke(obj);
	}

	[MonoPInvokeCallback(typeof(LckCoreCosmeticsNative.announce_player_presence_for_session_on_presence_expiry_received_delegate))]
	private static void OnPresenceAnnouncementExpiryReceivedStatic(ulong timeUntilExpirationSeconds)
	{
		if (Instance == null)
		{
			Debug.LogError("Player presence was announced while LckCoreCosmeticsCoordinator is uninitialized");
		}
		else
		{
			Instance.HandlePresenceAnnouncement(timeUntilExpirationSeconds);
		}
	}

	private void HandlePresenceAnnouncement(ulong expirationTimeSeconds)
	{
		_reannounceCancellationTokenSource?.Cancel();
		_reannounceCancellationTokenSource = new CancellationTokenSource();
		TimeSpan reannounceDelay = TimeSpan.FromSeconds((double)expirationTimeSeconds * 0.9);
		ReannouncePresenceAfterDelay(reannounceDelay, _reannounceCancellationTokenSource.Token);
	}

	private async Task ReannouncePresenceAfterDelay(TimeSpan reannounceDelay, CancellationToken cancellationToken)
	{
		_ = 1;
		try
		{
			await Task.Delay(reannounceDelay, cancellationToken);
			if (!cancellationToken.IsCancellationRequested)
			{
				Result<bool> result = await AnnouncePlayerPresenceForSessionAsync(_playerId, _sessionId);
				if (!result.IsOk)
				{
					Debug.LogError($"Failed to re-announce player presence: {result.Err}");
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
