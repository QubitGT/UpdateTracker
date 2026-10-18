using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Liv.Lck.Core.FFI;
using UnityEngine;

namespace Liv.Lck.Core;

public static class LckCore
{
	private static readonly object _loginLock = new object();

	private static ReturnCode _lastReturnCode;

	private static string _loginCode;

	[MonoPInvokeCallback(typeof(LckCoreNative.start_login_attempt_callback_delegate))]
	private static void StartLoginAttemptCallback(ReturnCode returnCode, IntPtr loginCodePtr)
	{
		lock (_loginLock)
		{
			_lastReturnCode = returnCode;
			if (returnCode == ReturnCode.Ok)
			{
				_loginCode = InteropUtilities.UTF8PointerToString(loginCodePtr);
			}
		}
	}

	public static void SetMaxLogLevel(LevelFilter levelFilter)
	{
		LckCoreNative.set_max_log_level(levelFilter);
	}

	public static Result<bool> Initialize(string trackingId, GameInfo gameInfo, LckInfo lckInfo)
	{
		if (string.IsNullOrEmpty(trackingId))
		{
			return Result<bool>.NewError(CoreError.MissingTrackingId, "Tracking ID cannot be null or empty.");
		}
		IntPtr intPtr = InteropUtilities.StringToUTF8Pointer(trackingId);
		Liv.Lck.Core.FFI.GameInfo game_info = Liv.Lck.Core.FFI.GameInfo.AllocateFromGameInfo(gameInfo);
		Liv.Lck.Core.FFI.LckInfo lck_info = Liv.Lck.Core.FFI.LckInfo.AllocateFromLckInfo(lckInfo);
		ReturnCode returnCode;
		try
		{
			returnCode = LckCoreNative.initialize(intPtr, game_info, lck_info);
		}
		finally
		{
			InteropUtilities.Free(intPtr);
			game_info.Free();
			lck_info.Free();
		}
		return returnCode switch
		{
			ReturnCode.Ok => Result<bool>.NewSuccess(result: true), 
			ReturnCode.InvalidArgument => Result<bool>.NewError(CoreError.InvalidArgument, "Invalid argument provided to initialize LckCore."), 
			ReturnCode.InvalidTrackingId => Result<bool>.NewError(CoreError.InvalidTrackingId, "Provided Tracking ID is not valid."), 
			_ => Result<bool>.NewError(CoreError.InternalError, $"Failed to initialize LckCore: {returnCode}"), 
		};
	}

	public static async Task<Result<bool>> HasUserConfiguredStreaming()
	{
		ReturnCode returnCode = ReturnCode.Ok;
		bool hasConfigured = false;
		IntPtr hasConfiguredPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bool)));
		Marshal.WriteByte(hasConfiguredPtr, 0);
		await Task.Run(delegate
		{
			returnCode = LckCoreNative.has_user_configured_streaming(hasConfiguredPtr);
		});
		if (returnCode == ReturnCode.Ok)
		{
			hasConfigured = Marshal.ReadByte(hasConfiguredPtr) != 0;
		}
		Marshal.FreeHGlobal(hasConfiguredPtr);
		if (returnCode != 0)
		{
			var (error, message) = MapReturnCodeToCoreError(returnCode);
			return Result<bool>.NewError(error, message);
		}
		return Result<bool>.NewSuccess(hasConfigured);
	}

	private static (CoreError, string) MapReturnCodeToCoreError(ReturnCode returnCode)
	{
		return returnCode switch
		{
			ReturnCode.UserNotLoggedIn => (CoreError.UserNotLoggedIn, "User is not logged in."), 
			ReturnCode.BackendUnavailable => (CoreError.ServiceUnavailable, "LIV backend service is unavailable."), 
			ReturnCode.RateLimiterBackoff => (CoreError.RateLimiterBackoff, "Client is in rate limiter backoff due to previous errors."), 
			_ => (CoreError.InternalError, $"Operation failed with return code: {returnCode}"), 
		};
	}

	public static async Task<Result<bool>> IsUserSubscribed()
	{
		ReturnCode returnCode = ReturnCode.Ok;
		bool isSubscribed = false;
		IntPtr isSubscribedPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bool)));
		Marshal.WriteByte(isSubscribedPtr, 0);
		await Task.Run(delegate
		{
			returnCode = LckCoreNative.is_user_subscribed(isSubscribedPtr);
		});
		if (returnCode == ReturnCode.Ok)
		{
			isSubscribed = Marshal.ReadByte(isSubscribedPtr) != 0;
		}
		Marshal.FreeHGlobal(isSubscribedPtr);
		if (returnCode != 0)
		{
			var (error, message) = MapReturnCodeToCoreError(returnCode);
			return Result<bool>.NewError(error, message);
		}
		return Result<bool>.NewSuccess(isSubscribed);
	}

	public static async Task<Result<string>> StartLoginAttemptAsync()
	{
		Debug.Log("LCK: Starting login attempt task...");
		await Task.Run(delegate
		{
			lock (_loginLock)
			{
				_loginCode = null;
				_lastReturnCode = ReturnCode.Ok;
			}
			LckCoreNative.start_login_attempt(StartLoginAttemptCallback);
		});
		ReturnCode lastReturnCode;
		string loginCode;
		lock (_loginLock)
		{
			lastReturnCode = _lastReturnCode;
			loginCode = _loginCode;
		}
		Debug.Log($"LCK: Login attempt task completed with return code: {lastReturnCode}");
		if (lastReturnCode != 0 || loginCode == null)
		{
			var (error, message) = MapReturnCodeToCoreError(lastReturnCode);
			return Result<string>.NewError(error, message);
		}
		return Result<string>.NewSuccess(loginCode);
	}

	public static async Task<Result<bool>> CheckLoginCompletedAsync()
	{
		ReturnCode returnCode = ReturnCode.Ok;
		bool isComplete = false;
		Debug.Log("LCK: Starting check login completed task...");
		await Task.Run(delegate
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bool)));
			Marshal.WriteByte(intPtr, 0);
			ReturnCode returnCode2 = LckCoreNative.check_login_attempt_completed(intPtr);
			if (returnCode2 != 0)
			{
				returnCode = returnCode2;
			}
			else
			{
				isComplete = Marshal.ReadByte(intPtr) != 0;
			}
			Marshal.FreeHGlobal(intPtr);
		});
		Debug.Log($"LCK: Check login completed task finished with return code: {returnCode}, isComplete: {isComplete}");
		if (returnCode != 0)
		{
			var (error, message) = MapReturnCodeToCoreError(returnCode);
			return Result<bool>.NewError(error, message);
		}
		return Result<bool>.NewSuccess(isComplete);
	}

	public static async Task<Result<float>> GetRemainingBackoffTimeSeconds()
	{
		ReturnCode returnCode = ReturnCode.Error;
		float remainingTime = 0f;
		await Task.Run(delegate
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)));
			Marshal.WriteInt32(intPtr, 0);
			ReturnCode returnCode2 = LckCoreNative.get_remaining_backoff_time_seconds(intPtr);
			if (returnCode2 != 0)
			{
				returnCode = returnCode2;
			}
			else
			{
				remainingTime = Marshal.ReadInt32(intPtr);
			}
			Marshal.FreeHGlobal(intPtr);
		});
		if (returnCode != 0)
		{
			var (error, message) = MapReturnCodeToCoreError(returnCode);
			return Result<float>.NewError(error, message);
		}
		return Result<float>.NewSuccess(remainingTime);
	}

	public static void Log(LogType level, string message, string memberName = "", string filePath = "", int lineNumber = 0)
	{
		IntPtr intPtr = InteropUtilities.StringToUTF8Pointer(message);
		IntPtr intPtr2 = InteropUtilities.StringToUTF8Pointer(memberName);
		IntPtr intPtr3 = InteropUtilities.StringToUTF8Pointer(filePath);
		try
		{
			LckCoreNative.log(level, intPtr, intPtr2, intPtr3, lineNumber);
		}
		finally
		{
			InteropUtilities.Free(intPtr);
			InteropUtilities.Free(intPtr2);
			InteropUtilities.Free(intPtr3);
		}
	}

	public static void Dispose()
	{
		ReturnCode returnCode = LckCoreNative.dispose();
		if (returnCode != 0)
		{
			throw new InvalidOperationException($"Failed to dispose LckCore: {returnCode}");
		}
	}
}
