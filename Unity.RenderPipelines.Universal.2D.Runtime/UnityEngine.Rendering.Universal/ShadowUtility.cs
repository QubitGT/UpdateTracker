using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal.UTess;
using UnityEngine.U2D;

namespace UnityEngine.Rendering.Universal;

[BurstCompile]
internal class ShadowUtility
{
	public enum ProjectionType
	{
		ProjectionNone = -1,
		ProjectionHard = 0,
		ProjectionSoftLeft = 1,
		ProjectionSoftRight = 3
	}

	internal struct ShadowMeshVertex
	{
		internal Vector3 position;

		internal Vector4 tangent;

		internal ShadowMeshVertex(ProjectionType inProjectionType, Vector2 inEdgePosition0, Vector2 inEdgePosition1)
		{
			position.x = inEdgePosition0.x;
			position.y = inEdgePosition0.y;
			position.z = 0f;
			tangent.x = (float)inProjectionType;
			tangent.y = 0f;
			tangent.z = inEdgePosition1.x;
			tangent.w = inEdgePosition1.y;
		}
	}

	internal struct RemappingInfo
	{
		public int count;

		public int index;

		public int v0Offset;

		public int v1Offset;

		public void Initialize()
		{
			count = 0;
			index = -1;
			v0Offset = 0;
			v1Offset = 0;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateProjectionInfo_000002F8_0024PostfixBurstDelegate(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<Vector2> outProjectionInfo);

	internal static class CalculateProjectionInfo_000002F8_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateProjectionInfo_000002F8_0024PostfixBurstDelegate>(CalculateProjectionInfo).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<Vector2> outProjectionInfo)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, ref NativeArray<int>, ref NativeArray<bool>, ref NativeArray<Vector2>, void>)functionPointer)(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outProjectionInfo);
					return;
				}
			}
			CalculateProjectionInfo_0024BurstManaged(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outProjectionInfo);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateVertices_000002F9_0024PostfixBurstDelegate(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<Vector2> inEdgeOtherPoints, ref NativeArray<ShadowMeshVertex> outMeshVertices);

	internal static class CalculateVertices_000002F9_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateVertices_000002F9_0024PostfixBurstDelegate>(CalculateVertices).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<Vector2> inEdgeOtherPoints, ref NativeArray<ShadowMeshVertex> outMeshVertices)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, ref NativeArray<Vector2>, ref NativeArray<ShadowMeshVertex>, void>)functionPointer)(ref inVertices, ref inEdges, ref inEdgeOtherPoints, ref outMeshVertices);
					return;
				}
			}
			CalculateVertices_0024BurstManaged(ref inVertices, ref inEdges, ref inEdgeOtherPoints, ref outMeshVertices);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateTriangles_000002FA_0024PostfixBurstDelegate(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<int> outMeshIndices);

	internal static class CalculateTriangles_000002FA_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateTriangles_000002FA_0024PostfixBurstDelegate>(CalculateTriangles).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<int> outMeshIndices)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, ref NativeArray<int>, ref NativeArray<bool>, ref NativeArray<int>, void>)functionPointer)(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outMeshIndices);
					return;
				}
			}
			CalculateTriangles_0024BurstManaged(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outMeshIndices);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateLocalBounds_000002FB_0024PostfixBurstDelegate(ref NativeArray<Vector3> inVertices, out Bounds retBounds);

	internal static class CalculateLocalBounds_000002FB_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateLocalBounds_000002FB_0024PostfixBurstDelegate>(CalculateLocalBounds).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> inVertices, out Bounds retBounds)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref Bounds, void>)functionPointer)(ref inVertices, ref retBounds);
					return;
				}
			}
			CalculateLocalBounds_0024BurstManaged(ref inVertices, out retBounds);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GenerateInteriorMesh_000002FC_0024PostfixBurstDelegate(ref NativeArray<ShadowMeshVertex> inVertices, ref NativeArray<int> inIndices, ref NativeArray<ShadowEdge> inEdges, out NativeArray<ShadowMeshVertex> outVertices, out NativeArray<int> outIndices, out int outStartIndex, out int outIndexCount);

	internal static class GenerateInteriorMesh_000002FC_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GenerateInteriorMesh_000002FC_0024PostfixBurstDelegate>(GenerateInteriorMesh).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<ShadowMeshVertex> inVertices, ref NativeArray<int> inIndices, ref NativeArray<ShadowEdge> inEdges, out NativeArray<ShadowMeshVertex> outVertices, out NativeArray<int> outIndices, out int outStartIndex, out int outIndexCount)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<ShadowMeshVertex>, ref NativeArray<int>, ref NativeArray<ShadowEdge>, ref NativeArray<ShadowMeshVertex>, ref NativeArray<int>, ref int, ref int, void>)functionPointer)(ref inVertices, ref inIndices, ref inEdges, ref outVertices, ref outIndices, ref outStartIndex, ref outIndexCount);
					return;
				}
			}
			GenerateInteriorMesh_0024BurstManaged(ref inVertices, ref inIndices, ref inEdges, out outVertices, out outIndices, out outStartIndex, out outIndexCount);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateEdgesFromLines_000002FE_0024PostfixBurstDelegate(ref NativeArray<int> indices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray);

	internal static class CalculateEdgesFromLines_000002FE_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateEdgesFromLines_000002FE_0024PostfixBurstDelegate>(CalculateEdgesFromLines).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<int> indices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<ShadowEdge>, ref NativeArray<int>, ref NativeArray<bool>, void>)functionPointer)(ref indices, ref outEdges, ref outShapeStartingEdge, ref outShapeIsClosedArray);
					return;
				}
			}
			CalculateEdgesFromLines_0024BurstManaged(ref indices, out outEdges, out outShapeStartingEdge, out outShapeIsClosedArray);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetVertexReferenceStats_000002FF_0024PostfixBurstDelegate(ref NativeArray<Vector3> vertices, ref NativeArray<ShadowEdge> edges, int vertexCount, out bool hasReusedVertices, out int newVertexCount, out NativeArray<RemappingInfo> remappingInfo);

	internal static class GetVertexReferenceStats_000002FF_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetVertexReferenceStats_000002FF_0024PostfixBurstDelegate>(GetVertexReferenceStats).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> vertices, ref NativeArray<ShadowEdge> edges, int vertexCount, out bool hasReusedVertices, out int newVertexCount, out NativeArray<RemappingInfo> remappingInfo)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, int, ref bool, ref int, ref NativeArray<RemappingInfo>, void>)functionPointer)(ref vertices, ref edges, vertexCount, ref hasReusedVertices, ref newVertexCount, ref remappingInfo);
					return;
				}
			}
			GetVertexReferenceStats_0024BurstManaged(ref vertices, ref edges, vertexCount, out hasReusedVertices, out newVertexCount, out remappingInfo);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateEdgesFromTriangles_00000301_0024PostfixBurstDelegate(ref NativeArray<Vector3> vertices, ref NativeArray<int> indices, bool duplicatesVertices, out NativeArray<Vector3> newVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray);

	internal static class CalculateEdgesFromTriangles_00000301_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateEdgesFromTriangles_00000301_0024PostfixBurstDelegate>(CalculateEdgesFromTriangles).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> vertices, ref NativeArray<int> indices, bool duplicatesVertices, out NativeArray<Vector3> newVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<int>, bool, ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, ref NativeArray<int>, ref NativeArray<bool>, void>)functionPointer)(ref vertices, ref indices, duplicatesVertices, ref newVertices, ref outEdges, ref outShapeStartingEdge, ref outShapeIsClosedArray);
					return;
				}
			}
			CalculateEdgesFromTriangles_0024BurstManaged(ref vertices, ref indices, duplicatesVertices, out newVertices, out outEdges, out outShapeStartingEdge, out outShapeIsClosedArray);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ReverseWindingOrder_00000302_0024PostfixBurstDelegate(ref NativeArray<int> inShapeStartingEdge, ref NativeArray<ShadowEdge> inOutSortedEdges);

	internal static class ReverseWindingOrder_00000302_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ReverseWindingOrder_00000302_0024PostfixBurstDelegate>(ReverseWindingOrder).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<int> inShapeStartingEdge, ref NativeArray<ShadowEdge> inOutSortedEdges)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<ShadowEdge>, void>)functionPointer)(ref inShapeStartingEdge, ref inOutSortedEdges);
					return;
				}
			}
			ReverseWindingOrder_0024BurstManaged(ref inShapeStartingEdge, ref inOutSortedEdges);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ClipEdges_00000305_0024PostfixBurstDelegate(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, float contractEdge, out NativeArray<Vector3> outVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge);

	internal static class ClipEdges_00000305_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ClipEdges_00000305_0024PostfixBurstDelegate>(ClipEdges).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, float contractEdge, out NativeArray<Vector3> outVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, ref NativeArray<int>, ref NativeArray<bool>, float, ref NativeArray<Vector3>, ref NativeArray<ShadowEdge>, ref NativeArray<int>, void>)functionPointer)(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, contractEdge, ref outVertices, ref outEdges, ref outShapeStartingEdge);
					return;
				}
			}
			ClipEdges_0024BurstManaged(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, contractEdge, out outVertices, out outEdges, out outShapeStartingEdge);
		}
	}

	internal const int k_AdditionalVerticesPerEdge = 4;

	internal const int k_VerticesPerTriangle = 3;

	internal const int k_TrianglesPerEdge = 3;

	internal const int k_MinimumEdges = 3;

	internal const int k_SafeSize = 40;

	private static VertexAttributeDescriptor[] m_VertexLayout = new VertexAttributeDescriptor[2]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4)
	};

	private unsafe static int GetNextShapeStart(int currentShape, int* inShapeStartingEdgePtr, int inShapeStartingEdgeLength, int maxValue)
	{
		if (currentShape + 1 >= inShapeStartingEdgeLength || inShapeStartingEdgePtr[currentShape + 1] < 0)
		{
			return maxValue;
		}
		return inShapeStartingEdgePtr[currentShape + 1];
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002ECalculateProjectionInfo_000002F8_0024PostfixBurstDelegate))]
	internal static void CalculateProjectionInfo(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<Vector2> outProjectionInfo)
	{
		CalculateProjectionInfo_000002F8_0024BurstDirectCall.Invoke(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outProjectionInfo);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002ECalculateVertices_000002F9_0024PostfixBurstDelegate))]
	internal static void CalculateVertices(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<Vector2> inEdgeOtherPoints, ref NativeArray<ShadowMeshVertex> outMeshVertices)
	{
		CalculateVertices_000002F9_0024BurstDirectCall.Invoke(ref inVertices, ref inEdges, ref inEdgeOtherPoints, ref outMeshVertices);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002ECalculateTriangles_000002FA_0024PostfixBurstDelegate))]
	internal static void CalculateTriangles(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<int> outMeshIndices)
	{
		CalculateTriangles_000002FA_0024BurstDirectCall.Invoke(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outMeshIndices);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002ECalculateLocalBounds_000002FB_0024PostfixBurstDelegate))]
	internal static void CalculateLocalBounds(ref NativeArray<Vector3> inVertices, out Bounds retBounds)
	{
		CalculateLocalBounds_000002FB_0024BurstDirectCall.Invoke(ref inVertices, out retBounds);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002EGenerateInteriorMesh_000002FC_0024PostfixBurstDelegate))]
	private static void GenerateInteriorMesh(ref NativeArray<ShadowMeshVertex> inVertices, ref NativeArray<int> inIndices, ref NativeArray<ShadowEdge> inEdges, out NativeArray<ShadowMeshVertex> outVertices, out NativeArray<int> outIndices, out int outStartIndex, out int outIndexCount)
	{
		GenerateInteriorMesh_000002FC_0024BurstDirectCall.Invoke(ref inVertices, ref inIndices, ref inEdges, out outVertices, out outIndices, out outStartIndex, out outIndexCount);
	}

	public static Bounds GenerateShadowMesh(Mesh mesh, NativeArray<Vector3> inVertices, NativeArray<ShadowEdge> inEdges, NativeArray<int> inShapeStartingEdge, NativeArray<bool> inShapeIsClosedArray, bool allowContraction, bool fill, ShadowShape2D.OutlineTopology topology)
	{
		int length = inVertices.Length + 4 * inEdges.Length;
		int length2 = inEdges.Length * 3 * 3;
		NativeArray<Vector2> outProjectionInfo = new NativeArray<Vector2>(length, Allocator.Persistent);
		NativeArray<int> outMeshIndices = new NativeArray<int>(length2, Allocator.Persistent);
		NativeArray<ShadowMeshVertex> outMeshVertices = new NativeArray<ShadowMeshVertex>(length, Allocator.Persistent);
		CalculateProjectionInfo(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outProjectionInfo);
		CalculateVertices(ref inVertices, ref inEdges, ref outProjectionInfo, ref outMeshVertices);
		CalculateTriangles(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, ref outMeshIndices);
		int outStartIndex = 0;
		int outIndexCount = 0;
		NativeArray<ShadowMeshVertex> outVertices;
		NativeArray<int> outIndices;
		if (fill)
		{
			GenerateInteriorMesh(ref outMeshVertices, ref outMeshIndices, ref inEdges, out outVertices, out outIndices, out outStartIndex, out outIndexCount);
			outMeshVertices.Dispose();
			outMeshIndices.Dispose();
		}
		else
		{
			outVertices = outMeshVertices;
			outIndices = outMeshIndices;
		}
		mesh.SetVertexBufferParams(outVertices.Length, m_VertexLayout);
		mesh.SetVertexBufferData(outVertices, 0, 0, outVertices.Length);
		mesh.SetIndexBufferParams(outIndices.Length, IndexFormat.UInt32);
		mesh.SetIndexBufferData(outIndices, 0, 0, outIndices.Length);
		mesh.SetSubMesh(0, new SubMeshDescriptor(0, outIndices.Length));
		mesh.subMeshCount = 1;
		outProjectionInfo.Dispose();
		outVertices.Dispose();
		outIndices.Dispose();
		CalculateLocalBounds(ref inVertices, out var retBounds);
		return retBounds;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002ECalculateEdgesFromLines_000002FE_0024PostfixBurstDelegate))]
	public static void CalculateEdgesFromLines(ref NativeArray<int> indices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray)
	{
		CalculateEdgesFromLines_000002FE_0024BurstDirectCall.Invoke(ref indices, out outEdges, out outShapeStartingEdge, out outShapeIsClosedArray);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002EGetVertexReferenceStats_000002FF_0024PostfixBurstDelegate))]
	internal static void GetVertexReferenceStats(ref NativeArray<Vector3> vertices, ref NativeArray<ShadowEdge> edges, int vertexCount, out bool hasReusedVertices, out int newVertexCount, out NativeArray<RemappingInfo> remappingInfo)
	{
		GetVertexReferenceStats_000002FF_0024BurstDirectCall.Invoke(ref vertices, ref edges, vertexCount, out hasReusedVertices, out newVertexCount, out remappingInfo);
	}

	public static bool IsTriangleReversed(NativeArray<Vector3> vertices, int idx0, int idx1, int idx2)
	{
		Vector3 vector = vertices[idx0];
		Vector3 vector2 = vertices[idx1];
		Vector3 vector3 = vertices[idx2];
		return Mathf.Sign(vector.x * vector2.y + vector2.x * vector3.y + vector3.x * vector.y - (vector.y * vector2.x + vector2.y * vector3.x + vector3.y * vector.x)) >= 0f;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002ECalculateEdgesFromTriangles_00000301_0024PostfixBurstDelegate))]
	public static void CalculateEdgesFromTriangles(ref NativeArray<Vector3> vertices, ref NativeArray<int> indices, bool duplicatesVertices, out NativeArray<Vector3> newVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray)
	{
		CalculateEdgesFromTriangles_00000301_0024BurstDirectCall.Invoke(ref vertices, ref indices, duplicatesVertices, out newVertices, out outEdges, out outShapeStartingEdge, out outShapeIsClosedArray);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002EReverseWindingOrder_00000302_0024PostfixBurstDelegate))]
	public static void ReverseWindingOrder(ref NativeArray<int> inShapeStartingEdge, ref NativeArray<ShadowEdge> inOutSortedEdges)
	{
		ReverseWindingOrder_00000302_0024BurstDirectCall.Invoke(ref inShapeStartingEdge, ref inOutSortedEdges);
	}

	private static int GetClosedPathCount(ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray)
	{
		int num = 0;
		for (int i = 0; i < inShapeStartingEdge.Length && inShapeStartingEdge[i] >= 0; i++)
		{
			num++;
		}
		return num;
	}

	private static void GetPathInfo(NativeArray<ShadowEdge> inEdges, NativeArray<int> inShapeStartingEdge, NativeArray<bool> inShapeIsClosedArray, out int closedPathArrayCount, out int closedPathsCount, out int openPathArrayCount, out int openPathsCount)
	{
		closedPathArrayCount = 0;
		openPathArrayCount = 0;
		closedPathsCount = 0;
		openPathsCount = 0;
		for (int i = 0; i < inShapeStartingEdge.Length && inShapeStartingEdge[i] >= 0; i++)
		{
			int num = inShapeStartingEdge[i];
			int num2 = ((i < inShapeStartingEdge.Length - 1 && inShapeStartingEdge[i + 1] != -1) ? inShapeStartingEdge[i + 1] : inEdges.Length) - num;
			if (inShapeIsClosedArray[i])
			{
				closedPathArrayCount += num2 + 1;
				closedPathsCount++;
			}
			else
			{
				openPathArrayCount += num2 + 1;
				openPathsCount++;
			}
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EUniversal_002EClipEdges_00000305_0024PostfixBurstDelegate))]
	public static void ClipEdges(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, float contractEdge, out NativeArray<Vector3> outVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge)
	{
		ClipEdges_00000305_0024BurstDirectCall.Invoke(ref inVertices, ref inEdges, ref inShapeStartingEdge, ref inShapeIsClosedArray, contractEdge, out outVertices, out outEdges, out outShapeStartingEdge);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateProjectionInfo_0024BurstManaged(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<Vector2> outProjectionInfo)
	{
		Vector3* buffer = (Vector3*)inVertices.m_Buffer;
		ShadowEdge* buffer2 = (ShadowEdge*)inEdges.m_Buffer;
		int* buffer3 = (int*)inShapeStartingEdge.m_Buffer;
		bool* buffer4 = (bool*)inShapeIsClosedArray.m_Buffer;
		Vector2* buffer5 = (Vector2*)outProjectionInfo.m_Buffer;
		Vector2 vector = default(Vector2);
		int length = inEdges.Length;
		int length2 = inShapeStartingEdge.Length;
		int length3 = inVertices.Length;
		int num = 0;
		int num2 = 0;
		int nextShapeStart = GetNextShapeStart(num, buffer3, length2, length);
		int num3 = nextShapeStart;
		for (int i = 0; i < length; i++)
		{
			if (i == nextShapeStart)
			{
				num++;
				num2 = nextShapeStart;
				nextShapeStart = GetNextShapeStart(num, buffer3, length2, length);
				num3 = nextShapeStart - num2;
			}
			int num4 = (i - num2 + 1) % num3 + num2;
			int num5 = (i - num2 + num3 - 1) % num3 + num2;
			int v = buffer2[i].v0;
			int v2 = buffer2[i].v1;
			int v3 = buffer2[num5].v0;
			int v4 = buffer2[num4].v1;
			vector.x = buffer[v].x;
			vector.y = buffer[v].y;
			Vector2 vector2 = vector;
			vector.x = buffer[v2].x;
			vector.y = buffer[v2].y;
			Vector2 vector3 = vector;
			vector.x = buffer[v3].x;
			vector.y = buffer[v3].y;
			vector.x = buffer[v4].x;
			vector.y = buffer[v4].y;
			buffer5[v] = vector3;
			int num6 = 4 * i + length3;
			buffer5[num6] = vector3;
			buffer5[num6 + 1] = vector2;
			buffer5[num6 + 2] = vector3;
			buffer5[num6 + 3] = vector3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateVertices_0024BurstManaged(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<Vector2> inEdgeOtherPoints, ref NativeArray<ShadowMeshVertex> outMeshVertices)
	{
		Vector3* buffer = (Vector3*)inVertices.m_Buffer;
		ShadowEdge* buffer2 = (ShadowEdge*)inEdges.m_Buffer;
		Vector2* buffer3 = (Vector2*)inEdgeOtherPoints.m_Buffer;
		ShadowMeshVertex* buffer4 = (ShadowMeshVertex*)outMeshVertices.m_Buffer;
		Vector2 vector = default(Vector2);
		int length = inEdges.Length;
		int length2 = inVertices.Length;
		for (int i = 0; i < length2; i++)
		{
			vector.x = buffer[i].x;
			vector.y = buffer[i].y;
			ShadowMeshVertex shadowMeshVertex = new ShadowMeshVertex(ProjectionType.ProjectionNone, vector, buffer3[i]);
			buffer4[i] = shadowMeshVertex;
		}
		for (int j = 0; j < length; j++)
		{
			int v = buffer2[j].v0;
			int v2 = buffer2[j].v1;
			vector.x = buffer[v].x;
			vector.y = buffer[v].y;
			Vector2 inEdgePosition = vector;
			vector.x = buffer[v2].x;
			vector.y = buffer[v2].y;
			Vector2 inEdgePosition2 = vector;
			int num = 4 * j + length2;
			ShadowMeshVertex shadowMeshVertex2 = new ShadowMeshVertex(ProjectionType.ProjectionHard, inEdgePosition, buffer3[num]);
			ShadowMeshVertex shadowMeshVertex3 = new ShadowMeshVertex(ProjectionType.ProjectionHard, inEdgePosition2, buffer3[num + 1]);
			ShadowMeshVertex shadowMeshVertex4 = new ShadowMeshVertex(ProjectionType.ProjectionSoftLeft, inEdgePosition, buffer3[num + 2]);
			ShadowMeshVertex shadowMeshVertex5 = new ShadowMeshVertex(ProjectionType.ProjectionSoftRight, inEdgePosition, buffer3[num + 3]);
			buffer4[num] = shadowMeshVertex2;
			buffer4[num + 1] = shadowMeshVertex3;
			buffer4[num + 2] = shadowMeshVertex4;
			buffer4[num + 3] = shadowMeshVertex5;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateTriangles_0024BurstManaged(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, ref NativeArray<int> outMeshIndices)
	{
		ShadowEdge* buffer = (ShadowEdge*)inEdges.m_Buffer;
		int* buffer2 = (int*)inShapeStartingEdge.m_Buffer;
		int* buffer3 = (int*)outMeshIndices.m_Buffer;
		int length = inEdges.Length;
		int length2 = inShapeStartingEdge.Length;
		int length3 = inVertices.Length;
		int num = 0;
		for (int i = 0; i < length2; i++)
		{
			int num2 = buffer2[i];
			if (num2 < 0)
			{
				break;
			}
			int num3 = length;
			if (i + 1 < length2 && buffer2[i + 1] > -1)
			{
				num3 = buffer2[i + 1];
			}
			for (int j = num2; j < num3; j++)
			{
				int v = buffer[j].v0;
				int v2 = buffer[j].v1;
				int num4 = 4 * j + length3;
				buffer3[num++] = (ushort)v;
				buffer3[num++] = (ushort)num4;
				buffer3[num++] = (ushort)(num4 + 1);
				buffer3[num++] = (ushort)(num4 + 1);
				buffer3[num++] = (ushort)v2;
				buffer3[num++] = (ushort)v;
			}
			for (int k = num2; k < num3; k++)
			{
				int v3 = buffer[k].v0;
				_ = buffer[k];
				int num5 = 4 * k + length3;
				buffer3[num++] = (ushort)v3;
				buffer3[num++] = (ushort)num5 + 2;
				buffer3[num++] = (ushort)num5 + 3;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateLocalBounds_0024BurstManaged(ref NativeArray<Vector3> inVertices, out Bounds retBounds)
	{
		if (inVertices.Length <= 0)
		{
			retBounds = new Bounds(Vector3.zero, Vector3.zero);
			return;
		}
		Vector2 vector = Vector2.positiveInfinity;
		Vector2 vector2 = Vector2.negativeInfinity;
		Vector3* buffer = (Vector3*)inVertices.m_Buffer;
		int length = inVertices.Length;
		for (int i = 0; i < length; i++)
		{
			Vector2 rhs = new Vector2(buffer[i].x, buffer[i].y);
			vector = Vector2.Min(vector, rhs);
			vector2 = Vector2.Max(vector2, rhs);
		}
		retBounds = new Bounds
		{
			max = vector2,
			min = vector
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GenerateInteriorMesh_0024BurstManaged(ref NativeArray<ShadowMeshVertex> inVertices, ref NativeArray<int> inIndices, ref NativeArray<ShadowEdge> inEdges, out NativeArray<ShadowMeshVertex> outVertices, out NativeArray<int> outIndices, out int outStartIndex, out int outIndexCount)
	{
		int length = inEdges.Length;
		NativeArray<int2> edges = new NativeArray<int2>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray<float2> points = new NativeArray<float2>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < length; i++)
		{
			int x = (edges[i] = new int2(inEdges[i].v0, inEdges[i].v1)).x;
			points[x] = new float2(inVertices[x].position.x, inVertices[x].position.y);
		}
		NativeArray<int> outIndices2 = new NativeArray<int>(points.Length * 8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray<float2> outVertices2 = new NativeArray<float2>(points.Length * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray<int2> outEdges = new NativeArray<int2>(edges.Length * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		int outVertexCount = 0;
		int outIndexCount2 = 0;
		int outEdgeCount = 0;
		UnityEngine.Rendering.Universal.UTess.ModuleHandle.Tessellate(Allocator.Persistent, points, edges, ref outVertices2, ref outVertexCount, ref outIndices2, ref outIndexCount2, ref outEdges, ref outEdgeCount);
		int length2 = inIndices.Length;
		int length3 = inVertices.Length;
		int length4 = outVertexCount + inVertices.Length;
		int length5 = outIndexCount2 + inIndices.Length;
		outVertices = new NativeArray<ShadowMeshVertex>(length4, Allocator.Persistent);
		outIndices = new NativeArray<int>(length5, Allocator.Persistent);
		for (int j = 0; j < inVertices.Length; j++)
		{
			outVertices[j] = inVertices[j];
		}
		for (int k = 0; k < outVertexCount; k++)
		{
			float2 @float = outVertices2[k];
			ShadowMeshVertex value = new ShadowMeshVertex(ProjectionType.ProjectionNone, @float, Vector2.zero);
			outVertices[k + length3] = value;
		}
		for (int l = 0; l < inIndices.Length; l++)
		{
			outIndices[l] = inIndices[l];
		}
		for (int m = 0; m < outIndexCount2; m++)
		{
			outIndices[m + length2] = outIndices2[m] + length3;
		}
		outStartIndex = length2;
		outIndexCount = outIndexCount2;
		edges.Dispose();
		points.Dispose();
		outIndices2.Dispose();
		outVertices2.Dispose();
		outEdges.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateEdgesFromLines_0024BurstManaged(ref NativeArray<int> indices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray)
	{
		int num = indices.Length >> 1;
		NativeArray<int> nativeArray = new NativeArray<int>(num, Allocator.Persistent);
		NativeArray<bool> nativeArray2 = new NativeArray<bool>(num, Allocator.Persistent);
		int* buffer = (int*)indices.m_Buffer;
		int* buffer2 = (int*)nativeArray.m_Buffer;
		bool* buffer3 = (bool*)nativeArray2.m_Buffer;
		int length = indices.Length;
		int num2 = 0;
		int num3 = *buffer;
		int num4 = *buffer;
		bool flag = false;
		*buffer2 = 0;
		for (int i = 0; i < length; i += 2)
		{
			if (flag)
			{
				num3 = buffer[i];
				buffer3[num2] = true;
				buffer2[++num2] = i >> 1;
				flag = false;
			}
			else if (buffer[i] != num4)
			{
				buffer3[num2] = false;
				buffer2[++num2] = i >> 1;
				num3 = buffer[i];
			}
			if (num3 == buffer[i + 1])
			{
				flag = true;
			}
			num4 = buffer[i + 1];
		}
		buffer3[num2++] = flag;
		outShapeStartingEdge = new NativeArray<int>(num2, Allocator.Persistent);
		outShapeIsClosedArray = new NativeArray<bool>(num2, Allocator.Persistent);
		int* buffer4 = (int*)outShapeStartingEdge.m_Buffer;
		bool* buffer5 = (bool*)outShapeIsClosedArray.m_Buffer;
		for (int j = 0; j < num2; j++)
		{
			buffer4[j] = buffer2[j];
			buffer5[j] = buffer3[j];
		}
		nativeArray.Dispose();
		nativeArray2.Dispose();
		outEdges = new NativeArray<ShadowEdge>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ShadowEdge* buffer6 = (ShadowEdge*)outEdges.m_Buffer;
		for (int k = 0; k < num; k++)
		{
			int num5 = k << 1;
			int indexA = buffer[num5];
			int indexB = buffer[num5 + 1];
			buffer6[k] = new ShadowEdge(indexA, indexB);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void GetVertexReferenceStats_0024BurstManaged(ref NativeArray<Vector3> vertices, ref NativeArray<ShadowEdge> edges, int vertexCount, out bool hasReusedVertices, out int newVertexCount, out NativeArray<RemappingInfo> remappingInfo)
	{
		int length = edges.Length;
		newVertexCount = 0;
		hasReusedVertices = false;
		remappingInfo = new NativeArray<RemappingInfo>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		RemappingInfo* unsafePtr = (RemappingInfo*)remappingInfo.GetUnsafePtr();
		ShadowEdge* unsafePtr2 = (ShadowEdge*)edges.GetUnsafePtr();
		for (int i = 0; i < vertexCount; i++)
		{
			unsafePtr[i].Initialize();
		}
		for (int j = 0; j < length; j++)
		{
			int v = unsafePtr2[j].v0;
			unsafePtr[v].count++;
			if (unsafePtr[v].count > 1)
			{
				hasReusedVertices = true;
			}
			newVertexCount++;
		}
		for (int k = 0; k < length; k++)
		{
			int v2 = unsafePtr2[k].v1;
			if (unsafePtr[v2].count == 0)
			{
				unsafePtr[v2].count = 1;
				newVertexCount++;
			}
		}
		int num = 0;
		for (int l = 0; l < vertexCount; l++)
		{
			if (unsafePtr[l].count > 0)
			{
				unsafePtr[l].index = num;
				num += unsafePtr[l].count;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void CalculateEdgesFromTriangles_0024BurstManaged(ref NativeArray<Vector3> vertices, ref NativeArray<int> indices, bool duplicatesVertices, out NativeArray<Vector3> newVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge, out NativeArray<bool> outShapeIsClosedArray)
	{
		Clipper2D.Solution solution = default(Clipper2D.Solution);
		Clipper2D.ExecuteArguments inExecuteArguments = new Clipper2D.ExecuteArguments(Clipper2D.InitOptions.ioDefault, Clipper2D.ClipType.ctUnion);
		int num = indices.Length / 3;
		NativeArray<Vector2> nativeArray = new NativeArray<Vector2>(indices.Length, Allocator.Persistent);
		NativeArray<int> nativeArray2 = new NativeArray<int>(num, Allocator.Persistent);
		NativeArray<Clipper2D.PathArguments> nativeArray3 = new NativeArray<Clipper2D.PathArguments>(num, Allocator.Persistent);
		Vector2* unsafePtr = (Vector2*)nativeArray.GetUnsafePtr();
		int* unsafePtr2 = (int*)nativeArray2.GetUnsafePtr();
		Clipper2D.PathArguments* unsafePtr3 = (Clipper2D.PathArguments*)nativeArray3.GetUnsafePtr();
		Vector3* unsafePtr4 = (Vector3*)vertices.GetUnsafePtr();
		Clipper2D.PathArguments pathArguments = new Clipper2D.PathArguments(Clipper2D.PolyType.ptSubject, inClosed: true);
		for (int i = 0; i < num; i++)
		{
			unsafePtr2[i] = 3;
			unsafePtr3[i] = pathArguments;
			int num2 = 3 * i;
			unsafePtr[num2] = unsafePtr4[indices[num2]];
			unsafePtr[num2 + 1] = unsafePtr4[indices[num2 + 1]];
			unsafePtr[num2 + 2] = unsafePtr4[indices[num2 + 2]];
		}
		Clipper2D.Execute(ref solution, nativeArray, nativeArray2, nativeArray3, inExecuteArguments, Allocator.Persistent);
		nativeArray.Dispose();
		nativeArray2.Dispose();
		nativeArray3.Dispose();
		int length = solution.points.Length;
		int length2 = solution.pathSizes.Length;
		newVertices = new NativeArray<Vector3>(length, Allocator.Persistent);
		outEdges = new NativeArray<ShadowEdge>(length, Allocator.Persistent);
		outShapeStartingEdge = new NativeArray<int>(length2, Allocator.Persistent);
		outShapeIsClosedArray = new NativeArray<bool>(length2, Allocator.Persistent);
		int* unsafePtr5 = (int*)solution.pathSizes.GetUnsafePtr();
		Vector2* unsafePtr6 = (Vector2*)solution.points.GetUnsafePtr();
		Vector3* unsafePtr7 = (Vector3*)newVertices.GetUnsafePtr();
		ShadowEdge* unsafePtr8 = (ShadowEdge*)outEdges.GetUnsafePtr();
		int* unsafePtr9 = (int*)outShapeStartingEdge.GetUnsafePtr();
		bool* unsafePtr10 = (bool*)outShapeIsClosedArray.GetUnsafePtr();
		int num3 = 0;
		for (int j = 0; j < length2; j++)
		{
			int num4 = num3;
			int num5 = unsafePtr5[j];
			unsafePtr9[j] = num3;
			num3 += num5;
			int indexA = num3 - 1;
			for (int k = num4; k < num3; k++)
			{
				unsafePtr7[k] = unsafePtr6[k];
				unsafePtr8[k] = new ShadowEdge(indexA, k);
				indexA = k;
			}
			unsafePtr10[j] = true;
		}
		solution.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ReverseWindingOrder_0024BurstManaged(ref NativeArray<int> inShapeStartingEdge, ref NativeArray<ShadowEdge> inOutSortedEdges)
	{
		for (int i = 0; i < inShapeStartingEdge.Length; i++)
		{
			int num = inShapeStartingEdge[i];
			if (num < 0)
			{
				break;
			}
			int num2 = inOutSortedEdges.Length;
			if (i + 1 < inShapeStartingEdge.Length && inShapeStartingEdge[i + 1] > -1)
			{
				num2 = inShapeStartingEdge[i + 1];
			}
			int num3 = num2 - num;
			for (int j = 0; j < num3 >> 1; j++)
			{
				int index = num + j;
				int index2 = num + num3 - 1 - j;
				ShadowEdge value = inOutSortedEdges[index];
				ShadowEdge value2 = inOutSortedEdges[index2];
				value.Reverse();
				value2.Reverse();
				inOutSortedEdges[index] = value2;
				inOutSortedEdges[index2] = value;
			}
			if ((num3 & 1) == 1)
			{
				int index3 = num + (num3 >> 1);
				ShadowEdge value3 = inOutSortedEdges[index3];
				value3.Reverse();
				inOutSortedEdges[index3] = value3;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void ClipEdges_0024BurstManaged(ref NativeArray<Vector3> inVertices, ref NativeArray<ShadowEdge> inEdges, ref NativeArray<int> inShapeStartingEdge, ref NativeArray<bool> inShapeIsClosedArray, float contractEdge, out NativeArray<Vector3> outVertices, out NativeArray<ShadowEdge> outEdges, out NativeArray<int> outShapeStartingEdge)
	{
		Allocator allocator = Allocator.Persistent;
		int num = 65536;
		GetPathInfo(inEdges, inShapeStartingEdge, inShapeIsClosedArray, out var closedPathArrayCount, out var closedPathsCount, out var openPathArrayCount, out var openPathsCount);
		NativeArray<Clipper2D.PathArguments> inPathArguments = new NativeArray<Clipper2D.PathArguments>(closedPathsCount, allocator);
		NativeArray<int> nativeArray = new NativeArray<int>(closedPathsCount, allocator);
		NativeArray<Vector2> nativeArray2 = new NativeArray<Vector2>(closedPathArrayCount, allocator);
		NativeArray<int> nativeArray3 = new NativeArray<int>(openPathsCount, allocator);
		NativeArray<Vector2> nativeArray4 = new NativeArray<Vector2>(openPathArrayCount, allocator);
		Clipper2D.PathArguments* buffer = (Clipper2D.PathArguments*)inPathArguments.m_Buffer;
		int* buffer2 = (int*)nativeArray.m_Buffer;
		Vector2* buffer3 = (Vector2*)nativeArray2.m_Buffer;
		int* buffer4 = (int*)nativeArray3.m_Buffer;
		Vector2* buffer5 = (Vector2*)nativeArray4.m_Buffer;
		int* buffer6 = (int*)inShapeStartingEdge.m_Buffer;
		bool* buffer7 = (bool*)inShapeIsClosedArray.m_Buffer;
		Vector3* buffer8 = (Vector3*)inVertices.m_Buffer;
		ShadowEdge* buffer9 = (ShadowEdge*)inEdges.m_Buffer;
		int length = inEdges.Length;
		Vector2 vector = default(Vector2);
		Vector3 zero = Vector3.zero;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = closedPathsCount + openPathsCount;
		for (int i = 0; i < num6; i++)
		{
			int num7 = buffer6[i];
			int num8 = ((i + 1 < num6) ? buffer6[i + 1] : length) - num7;
			if (buffer7[i])
			{
				buffer2[num3] = num8 + 1;
				buffer[num3] = new Clipper2D.PathArguments(Clipper2D.PolyType.ptSubject, inClosed: true);
				num3++;
				for (int j = 0; j < num8; j++)
				{
					Vector3 vector2 = buffer8[buffer9[j + num7].v0];
					vector.x = vector2.x;
					vector.y = vector2.y;
					buffer3[num2++] = vector;
				}
				buffer3[num2++] = buffer8[buffer9[num8 + num7 - 1].v1];
			}
			else
			{
				buffer4[num5++] = num8 + 1;
				for (int k = 0; k < num8; k++)
				{
					Vector3 vector3 = buffer8[buffer9[k + num7].v0];
					vector.x = vector3.x;
					vector.y = vector3.y;
					buffer5[num4++] = vector;
				}
				buffer5[num4++] = buffer8[buffer9[num8 + num7 - 1].v1];
			}
		}
		NativeArray<Vector2> inPoints = nativeArray2;
		NativeArray<int> inPathSizes = nativeArray;
		Clipper2D.Solution solution = default(Clipper2D.Solution);
		if (nativeArray.Length > 1)
		{
			Clipper2D.ExecuteArguments inExecuteArguments = default(Clipper2D.ExecuteArguments);
			inExecuteArguments.clipType = Clipper2D.ClipType.ctUnion;
			inExecuteArguments.clipFillType = Clipper2D.PolyFillType.pftEvenOdd;
			inExecuteArguments.subjFillType = Clipper2D.PolyFillType.pftEvenOdd;
			inExecuteArguments.strictlySimple = false;
			inExecuteArguments.preserveColinear = false;
			Clipper2D.Execute(ref solution, nativeArray2, nativeArray, inPathArguments, inExecuteArguments, allocator, num, useRounding: true);
			inPoints = solution.points;
			inPathSizes = solution.pathSizes;
		}
		ClipperOffset2D.Solution solution2 = default(ClipperOffset2D.Solution);
		NativeArray<ClipperOffset2D.PathArguments> inPathArguments2 = new NativeArray<ClipperOffset2D.PathArguments>(inPathSizes.Length, allocator);
		ClipperOffset2D.Execute(ref solution2, inPoints, inPathSizes, inPathArguments2, allocator, 0f - contractEdge, 2.0, 0.25, 0.0, num);
		if (solution2.pathSizes.Length > 0 || openPathsCount > 0)
		{
			int num9 = 0;
			int length2 = solution2.pathSizes.Length + openPathsCount;
			outVertices = new NativeArray<Vector3>(solution2.points.Length + openPathArrayCount, allocator);
			outEdges = new NativeArray<ShadowEdge>(solution2.points.Length + openPathArrayCount, allocator);
			outShapeStartingEdge = new NativeArray<int>(length2, allocator);
			Vector3* buffer10 = (Vector3*)outVertices.m_Buffer;
			ShadowEdge* buffer11 = (ShadowEdge*)outEdges.m_Buffer;
			int* buffer12 = (int*)outShapeStartingEdge.m_Buffer;
			Vector2* buffer13 = (Vector2*)solution2.points.m_Buffer;
			int length3 = solution2.points.Length;
			int* buffer14 = (int*)solution2.pathSizes.m_Buffer;
			int length4 = solution2.pathSizes.Length;
			for (int l = 0; l < length3; l++)
			{
				zero.x = buffer13[l].x;
				zero.y = buffer13[l].y;
				buffer10[num9++] = zero;
			}
			int num10 = 0;
			for (int m = 0; m < length4; m++)
			{
				int num11 = buffer14[m];
				int num12 = num10 + num11;
				buffer12[m] = num10;
				for (int n = 0; n < num11; n++)
				{
					ShadowEdge shadowEdge = new ShadowEdge(n + num10, (n + 1) % num11 + num10);
					buffer11[n + num10] = shadowEdge;
				}
				num10 = num12;
			}
			int num13 = length4;
			num10 = num9;
			for (int num14 = 0; num14 < nativeArray4.Length; num14++)
			{
				zero.x = buffer5[num14].x;
				zero.y = buffer5[num14].y;
				buffer10[num9++] = zero;
			}
			for (int num15 = 0; num15 < openPathsCount; num15++)
			{
				int num16 = buffer4[num15];
				int num17 = num10 + num16;
				buffer12[num13 + num15] = num10;
				for (int num18 = 0; num18 < num16 - 1; num18++)
				{
					ShadowEdge shadowEdge2 = new ShadowEdge(num18 + num10, num18 + 1);
					buffer11[num18 + num10] = shadowEdge2;
				}
				num10 = num17;
			}
		}
		else
		{
			outVertices = new NativeArray<Vector3>(0, allocator);
			outEdges = new NativeArray<ShadowEdge>(0, allocator);
			outShapeStartingEdge = new NativeArray<int>(0, allocator);
		}
		nativeArray.Dispose();
		nativeArray2.Dispose();
		nativeArray3.Dispose();
		nativeArray4.Dispose();
		inPathArguments.Dispose();
		inPathArguments2.Dispose();
		solution.Dispose();
		solution2.Dispose();
	}
}
