using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Oculus.Haptics;

public class Ffi
{
	public enum Result
	{
		Success = 0,
		Error = -1,
		InstanceInitializationFailed = -2,
		InstanceAlreadyInitialized = -3,
		InstanceAlreadyUninitialized = -4,
		InstanceNotInitialized = -5,
		InvalidUtf8 = -6,
		LoadClipFailed = -7,
		CreatePlayerFailed = -8,
		ClipIdInvalid = -9,
		PlayerIdInvalid = -10,
		PlayerInvalidAmplitude = -11,
		PlayerInvalidFrequencyShift = -12,
		PlayerInvalidPriority = -13,
		NoClipLoaded = -14,
		InvalidPlayCallbackPointer = -15,
		PlayerInvalidSeekPosition = -16
	}

	public struct SdkVersion
	{
		public ushort major;

		public ushort minor;

		public ushort patch;
	}

	public enum Controller
	{
		Left,
		Right,
		Both
	}

	public enum LogLevel
	{
		Trace,
		Debug,
		Info,
		Warn,
		Error
	}

	public delegate void LogCallback(LogLevel level, string message);

	public delegate void HapticsSdkPlayCallback(IntPtr context, Controller controller, float duration, float amplitude);

	public struct NullBackendStatistics
	{
		public long stream_count;

		public long play_call_count;
	}

	private const string NativeLibName = "haptics_sdk";

	public const int InvalidId = -1;

	public static bool Succeeded(Result result)
	{
		return result >= Result.Success;
	}

	public static bool Failed(Result result)
	{
		return result < Result.Success;
	}

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_version")]
	public static extern SdkVersion version();

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_initialize_logging")]
	public static extern Result initialize_logging([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback? logCallback);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_initialize_with_callback_backend")]
	public static extern Result initialize_with_callback_backend(IntPtr context, [MarshalAs(UnmanagedType.FunctionPtr)] HapticsSdkPlayCallback? playCallback);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_initialize_with_ovr_plugin")]
	private static extern Result initialize_with_ovr_plugin_bytes([In] byte[] game_engine_name, [In] byte[] game_engine_version, [In] byte[] game_engine_haptics_sdk_version);

	public static Result initialize_with_ovr_plugin(string game_engine_name, string game_engine_version, string game_engine_haptics_sdk_version)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(game_engine_name + "\0");
		byte[] bytes2 = Encoding.UTF8.GetBytes(game_engine_version + "\0");
		byte[] bytes3 = Encoding.UTF8.GetBytes(game_engine_haptics_sdk_version + "\0");
		return initialize_with_ovr_plugin_bytes(bytes, bytes2, bytes3);
	}

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_initialize_with_null_backend")]
	public static extern Result initialize_with_null_backend();

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_uninitialize")]
	public static extern Result uninitialize();

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_initialized")]
	public static extern Result initialized(out bool initialized);

	[DllImport("haptics_sdk")]
	private static extern IntPtr haptics_sdk_error_message();

	[DllImport("haptics_sdk")]
	private static extern int haptics_sdk_error_message_length();

	public static string error_message()
	{
		IntPtr intPtr = haptics_sdk_error_message();
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("No error message is available");
		}
		int num = haptics_sdk_error_message_length();
		byte[] array = new byte[num];
		Marshal.Copy(intPtr, array, 0, num);
		return Encoding.UTF8.GetString(array);
	}

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_set_suspended")]
	public static extern Result set_suspended(bool suspended);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_load_clip")]
	private static extern Result load_clip_bytes([In] byte[] data, uint data_length, out int clip_id_out);

	public static Result load_clip(string data, out int clip_id_out)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(data);
		return load_clip_bytes(bytes, (uint)bytes.Length, out clip_id_out);
	}

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_clip_duration")]
	public static extern Result clip_duration(int clipId, out float clip_duration);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_release_clip")]
	public static extern Result release_clip(int clipId);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_create_player")]
	public static extern Result create_player(out int player_id);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_release_player")]
	public static extern Result release_player(int playerId);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_set_clip")]
	public static extern Result player_set_clip(int playerId, int clipId);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_play")]
	public static extern Result player_play(int playerId, Controller controller);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_pause")]
	public static extern Result player_pause(int playerId);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_resume")]
	public static extern Result player_resume(int playerId);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_stop")]
	public static extern Result player_stop(int playerId);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_seek")]
	public static extern Result player_seek(int playerId, float time);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_set_amplitude")]
	public static extern Result player_set_amplitude(int playerId, float amplitude);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_amplitude")]
	public static extern Result player_amplitude(int playerId, out float amplitude);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_set_frequency_shift")]
	public static extern Result player_set_frequency_shift(int playerId, float amount);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_frequency_shift")]
	public static extern Result player_frequency_shift(int playerId, out float frequency_shift);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_set_looping_enabled")]
	public static extern Result player_set_looping_enabled(int playerId, bool enabled);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_looping_enabled")]
	public static extern Result player_looping_enabled(int playerId, out bool looping_enabled);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_set_priority")]
	public static extern Result player_set_priority(int playerId, uint priority);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_player_priority")]
	public static extern Result player_priority(int playerId, out uint priority);

	[DllImport("haptics_sdk", EntryPoint = "haptics_sdk_get_null_backend_statistics")]
	public static extern NullBackendStatistics get_null_backend_statistics();
}
