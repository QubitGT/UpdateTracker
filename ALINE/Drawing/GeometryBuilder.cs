using System;
using System.Collections.Generic;
using Drawing.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Drawing;

internal static class GeometryBuilder
{
	public struct CameraInfo
	{
		public float3 cameraPosition;

		public quaternion cameraRotation;

		public float2 cameraDepthToPixelSize;

		public bool cameraIsOrthographic;

		public CameraInfo(Camera camera)
		{
			Transform transform = camera?.transform;
			cameraPosition = ((transform != null) ? ((float3)transform.position) : float3.zero);
			cameraRotation = ((transform != null) ? ((quaternion)transform.rotation) : quaternion.identity);
			cameraDepthToPixelSize = ((camera != null) ? CameraDepthToPixelSize(camera) : ((float2)0));
			cameraIsOrthographic = camera != null && camera.orthographic;
		}
	}

	internal unsafe static JobHandle Build(DrawingData gizmos, DrawingData.ProcessedBuilderData.MeshBuffers* buffers, ref CameraInfo cameraInfo, JobHandle dependency)
	{
		GeometryBuilderJob jobData = default(GeometryBuilderJob);
		jobData.buffers = buffers;
		jobData.currentMatrix = Matrix4x4.identity;
		jobData.currentLineWidthData = new CommandBuilder.LineWidthData
		{
			pixels = 1f,
			automaticJoins = false
		};
		jobData.lineWidthMultiplier = DrawingManager.lineWidthMultiplier;
		jobData.currentColor = Color.white;
		jobData.cameraPosition = cameraInfo.cameraPosition;
		jobData.cameraRotation = cameraInfo.cameraRotation;
		jobData.cameraDepthToPixelSize = cameraInfo.cameraDepthToPixelSize;
		jobData.cameraIsOrthographic = cameraInfo.cameraIsOrthographic;
		jobData.characterInfo = (SDFCharacter*)gizmos.fontData.characters.GetUnsafeReadOnlyPtr();
		jobData.characterInfoLength = gizmos.fontData.characters.Length;
		jobData.maxPixelError = 0.5f / math.max(0.1f, gizmos.settingsRef.curveResolution);
		return jobData.Schedule(dependency);
	}

	private static float2 CameraDepthToPixelSize(Camera camera)
	{
		if (camera.orthographic)
		{
			return new float2(0f, 2f * camera.orthographicSize / (float)camera.pixelHeight);
		}
		return new float2(Mathf.Tan(camera.fieldOfView * (MathF.PI / 180f) * 0.5f) / (0.5f * (float)camera.pixelHeight), 0f);
	}

	private unsafe static NativeArray<T> ConvertExistingDataToNativeArray<T>(UnsafeAppendBuffer data) where T : struct
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data.Ptr, data.Length / UnsafeUtility.SizeOf<T>(), Allocator.Invalid);
	}

	internal unsafe static void BuildMesh(DrawingData gizmos, List<DrawingData.MeshWithType> meshes, DrawingData.ProcessedBuilderData.MeshBuffers* inputBuffers)
	{
		if (inputBuffers->triangles.Length > 0)
		{
			Mesh mesh = AssignMeshData<GeometryBuilderJob.Vertex>(gizmos, inputBuffers->bounds, inputBuffers->vertices, inputBuffers->triangles, MeshLayouts.MeshLayout);
			meshes.Add(new DrawingData.MeshWithType
			{
				mesh = mesh,
				type = DrawingData.MeshType.Lines
			});
		}
		if (inputBuffers->solidTriangles.Length > 0)
		{
			Mesh mesh2 = AssignMeshData<GeometryBuilderJob.Vertex>(gizmos, inputBuffers->bounds, inputBuffers->solidVertices, inputBuffers->solidTriangles, MeshLayouts.MeshLayout);
			meshes.Add(new DrawingData.MeshWithType
			{
				mesh = mesh2,
				type = DrawingData.MeshType.Solid
			});
		}
		if (inputBuffers->textTriangles.Length > 0)
		{
			Mesh mesh3 = AssignMeshData<GeometryBuilderJob.TextVertex>(gizmos, inputBuffers->bounds, inputBuffers->textVertices, inputBuffers->textTriangles, MeshLayouts.MeshLayoutText);
			meshes.Add(new DrawingData.MeshWithType
			{
				mesh = mesh3,
				type = DrawingData.MeshType.Text
			});
		}
	}

	private static Mesh AssignMeshData<VertexType>(DrawingData gizmos, Bounds bounds, UnsafeAppendBuffer vertices, UnsafeAppendBuffer triangles, VertexAttributeDescriptor[] layout) where VertexType : struct
	{
		NativeArray<VertexType> data = ConvertExistingDataToNativeArray<VertexType>(vertices);
		NativeArray<int> data2 = ConvertExistingDataToNativeArray<int>(triangles);
		Mesh mesh = gizmos.GetMesh(data.Length);
		mesh.SetVertexBufferParams(math.ceilpow2(data.Length), layout);
		mesh.SetIndexBufferParams(math.ceilpow2(data2.Length), IndexFormat.UInt32);
		mesh.SetVertexBufferData(data, 0, 0, data.Length);
		mesh.SetIndexBufferData(data2, 0, 0, data2.Length, MeshUpdateFlags.DontValidateIndices);
		mesh.subMeshCount = 1;
		SubMeshDescriptor desc = new SubMeshDescriptor(0, data2.Length)
		{
			vertexCount = data.Length,
			bounds = bounds
		};
		mesh.SetSubMesh(0, desc, MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
		mesh.bounds = bounds;
		return mesh;
	}
}
