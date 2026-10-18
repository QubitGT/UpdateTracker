using System;
using System.Runtime.InteropServices;
using Meta.XR.Acoustics;
using UnityEngine;

public class MetaXRAcousticNativeInterface
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

	public interface INativeInterface
	{
		int SetAcousticModel(AcousticModel model);

		int ResetReverb();

		int SetEnabled(int feature, bool enabled);

		int SetEnabled(EnableFlagInternal feature, bool enabled);

		int CreateAudioGeometry(out IntPtr geometry);

		int DestroyAudioGeometry(IntPtr geometry);

		int AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, bool enabled);

		int AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount);

		int AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount, ref MeshSimplification simplification);

		int AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix);

		int AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4);

		int AudioGeometryWriteMeshFile(IntPtr geometry, string filePath);

		int AudioGeometryReadMeshFile(IntPtr geometry, string filePath);

		int AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength);

		int AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath);

		int AudioGeometryGetSimplifiedMesh(IntPtr geometry, out float[] vertices, out uint[] indices, out uint[] materialIndices);

		int AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value);

		int CreateAudioMaterial(out IntPtr material);

		int DestroyAudioMaterial(IntPtr material);

		int AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value);

		int AudioMaterialReset(IntPtr material, MaterialProperty property);

		int CreateAudioSceneIR(out IntPtr sceneIR);

		int DestroyAudioSceneIR(IntPtr sceneIR);

		int AudioSceneIRSetEnabled(IntPtr sceneIR, bool enabled);

		int AudioSceneIRGetEnabled(IntPtr sceneIR, out bool enabled);

		int AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status);

		int InitializeAudioSceneIRParameters(out MapParameters parameters);

		int AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters);

		int AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters);

		int AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount);

		int AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount);

		int AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix);

		int AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4);

		int AudioSceneIRWriteFile(IntPtr sceneIR, string filePath);

		int AudioSceneIRReadFile(IntPtr sceneIR, string filePath);

		int AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength);

		int CreateControlZone(out IntPtr control);

		int DestroyControlZone(IntPtr control);

		int ControlZoneSetEnabled(IntPtr control, bool enabled);

		int ControlZoneGetEnabled(IntPtr control, out bool enabled);

		int ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix);

		int ControlZoneGetTransform(IntPtr control, out float[] matrix4x4);

		int ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		int ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		int ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		int ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		int ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		int ControlZoneReset(IntPtr control, ControlZoneProperty property);
	}

	public class UnityNativeInterface : INativeInterface
	{
		public const string binaryName = "MetaXRAudioUnity";

		private IntPtr context_ = IntPtr.Zero;

		private int version;

		private IntPtr context
		{
			get
			{
				if (context_ == IntPtr.Zero)
				{
					ovrAudio_GetPluginContext(out context_);
					ovrAudio_GetVersion(out var _, out version, out var _);
				}
				return context_;
			}
		}

		[DllImport("MetaXRAudioUnity")]
		public static extern int ovrAudio_GetPluginContext(out IntPtr context);

		[DllImport("MetaXRAudioUnity")]
		public static extern IntPtr ovrAudio_GetVersion(out int Major, out int Minor, out int Patch);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_SetAcousticModel(IntPtr context, AcousticModel quality);

		public int SetAcousticModel(AcousticModel model)
		{
			return ovrAudio_SetAcousticModel(context, model);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ResetSharedReverb(IntPtr context);

		public int ResetReverb()
		{
			return ovrAudio_ResetSharedReverb(context);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);

		public int SetEnabled(int feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_Enable(IntPtr context, EnableFlagInternal what, int enable);

		public int SetEnabled(EnableFlagInternal feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_CreateAudioGeometry(IntPtr context, out IntPtr geometry);

		public int CreateAudioGeometry(out IntPtr geometry)
		{
			return ovrAudio_CreateAudioGeometry(context, out geometry);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_DestroyAudioGeometry(IntPtr geometry);

		public int DestroyAudioGeometry(IntPtr geometry)
		{
			return ovrAudio_DestroyAudioGeometry(geometry);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, int enabled);

		public int AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, bool enabled)
		{
			if (version < 94)
			{
				return -1;
			}
			return ovrAudio_AudioGeometrySetObjectFlag(geometry, flag, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, UIntPtr verticesBytesOffset, UIntPtr vertexCount, UIntPtr vertexStride, ovrAudioScalarType vertexType, int[] indices, UIntPtr indicesByteOffset, UIntPtr indexCount, ovrAudioScalarType indexType, MeshGroup[] groups, UIntPtr groupCount);

		public int AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount)
		{
			return ovrAudio_AudioGeometryUploadMeshArrays(geometry, vertices, UIntPtr.Zero, (UIntPtr)(ulong)vertexCount, UIntPtr.Zero, ovrAudioScalarType.Float32, indices, UIntPtr.Zero, (UIntPtr)(ulong)indexCount, ovrAudioScalarType.UInt32, groups, (UIntPtr)(ulong)groupCount);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, UIntPtr verticesBytesOffset, UIntPtr vertexCount, UIntPtr vertexStride, ovrAudioScalarType vertexType, int[] indices, UIntPtr indicesByteOffset, UIntPtr indexCount, ovrAudioScalarType indexType, MeshGroup[] groups, UIntPtr groupCount, ref MeshSimplification simplification);

		public int AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount, ref MeshSimplification simplification)
		{
			return ovrAudio_AudioGeometryUploadSimplifiedMeshArrays(geometry, vertices, UIntPtr.Zero, (UIntPtr)(ulong)vertexCount, UIntPtr.Zero, ovrAudioScalarType.Float32, indices, UIntPtr.Zero, (UIntPtr)(ulong)indexCount, ovrAudioScalarType.UInt32, groups, (UIntPtr)(ulong)groupCount, ref simplification);
		}

		[DllImport("MetaXRAudioUnity")]
		private unsafe static extern int ovrAudio_AudioGeometrySetTransform(IntPtr geometry, float* matrix4x4);

		public unsafe int AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			return ovrAudio_AudioGeometrySetTransform(geometry, ptr);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4);

		public int AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4)
		{
			return ovrAudio_AudioGeometryGetTransform(geometry, out matrix4x4);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryWriteMeshFile(IntPtr geometry, string filePath);

		public int AudioGeometryWriteMeshFile(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryWriteMeshFile(geometry, filePath);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryReadMeshFile(IntPtr geometry, string filePath);

		public int AudioGeometryReadMeshFile(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryReadMeshFile(geometry, filePath);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength);

		public int AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength)
		{
			return ovrAudio_AudioGeometryReadMeshMemory(geometry, data, dataLength);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath);

		public int AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryWriteMeshFileObj(geometry, filePath);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(IntPtr geometry, IntPtr unused1, out uint numVertices, IntPtr unused2, IntPtr unused3, out uint numTriangles);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(IntPtr geometry, float[] vertices, ref uint numVertices, uint[] indices, uint[] materialIndices, ref uint numTriangles);

		public int AudioGeometryGetSimplifiedMesh(IntPtr geometry, out float[] vertices, out uint[] indices, out uint[] materialIndices)
		{
			uint numVertices;
			uint numTriangles;
			int num = ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(geometry, IntPtr.Zero, out numVertices, IntPtr.Zero, IntPtr.Zero, out numTriangles);
			if (num != 0)
			{
				Debug.LogError("unexpected error getting simplified mesh array sizes");
				vertices = null;
				indices = null;
				materialIndices = null;
				return num;
			}
			vertices = new float[numVertices * 3];
			indices = new uint[numTriangles * 3];
			materialIndices = new uint[numTriangles];
			return ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(geometry, vertices, ref numVertices, indices, materialIndices, ref numTriangles);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_CreateAudioMaterial(IntPtr context, out IntPtr material);

		public int CreateAudioMaterial(out IntPtr material)
		{
			return ovrAudio_CreateAudioMaterial(context, out material);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_DestroyAudioMaterial(IntPtr material);

		public int DestroyAudioMaterial(IntPtr material)
		{
			return ovrAudio_DestroyAudioMaterial(material);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value);

		public int AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value)
		{
			return ovrAudio_AudioMaterialSetFrequency(material, property, frequency, value);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value);

		public int AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value)
		{
			return ovrAudio_AudioMaterialGetFrequency(material, property, frequency, out value);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioMaterialReset(IntPtr material, MaterialProperty property);

		public int AudioMaterialReset(IntPtr material, MaterialProperty property)
		{
			return ovrAudio_AudioMaterialReset(material, property);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_CreateAudioSceneIR(IntPtr context, out IntPtr sceneIR);

		public int CreateAudioSceneIR(out IntPtr sceneIR)
		{
			return ovrAudio_CreateAudioSceneIR(context, out sceneIR);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_DestroyAudioSceneIR(IntPtr sceneIR);

		public int DestroyAudioSceneIR(IntPtr sceneIR)
		{
			return ovrAudio_DestroyAudioSceneIR(sceneIR);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRSetEnabled(IntPtr sceneIR, int enabled);

		public int AudioSceneIRSetEnabled(IntPtr sceneIR, bool enabled)
		{
			return ovrAudio_AudioSceneIRSetEnabled(sceneIR, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRGetEnabled(IntPtr sceneIR, out int enabled);

		public int AudioSceneIRGetEnabled(IntPtr sceneIR, out bool enabled)
		{
			int enabled2;
			int result = ovrAudio_AudioSceneIRGetEnabled(sceneIR, out enabled2);
			enabled = enabled2 != 0;
			return result;
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status);

		public int AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status)
		{
			return ovrAudio_AudioSceneIRGetStatus(sceneIR, out status);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_InitializeAudioSceneIRParameters(out MapParameters parameters);

		public int InitializeAudioSceneIRParameters(out MapParameters parameters)
		{
			return ovrAudio_InitializeAudioSceneIRParameters(out parameters);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters);

		public int AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters)
		{
			return ovrAudio_AudioSceneIRCompute(sceneIR, ref parameters);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters);

		public int AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters)
		{
			return ovrAudio_AudioSceneIRComputeCustomPoints(sceneIR, points, pointCount, ref parameters);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount);

		public int AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount)
		{
			return ovrAudio_AudioSceneIRGetPointCount(sceneIR, out pointCount);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount);

		public int AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount)
		{
			return ovrAudio_AudioSceneIRGetPoints(sceneIR, points, maxPointCount);
		}

		[DllImport("MetaXRAudioUnity")]
		private unsafe static extern int ovrAudio_AudioSceneIRSetTransform(IntPtr sceneIR, float* matrix4x4);

		public unsafe int AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			return ovrAudio_AudioSceneIRSetTransform(sceneIR, ptr);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4);

		public int AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4)
		{
			return ovrAudio_AudioSceneIRGetTransform(sceneIR, out matrix4x4);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRWriteFile(IntPtr sceneIR, string filePath);

		public int AudioSceneIRWriteFile(IntPtr sceneIR, string filePath)
		{
			return ovrAudio_AudioSceneIRWriteFile(sceneIR, filePath);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRReadFile(IntPtr sceneIR, string filePath);

		public int AudioSceneIRReadFile(IntPtr sceneIR, string filePath)
		{
			return ovrAudio_AudioSceneIRReadFile(sceneIR, filePath);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength);

		public int AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength)
		{
			return ovrAudio_AudioSceneIRReadMemory(sceneIR, data, dataLength);
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_CreateControlZone(IntPtr context, out IntPtr control);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_CreateControlVolume(IntPtr context, out IntPtr control);

		public int CreateControlZone(out IntPtr control)
		{
			try
			{
				return ovrAudio_CreateControlZone(context, out control);
			}
			catch
			{
				return ovrAudio_CreateControlVolume(context, out control);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_DestroyControlZone(IntPtr control);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_DestroyControlVolume(IntPtr control);

		public int DestroyControlZone(IntPtr control)
		{
			try
			{
				return ovrAudio_DestroyControlZone(control);
			}
			catch
			{
				return ovrAudio_DestroyControlVolume(control);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneSetEnabled(IntPtr control, int enabled);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeSetEnabled(IntPtr control, int enabled);

		public int ControlZoneSetEnabled(IntPtr control, bool enabled)
		{
			try
			{
				return ovrAudio_ControlZoneSetEnabled(control, enabled ? 1 : 0);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetEnabled(control, enabled ? 1 : 0);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneGetEnabled(IntPtr control, out int enabled);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeGetEnabled(IntPtr control, out int enabled);

		public int ControlZoneGetEnabled(IntPtr control, out bool enabled)
		{
			int enabled2 = 0;
			int result;
			try
			{
				result = ovrAudio_ControlZoneGetEnabled(control, out enabled2);
			}
			catch
			{
				result = ovrAudio_ControlVolumeGetEnabled(control, out enabled2);
			}
			enabled = enabled2 != 0;
			return result;
		}

		[DllImport("MetaXRAudioUnity")]
		private unsafe static extern int ovrAudio_ControlZoneSetTransform(IntPtr control, float* matrix4x4);

		[DllImport("MetaXRAudioUnity")]
		private unsafe static extern int ovrAudio_ControlVolumeSetTransform(IntPtr control, float* matrix4x4);

		public unsafe int ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			try
			{
				return ovrAudio_ControlZoneSetTransform(control, ptr);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetTransform(control, ptr);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneGetTransform(IntPtr control, out float[] matrix4x4);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeGetTransform(IntPtr control, out float[] matrix4x4);

		public int ControlZoneGetTransform(IntPtr control, out float[] matrix4x4)
		{
			try
			{
				return ovrAudio_ControlZoneGetTransform(control, out matrix4x4);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetTransform(control, out matrix4x4);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		public int ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ)
		{
			try
			{
				return ovrAudio_ControlZoneSetBox(control, sizeX, sizeY, sizeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetBox(control, sizeX, sizeY, sizeZ);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		public int ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ)
		{
			try
			{
				return ovrAudio_ControlZoneGetBox(control, out sizeX, out sizeY, out sizeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetBox(control, out sizeX, out sizeY, out sizeZ);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		public int ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ)
		{
			try
			{
				return ovrAudio_ControlZoneSetFadeDistance(control, fadeX, fadeY, fadeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetFadeDistance(control, fadeX, fadeY, fadeZ);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		public int ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ)
		{
			try
			{
				return ovrAudio_ControlZoneGetFadeDistance(control, out fadeX, out fadeY, out fadeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetFadeDistance(control, out fadeX, out fadeY, out fadeZ);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		public int ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value)
		{
			try
			{
				return ovrAudio_ControlZoneSetFrequency(control, property, frequency, value);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetFrequency(control, property, frequency, value);
			}
		}

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlZoneReset(IntPtr control, ControlZoneProperty property);

		[DllImport("MetaXRAudioUnity")]
		private static extern int ovrAudio_ControlVolumeReset(IntPtr control, ControlZoneProperty property);

		public int ControlZoneReset(IntPtr control, ControlZoneProperty property)
		{
			try
			{
				return ovrAudio_ControlZoneReset(control, property);
			}
			catch
			{
				return ovrAudio_ControlVolumeReset(control, property);
			}
		}

		int INativeInterface.AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			return AudioGeometrySetTransform(geometry, in matrix);
		}

		int INativeInterface.AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			return AudioSceneIRSetTransform(sceneIR, in matrix);
		}

		int INativeInterface.ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			return ControlZoneSetTransform(control, in matrix);
		}
	}

	public class WwisePluginInterface : INativeInterface
	{
		public const string binaryName = "MetaXRAudioWwise";

		private IntPtr context_ = IntPtr.Zero;

		private int version;

		private IntPtr context
		{
			get
			{
				if (context_ == IntPtr.Zero)
				{
					context_ = getOrCreateGlobalOvrAudioContext();
					ovrAudio_GetVersion(out var _, out version, out var _);
				}
				return context_;
			}
		}

		[DllImport("MetaXRAudioWwise")]
		public static extern IntPtr getOrCreateGlobalOvrAudioContext();

		[DllImport("MetaXRAudioWwise")]
		public static extern IntPtr ovrAudio_GetVersion(out int Major, out int Minor, out int Patch);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_SetAcousticModel(IntPtr context, AcousticModel quality);

		public int SetAcousticModel(AcousticModel model)
		{
			return ovrAudio_SetAcousticModel(context, model);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ResetSharedReverb(IntPtr context);

		public int ResetReverb()
		{
			return ovrAudio_ResetSharedReverb(context);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);

		public int SetEnabled(int feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_Enable(IntPtr context, EnableFlagInternal what, int enable);

		public int SetEnabled(EnableFlagInternal feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_CreateAudioGeometry(IntPtr context, out IntPtr geometry);

		public int CreateAudioGeometry(out IntPtr geometry)
		{
			return ovrAudio_CreateAudioGeometry(context, out geometry);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_DestroyAudioGeometry(IntPtr geometry);

		public int DestroyAudioGeometry(IntPtr geometry)
		{
			return ovrAudio_DestroyAudioGeometry(geometry);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, int enabled);

		public int AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, bool enabled)
		{
			if (version < 94)
			{
				return -1;
			}
			return ovrAudio_AudioGeometrySetObjectFlag(geometry, flag, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, UIntPtr verticesBytesOffset, UIntPtr vertexCount, UIntPtr vertexStride, ovrAudioScalarType vertexType, int[] indices, UIntPtr indicesByteOffset, UIntPtr indexCount, ovrAudioScalarType indexType, MeshGroup[] groups, UIntPtr groupCount);

		public int AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount)
		{
			return ovrAudio_AudioGeometryUploadMeshArrays(geometry, vertices, UIntPtr.Zero, (UIntPtr)(ulong)vertexCount, UIntPtr.Zero, ovrAudioScalarType.Float32, indices, UIntPtr.Zero, (UIntPtr)(ulong)indexCount, ovrAudioScalarType.UInt32, groups, (UIntPtr)(ulong)groupCount);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, UIntPtr verticesBytesOffset, UIntPtr vertexCount, UIntPtr vertexStride, ovrAudioScalarType vertexType, int[] indices, UIntPtr indicesByteOffset, UIntPtr indexCount, ovrAudioScalarType indexType, MeshGroup[] groups, UIntPtr groupCount, ref MeshSimplification simplification);

		public int AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount, ref MeshSimplification simplification)
		{
			return ovrAudio_AudioGeometryUploadSimplifiedMeshArrays(geometry, vertices, UIntPtr.Zero, (UIntPtr)(ulong)vertexCount, UIntPtr.Zero, ovrAudioScalarType.Float32, indices, UIntPtr.Zero, (UIntPtr)(ulong)indexCount, ovrAudioScalarType.UInt32, groups, (UIntPtr)(ulong)groupCount, ref simplification);
		}

		[DllImport("MetaXRAudioWwise")]
		private unsafe static extern int ovrAudio_AudioGeometrySetTransform(IntPtr geometry, float* matrix4x4);

		public unsafe int AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			return ovrAudio_AudioGeometrySetTransform(geometry, ptr);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4);

		public int AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4)
		{
			return ovrAudio_AudioGeometryGetTransform(geometry, out matrix4x4);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryWriteMeshFile(IntPtr geometry, string filePath);

		public int AudioGeometryWriteMeshFile(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryWriteMeshFile(geometry, filePath);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryReadMeshFile(IntPtr geometry, string filePath);

		public int AudioGeometryReadMeshFile(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryReadMeshFile(geometry, filePath);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength);

		public int AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength)
		{
			return ovrAudio_AudioGeometryReadMeshMemory(geometry, data, dataLength);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath);

		public int AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryWriteMeshFileObj(geometry, filePath);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(IntPtr geometry, IntPtr unused1, out uint numVertices, IntPtr unused2, IntPtr unused3, out uint numTriangles);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(IntPtr geometry, float[] vertices, ref uint numVertices, uint[] indices, uint[] materialIndices, ref uint numTriangles);

		public int AudioGeometryGetSimplifiedMesh(IntPtr geometry, out float[] vertices, out uint[] indices, out uint[] materialIndices)
		{
			uint numVertices;
			uint numTriangles;
			int num = ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(geometry, IntPtr.Zero, out numVertices, IntPtr.Zero, IntPtr.Zero, out numTriangles);
			if (num != 0)
			{
				Debug.LogError("unexpected error getting simplified mesh array sizes");
				vertices = null;
				indices = null;
				materialIndices = null;
				return num;
			}
			vertices = new float[numVertices * 3];
			indices = new uint[numTriangles * 3];
			materialIndices = new uint[numTriangles];
			return ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(geometry, vertices, ref numVertices, indices, materialIndices, ref numTriangles);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_CreateAudioMaterial(IntPtr context, out IntPtr material);

		public int CreateAudioMaterial(out IntPtr material)
		{
			return ovrAudio_CreateAudioMaterial(context, out material);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_DestroyAudioMaterial(IntPtr material);

		public int DestroyAudioMaterial(IntPtr material)
		{
			return ovrAudio_DestroyAudioMaterial(material);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value);

		public int AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value)
		{
			return ovrAudio_AudioMaterialSetFrequency(material, property, frequency, value);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value);

		public int AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value)
		{
			return ovrAudio_AudioMaterialGetFrequency(material, property, frequency, out value);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioMaterialReset(IntPtr material, MaterialProperty property);

		public int AudioMaterialReset(IntPtr material, MaterialProperty property)
		{
			return ovrAudio_AudioMaterialReset(material, property);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_CreateAudioSceneIR(IntPtr context, out IntPtr sceneIR);

		public int CreateAudioSceneIR(out IntPtr sceneIR)
		{
			return ovrAudio_CreateAudioSceneIR(context, out sceneIR);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_DestroyAudioSceneIR(IntPtr sceneIR);

		public int DestroyAudioSceneIR(IntPtr sceneIR)
		{
			return ovrAudio_DestroyAudioSceneIR(sceneIR);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRSetEnabled(IntPtr sceneIR, int enabled);

		public int AudioSceneIRSetEnabled(IntPtr sceneIR, bool enabled)
		{
			return ovrAudio_AudioSceneIRSetEnabled(sceneIR, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRGetEnabled(IntPtr sceneIR, out int enabled);

		public int AudioSceneIRGetEnabled(IntPtr sceneIR, out bool enabled)
		{
			int enabled2;
			int result = ovrAudio_AudioSceneIRGetEnabled(sceneIR, out enabled2);
			enabled = enabled2 != 0;
			return result;
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status);

		public int AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status)
		{
			return ovrAudio_AudioSceneIRGetStatus(sceneIR, out status);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_InitializeAudioSceneIRParameters(out MapParameters parameters);

		public int InitializeAudioSceneIRParameters(out MapParameters parameters)
		{
			return ovrAudio_InitializeAudioSceneIRParameters(out parameters);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters);

		public int AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters)
		{
			return ovrAudio_AudioSceneIRCompute(sceneIR, ref parameters);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters);

		public int AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters)
		{
			return ovrAudio_AudioSceneIRComputeCustomPoints(sceneIR, points, pointCount, ref parameters);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount);

		public int AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount)
		{
			return ovrAudio_AudioSceneIRGetPointCount(sceneIR, out pointCount);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount);

		public int AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount)
		{
			return ovrAudio_AudioSceneIRGetPoints(sceneIR, points, maxPointCount);
		}

		[DllImport("MetaXRAudioWwise")]
		private unsafe static extern int ovrAudio_AudioSceneIRSetTransform(IntPtr sceneIR, float* matrix4x4);

		public unsafe int AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			return ovrAudio_AudioSceneIRSetTransform(sceneIR, ptr);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4);

		public int AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4)
		{
			return ovrAudio_AudioSceneIRGetTransform(sceneIR, out matrix4x4);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRWriteFile(IntPtr sceneIR, string filePath);

		public int AudioSceneIRWriteFile(IntPtr sceneIR, string filePath)
		{
			return ovrAudio_AudioSceneIRWriteFile(sceneIR, filePath);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRReadFile(IntPtr sceneIR, string filePath);

		public int AudioSceneIRReadFile(IntPtr sceneIR, string filePath)
		{
			return ovrAudio_AudioSceneIRReadFile(sceneIR, filePath);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength);

		public int AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength)
		{
			return ovrAudio_AudioSceneIRReadMemory(sceneIR, data, dataLength);
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_CreateControlZone(IntPtr context, out IntPtr control);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_CreateControlVolume(IntPtr context, out IntPtr control);

		public int CreateControlZone(out IntPtr control)
		{
			try
			{
				return ovrAudio_CreateControlZone(context, out control);
			}
			catch
			{
				return ovrAudio_CreateControlVolume(context, out control);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_DestroyControlZone(IntPtr control);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_DestroyControlVolume(IntPtr control);

		public int DestroyControlZone(IntPtr control)
		{
			try
			{
				return ovrAudio_DestroyControlZone(control);
			}
			catch
			{
				return ovrAudio_DestroyControlVolume(control);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneSetEnabled(IntPtr control, int enabled);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeSetEnabled(IntPtr control, int enabled);

		public int ControlZoneSetEnabled(IntPtr control, bool enabled)
		{
			try
			{
				return ovrAudio_ControlZoneSetEnabled(control, enabled ? 1 : 0);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetEnabled(control, enabled ? 1 : 0);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneGetEnabled(IntPtr control, out int enabled);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeGetEnabled(IntPtr control, out int enabled);

		public int ControlZoneGetEnabled(IntPtr control, out bool enabled)
		{
			int enabled2 = 0;
			int result;
			try
			{
				result = ovrAudio_ControlZoneGetEnabled(control, out enabled2);
			}
			catch
			{
				result = ovrAudio_ControlVolumeGetEnabled(control, out enabled2);
			}
			enabled = enabled2 != 0;
			return result;
		}

		[DllImport("MetaXRAudioWwise")]
		private unsafe static extern int ovrAudio_ControlZoneSetTransform(IntPtr control, float* matrix4x4);

		[DllImport("MetaXRAudioWwise")]
		private unsafe static extern int ovrAudio_ControlVolumeSetTransform(IntPtr control, float* matrix4x4);

		public unsafe int ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			try
			{
				return ovrAudio_ControlZoneSetTransform(control, ptr);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetTransform(control, ptr);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneGetTransform(IntPtr control, out float[] matrix4x4);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeGetTransform(IntPtr control, out float[] matrix4x4);

		public int ControlZoneGetTransform(IntPtr control, out float[] matrix4x4)
		{
			try
			{
				return ovrAudio_ControlZoneGetTransform(control, out matrix4x4);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetTransform(control, out matrix4x4);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		public int ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ)
		{
			try
			{
				return ovrAudio_ControlZoneSetBox(control, sizeX, sizeY, sizeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetBox(control, sizeX, sizeY, sizeZ);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		public int ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ)
		{
			try
			{
				return ovrAudio_ControlZoneGetBox(control, out sizeX, out sizeY, out sizeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetBox(control, out sizeX, out sizeY, out sizeZ);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		public int ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ)
		{
			try
			{
				return ovrAudio_ControlZoneSetFadeDistance(control, fadeX, fadeY, fadeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetFadeDistance(control, fadeX, fadeY, fadeZ);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		public int ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ)
		{
			try
			{
				return ovrAudio_ControlZoneGetFadeDistance(control, out fadeX, out fadeY, out fadeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetFadeDistance(control, out fadeX, out fadeY, out fadeZ);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		public int ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value)
		{
			try
			{
				return ovrAudio_ControlZoneSetFrequency(control, property, frequency, value);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetFrequency(control, property, frequency, value);
			}
		}

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlZoneReset(IntPtr control, ControlZoneProperty property);

		[DllImport("MetaXRAudioWwise")]
		private static extern int ovrAudio_ControlVolumeReset(IntPtr control, ControlZoneProperty property);

		public int ControlZoneReset(IntPtr control, ControlZoneProperty property)
		{
			try
			{
				return ovrAudio_ControlZoneReset(control, property);
			}
			catch
			{
				return ovrAudio_ControlVolumeReset(control, property);
			}
		}

		int INativeInterface.AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			return AudioGeometrySetTransform(geometry, in matrix);
		}

		int INativeInterface.AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			return AudioSceneIRSetTransform(sceneIR, in matrix);
		}

		int INativeInterface.ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			return ControlZoneSetTransform(control, in matrix);
		}
	}

	public class FMODPluginInterface : INativeInterface
	{
		public const string binaryName = "MetaXRAudioFMOD";

		private IntPtr context_ = IntPtr.Zero;

		private int version;

		private IntPtr context
		{
			get
			{
				if (context_ == IntPtr.Zero)
				{
					ovrAudio_GetPluginContext(out context_);
					ovrAudio_GetVersion(out var _, out version, out var _);
				}
				return context_;
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		public static extern int ovrAudio_GetPluginContext(out IntPtr context);

		[DllImport("MetaXRAudioFMOD")]
		public static extern IntPtr ovrAudio_GetVersion(out int Major, out int Minor, out int Patch);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_SetAcousticModel(IntPtr context, AcousticModel quality);

		public int SetAcousticModel(AcousticModel model)
		{
			return ovrAudio_SetAcousticModel(context, model);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ResetSharedReverb(IntPtr context);

		public int ResetReverb()
		{
			return ovrAudio_ResetSharedReverb(context);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);

		public int SetEnabled(int feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_Enable(IntPtr context, EnableFlagInternal what, int enable);

		public int SetEnabled(EnableFlagInternal feature, bool enabled)
		{
			return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_CreateAudioGeometry(IntPtr context, out IntPtr geometry);

		public int CreateAudioGeometry(out IntPtr geometry)
		{
			return ovrAudio_CreateAudioGeometry(context, out geometry);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_DestroyAudioGeometry(IntPtr geometry);

		public int DestroyAudioGeometry(IntPtr geometry)
		{
			return ovrAudio_DestroyAudioGeometry(geometry);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, int enabled);

		public int AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, bool enabled)
		{
			if (version < 94)
			{
				return -1;
			}
			return ovrAudio_AudioGeometrySetObjectFlag(geometry, flag, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, UIntPtr verticesBytesOffset, UIntPtr vertexCount, UIntPtr vertexStride, ovrAudioScalarType vertexType, int[] indices, UIntPtr indicesByteOffset, UIntPtr indexCount, ovrAudioScalarType indexType, MeshGroup[] groups, UIntPtr groupCount);

		public int AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount)
		{
			return ovrAudio_AudioGeometryUploadMeshArrays(geometry, vertices, UIntPtr.Zero, (UIntPtr)(ulong)vertexCount, UIntPtr.Zero, ovrAudioScalarType.Float32, indices, UIntPtr.Zero, (UIntPtr)(ulong)indexCount, ovrAudioScalarType.UInt32, groups, (UIntPtr)(ulong)groupCount);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, UIntPtr verticesBytesOffset, UIntPtr vertexCount, UIntPtr vertexStride, ovrAudioScalarType vertexType, int[] indices, UIntPtr indicesByteOffset, UIntPtr indexCount, ovrAudioScalarType indexType, MeshGroup[] groups, UIntPtr groupCount, ref MeshSimplification simplification);

		public int AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount, ref MeshSimplification simplification)
		{
			return ovrAudio_AudioGeometryUploadSimplifiedMeshArrays(geometry, vertices, UIntPtr.Zero, (UIntPtr)(ulong)vertexCount, UIntPtr.Zero, ovrAudioScalarType.Float32, indices, UIntPtr.Zero, (UIntPtr)(ulong)indexCount, ovrAudioScalarType.UInt32, groups, (UIntPtr)(ulong)groupCount, ref simplification);
		}

		[DllImport("MetaXRAudioFMOD")]
		private unsafe static extern int ovrAudio_AudioGeometrySetTransform(IntPtr geometry, float* matrix4x4);

		public unsafe int AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			return ovrAudio_AudioGeometrySetTransform(geometry, ptr);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4);

		public int AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4)
		{
			return ovrAudio_AudioGeometryGetTransform(geometry, out matrix4x4);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryWriteMeshFile(IntPtr geometry, string filePath);

		public int AudioGeometryWriteMeshFile(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryWriteMeshFile(geometry, filePath);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryReadMeshFile(IntPtr geometry, string filePath);

		public int AudioGeometryReadMeshFile(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryReadMeshFile(geometry, filePath);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength);

		public int AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength)
		{
			return ovrAudio_AudioGeometryReadMeshMemory(geometry, data, dataLength);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath);

		public int AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath)
		{
			return ovrAudio_AudioGeometryWriteMeshFileObj(geometry, filePath);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(IntPtr geometry, IntPtr unused1, out uint numVertices, IntPtr unused2, IntPtr unused3, out uint numTriangles);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(IntPtr geometry, float[] vertices, ref uint numVertices, uint[] indices, uint[] materialIndices, ref uint numTriangles);

		public int AudioGeometryGetSimplifiedMesh(IntPtr geometry, out float[] vertices, out uint[] indices, out uint[] materialIndices)
		{
			uint numVertices;
			uint numTriangles;
			int num = ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(geometry, IntPtr.Zero, out numVertices, IntPtr.Zero, IntPtr.Zero, out numTriangles);
			if (num != 0)
			{
				Debug.LogError("unexpected error getting simplified mesh array sizes");
				vertices = null;
				indices = null;
				materialIndices = null;
				return num;
			}
			vertices = new float[numVertices * 3];
			indices = new uint[numTriangles * 3];
			materialIndices = new uint[numTriangles];
			return ovrAudio_AudioGeometryGetSimplifiedMeshWithMaterials(geometry, vertices, ref numVertices, indices, materialIndices, ref numTriangles);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_CreateAudioMaterial(IntPtr context, out IntPtr material);

		public int CreateAudioMaterial(out IntPtr material)
		{
			return ovrAudio_CreateAudioMaterial(context, out material);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_DestroyAudioMaterial(IntPtr material);

		public int DestroyAudioMaterial(IntPtr material)
		{
			return ovrAudio_DestroyAudioMaterial(material);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value);

		public int AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value)
		{
			return ovrAudio_AudioMaterialSetFrequency(material, property, frequency, value);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value);

		public int AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value)
		{
			return ovrAudio_AudioMaterialGetFrequency(material, property, frequency, out value);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioMaterialReset(IntPtr material, MaterialProperty property);

		public int AudioMaterialReset(IntPtr material, MaterialProperty property)
		{
			return ovrAudio_AudioMaterialReset(material, property);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_CreateAudioSceneIR(IntPtr context, out IntPtr sceneIR);

		public int CreateAudioSceneIR(out IntPtr sceneIR)
		{
			return ovrAudio_CreateAudioSceneIR(context, out sceneIR);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_DestroyAudioSceneIR(IntPtr sceneIR);

		public int DestroyAudioSceneIR(IntPtr sceneIR)
		{
			return ovrAudio_DestroyAudioSceneIR(sceneIR);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRSetEnabled(IntPtr sceneIR, int enabled);

		public int AudioSceneIRSetEnabled(IntPtr sceneIR, bool enabled)
		{
			return ovrAudio_AudioSceneIRSetEnabled(sceneIR, enabled ? 1 : 0);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRGetEnabled(IntPtr sceneIR, out int enabled);

		public int AudioSceneIRGetEnabled(IntPtr sceneIR, out bool enabled)
		{
			int enabled2;
			int result = ovrAudio_AudioSceneIRGetEnabled(sceneIR, out enabled2);
			enabled = enabled2 != 0;
			return result;
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status);

		public int AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status)
		{
			return ovrAudio_AudioSceneIRGetStatus(sceneIR, out status);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_InitializeAudioSceneIRParameters(out MapParameters parameters);

		public int InitializeAudioSceneIRParameters(out MapParameters parameters)
		{
			return ovrAudio_InitializeAudioSceneIRParameters(out parameters);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters);

		public int AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters)
		{
			return ovrAudio_AudioSceneIRCompute(sceneIR, ref parameters);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters);

		public int AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters)
		{
			return ovrAudio_AudioSceneIRComputeCustomPoints(sceneIR, points, pointCount, ref parameters);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount);

		public int AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount)
		{
			return ovrAudio_AudioSceneIRGetPointCount(sceneIR, out pointCount);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount);

		public int AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount)
		{
			return ovrAudio_AudioSceneIRGetPoints(sceneIR, points, maxPointCount);
		}

		[DllImport("MetaXRAudioFMOD")]
		private unsafe static extern int ovrAudio_AudioSceneIRSetTransform(IntPtr sceneIR, float* matrix4x4);

		public unsafe int AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			return ovrAudio_AudioSceneIRSetTransform(sceneIR, ptr);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4);

		public int AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4)
		{
			return ovrAudio_AudioSceneIRGetTransform(sceneIR, out matrix4x4);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRWriteFile(IntPtr sceneIR, string filePath);

		public int AudioSceneIRWriteFile(IntPtr sceneIR, string filePath)
		{
			return ovrAudio_AudioSceneIRWriteFile(sceneIR, filePath);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRReadFile(IntPtr sceneIR, string filePath);

		public int AudioSceneIRReadFile(IntPtr sceneIR, string filePath)
		{
			return ovrAudio_AudioSceneIRReadFile(sceneIR, filePath);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength);

		public int AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength)
		{
			return ovrAudio_AudioSceneIRReadMemory(sceneIR, data, dataLength);
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_CreateControlZone(IntPtr context, out IntPtr control);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_CreateControlVolume(IntPtr context, out IntPtr control);

		public int CreateControlZone(out IntPtr control)
		{
			try
			{
				return ovrAudio_CreateControlZone(context, out control);
			}
			catch
			{
				return ovrAudio_CreateControlVolume(context, out control);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_DestroyControlZone(IntPtr control);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_DestroyControlVolume(IntPtr control);

		public int DestroyControlZone(IntPtr control)
		{
			try
			{
				return ovrAudio_DestroyControlZone(control);
			}
			catch
			{
				return ovrAudio_DestroyControlVolume(control);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneSetEnabled(IntPtr control, int enabled);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeSetEnabled(IntPtr control, int enabled);

		public int ControlZoneSetEnabled(IntPtr control, bool enabled)
		{
			try
			{
				return ovrAudio_ControlZoneSetEnabled(control, enabled ? 1 : 0);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetEnabled(control, enabled ? 1 : 0);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneGetEnabled(IntPtr control, out int enabled);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeGetEnabled(IntPtr control, out int enabled);

		public int ControlZoneGetEnabled(IntPtr control, out bool enabled)
		{
			int enabled2 = 0;
			int result;
			try
			{
				result = ovrAudio_ControlZoneGetEnabled(control, out enabled2);
			}
			catch
			{
				result = ovrAudio_ControlVolumeGetEnabled(control, out enabled2);
			}
			enabled = enabled2 != 0;
			return result;
		}

		[DllImport("MetaXRAudioFMOD")]
		private unsafe static extern int ovrAudio_ControlZoneSetTransform(IntPtr control, float* matrix4x4);

		[DllImport("MetaXRAudioFMOD")]
		private unsafe static extern int ovrAudio_ControlVolumeSetTransform(IntPtr control, float* matrix4x4);

		public unsafe int ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			float* ptr = stackalloc float[16];
			*ptr = matrix.m00;
			ptr[1] = matrix.m10;
			ptr[2] = 0f - matrix.m20;
			ptr[3] = matrix.m30;
			ptr[4] = matrix.m01;
			ptr[5] = matrix.m11;
			ptr[6] = 0f - matrix.m21;
			ptr[7] = matrix.m31;
			ptr[8] = matrix.m02;
			ptr[9] = matrix.m12;
			ptr[10] = 0f - matrix.m22;
			ptr[11] = matrix.m32;
			ptr[12] = matrix.m03;
			ptr[13] = matrix.m13;
			ptr[14] = 0f - matrix.m23;
			ptr[15] = matrix.m33;
			try
			{
				return ovrAudio_ControlZoneSetTransform(control, ptr);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetTransform(control, ptr);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneGetTransform(IntPtr control, out float[] matrix4x4);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeGetTransform(IntPtr control, out float[] matrix4x4);

		public int ControlZoneGetTransform(IntPtr control, out float[] matrix4x4)
		{
			try
			{
				return ovrAudio_ControlZoneGetTransform(control, out matrix4x4);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetTransform(control, out matrix4x4);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ);

		public int ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ)
		{
			try
			{
				return ovrAudio_ControlZoneSetBox(control, sizeX, sizeY, sizeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetBox(control, sizeX, sizeY, sizeZ);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ);

		public int ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ)
		{
			try
			{
				return ovrAudio_ControlZoneGetBox(control, out sizeX, out sizeY, out sizeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetBox(control, out sizeX, out sizeY, out sizeZ);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ);

		public int ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ)
		{
			try
			{
				return ovrAudio_ControlZoneSetFadeDistance(control, fadeX, fadeY, fadeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetFadeDistance(control, fadeX, fadeY, fadeZ);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ);

		public int ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ)
		{
			try
			{
				return ovrAudio_ControlZoneGetFadeDistance(control, out fadeX, out fadeY, out fadeZ);
			}
			catch
			{
				return ovrAudio_ControlVolumeGetFadeDistance(control, out fadeX, out fadeY, out fadeZ);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value);

		public int ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value)
		{
			try
			{
				return ovrAudio_ControlZoneSetFrequency(control, property, frequency, value);
			}
			catch
			{
				return ovrAudio_ControlVolumeSetFrequency(control, property, frequency, value);
			}
		}

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlZoneReset(IntPtr control, ControlZoneProperty property);

		[DllImport("MetaXRAudioFMOD")]
		private static extern int ovrAudio_ControlVolumeReset(IntPtr control, ControlZoneProperty property);

		public int ControlZoneReset(IntPtr control, ControlZoneProperty property)
		{
			try
			{
				return ovrAudio_ControlZoneReset(control, property);
			}
			catch
			{
				return ovrAudio_ControlVolumeReset(control, property);
			}
		}

		int INativeInterface.AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			return AudioGeometrySetTransform(geometry, in matrix);
		}

		int INativeInterface.AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			return AudioSceneIRSetTransform(sceneIR, in matrix);
		}

		int INativeInterface.ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			return ControlZoneSetTransform(control, in matrix);
		}
	}

	public class DummyInterface : INativeInterface
	{
		public int SetAcousticModel(AcousticModel model)
		{
			return -1;
		}

		public int ResetReverb()
		{
			return -1;
		}

		public int SetEnabled(int feature, bool enabled)
		{
			return -1;
		}

		public int SetEnabled(EnableFlagInternal feature, bool enabled)
		{
			return -1;
		}

		public int CreateAudioGeometry(out IntPtr geometry)
		{
			geometry = IntPtr.Zero;
			return -1;
		}

		public int DestroyAudioGeometry(IntPtr geometry)
		{
			return -1;
		}

		public int AudioGeometrySetObjectFlag(IntPtr geometry, ObjectFlags flag, bool enabled)
		{
			return -1;
		}

		public int AudioGeometryUploadMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount)
		{
			return -1;
		}

		public int AudioGeometryUploadSimplifiedMeshArrays(IntPtr geometry, float[] vertices, int vertexCount, int[] indices, int indexCount, MeshGroup[] groups, int groupCount, ref MeshSimplification simplification)
		{
			return -1;
		}

		public int AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			return -1;
		}

		public int AudioGeometryGetTransform(IntPtr geometry, out float[] matrix4x4)
		{
			matrix4x4 = null;
			return -1;
		}

		public int AudioGeometryWriteMeshFile(IntPtr geometry, string filePath)
		{
			return -1;
		}

		public int AudioGeometryReadMeshFile(IntPtr geometry, string filePath)
		{
			return -1;
		}

		public int AudioGeometryReadMeshMemory(IntPtr geometry, IntPtr data, ulong dataLength)
		{
			return -1;
		}

		public int AudioGeometryWriteMeshFileObj(IntPtr geometry, string filePath)
		{
			return -1;
		}

		public int AudioGeometryGetSimplifiedMesh(IntPtr geometry, out float[] vertices, out uint[] indices, out uint[] materialIndices)
		{
			vertices = null;
			indices = null;
			materialIndices = null;
			return -1;
		}

		public int AudioMaterialGetFrequency(IntPtr material, MaterialProperty property, float frequency, out float value)
		{
			value = 0f;
			return -1;
		}

		public int CreateAudioMaterial(out IntPtr material)
		{
			material = IntPtr.Zero;
			return -1;
		}

		public int DestroyAudioMaterial(IntPtr material)
		{
			return -1;
		}

		public int AudioMaterialSetFrequency(IntPtr material, MaterialProperty property, float frequency, float value)
		{
			return -1;
		}

		public int AudioMaterialReset(IntPtr material, MaterialProperty property)
		{
			return -1;
		}

		public int CreateAudioSceneIR(out IntPtr sceneIR)
		{
			sceneIR = IntPtr.Zero;
			return -1;
		}

		public int DestroyAudioSceneIR(IntPtr sceneIR)
		{
			return -1;
		}

		public int AudioSceneIRSetEnabled(IntPtr sceneIR, bool enabled)
		{
			return -1;
		}

		public int AudioSceneIRGetEnabled(IntPtr sceneIR, out bool enabled)
		{
			enabled = false;
			return -1;
		}

		public int AudioSceneIRGetStatus(IntPtr sceneIR, out AcousticMapStatus status)
		{
			status = AcousticMapStatus.EMPTY;
			return -1;
		}

		public int InitializeAudioSceneIRParameters(out MapParameters parameters)
		{
			parameters = default(MapParameters);
			return -1;
		}

		public int AudioSceneIRCompute(IntPtr sceneIR, ref MapParameters parameters)
		{
			return -1;
		}

		public int AudioSceneIRComputeCustomPoints(IntPtr sceneIR, float[] points, UIntPtr pointCount, ref MapParameters parameters)
		{
			return -1;
		}

		public int AudioSceneIRGetPointCount(IntPtr sceneIR, out UIntPtr pointCount)
		{
			pointCount = UIntPtr.Zero;
			return -1;
		}

		public int AudioSceneIRGetPoints(IntPtr sceneIR, float[] points, UIntPtr maxPointCount)
		{
			return -1;
		}

		public int AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			return -1;
		}

		public int AudioSceneIRGetTransform(IntPtr sceneIR, out float[] matrix4x4)
		{
			matrix4x4 = new float[16];
			return -1;
		}

		public int AudioSceneIRWriteFile(IntPtr sceneIR, string filePath)
		{
			return -1;
		}

		public int AudioSceneIRReadFile(IntPtr sceneIR, string filePath)
		{
			return -1;
		}

		public int AudioSceneIRReadMemory(IntPtr sceneIR, IntPtr data, ulong dataLength)
		{
			return -1;
		}

		public int CreateControlZone(out IntPtr control)
		{
			control = IntPtr.Zero;
			return -1;
		}

		public int DestroyControlZone(IntPtr control)
		{
			return -1;
		}

		public int ControlZoneSetEnabled(IntPtr control, bool enabled)
		{
			return -1;
		}

		public int ControlZoneGetEnabled(IntPtr control, out bool enabled)
		{
			enabled = false;
			return -1;
		}

		public int ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			return -1;
		}

		public int ControlZoneGetTransform(IntPtr control, out float[] matrix4x4)
		{
			matrix4x4 = new float[16];
			return -1;
		}

		public int ControlZoneSetBox(IntPtr control, float sizeX, float sizeY, float sizeZ)
		{
			return -1;
		}

		public int ControlZoneGetBox(IntPtr control, out float sizeX, out float sizeY, out float sizeZ)
		{
			sizeX = 0f;
			sizeY = 0f;
			sizeZ = 0f;
			return -1;
		}

		public int ControlZoneSetFadeDistance(IntPtr control, float fadeX, float fadeY, float fadeZ)
		{
			return -1;
		}

		public int ControlZoneGetFadeDistance(IntPtr control, out float fadeX, out float fadeY, out float fadeZ)
		{
			fadeX = 0f;
			fadeY = 0f;
			fadeZ = 0f;
			return -1;
		}

		public int ControlZoneSetFrequency(IntPtr control, ControlZoneProperty property, float frequency, float value)
		{
			return -1;
		}

		public int ControlZoneReset(IntPtr control, ControlZoneProperty property)
		{
			return -1;
		}

		int INativeInterface.AudioGeometrySetTransform(IntPtr geometry, in Matrix4x4 matrix)
		{
			return AudioGeometrySetTransform(geometry, in matrix);
		}

		int INativeInterface.AudioSceneIRSetTransform(IntPtr sceneIR, in Matrix4x4 matrix)
		{
			return AudioSceneIRSetTransform(sceneIR, in matrix);
		}

		int INativeInterface.ControlZoneSetTransform(IntPtr control, in Matrix4x4 matrix)
		{
			return ControlZoneSetTransform(control, in matrix);
		}
	}

	private static INativeInterface CachedInterface;

	public static INativeInterface Interface
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

	private static INativeInterface FindInterface()
	{
		try
		{
			WwisePluginInterface.getOrCreateGlobalOvrAudioContext();
			WwisePluginInterface.ovrAudio_GetVersion(out var _, out var Minor, out var _);
			if (Minor < 92)
			{
				Debug.LogError("Incompatible SDK version, update your MetaXRAudioWwise plugin");
				return new DummyInterface();
			}
			Debug.Log("Meta XR Audio Native Interface initialized with Wwise plugin");
			return new WwisePluginInterface();
		}
		catch (DllNotFoundException)
		{
		}
		try
		{
			FMODPluginInterface.ovrAudio_GetPluginContext(out var _);
			FMODPluginInterface.ovrAudio_GetVersion(out var _, out var Minor2, out var _);
			if (Minor2 < 92)
			{
				Debug.LogError("Incompatible SDK version, update your MetaXRAudioFMOD plugin");
				return new DummyInterface();
			}
			Debug.Log("Meta XR Audio Native Interface initialized with FMOD plugin");
			return new FMODPluginInterface();
		}
		catch (DllNotFoundException)
		{
		}
		try
		{
			UnityNativeInterface.ovrAudio_GetPluginContext(out var _);
			UnityNativeInterface.ovrAudio_GetVersion(out var _, out var Minor3, out var _);
			if (Minor3 < 92)
			{
				Debug.LogError("Incompatible SDK version, update your MetaXRAudioFMOD plugin");
				return new DummyInterface();
			}
			Debug.Log("Meta XR Audio Native Interface initialized with Unity plugin");
			return new UnityNativeInterface();
		}
		catch
		{
			Debug.LogError("Unable to locate MetaXRAudio plugin for MetaXRAcoustics!\nIf you're using Unity audio make sure you have imported the MetaXRAudioUnity package\nIf you're using Wwise or FMOD make sure you have their Unity integration in your project and the MetaXRAudioWwise or MetaXRAudioFMOD plugins in correct location in the Assets folder");
		}
		return new DummyInterface();
	}
}
