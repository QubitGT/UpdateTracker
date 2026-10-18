using System;
using System.Runtime.InteropServices;
using Meta.XR.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-customize-passthrough-color-mapping/#color-look-up-tables-luts")]
[Feature(Feature.Passthrough)]
public class OVRPassthroughColorLut : IDisposable
{
	public enum ColorChannels
	{
		Rgb = 1,
		Rgba
	}

	private struct WriteColorsAsBytesJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<byte> target;

		[NativeDisableParallelForRestriction]
		[ReadOnly]
		public NativeArray<Color> source;

		public int channelCount;

		public void Execute(int index)
		{
			for (int i = 0; i < channelCount; i++)
			{
				target[index * channelCount + i] = (byte)Mathf.Min(source[index][i] * 255f, 255f);
			}
		}
	}

	private static class ColorLutTextureConverter
	{
		private struct MapColorValuesJob : IJobParallelFor
		{
			public TextureSettings settings;

			[NativeDisableParallelForRestriction]
			[WriteOnly]
			public NativeArray<byte> target;

			[NativeDisableParallelForRestriction]
			[ReadOnly]
			public NativeArray<byte> source;

			public void Execute(int index)
			{
				int num = index / settings.Resolution;
				int num2 = index % settings.Resolution;
				int num3 = num % settings.SlicesPerRow;
				int num4 = (int)Mathf.Floor(num / settings.SlicesPerRow);
				int num5 = num2 + num4 * settings.Resolution;
				int num6 = (settings.FlipY ? (settings.Height - num5 - 1) : num5);
				int num7 = (num3 * settings.Resolution + num6 * settings.Width) * settings.ChannelCount;
				int num8 = (num * settings.Resolution * settings.Resolution + num2 * settings.Resolution) * settings.ChannelCount;
				for (int i = 0; i < settings.Resolution * settings.ChannelCount; i++)
				{
					target[num8 + i] = source[num7 + i];
				}
			}
		}

		private struct TextureSettings
		{
			public int Width { get; }

			public int Height { get; }

			public int Resolution { get; }

			public int SlicesPerRow { get; }

			public int ChannelCount { get; }

			public bool FlipY { get; }

			public TextureSettings(int width, int height, int resolution, int slicesPerRow, int channelCount, bool flipY)
			{
				Width = width;
				Height = height;
				Resolution = resolution;
				SlicesPerRow = slicesPerRow;
				ChannelCount = channelCount;
				FlipY = flipY;
			}
		}

		public static void TextureToColorByteMap(Texture2D lut, int channelCount, byte[] target, bool flipY)
		{
			MapColorValues(GetTextureSettings(lut, channelCount, flipY), lut.GetPixelData<byte>(0), target);
		}

		private static void MapColorValues(TextureSettings settings, NativeArray<byte> source, byte[] target)
		{
			using NativeArray<byte> target2 = new NativeArray<byte>(target, Allocator.TempJob);
			MapColorValuesJob jobData = default(MapColorValuesJob);
			jobData.settings = settings;
			jobData.source = source;
			jobData.target = target2;
			jobData.Schedule(settings.Resolution * settings.Resolution, settings.Resolution).Complete();
			target2.CopyTo(target);
		}

		private static TextureSettings GetTextureSettings(Texture2D lut, int channelCount, bool flipY)
		{
			if (TryGetTextureLayout(lut.width, lut.height, out var resolution, out var slicesPerRow, out var errorMessage))
			{
				return new TextureSettings(lut.width, lut.height, resolution, slicesPerRow, channelCount, flipY);
			}
			throw new Exception(errorMessage);
		}

		internal static bool TryGetTextureLayout(int width, int height, out int resolution, out int slicesPerRow, out string errorMessage)
		{
			resolution = -1;
			slicesPerRow = -1;
			if (width == height)
			{
				float num = Mathf.Pow(width, 2f / 3f);
				if ((double)Mathf.Abs(num - Mathf.Round(num)) > 0.001)
				{
					errorMessage = "Texture layout is not compatible for color LUTs: the dimensions don't result in a power-of-two resolution for the LUT. Acceptable image sizes are e.g. 64 (for a LUT resolution of 16) or 512 (for a LUT resolution of 64).";
					return false;
				}
				resolution = (int)Mathf.Round(num);
				slicesPerRow = (int)Mathf.Sqrt(resolution);
			}
			else
			{
				if (width != height * height)
				{
					errorMessage = "Texture layout is not compatible for color LUTs: for horizontal layouts, the Width is expected to be equal to Height * Height.";
					return false;
				}
				resolution = height;
				slicesPerRow = resolution;
			}
			errorMessage = string.Empty;
			return true;
		}
	}

	private enum CreateState
	{
		Invalid,
		Pending,
		Created
	}

	private const int RecomendedBatchSize = 128;

	internal ulong _colorLutHandle;

	private GCHandle _allocHandle;

	private OVRPlugin.PassthroughColorLutData _lutData;

	private int _channelCount;

	private byte[] _colorBytes;

	private object _locker = new object();

	private CreateState _createState;

	public uint Resolution { get; private set; }

	public ColorChannels Channels { get; private set; }

	[Obsolete("IsInitialized is deprecated. Use IsValid instead.", false)]
	public bool IsInitialized => IsValid;

	public bool IsValid => _createState != CreateState.Invalid;

	public OVRPassthroughColorLut(Texture2D initialLutTexture, bool flipY = true)
		: this(GetTextureSize(initialLutTexture), GetChannelsForTextureFormat(initialLutTexture.format))
	{
		Create(CreateLutDataFromTexture(initialLutTexture, flipY));
	}

	public OVRPassthroughColorLut(Color[] initialColorLut, ColorChannels channels)
		: this(GetArraySize(initialColorLut), channels)
	{
		Create(CreateLutDataFromArray(initialColorLut));
	}

	public OVRPassthroughColorLut(Color32[] initialColorLut, ColorChannels channels)
		: this(GetArraySize(initialColorLut), channels)
	{
		Create(CreateLutDataFromArray(initialColorLut));
	}

	public OVRPassthroughColorLut(byte[] initialColorLut, ColorChannels channels)
		: this(GetTextureSizeFromByteArray(initialColorLut, channels), channels)
	{
		Create(CreateLutDataFromArray(initialColorLut));
	}

	public void UpdateFrom(Color[] colors)
	{
		if (IsValidLutUpdate(colors, _channelCount))
		{
			WriteColorsAsBytes(colors, _colorBytes);
			OVRPlugin.UpdatePassthroughColorLut(_colorLutHandle, _lutData);
		}
	}

	public void UpdateFrom(Color32[] colors)
	{
		if (IsValidLutUpdate(colors, _channelCount))
		{
			WriteColorsAsBytes(colors, _colorBytes);
			OVRPlugin.UpdatePassthroughColorLut(_colorLutHandle, _lutData);
		}
	}

	public void UpdateFrom(byte[] colors)
	{
		if (IsValidLutUpdate(colors, 1))
		{
			colors.CopyTo(_colorBytes, 0);
			OVRPlugin.UpdatePassthroughColorLut(_colorLutHandle, _lutData);
		}
	}

	public void UpdateFrom(Texture2D lutTexture, bool flipY = true)
	{
		if (IsValidUpdateResolution(GetTextureSize(lutTexture), _channelCount))
		{
			ColorLutTextureConverter.TextureToColorByteMap(lutTexture, _channelCount, _colorBytes, flipY);
			OVRPlugin.UpdatePassthroughColorLut(_colorLutHandle, _lutData);
		}
	}

	public void Dispose()
	{
		if (IsValid)
		{
			OVRManager.OnPassthroughInitializedStateChange = (Action<bool>)Delegate.Remove(OVRManager.OnPassthroughInitializedStateChange, new Action<bool>(RefreshIfInitialized));
		}
		Destroy();
		FreeAllocHandle();
	}

	private void FreeAllocHandle()
	{
		_ = _allocHandle;
		if (_allocHandle.IsAllocated)
		{
			_allocHandle.Free();
		}
	}

	public static bool IsTextureSupported(Texture2D texture, out string errorMessage)
	{
		try
		{
			GetChannelsForTextureFormat(texture.format);
		}
		catch (ArgumentException ex)
		{
			errorMessage = ex.Message;
			return false;
		}
		if (!ColorLutTextureConverter.TryGetTextureLayout(texture.width, texture.height, out var _, out var _, out var errorMessage2))
		{
			errorMessage = errorMessage2;
			return false;
		}
		int size = texture.width * texture.height;
		if (!IsResolutionAccepted(GetResolutionFromSize(size), size, out var errorMessage3))
		{
			errorMessage = errorMessage3;
			return false;
		}
		errorMessage = string.Empty;
		return true;
	}

	private OVRPassthroughColorLut(int size, ColorChannels channels)
	{
		Channels = channels;
		Resolution = GetResolutionFromSize(size);
		_channelCount = ChannelsToCount(channels);
		if (!IsResolutionAccepted(Resolution, size, out var errorMessage))
		{
			throw new ArgumentException(errorMessage);
		}
		OVRManager.PassthroughCapabilities passthroughCapabilities = OVRManager.GetPassthroughCapabilities();
		if (passthroughCapabilities != null)
		{
			if (passthroughCapabilities.MaxColorLutResolution == 0)
			{
				throw new Exception("Passthrough Color LUTs are not supported.");
			}
			if (Resolution > passthroughCapabilities.MaxColorLutResolution)
			{
				throw new Exception($"Color LUT resolution {Resolution} exceeds the maximum of {passthroughCapabilities.MaxColorLutResolution}.");
			}
		}
		else
		{
			Debug.LogWarning("Unable to validate the maximum LUT resolution. Please instantiate OVRPassthroughColorLut after initializing the Oculus XR Plugin.");
		}
	}

	private bool IsValidUpdateResolution(int lutSize, int elementByteSize)
	{
		if (!IsValid)
		{
			Debug.LogError("Can not update an uninitialized lut object.");
			return false;
		}
		if (GetResolutionFromSize(lutSize * elementByteSize / _channelCount) != Resolution)
		{
			Debug.LogError($"Can only update with the same resolution of {Resolution}.");
			return false;
		}
		return true;
	}

	private bool IsValidLutUpdate<T>(T[] colorArray, int elementByteSize)
	{
		int arraySize = GetArraySize(colorArray);
		if (!IsValidUpdateResolution(arraySize, elementByteSize))
		{
			return false;
		}
		if (arraySize * elementByteSize != _colorBytes.Length)
		{
			Debug.LogError("New color byte array doesn't match LUT size.");
			return false;
		}
		return true;
	}

	private static ColorChannels GetChannelsForTextureFormat(TextureFormat format)
	{
		return format switch
		{
			TextureFormat.RGB24 => ColorChannels.Rgb, 
			TextureFormat.RGBA32 => ColorChannels.Rgba, 
			_ => throw new ArgumentException($"Texture format {format} not supported for Color LUTs. Supported formats are RGB24 and RGBA32."), 
		};
	}

	private static int GetTextureSizeFromByteArray(byte[] initialColorLut, ColorChannels channels)
	{
		int arraySize = GetArraySize(initialColorLut);
		int num = ChannelsToCount(channels);
		if (arraySize % num != 0)
		{
			throw new ArgumentException($"Invalid byte array given, {num} bytes required for each color for {channels} color channels.");
		}
		return initialColorLut.Length / num;
	}

	private static int GetTextureSize(Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("Lut texture is undefined.");
		}
		return texture.width * texture.height;
	}

	private static int GetArraySize<T>(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("Lut " + typeof(T).Name + " array is undefined.");
		}
		return array.Length;
	}

	private static int ChannelsToCount(ColorChannels channels)
	{
		if (channels != ColorChannels.Rgb)
		{
			return 4;
		}
		return 3;
	}

	private static bool IsResolutionAccepted(uint resolution, int size, out string errorMessage)
	{
		if (!IsPowerOfTwo(resolution))
		{
			errorMessage = "Color LUT texture resolution should be a power of 2.";
			return false;
		}
		if (resolution * resolution * resolution != size)
		{
			errorMessage = "Unexpected LUT resolution, LUT size should be resolution in a power of 3.";
			return false;
		}
		errorMessage = string.Empty;
		return true;
	}

	private static bool IsPowerOfTwo(uint x)
	{
		if (x != 0)
		{
			return (x & (x - 1)) == 0;
		}
		return false;
	}

	private void Create(OVRPlugin.PassthroughColorLutData lutData)
	{
		_lutData = lutData;
		if (OVRManager.IsInsightPassthroughInitialized())
		{
			InternalCreate();
		}
		else
		{
			_createState = CreateState.Pending;
		}
		if (IsValid)
		{
			OVRManager.OnPassthroughInitializedStateChange = (Action<bool>)Delegate.Combine(OVRManager.OnPassthroughInitializedStateChange, new Action<bool>(RefreshIfInitialized));
		}
	}

	private void RefreshIfInitialized(bool isInitialized)
	{
		if (isInitialized)
		{
			Recreate();
		}
	}

	private void Recreate()
	{
		Destroy();
		InternalCreate();
	}

	private void InternalCreate()
	{
		bool flag = OVRPlugin.CreatePassthroughColorLut((OVRPlugin.PassthroughColorLutChannels)Channels, Resolution, _lutData, out _colorLutHandle);
		_createState = (flag ? CreateState.Created : CreateState.Invalid);
		if (!IsValid)
		{
			Debug.LogError("Failed to create Passthrough Color LUT.");
		}
	}

	private static uint GetResolutionFromSize(int size)
	{
		return (uint)Mathf.Round(Mathf.Pow(size, 1f / 3f));
	}

	private OVRPlugin.PassthroughColorLutData CreateLutData(out byte[] colorBytes)
	{
		OVRPlugin.PassthroughColorLutData result = default(OVRPlugin.PassthroughColorLutData);
		result.BufferSize = (uint)(Resolution * Resolution * Resolution * _channelCount);
		colorBytes = new byte[result.BufferSize];
		_allocHandle = GCHandle.Alloc(colorBytes, GCHandleType.Pinned);
		result.Buffer = _allocHandle.AddrOfPinnedObject();
		return result;
	}

	private OVRPlugin.PassthroughColorLutData CreateLutDataFromTexture(Texture2D lut, bool flipY)
	{
		OVRPlugin.PassthroughColorLutData result = CreateLutData(out _colorBytes);
		ColorLutTextureConverter.TextureToColorByteMap(lut, _channelCount, _colorBytes, flipY);
		return result;
	}

	private OVRPlugin.PassthroughColorLutData CreateLutDataFromArray(Color[] colors)
	{
		OVRPlugin.PassthroughColorLutData result = CreateLutData(out _colorBytes);
		WriteColorsAsBytes(colors, _colorBytes);
		return result;
	}

	private OVRPlugin.PassthroughColorLutData CreateLutDataFromArray(Color32[] colors)
	{
		OVRPlugin.PassthroughColorLutData result = CreateLutData(out _colorBytes);
		WriteColorsAsBytes(colors, _colorBytes);
		return result;
	}

	private OVRPlugin.PassthroughColorLutData CreateLutDataFromArray(byte[] colors)
	{
		OVRPlugin.PassthroughColorLutData result = CreateLutData(out _colorBytes);
		colors.CopyTo(_colorBytes, 0);
		return result;
	}

	private void WriteColorsAsBytes(Color[] colors, byte[] target)
	{
		using NativeArray<Color> source = new NativeArray<Color>(colors, Allocator.TempJob);
		using NativeArray<byte> target2 = new NativeArray<byte>(target, Allocator.TempJob);
		WriteColorsAsBytesJob jobData = default(WriteColorsAsBytesJob);
		jobData.source = source;
		jobData.target = target2;
		jobData.channelCount = _channelCount;
		jobData.Schedule(source.Length, 128).Complete();
		target2.CopyTo(target);
	}

	private void WriteColorsAsBytes(Color32[] colors, byte[] target)
	{
		for (int i = 0; i < colors.Length; i++)
		{
			for (int j = 0; j < _channelCount; j++)
			{
				target[i * _channelCount + j] = colors[i][j];
			}
		}
	}

	~OVRPassthroughColorLut()
	{
		Dispose();
	}

	private void Destroy()
	{
		if (_createState == CreateState.Created)
		{
			lock (_locker)
			{
				OVRPlugin.DestroyPassthroughColorLut(_colorLutHandle);
			}
		}
		_createState = CreateState.Invalid;
	}
}
