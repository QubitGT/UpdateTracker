using System;
using System.Runtime.InteropServices;
using Meta.XR.Audio;
using UnityEngine;

public class MetaXRAudioNativeInterface
{
	public enum ovrAudioScalarType : uint
	{
		Int8,
		UInt8,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Float16,
		Float32,
		Float64
	}

	public interface NativeInterface
	{
		int SetAdvancedBoxRoomParameters(float width, float height, float depth, bool lockToListenerPosition, Vector3 position, float[] wallMaterials);

		int SetSharedReverbWetLevel(float linearLevel);

		int SetEnabled(int feature, bool enabled);

		int SetRoomClutterFactor(float[] clutterFactor);

		int SetDynamicRoomRaysPerSecond(int RaysPerSecond);

		int SetDynamicRoomInterpSpeed(float InterpSpeed);

		int SetDynamicRoomMaxWallDistance(float MaxWallDistance);

		int SetDynamicRoomRaysRayCacheSize(int RayCacheSize);

		int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);

		int GetRaycastHits(Vector3[] points, Vector3[] normals, int length);
	}

	public class UnityNativeInterface : NativeInterface
	{
		public const string binaryName = "MetaXRAudioUnity";

		private IntPtr context_ = IntPtr.Zero;

		private IntPtr context
		{
			get
			{
				if (context_ == IntPtr.Zero)
				{
					ovrAudio_GetPluginContext(out context_);
				}
				return context_;
			}
		}

		[DllImport("MetaXRAudioUnity")]
		public static extern int ovrAudio_GetPluginContext(out IntPtr context);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth, bool lockToListenerPosition, float positionX, float positionY, float positionZ, float[] wallMaterials);

		public int SetAdvancedBoxRoomParameters(float width, float height, float depth, bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
		{
			return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth, lockToListenerPosition, position.x, position.y, 0f - position.z, wallMaterials);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);

		public int SetRoomClutterFactor(float[] clutterFactor)
		{
			return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetSharedReverbWetLevel(IntPtr context, float linearLevel);

		public int SetSharedReverbWetLevel(float linearLevel)
		{
			return ovrAudio_SetSharedReverbWetLevel(context, linearLevel);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);

		public int SetEnabled(int feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);

		public int SetEnabled(EnableFlag feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);

		public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
		{
			return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);

		public int SetDynamicRoomInterpSpeed(float InterpSpeed)
		{
			return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);

		public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
		{
			return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);

		public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
		{
			return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);

		public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
		{
			return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);

		public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
		{
			return ovrAudio_GetRaycastHits(context, points, normals, length);
		}
	}

	public class WwisePluginInterface : NativeInterface
	{
		public const string binaryName = "MetaXRAudioWwise";

		private IntPtr context_ = IntPtr.Zero;

		private IntPtr context
		{
			get
			{
				if (context_ == IntPtr.Zero)
				{
					context_ = getOrCreateGlobalOvrAudioContext();
				}
				return context_;
			}
		}

		[DllImport("MetaXRAudioWwise")]
		public static extern IntPtr getOrCreateGlobalOvrAudioContext();

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth, bool lockToListenerPosition, float positionX, float positionY, float positionZ, float[] wallMaterials);

		public int SetAdvancedBoxRoomParameters(float width, float height, float depth, bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
		{
			return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth, lockToListenerPosition, position.x, position.y, 0f - position.z, wallMaterials);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);

		public int SetRoomClutterFactor(float[] clutterFactor)
		{
			return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetSharedReverbWetLevel(IntPtr context, float linearLevel);

		public int SetSharedReverbWetLevel(float linearLevel)
		{
			return ovrAudio_SetSharedReverbWetLevel(context, linearLevel);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);

		public int SetEnabled(int feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);

		public int SetEnabled(EnableFlag feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);

		public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
		{
			return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);

		public int SetDynamicRoomInterpSpeed(float InterpSpeed)
		{
			return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);

		public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
		{
			return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);

		public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
		{
			return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);

		public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
		{
			return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);

		public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
		{
			return ovrAudio_GetRaycastHits(context, points, normals, length);
		}
	}

	public class FMODPluginInterface : NativeInterface
	{
		public const string binaryName = "MetaXRAudioFMOD";

		private IntPtr context_ = IntPtr.Zero;

		private IntPtr context
		{
			get
			{
				if (context_ == IntPtr.Zero)
				{
					ovrAudio_GetPluginContext(out context_);
				}
				return context_;
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		public static extern int ovrAudio_GetPluginContext(out IntPtr context);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth, bool lockToListenerPosition, float positionX, float positionY, float positionZ, float[] wallMaterials);

		public int SetAdvancedBoxRoomParameters(float width, float height, float depth, bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
		{
			return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth, lockToListenerPosition, position.x, position.y, 0f - position.z, wallMaterials);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);

		public int SetRoomClutterFactor(float[] clutterFactor)
		{
			return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetSharedReverbWetLevel(IntPtr context, float linearLevel);

		public int SetSharedReverbWetLevel(float linearLevel)
		{
			return ovrAudio_SetSharedReverbWetLevel(context, linearLevel);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);

		public int SetEnabled(int feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);

		public int SetEnabled(EnableFlag feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);

		public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
		{
			return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);

		public int SetDynamicRoomInterpSpeed(float InterpSpeed)
		{
			return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);

		public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
		{
			return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);

		public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
		{
			return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);

		public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
		{
			return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);

		public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
		{
			return ovrAudio_GetRaycastHits(context, points, normals, length);
		}
	}

	private static NativeInterface CachedInterface;

	public static NativeInterface Interface
	{
		get
		{
			if (CachedInterface == null)
			{
				CachedInterface = FindInterface();
			}
			return CachedInterface;
		}
	}

	private static NativeInterface FindInterface()
	{
		IntPtr context;
		try
		{
			context = WwisePluginInterface.getOrCreateGlobalOvrAudioContext();
			Debug.Log("Meta XR Audio Native Interface initialized with Wwise plugin");
			return new WwisePluginInterface();
		}
		catch (DllNotFoundException)
		{
		}
		try
		{
			FMODPluginInterface.ovrAudio_GetPluginContext(out context);
			Debug.Log("Meta XR Audio Native Interface initialized with FMOD plugin");
			return new FMODPluginInterface();
		}
		catch (DllNotFoundException)
		{
		}
		Debug.Log("Meta XR Audio Native Interface initialized with Unity plugin");
		return new UnityNativeInterface();
	}
}
