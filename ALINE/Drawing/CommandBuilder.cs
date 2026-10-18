using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Drawing;

[BurstCompile]
public struct CommandBuilder : IDisposable
{
	[Flags]
	internal enum Command
	{
		PushColorInline = 0x100,
		PushColor = 0,
		PopColor = 1,
		PushMatrix = 2,
		PushSetMatrix = 3,
		PopMatrix = 4,
		Line = 5,
		Circle = 6,
		CircleXZ = 7,
		Disc = 8,
		DiscXZ = 9,
		SphereOutline = 0xA,
		Box = 0xB,
		WirePlane = 0xC,
		WireBox = 0xD,
		SolidTriangle = 0xE,
		PushPersist = 0xF,
		PopPersist = 0x10,
		Text = 0x11,
		Text3D = 0x12,
		PushLineWidth = 0x13,
		PopLineWidth = 0x14,
		CaptureState = 0x15
	}

	internal struct TriangleData
	{
		public float3 a;

		public float3 b;

		public float3 c;
	}

	internal struct LineData
	{
		public float3 a;

		public float3 b;
	}

	internal struct LineDataV3
	{
		public Vector3 a;

		public Vector3 b;
	}

	internal struct CircleXZData
	{
		public float3 center;

		public float radius;

		public float startAngle;

		public float endAngle;
	}

	internal struct CircleData
	{
		public float3 center;

		public float3 normal;

		public float radius;
	}

	internal struct SphereData
	{
		public float3 center;

		public float radius;
	}

	internal struct BoxData
	{
		public float3 center;

		public float3 size;
	}

	internal struct PlaneData
	{
		public float3 center;

		public quaternion rotation;

		public float2 size;
	}

	internal struct PersistData
	{
		public float endTime;
	}

	internal struct LineWidthData
	{
		public float pixels;

		public bool automaticJoins;
	}

	internal struct TextData
	{
		public float3 center;

		public LabelAlignment alignment;

		public float sizeInPixels;

		public int numCharacters;
	}

	internal struct TextData3D
	{
		public float3 center;

		public quaternion rotation;

		public LabelAlignment alignment;

		public float size;

		public int numCharacters;
	}

	public struct ScopeMatrix : IDisposable
	{
		internal CommandBuilder builder;

		public unsafe void Dispose()
		{
			builder.PopMatrix();
			builder.buffer = null;
		}
	}

	public struct ScopeColor : IDisposable
	{
		internal CommandBuilder builder;

		public unsafe void Dispose()
		{
			builder.PopColor();
			builder.buffer = null;
		}
	}

	public struct ScopePersist : IDisposable
	{
		internal CommandBuilder builder;

		public unsafe void Dispose()
		{
			builder.PopDuration();
			builder.buffer = null;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ScopeEmpty : IDisposable
	{
		public void Dispose()
		{
		}
	}

	public struct ScopeLineWidth : IDisposable
	{
		internal CommandBuilder builder;

		public unsafe void Dispose()
		{
			builder.PopLineWidth();
			builder.buffer = null;
		}
	}

	public enum SymbolDecoration
	{
		None,
		ArrowHead,
		Circle
	}

	public struct PolylineWithSymbol
	{
		private float3 prev;

		private float offset;

		private readonly float symbolSize;

		private readonly float symbolSpacing;

		private readonly float symbolPadding;

		private readonly float symbolOffset;

		private readonly SymbolDecoration symbol;

		private readonly bool reverseSymbols;

		private bool odd;

		public PolylineWithSymbol(SymbolDecoration symbol, float symbolSize, float symbolPadding, float symbolSpacing, bool reverseSymbols = false)
		{
			if (symbolSpacing <= 1.1754944E-38f)
			{
				throw new ArgumentOutOfRangeException("symbolSpacing", "Symbol spacing must be greater than zero");
			}
			if (symbolSize <= 1.1754944E-38f)
			{
				throw new ArgumentOutOfRangeException("symbolSize", "Symbol size must be greater than zero");
			}
			if (symbolPadding < 0f)
			{
				throw new ArgumentOutOfRangeException("symbolPadding", "Symbol padding must non-negative");
			}
			prev = float3.zero;
			this.symbol = symbol;
			this.symbolSize = symbolSize;
			this.symbolPadding = symbolPadding;
			this.symbolSpacing = math.max(0f, symbolSpacing - symbolPadding * 2f - symbolSize);
			this.reverseSymbols = reverseSymbols;
			symbolOffset = ((symbol == SymbolDecoration.ArrowHead) ? (-0.25f * symbolSize) : 0f);
			if (reverseSymbols)
			{
				symbolOffset = 0f - symbolOffset;
			}
			symbolOffset += 0.5f * symbolSize;
			offset = -1f;
			odd = false;
		}

		public void MoveTo(ref CommandBuilder draw, float3 next)
		{
			if (offset == -1f)
			{
				offset = symbolSpacing * 0.5f;
				prev = next;
				return;
			}
			float num = math.length(next - prev);
			float num2 = math.rcp(num);
			float3 @float = next - prev;
			float3 float2 = default(float3);
			if (symbol != 0)
			{
				float2 = math.normalizesafe(math.cross(@float, math.cross(@float, new float3(0f, 1f, 0f))));
				if (math.all(float2 == 0f))
				{
					float2 = new float3(0f, 0f, 1f);
				}
			}
			if (reverseSymbols)
			{
				@float = -@float;
			}
			if (offset > 0f && !odd)
			{
				draw.Line(prev, math.lerp(prev, next, math.min(offset * num2, 1f)));
			}
			while (offset < num)
			{
				if (odd)
				{
					float3 a = math.lerp(prev, next, offset * num2);
					offset += symbolSpacing;
					float3 b = math.lerp(prev, next, math.min(offset * num2, 1f));
					draw.Line(a, b);
					offset += symbolPadding;
				}
				else
				{
					float3 center = math.lerp(prev, next, (offset + symbolOffset) * num2);
					switch (symbol)
					{
					case SymbolDecoration.ArrowHead:
						draw.Arrowhead(center, @float, float2, symbolSize);
						break;
					default:
						draw.Circle(center, float2, symbolSize * 0.5f);
						break;
					case SymbolDecoration.None:
						break;
					}
					offset += symbolSize + symbolPadding;
				}
				odd = !odd;
			}
			offset -= num;
			prev = next;
		}
	}

	[BurstCompile]
	private class JobWireMesh
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void JobWireMeshDelegate(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void WireMesh_00000109_0024PostfixBurstDelegate(float3* verts, int* indices, int vertexCount, int indexCount, ref CommandBuilder draw);

		internal static class WireMesh_00000109_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<WireMesh_00000109_0024PostfixBurstDelegate>(WireMesh).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static void Invoke(float3* verts, int* indices, int vertexCount, int indexCount, ref CommandBuilder draw)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						((delegate* unmanaged[Cdecl]<float3*, int*, int, int, ref CommandBuilder, void>)functionPointer)(verts, indices, vertexCount, indexCount, ref draw);
						return;
					}
				}
				WireMesh_0024BurstManaged(verts, indices, vertexCount, indexCount, ref draw);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void Execute_0000010A_0024PostfixBurstDelegate(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw);

		internal static class Execute_0000010A_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<Execute_0000010A_0024PostfixBurstDelegate>(Execute).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static void Invoke(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						((delegate* unmanaged[Cdecl]<ref Mesh.MeshData, ref CommandBuilder, void>)functionPointer)(ref rawMeshData, ref draw);
						return;
					}
				}
				Execute_0024BurstManaged(ref rawMeshData, ref draw);
			}
		}

		public static readonly JobWireMeshDelegate JobWireMeshFunctionPointer = BurstCompiler.CompileFunctionPointer<JobWireMeshDelegate>(Execute).Invoke;

		[BurstCompile]
		[MonoPInvokeCallback(typeof(WireMesh_00000109_0024PostfixBurstDelegate))]
		public unsafe static void WireMesh(float3* verts, int* indices, int vertexCount, int indexCount, ref CommandBuilder draw)
		{
			WireMesh_00000109_0024BurstDirectCall.Invoke(verts, indices, vertexCount, indexCount, ref draw);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(JobWireMeshDelegate))]
		private static void Execute(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw)
		{
			Execute_0000010A_0024BurstDirectCall.Invoke(ref rawMeshData, ref draw);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		internal unsafe static void WireMesh_0024BurstManaged(float3* verts, int* indices, int vertexCount, int indexCount, ref CommandBuilder draw)
		{
			NativeHashMap<int2, bool> nativeHashMap = new NativeHashMap<int2, bool>(indexCount, Allocator.Temp);
			for (int i = 0; i < indexCount; i += 3)
			{
				int num = indices[i];
				int num2 = indices[i + 1];
				int num3 = indices[i + 2];
				if (num < 0 || num2 < 0 || num3 < 0 || num >= vertexCount || num2 >= vertexCount || num3 >= vertexCount)
				{
					throw new Exception("Invalid vertex index. Index out of bounds");
				}
				int num4 = math.min(num, num2);
				int num5 = math.max(num, num2);
				if (!nativeHashMap.ContainsKey(new int2(num4, num5)))
				{
					nativeHashMap.Add(new int2(num4, num5), item: true);
					draw.Line(verts[num4], verts[num5]);
				}
				num4 = math.min(num2, num3);
				num5 = math.max(num2, num3);
				if (!nativeHashMap.ContainsKey(new int2(num4, num5)))
				{
					nativeHashMap.Add(new int2(num4, num5), item: true);
					draw.Line(verts[num4], verts[num5]);
				}
				num4 = math.min(num3, num);
				num5 = math.max(num3, num);
				if (!nativeHashMap.ContainsKey(new int2(num4, num5)))
				{
					nativeHashMap.Add(new int2(num4, num5), item: true);
					draw.Line(verts[num4], verts[num5]);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(JobWireMeshDelegate))]
		internal unsafe static void Execute_0024BurstManaged(ref Mesh.MeshData rawMeshData, ref CommandBuilder draw)
		{
			int num = 0;
			for (int i = 0; i < rawMeshData.subMeshCount; i++)
			{
				num = math.max(num, rawMeshData.GetSubMesh(i).indexCount);
			}
			NativeArray<int> nativeArray = new NativeArray<int>(num, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vector3> nativeArray2 = new NativeArray<Vector3>(rawMeshData.vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			rawMeshData.GetVertices(nativeArray2);
			for (int j = 0; j < rawMeshData.subMeshCount; j++)
			{
				SubMeshDescriptor subMesh = rawMeshData.GetSubMesh(j);
				rawMeshData.GetIndices(nativeArray, j);
				WireMesh((float3*)nativeArray2.GetUnsafeReadOnlyPtr(), (int*)nativeArray.GetUnsafeReadOnlyPtr(), nativeArray2.Length, subMesh.indexCount, ref draw);
			}
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeAppendBuffer* buffer;

	private GCHandle gizmos;

	[NativeSetThreadIndex]
	private int threadIndex;

	private DrawingData.BuilderData.BitPackedMeta uniqueID;

	private static readonly float3 DEFAULT_UP = new float3(0f, 1f, 0f);

	internal static readonly float4x4 XZtoXYPlaneMatrix = float4x4.RotateX(-MathF.PI / 2f);

	internal static readonly float4x4 XZtoYZPlaneMatrix = float4x4.RotateZ(MathF.PI / 2f);

	internal unsafe int BufferSize
	{
		get
		{
			return buffer->Length;
		}
		set
		{
			buffer->Length = value;
		}
	}

	public CommandBuilder2D xy => new CommandBuilder2D(this, xy: true);

	public CommandBuilder2D xz => new CommandBuilder2D(this, xy: false);

	public Camera[] cameraTargets
	{
		get
		{
			if (gizmos.IsAllocated && gizmos.Target != null)
			{
				DrawingData drawingData = gizmos.Target as DrawingData;
				if (drawingData.data.StillExists(uniqueID))
				{
					return drawingData.data.Get(uniqueID).meta.cameraTargets;
				}
			}
			throw new Exception("Cannot get cameraTargets because the command builder has already been disposed or does not exist.");
		}
		set
		{
			if (uniqueID.isBuiltInCommandBuilder)
			{
				throw new Exception("You cannot set the camera targets for a built-in command builder. Create a custom command builder instead.");
			}
			if (gizmos.IsAllocated && gizmos.Target != null)
			{
				DrawingData obj = gizmos.Target as DrawingData;
				if (!obj.data.StillExists(uniqueID))
				{
					throw new Exception("Cannot set cameraTargets because the command builder has already been disposed or does not exist.");
				}
				obj.data.Get(uniqueID).meta.cameraTargets = value;
			}
		}
	}

	internal unsafe CommandBuilder(UnsafeAppendBuffer* buffer, GCHandle gizmos, int threadIndex, DrawingData.BuilderData.BitPackedMeta uniqueID)
	{
		this.buffer = buffer;
		this.gizmos = gizmos;
		this.threadIndex = threadIndex;
		this.uniqueID = uniqueID;
	}

	internal unsafe CommandBuilder(DrawingData gizmos, DrawingData.Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, bool isBuiltInCommandBuilder, int sceneModeVersion)
	{
		this.gizmos = GCHandle.Alloc(gizmos, GCHandleType.Normal);
		threadIndex = 0;
		uniqueID = gizmos.data.Reserve(isBuiltInCommandBuilder);
		gizmos.data.Get(uniqueID).Init(hasher, frameRedrawScope, customRedrawScope, isGizmos, gizmos.GetNextDrawOrderIndex(), sceneModeVersion);
		buffer = gizmos.data.Get(uniqueID).bufferPtr;
	}

	public void Dispose()
	{
		if (uniqueID.isBuiltInCommandBuilder)
		{
			throw new Exception("You cannot dispose a built-in command builder");
		}
		DisposeInternal();
	}

	public void DisposeAfter(JobHandle dependency, AllowedDelay allowedDelay = AllowedDelay.EndOfFrame)
	{
		if (!gizmos.IsAllocated)
		{
			throw new Exception("You cannot dispose an invalid command builder. Are you trying to dispose it twice?");
		}
		try
		{
			if (gizmos.IsAllocated && gizmos.Target != null)
			{
				DrawingData obj = gizmos.Target as DrawingData;
				if (!obj.data.StillExists(uniqueID))
				{
					throw new Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
				}
				obj.data.Get(uniqueID).SubmitWithDependency(gizmos, dependency, allowedDelay);
			}
		}
		finally
		{
			this = default(CommandBuilder);
		}
	}

	internal void DisposeInternal()
	{
		if (!gizmos.IsAllocated)
		{
			throw new Exception("You cannot dispose an invalid command builder. Are you trying to dispose it twice?");
		}
		try
		{
			if (gizmos.IsAllocated && gizmos.Target != null)
			{
				DrawingData obj = gizmos.Target as DrawingData;
				if (!obj.data.StillExists(uniqueID))
				{
					throw new Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
				}
				obj.data.Get(uniqueID).Submit(gizmos.Target as DrawingData);
			}
		}
		finally
		{
			gizmos.Free();
			this = default(CommandBuilder);
		}
	}

	public void DiscardAndDispose()
	{
		if (uniqueID.isBuiltInCommandBuilder)
		{
			throw new Exception("You cannot dispose a built-in command builder");
		}
		DiscardAndDisposeInternal();
	}

	internal void DiscardAndDisposeInternal()
	{
		try
		{
			if (gizmos.IsAllocated && gizmos.Target != null)
			{
				DrawingData obj = gizmos.Target as DrawingData;
				if (!obj.data.StillExists(uniqueID))
				{
					throw new Exception("Cannot dispose the command builder because the drawing manager has been destroyed");
				}
				obj.data.Release(uniqueID);
			}
		}
		finally
		{
			if (gizmos.IsAllocated)
			{
				gizmos.Free();
			}
			this = default(CommandBuilder);
		}
	}

	public void Preallocate(int size)
	{
		Reserve(size);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void Reserve(int additionalSpace)
	{
		if (Hint.Unlikely(threadIndex >= 0))
		{
			buffer += threadIndex;
			threadIndex = -1;
		}
		int num = buffer->Length + additionalSpace;
		if (num > buffer->Capacity)
		{
			buffer->SetCapacity(math.max(num, buffer->Length * 2));
		}
	}

	[BurstDiscard]
	private void AssertBufferExists()
	{
		if (!gizmos.IsAllocated || gizmos.Target == null || !(gizmos.Target as DrawingData).data.StillExists(uniqueID))
		{
			this = default(CommandBuilder);
			throw new Exception("This command builder no longer exists. Are you trying to draw to a command builder which has already been disposed?");
		}
	}

	[BurstDiscard]
	private static void AssertNotRendering()
	{
		if (!GizmoContext.drawingGizmos && !JobsUtility.IsExecutingJob && (Time.renderedFrameCount & 0x7F) == 0 && StackTraceUtility.ExtractStackTrace().Contains("OnDrawGizmos"))
		{
			throw new Exception("You are trying to use Draw.* functions from within Unity's OnDrawGizmos function. Use this package's gizmo callbacks instead (see the documentation).");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Reserve<A>() where A : struct
	{
		Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<A>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Reserve<A, B>() where A : struct where B : struct
	{
		Reserve(UnsafeUtility.SizeOf<Command>() * 2 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Reserve<A, B, C>() where A : struct where B : struct where C : struct
	{
		Reserve(UnsafeUtility.SizeOf<Command>() * 3 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>() + UnsafeUtility.SizeOf<C>());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static uint ConvertColor(Color color)
	{
		if (X86.Sse2.IsSse2Supported)
		{
			int4 @int = (int4)(255f * new float4(color.r, color.g, color.b, color.a) + 0.5f);
			v128 v = new v128(@int.x, @int.y, @int.z, @int.w);
			v128 v2 = X86.Sse2.packs_epi32(v, v);
			return X86.Sse2.packus_epi16(v2, v2).UInt0;
		}
		uint num = (uint)Mathf.Clamp((int)(color.r * 255f + 0.5f), 0, 255);
		uint num2 = (uint)Mathf.Clamp((int)(color.g * 255f + 0.5f), 0, 255);
		uint num3 = (uint)Mathf.Clamp((int)(color.b * 255f + 0.5f), 0, 255);
		return (uint)(Mathf.Clamp((int)(color.a * 255f + 0.5f), 0, 255) << 24) | (num3 << 16) | (num2 << 8) | num;
	}

	internal unsafe void Add<T>(T value) where T : struct
	{
		int num = UnsafeUtility.SizeOf<T>();
		UnsafeAppendBuffer* ptr = buffer;
		int length = ptr->Length;
		Hint.Assume(ptr->Ptr != null);
		Hint.Assume(ptr->Ptr + length != null);
		UnsafeUtility.CopyStructureToPtr(ref value, ptr->Ptr + length);
		ptr->Length = length + num;
	}

	[BurstDiscard]
	public ScopeMatrix WithMatrix(Matrix4x4 matrix)
	{
		PushMatrix(matrix);
		ScopeMatrix result = default(ScopeMatrix);
		result.builder = this;
		return result;
	}

	[BurstDiscard]
	public ScopeMatrix WithMatrix(float3x3 matrix)
	{
		PushMatrix(new float4x4(matrix, float3.zero));
		ScopeMatrix result = default(ScopeMatrix);
		result.builder = this;
		return result;
	}

	[BurstDiscard]
	public ScopeColor WithColor(Color color)
	{
		PushColor(color);
		ScopeColor result = default(ScopeColor);
		result.builder = this;
		return result;
	}

	[BurstDiscard]
	public ScopePersist WithDuration(float duration)
	{
		PushDuration(duration);
		ScopePersist result = default(ScopePersist);
		result.builder = this;
		return result;
	}

	[BurstDiscard]
	public ScopeLineWidth WithLineWidth(float pixels, bool automaticJoins = true)
	{
		PushLineWidth(pixels, automaticJoins);
		ScopeLineWidth result = default(ScopeLineWidth);
		result.builder = this;
		return result;
	}

	[BurstDiscard]
	public ScopeMatrix InLocalSpace(Transform transform)
	{
		return WithMatrix(transform.localToWorldMatrix);
	}

	[BurstDiscard]
	public ScopeMatrix InScreenSpace(Camera camera)
	{
		return WithMatrix(camera.cameraToWorldMatrix * camera.nonJitteredProjectionMatrix.inverse * Matrix4x4.TRS(new Vector3(-1f, -1f, 0f), Quaternion.identity, new Vector3(2f / (float)camera.pixelWidth, 2f / (float)camera.pixelHeight, 1f)));
	}

	public void PushMatrix(Matrix4x4 matrix)
	{
		Reserve<float4x4>();
		Add(Command.PushMatrix);
		Add(matrix);
	}

	public void PushMatrix(float4x4 matrix)
	{
		Reserve<float4x4>();
		Add(Command.PushMatrix);
		Add(matrix);
	}

	public void PushSetMatrix(Matrix4x4 matrix)
	{
		Reserve<float4x4>();
		Add(Command.PushSetMatrix);
		Add((float4x4)matrix);
	}

	public void PushSetMatrix(float4x4 matrix)
	{
		Reserve<float4x4>();
		Add(Command.PushSetMatrix);
		Add(matrix);
	}

	public void PopMatrix()
	{
		Reserve(4);
		Add(Command.PopMatrix);
	}

	public void PushColor(Color color)
	{
		Reserve<Color32>();
		Add(Command.PushColor);
		Add(ConvertColor(color));
	}

	public void PopColor()
	{
		Reserve(4);
		Add(Command.PopColor);
	}

	public void PushDuration(float duration)
	{
		Reserve<PersistData>();
		Add(Command.PushPersist);
		Add(new PersistData
		{
			endTime = SharedDrawingData.BurstTime.Data + duration
		});
	}

	public void PopDuration()
	{
		Reserve(4);
		Add(Command.PopPersist);
	}

	[Obsolete("Renamed to PushDuration for consistency")]
	public void PushPersist(float duration)
	{
		PushDuration(duration);
	}

	[Obsolete("Renamed to PopDuration for consistency")]
	public void PopPersist()
	{
		PopDuration();
	}

	public void PushLineWidth(float pixels, bool automaticJoins = true)
	{
		if (pixels < 0f)
		{
			throw new ArgumentOutOfRangeException("pixels", "Line width must be positive");
		}
		Reserve<LineWidthData>();
		Add(Command.PushLineWidth);
		Add(new LineWidthData
		{
			pixels = pixels,
			automaticJoins = automaticJoins
		});
	}

	public void PopLineWidth()
	{
		Reserve(4);
		Add(Command.PopLineWidth);
	}

	public void Line(float3 a, float3 b)
	{
		Reserve<LineData>();
		Add(Command.Line);
		Add(new LineData
		{
			a = a,
			b = b
		});
	}

	public unsafe void Line(Vector3 a, Vector3 b)
	{
		Reserve<LineData>();
		int bufferSize = BufferSize;
		int length = bufferSize + 4 + 24;
		byte* num = buffer->Ptr + bufferSize;
		*(int*)num = 5;
		LineDataV3* ptr = (LineDataV3*)(num + 4);
		ptr->a = a;
		ptr->b = b;
		buffer->Length = length;
	}

	public unsafe void Line(Vector3 a, Vector3 b, Color color)
	{
		Reserve<Color32, LineData>();
		int bufferSize = BufferSize;
		int length = bufferSize + 4 + 24 + 4;
		byte* num = buffer->Ptr + bufferSize;
		*(int*)num = 261;
		*(uint*)(num + 4) = ConvertColor(color);
		LineDataV3* ptr = (LineDataV3*)(num + 8);
		ptr->a = a;
		ptr->b = b;
		buffer->Length = length;
	}

	public void Ray(float3 origin, float3 direction)
	{
		Line(origin, origin + direction);
	}

	public void Ray(Ray ray, float length)
	{
		Line(ray.origin, ray.origin + ray.direction * length);
	}

	public void Arc(float3 center, float3 start, float3 end)
	{
		float3 @float = start - center;
		float3 float2 = end - center;
		float3 float3 = math.cross(float2, @float);
		if (math.any(float3 != 0f) && math.all(math.isfinite(float3)))
		{
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(@float, float3), Vector3.one);
			float num = Vector3.SignedAngle(@float, float2, float3) * (MathF.PI / 180f);
			PushMatrix(matrix);
			CircleXZInternal(float3.zero, math.length(@float), MathF.PI / 2f, MathF.PI / 2f - num);
			PopMatrix();
		}
	}

	[Obsolete("Use Draw.xz.Circle instead")]
	public void CircleXZ(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
		CircleXZInternal(center, radius, startAngle, endAngle);
	}

	internal void CircleXZInternal(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
		Reserve<CircleXZData>();
		Add(Command.CircleXZ);
		Add(new CircleXZData
		{
			center = center,
			radius = radius,
			startAngle = startAngle,
			endAngle = endAngle
		});
	}

	internal void CircleXZInternal(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
		Reserve<Color32, CircleXZData>();
		Add(Command.CircleXZ | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new CircleXZData
		{
			center = center,
			radius = radius,
			startAngle = startAngle,
			endAngle = endAngle
		});
	}

	[Obsolete("Use Draw.xy.Circle instead")]
	public void CircleXY(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
		PushMatrix(XZtoXYPlaneMatrix);
		CircleXZ(new float3(center.x, 0f - center.z, center.y), radius, startAngle, endAngle);
		PopMatrix();
	}

	public void Circle(float3 center, float3 normal, float radius)
	{
		Reserve<CircleData>();
		Add(Command.Circle);
		Add(new CircleData
		{
			center = center,
			normal = normal,
			radius = radius
		});
	}

	public void SolidArc(float3 center, float3 start, float3 end)
	{
		float3 @float = start - center;
		float3 float2 = end - center;
		float3 float3 = math.cross(float2, @float);
		if (math.any(float3))
		{
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(@float, float3), Vector3.one);
			float num = Vector3.SignedAngle(@float, float2, float3) * (MathF.PI / 180f);
			PushMatrix(matrix);
			SolidCircleXZInternal(float3.zero, math.length(@float), MathF.PI / 2f, MathF.PI / 2f - num);
			PopMatrix();
		}
	}

	[Obsolete("Use Draw.xz.SolidCircle instead")]
	public void SolidCircleXZ(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
		SolidCircleXZInternal(center, radius, startAngle, endAngle);
	}

	internal void SolidCircleXZInternal(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
		Reserve<CircleXZData>();
		Add(Command.DiscXZ);
		Add(new CircleXZData
		{
			center = center,
			radius = radius,
			startAngle = startAngle,
			endAngle = endAngle
		});
	}

	internal void SolidCircleXZInternal(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
		Reserve<Color32, CircleXZData>();
		Add(Command.DiscXZ | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new CircleXZData
		{
			center = center,
			radius = radius,
			startAngle = startAngle,
			endAngle = endAngle
		});
	}

	[Obsolete("Use Draw.xy.SolidCircle instead")]
	public void SolidCircleXY(float3 center, float radius, float startAngle = 0f, float endAngle = MathF.PI * 2f)
	{
		PushMatrix(XZtoXYPlaneMatrix);
		SolidCircleXZInternal(new float3(center.x, 0f - center.z, center.y), radius, startAngle, endAngle);
		PopMatrix();
	}

	public void SolidCircle(float3 center, float3 normal, float radius)
	{
		Reserve<CircleData>();
		Add(Command.Disc);
		Add(new CircleData
		{
			center = center,
			normal = normal,
			radius = radius
		});
	}

	public void SphereOutline(float3 center, float radius)
	{
		Reserve<SphereData>();
		Add(Command.SphereOutline);
		Add(new SphereData
		{
			center = center,
			radius = radius
		});
	}

	public void WireCylinder(float3 bottom, float3 top, float radius)
	{
		WireCylinder(bottom, top - bottom, math.length(top - bottom), radius);
	}

	public void WireCylinder(float3 position, float3 up, float height, float radius)
	{
		up = math.normalizesafe(up);
		if (!math.all(up == 0f) && !math.any(math.isnan(up)) && !math.isnan(height) && !math.isnan(radius))
		{
			OrthonormalBasis(up, out var basis, out var basis2);
			PushMatrix(new float4x4(new float4(basis * radius, 0f), new float4(up * height, 0f), new float4(basis2 * radius, 0f), new float4(position, 1f)));
			CircleXZInternal(float3.zero, 1f);
			if (height > 0f)
			{
				CircleXZInternal(new float3(0f, 1f, 0f), 1f);
				Line(new float3(1f, 0f, 0f), new float3(1f, 1f, 0f));
				Line(new float3(-1f, 0f, 0f), new float3(-1f, 1f, 0f));
				Line(new float3(0f, 0f, 1f), new float3(0f, 1f, 1f));
				Line(new float3(0f, 0f, -1f), new float3(0f, 1f, -1f));
			}
			PopMatrix();
		}
	}

	private static void OrthonormalBasis(float3 normal, out float3 basis1, out float3 basis2)
	{
		basis1 = math.cross(normal, new float3(1f, 1f, 1f));
		if (math.all(basis1 == 0f))
		{
			basis1 = math.cross(normal, new float3(-1f, 1f, 1f));
		}
		basis1 = math.normalizesafe(basis1);
		basis2 = math.cross(normal, basis1);
	}

	public void WireCapsule(float3 start, float3 end, float radius)
	{
		float3 @float = end - start;
		float num = math.length(@float);
		if ((double)num < 0.0001)
		{
			WireSphere(start, radius);
			return;
		}
		float3 float2 = @float / num;
		WireCapsule(start - float2 * radius, float2, num + 2f * radius, radius);
	}

	public void WireCapsule(float3 position, float3 direction, float length, float radius)
	{
		direction = math.normalizesafe(direction);
		if (math.all(direction == 0f) || math.any(math.isnan(direction)) || math.isnan(length) || math.isnan(radius))
		{
			return;
		}
		if (radius <= 0f)
		{
			Line(position, position + direction * length);
			return;
		}
		length = math.max(length, radius * 2f);
		OrthonormalBasis(direction, out var basis, out var basis2);
		PushMatrix(new float4x4(new float4(basis, 0f), new float4(direction, 0f), new float4(basis2, 0f), new float4(position, 1f)));
		CircleXZInternal(new float3(0f, radius, 0f), radius);
		PushMatrix(XZtoXYPlaneMatrix);
		CircleXZInternal(new float3(0f, 0f, radius), radius, MathF.PI);
		PopMatrix();
		PushMatrix(XZtoYZPlaneMatrix);
		CircleXZInternal(new float3(radius, 0f, 0f), radius, MathF.PI / 2f, 4.712389f);
		PopMatrix();
		if (length > 0f)
		{
			float num = length - radius;
			CircleXZInternal(new float3(0f, num, 0f), radius);
			PushMatrix(XZtoXYPlaneMatrix);
			CircleXZInternal(new float3(0f, 0f, num), radius, 0f, MathF.PI);
			PopMatrix();
			PushMatrix(XZtoYZPlaneMatrix);
			CircleXZInternal(new float3(num, 0f, 0f), radius, -MathF.PI / 2f, MathF.PI / 2f);
			PopMatrix();
			Line(new float3(radius, radius, 0f), new float3(radius, num, 0f));
			Line(new float3(0f - radius, radius, 0f), new float3(0f - radius, num, 0f));
			Line(new float3(0f, radius, radius), new float3(0f, num, radius));
			Line(new float3(0f, radius, 0f - radius), new float3(0f, num, 0f - radius));
		}
		PopMatrix();
	}

	public void WireSphere(float3 position, float radius)
	{
		SphereOutline(position, radius);
		Circle(position, new float3(1f, 0f, 0f), radius);
		Circle(position, new float3(0f, 1f, 0f), radius);
		Circle(position, new float3(0f, 0f, 1f), radius);
	}

	[BurstDiscard]
	public void Polyline(List<Vector3> points, bool cycle = false)
	{
		for (int i = 0; i < points.Count - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Count > 1)
		{
			Line(points[points.Count - 1], points[0]);
		}
	}

	public void Polyline<T>(T points, bool cycle = false) where T : IReadOnlyList<float3>
	{
		for (int i = 0; i < points.Count - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Count > 1)
		{
			Line(points[points.Count - 1], points[0]);
		}
	}

	[BurstDiscard]
	public void Polyline(Vector3[] points, bool cycle = false)
	{
		for (int i = 0; i < points.Length - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Length > 1)
		{
			Line(points[^1], points[0]);
		}
	}

	[BurstDiscard]
	public void Polyline(float3[] points, bool cycle = false)
	{
		for (int i = 0; i < points.Length - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Length > 1)
		{
			Line(points[^1], points[0]);
		}
	}

	public void Polyline(NativeArray<float3> points, bool cycle = false)
	{
		for (int i = 0; i < points.Length - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Length > 1)
		{
			Line(points[points.Length - 1], points[0]);
		}
	}

	public void DashedLine(float3 a, float3 b, float dash, float gap)
	{
		PolylineWithSymbol polylineWithSymbol = new PolylineWithSymbol(SymbolDecoration.None, gap, 0f, dash + gap);
		polylineWithSymbol.MoveTo(ref this, a);
		polylineWithSymbol.MoveTo(ref this, b);
	}

	public void DashedPolyline(List<Vector3> points, float dash, float gap)
	{
		PolylineWithSymbol polylineWithSymbol = new PolylineWithSymbol(SymbolDecoration.None, gap, 0f, dash + gap);
		for (int i = 0; i < points.Count; i++)
		{
			polylineWithSymbol.MoveTo(ref this, points[i]);
		}
	}

	public void WireBox(float3 center, float3 size)
	{
		Reserve<BoxData>();
		Add(Command.WireBox);
		Add(new BoxData
		{
			center = center,
			size = size
		});
	}

	public void WireBox(float3 center, quaternion rotation, float3 size)
	{
		PushMatrix(float4x4.TRS(center, rotation, size));
		WireBox(float3.zero, new float3(1f, 1f, 1f));
		PopMatrix();
	}

	public void WireBox(Bounds bounds)
	{
		WireBox(bounds.center, bounds.size);
	}

	public void WireMesh(Mesh mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException();
		}
		Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
		Mesh.MeshData rawMeshData = meshDataArray[0];
		JobWireMesh.JobWireMeshFunctionPointer(ref rawMeshData, ref this);
		meshDataArray.Dispose();
	}

	public unsafe void WireMesh(NativeArray<float3> vertices, NativeArray<int> triangles)
	{
		JobWireMesh.WireMesh((float3*)vertices.GetUnsafeReadOnlyPtr(), (int*)triangles.GetUnsafeReadOnlyPtr(), vertices.Length, triangles.Length, ref this);
	}

	public void SolidMesh(Mesh mesh)
	{
		SolidMeshInternal(mesh, temporary: false);
	}

	private void SolidMeshInternal(Mesh mesh, bool temporary, Color color)
	{
		PushColor(color);
		SolidMeshInternal(mesh, temporary);
		PopColor();
	}

	private void SolidMeshInternal(Mesh mesh, bool temporary)
	{
		(gizmos.Target as DrawingData).data.Get(uniqueID).meshes.Add(new DrawingData.SubmittedMesh
		{
			mesh = mesh,
			temporary = temporary
		});
		Reserve(4);
		Add(Command.CaptureState);
	}

	[BurstDiscard]
	public void SolidMesh(List<Vector3> vertices, List<int> triangles, List<Color> colors)
	{
		if (vertices.Count != colors.Count)
		{
			throw new ArgumentException("Number of colors must be the same as the number of vertices");
		}
		Mesh mesh = (gizmos.Target as DrawingData).GetMesh(vertices.Count);
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetColors(colors);
		mesh.UploadMeshData(markNoLongerReadable: false);
		SolidMeshInternal(mesh, temporary: true);
	}

	[BurstDiscard]
	public void SolidMesh(Vector3[] vertices, int[] triangles, Color[] colors, int vertexCount, int indexCount)
	{
		if (vertices.Length != colors.Length)
		{
			throw new ArgumentException("Number of colors must be the same as the number of vertices");
		}
		Mesh mesh = (gizmos.Target as DrawingData).GetMesh(vertices.Length);
		mesh.Clear();
		mesh.SetVertices(vertices, 0, vertexCount);
		mesh.SetTriangles(triangles, 0, indexCount, 0);
		mesh.SetColors(colors, 0, vertexCount);
		mesh.UploadMeshData(markNoLongerReadable: false);
		SolidMeshInternal(mesh, temporary: true);
	}

	public void Cross(float3 position, float size = 1f)
	{
		size *= 0.5f;
		Line(position - new float3(size, 0f, 0f), position + new float3(size, 0f, 0f));
		Line(position - new float3(0f, size, 0f), position + new float3(0f, size, 0f));
		Line(position - new float3(0f, 0f, size), position + new float3(0f, 0f, size));
	}

	[Obsolete("Use Draw.xz.Cross instead")]
	public void CrossXZ(float3 position, float size = 1f)
	{
		size *= 0.5f;
		Line(position - new float3(size, 0f, 0f), position + new float3(size, 0f, 0f));
		Line(position - new float3(0f, 0f, size), position + new float3(0f, 0f, size));
	}

	[Obsolete("Use Draw.xy.Cross instead")]
	public void CrossXY(float3 position, float size = 1f)
	{
		size *= 0.5f;
		Line(position - new float3(size, 0f, 0f), position + new float3(size, 0f, 0f));
		Line(position - new float3(0f, size, 0f), position + new float3(0f, size, 0f));
	}

	public static float3 EvaluateCubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t)
	{
		t = math.clamp(t, 0f, 1f);
		float num = 1f - t;
		return num * num * num * p0 + 3f * num * num * t * p1 + 3f * num * t * t * p2 + t * t * t * p3;
	}

	public void Bezier(float3 p0, float3 p1, float3 p2, float3 p3)
	{
		float3 a = p0;
		for (int i = 1; i <= 20; i++)
		{
			float t = (float)i / 20f;
			float3 @float = EvaluateCubicBezier(p0, p1, p2, p3, t);
			Line(a, @float);
			a = @float;
		}
	}

	public void CatmullRom(List<Vector3> points)
	{
		if (points.Count < 2)
		{
			return;
		}
		if (points.Count == 2)
		{
			Line(points[0], points[1]);
			return;
		}
		int count = points.Count;
		CatmullRom(points[0], points[0], points[1], points[2]);
		for (int i = 0; i + 3 < count; i++)
		{
			CatmullRom(points[i], points[i + 1], points[i + 2], points[i + 3]);
		}
		CatmullRom(points[count - 3], points[count - 2], points[count - 1], points[count - 1]);
	}

	public void CatmullRom(float3 p0, float3 p1, float3 p2, float3 p3)
	{
		float3 p4 = (-p0 + 6f * p1 + 1f * p2) * (1f / 6f);
		float3 p5 = (p1 + 6f * p2 - p3) * (1f / 6f);
		Bezier(p1, p4, p5, p2);
	}

	public void Arrow(float3 from, float3 to)
	{
		ArrowRelativeSizeHead(from, to, DEFAULT_UP, 0.2f);
	}

	public void Arrow(float3 from, float3 to, float3 up, float headSize)
	{
		float num = math.lengthsq(to - from);
		if (num > 1E-06f)
		{
			ArrowRelativeSizeHead(from, to, up, headSize * math.rsqrt(num));
		}
	}

	public void ArrowRelativeSizeHead(float3 from, float3 to, float3 up, float headFraction)
	{
		Line(from, to);
		float3 @float = to - from;
		float3 float2 = math.cross(@float, up);
		if (math.all(float2 == 0f))
		{
			float2 = math.cross(new float3(1f, 0f, 0f), @float);
		}
		if (math.all(float2 == 0f))
		{
			float2 = math.cross(new float3(0f, 1f, 0f), @float);
		}
		float2 = math.normalizesafe(float2) * math.length(@float);
		Line(to, to - (@float + float2) * headFraction);
		Line(to, to - (@float - float2) * headFraction);
	}

	public void Arrowhead(float3 center, float3 direction, float radius)
	{
		Arrowhead(center, direction, DEFAULT_UP, radius);
	}

	public void Arrowhead(float3 center, float3 direction, float3 up, float radius)
	{
		if (!math.all(direction == 0f))
		{
			direction = math.normalizesafe(direction);
			float3 @float = math.cross(direction, up);
			float3 float2 = center - radius * 0.5f * 0.5f * direction;
			float3 float3 = float2 + radius * direction;
			float3 float4 = float2 - radius * 0.5f * direction + radius * 0.866025f * @float;
			float3 float5 = float2 - radius * 0.5f * direction - radius * 0.866025f * @float;
			Line(float3, float4);
			Line(float4, float2);
			Line(float2, float5);
			Line(float5, float3);
		}
	}

	public void ArrowheadArc(float3 origin, float3 direction, float offset, float width = 60f)
	{
		if (math.any(direction))
		{
			if (offset < 0f)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset != 0f)
			{
				Quaternion q = Quaternion.LookRotation(direction, DEFAULT_UP);
				PushMatrix(Matrix4x4.TRS(origin, q, Vector3.one));
				float num = MathF.PI / 2f - width * (MathF.PI / 360f);
				float num2 = MathF.PI / 2f + width * (MathF.PI / 360f);
				CircleXZInternal(float3.zero, offset, num, num2);
				float3 a = new float3(math.cos(num), 0f, math.sin(num)) * offset;
				float3 b = new float3(math.cos(num2), 0f, math.sin(num2)) * offset;
				float3 @float = new float3(0f, 0f, 1.4142f * offset);
				Line(a, @float);
				Line(@float, b);
				PopMatrix();
			}
		}
	}

	public void WireGrid(float3 center, quaternion rotation, int2 cells, float2 totalSize)
	{
		cells = math.max(cells, new int2(1, 1));
		PushMatrix(float4x4.TRS(center, rotation, new Vector3(totalSize.x, 0f, totalSize.y)));
		int x = cells.x;
		int y = cells.y;
		for (int i = 0; i <= x; i++)
		{
			Line(new float3((float)i / (float)x - 0.5f, 0f, -0.5f), new float3((float)i / (float)x - 0.5f, 0f, 0.5f));
		}
		for (int j = 0; j <= y; j++)
		{
			Line(new float3(-0.5f, 0f, (float)j / (float)y - 0.5f), new float3(0.5f, 0f, (float)j / (float)y - 0.5f));
		}
		PopMatrix();
	}

	public void WireTriangle(float3 a, float3 b, float3 c)
	{
		Line(a, b);
		Line(b, c);
		Line(c, a);
	}

	[Obsolete("Use Draw.xz.WireRectangle instead")]
	public void WireRectangleXZ(float3 center, float2 size)
	{
		WireRectangle(center, quaternion.identity, size);
	}

	public void WireRectangle(float3 center, quaternion rotation, float2 size)
	{
		WirePlane(center, rotation, size);
	}

	[Obsolete("Use Draw.xy.WireRectangle instead")]
	public void WireRectangle(Rect rect)
	{
		xy.WireRectangle(rect);
	}

	public void WireTriangle(float3 center, quaternion rotation, float radius)
	{
		WirePolygon(center, 3, rotation, radius);
	}

	public void WirePentagon(float3 center, quaternion rotation, float radius)
	{
		WirePolygon(center, 5, rotation, radius);
	}

	public void WireHexagon(float3 center, quaternion rotation, float radius)
	{
		WirePolygon(center, 6, rotation, radius);
	}

	public void WirePolygon(float3 center, int vertices, quaternion rotation, float radius)
	{
		PushMatrix(float4x4.TRS(center, rotation, new float3(radius, radius, radius)));
		float3 a = new float3(0f, 0f, 1f);
		for (int i = 1; i <= vertices; i++)
		{
			float x = MathF.PI * 2f * ((float)i / (float)vertices);
			float3 @float = new float3(math.sin(x), 0f, math.cos(x));
			Line(a, @float);
			a = @float;
		}
		PopMatrix();
	}

	[Obsolete("Use Draw.xy.SolidRectangle instead")]
	public void SolidRectangle(Rect rect)
	{
		xy.SolidRectangle(rect);
	}

	public void SolidPlane(float3 center, float3 normal, float2 size)
	{
		if (math.any(normal))
		{
			SolidPlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
		}
	}

	public void SolidPlane(float3 center, quaternion rotation, float2 size)
	{
		PushMatrix(float4x4.TRS(center, rotation, new float3(size.x, 0f, size.y)));
		Reserve<BoxData>();
		Add(Command.Box);
		Add(new BoxData
		{
			center = 0,
			size = 1
		});
		PopMatrix();
	}

	private static float3 calculateTangent(float3 normal)
	{
		float3 @float = math.cross(new float3(0f, 1f, 0f), normal);
		if (math.all(@float == 0f))
		{
			@float = math.cross(new float3(1f, 0f, 0f), normal);
		}
		return @float;
	}

	public void WirePlane(float3 center, float3 normal, float2 size)
	{
		if (math.any(normal))
		{
			WirePlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
		}
	}

	public void WirePlane(float3 center, quaternion rotation, float2 size)
	{
		Reserve<PlaneData>();
		Add(Command.WirePlane);
		Add(new PlaneData
		{
			center = center,
			rotation = rotation,
			size = size
		});
	}

	public void PlaneWithNormal(float3 center, float3 normal, float2 size)
	{
		if (math.any(normal))
		{
			PlaneWithNormal(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
		}
	}

	public void PlaneWithNormal(float3 center, quaternion rotation, float2 size)
	{
		SolidPlane(center, rotation, size);
		WirePlane(center, rotation, size);
		ArrowRelativeSizeHead(center, center + math.mul(rotation, new float3(0f, 1f, 0f)) * 0.5f, math.mul(rotation, new float3(0f, 0f, 1f)), 0.2f);
	}

	public void SolidTriangle(float3 a, float3 b, float3 c)
	{
		Reserve<TriangleData>();
		Add(Command.SolidTriangle);
		Add(new TriangleData
		{
			a = a,
			b = b,
			c = c
		});
	}

	public void SolidBox(float3 center, float3 size)
	{
		Reserve<BoxData>();
		Add(Command.Box);
		Add(new BoxData
		{
			center = center,
			size = size
		});
	}

	public void SolidBox(Bounds bounds)
	{
		SolidBox(bounds.center, bounds.size);
	}

	public void SolidBox(float3 center, quaternion rotation, float3 size)
	{
		PushMatrix(float4x4.TRS(center, rotation, size));
		SolidBox(float3.zero, Vector3.one);
		PopMatrix();
	}

	public void Label3D(float3 position, quaternion rotation, string text, float size)
	{
		Label3D(position, rotation, text, size, LabelAlignment.MiddleLeft);
	}

	public void Label3D(float3 position, quaternion rotation, string text, float size, LabelAlignment alignment)
	{
		AssertBufferExists();
		DrawingData drawingData = gizmos.Target as DrawingData;
		Reserve<TextData3D>();
		Add(Command.Text3D);
		Add(new TextData3D
		{
			center = position,
			rotation = rotation,
			numCharacters = text.Length,
			size = size,
			alignment = alignment
		});
		Reserve(UnsafeUtility.SizeOf<ushort>() * text.Length);
		foreach (char c in text)
		{
			ushort value = (ushort)drawingData.fontData.GetIndex(c);
			Add(value);
		}
	}

	public void Label2D(float3 position, string text, float sizeInPixels = 14f)
	{
		Label2D(position, text, sizeInPixels, LabelAlignment.MiddleLeft);
	}

	public void Label2D(float3 position, string text, float sizeInPixels, LabelAlignment alignment)
	{
		AssertBufferExists();
		DrawingData drawingData = gizmos.Target as DrawingData;
		Reserve<TextData>();
		Add(Command.Text);
		Add(new TextData
		{
			center = position,
			numCharacters = text.Length,
			sizeInPixels = sizeInPixels,
			alignment = alignment
		});
		Reserve(UnsafeUtility.SizeOf<ushort>() * text.Length);
		foreach (char c in text)
		{
			ushort value = (ushort)drawingData.fontData.GetIndex(c);
			Add(value);
		}
	}

	public void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels = 14f)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
	}

	public void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels = 14f)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
	}

	public void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels = 14f)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
	}

	public void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels = 14f)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft);
	}

	public unsafe void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
	}

	public unsafe void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
	}

	public unsafe void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
	}

	public unsafe void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment)
	{
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
	}

	internal unsafe void Label2D(float3 position, byte* text, int byteCount, float sizeInPixels, LabelAlignment alignment)
	{
		AssertBufferExists();
		Reserve<TextData>();
		Add(Command.Text);
		Add(new TextData
		{
			center = position,
			numCharacters = byteCount,
			sizeInPixels = sizeInPixels,
			alignment = alignment
		});
		Reserve(UnsafeUtility.SizeOf<ushort>() * byteCount);
		for (int i = 0; i < byteCount; i++)
		{
			ushort num = text[i];
			if (num >= 128)
			{
				num = 63;
			}
			if (num == 10)
			{
				num = ushort.MaxValue;
			}
			if (num != 13)
			{
				Add(num);
			}
		}
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft);
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment)
	{
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment)
	{
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment)
	{
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment)
	{
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
	}

	internal unsafe void Label3D(float3 position, quaternion rotation, byte* text, int byteCount, float size, LabelAlignment alignment)
	{
		AssertBufferExists();
		Reserve<TextData3D>();
		Add(Command.Text3D);
		Add(new TextData3D
		{
			center = position,
			rotation = rotation,
			numCharacters = byteCount,
			size = size,
			alignment = alignment
		});
		Reserve(UnsafeUtility.SizeOf<ushort>() * byteCount);
		for (int i = 0; i < byteCount; i++)
		{
			ushort num = text[i];
			if (num >= 128)
			{
				num = 63;
			}
			if (num == 10)
			{
				num = ushort.MaxValue;
			}
			Add(num);
		}
	}

	public void Line(float3 a, float3 b, Color color)
	{
		Reserve<Color32, LineData>();
		Add(Command.Line | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new LineData
		{
			a = a,
			b = b
		});
	}

	public void Ray(float3 origin, float3 direction, Color color)
	{
		Line(origin, origin + direction, color);
	}

	public void Ray(Ray ray, float length, Color color)
	{
		Line(ray.origin, ray.origin + ray.direction * length, color);
	}

	public void Arc(float3 center, float3 start, float3 end, Color color)
	{
		PushColor(color);
		float3 @float = start - center;
		float3 float2 = end - center;
		float3 float3 = math.cross(float2, @float);
		if (math.any(float3 != 0f) && math.all(math.isfinite(float3)))
		{
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(@float, float3), Vector3.one);
			float num = Vector3.SignedAngle(@float, float2, float3) * (MathF.PI / 180f);
			PushMatrix(matrix);
			CircleXZInternal(float3.zero, math.length(@float), MathF.PI / 2f, MathF.PI / 2f - num);
			PopMatrix();
		}
		PopColor();
	}

	[Obsolete("Use Draw.xz.Circle instead")]
	public void CircleXZ(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
		CircleXZInternal(center, radius, startAngle, endAngle, color);
	}

	[Obsolete("Use Draw.xz.Circle instead")]
	public void CircleXZ(float3 center, float radius, Color color)
	{
		CircleXZ(center, radius, 0f, MathF.PI * 2f, color);
	}

	[Obsolete("Use Draw.xy.Circle instead")]
	public void CircleXY(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
		PushColor(color);
		PushMatrix(XZtoXYPlaneMatrix);
		CircleXZ(new float3(center.x, 0f - center.z, center.y), radius, startAngle, endAngle);
		PopMatrix();
		PopColor();
	}

	[Obsolete("Use Draw.xy.Circle instead")]
	public void CircleXY(float3 center, float radius, Color color)
	{
		CircleXY(center, radius, 0f, MathF.PI * 2f, color);
	}

	public void Circle(float3 center, float3 normal, float radius, Color color)
	{
		Reserve<Color32, CircleData>();
		Add(Command.Circle | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new CircleData
		{
			center = center,
			normal = normal,
			radius = radius
		});
	}

	public void SolidArc(float3 center, float3 start, float3 end, Color color)
	{
		PushColor(color);
		float3 @float = start - center;
		float3 float2 = end - center;
		float3 float3 = math.cross(float2, @float);
		if (math.any(float3))
		{
			Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(@float, float3), Vector3.one);
			float num = Vector3.SignedAngle(@float, float2, float3) * (MathF.PI / 180f);
			PushMatrix(matrix);
			SolidCircleXZInternal(float3.zero, math.length(@float), MathF.PI / 2f, MathF.PI / 2f - num);
			PopMatrix();
		}
		PopColor();
	}

	[Obsolete("Use Draw.xz.SolidCircle instead")]
	public void SolidCircleXZ(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
		SolidCircleXZInternal(center, radius, startAngle, endAngle, color);
	}

	[Obsolete("Use Draw.xz.SolidCircle instead")]
	public void SolidCircleXZ(float3 center, float radius, Color color)
	{
		SolidCircleXZ(center, radius, 0f, MathF.PI * 2f, color);
	}

	[Obsolete("Use Draw.xy.SolidCircle instead")]
	public void SolidCircleXY(float3 center, float radius, float startAngle, float endAngle, Color color)
	{
		PushColor(color);
		PushMatrix(XZtoXYPlaneMatrix);
		SolidCircleXZInternal(new float3(center.x, 0f - center.z, center.y), radius, startAngle, endAngle);
		PopMatrix();
		PopColor();
	}

	[Obsolete("Use Draw.xy.SolidCircle instead")]
	public void SolidCircleXY(float3 center, float radius, Color color)
	{
		SolidCircleXY(center, radius, 0f, MathF.PI * 2f, color);
	}

	public void SolidCircle(float3 center, float3 normal, float radius, Color color)
	{
		Reserve<Color32, CircleData>();
		Add(Command.PushColorInline | Command.Disc);
		Add(ConvertColor(color));
		Add(new CircleData
		{
			center = center,
			normal = normal,
			radius = radius
		});
	}

	public void SphereOutline(float3 center, float radius, Color color)
	{
		Reserve<Color32, SphereData>();
		Add(Command.SphereOutline | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new SphereData
		{
			center = center,
			radius = radius
		});
	}

	public void WireCylinder(float3 bottom, float3 top, float radius, Color color)
	{
		WireCylinder(bottom, top - bottom, math.length(top - bottom), radius, color);
	}

	public void WireCylinder(float3 position, float3 up, float height, float radius, Color color)
	{
		up = math.normalizesafe(up);
		if (!math.all(up == 0f) && !math.any(math.isnan(up)) && !math.isnan(height) && !math.isnan(radius))
		{
			PushColor(color);
			OrthonormalBasis(up, out var basis, out var basis2);
			PushMatrix(new float4x4(new float4(basis * radius, 0f), new float4(up * height, 0f), new float4(basis2 * radius, 0f), new float4(position, 1f)));
			CircleXZInternal(float3.zero, 1f);
			if (height > 0f)
			{
				CircleXZInternal(new float3(0f, 1f, 0f), 1f);
				Line(new float3(1f, 0f, 0f), new float3(1f, 1f, 0f));
				Line(new float3(-1f, 0f, 0f), new float3(-1f, 1f, 0f));
				Line(new float3(0f, 0f, 1f), new float3(0f, 1f, 1f));
				Line(new float3(0f, 0f, -1f), new float3(0f, 1f, -1f));
			}
			PopMatrix();
			PopColor();
		}
	}

	public void WireCapsule(float3 start, float3 end, float radius, Color color)
	{
		PushColor(color);
		float3 @float = end - start;
		float num = math.length(@float);
		if ((double)num < 0.0001)
		{
			WireSphere(start, radius);
		}
		else
		{
			float3 float2 = @float / num;
			WireCapsule(start - float2 * radius, float2, num + 2f * radius, radius);
		}
		PopColor();
	}

	public void WireCapsule(float3 position, float3 direction, float length, float radius, Color color)
	{
		direction = math.normalizesafe(direction);
		if (math.all(direction == 0f) || math.any(math.isnan(direction)) || math.isnan(length) || math.isnan(radius))
		{
			return;
		}
		PushColor(color);
		if (radius <= 0f)
		{
			Line(position, position + direction * length);
		}
		else
		{
			length = math.max(length, radius * 2f);
			OrthonormalBasis(direction, out var basis, out var basis2);
			PushMatrix(new float4x4(new float4(basis, 0f), new float4(direction, 0f), new float4(basis2, 0f), new float4(position, 1f)));
			CircleXZInternal(new float3(0f, radius, 0f), radius);
			PushMatrix(XZtoXYPlaneMatrix);
			CircleXZInternal(new float3(0f, 0f, radius), radius, MathF.PI);
			PopMatrix();
			PushMatrix(XZtoYZPlaneMatrix);
			CircleXZInternal(new float3(radius, 0f, 0f), radius, MathF.PI / 2f, 4.712389f);
			PopMatrix();
			if (length > 0f)
			{
				float num = length - radius;
				CircleXZInternal(new float3(0f, num, 0f), radius);
				PushMatrix(XZtoXYPlaneMatrix);
				CircleXZInternal(new float3(0f, 0f, num), radius, 0f, MathF.PI);
				PopMatrix();
				PushMatrix(XZtoYZPlaneMatrix);
				CircleXZInternal(new float3(num, 0f, 0f), radius, -MathF.PI / 2f, MathF.PI / 2f);
				PopMatrix();
				Line(new float3(radius, radius, 0f), new float3(radius, num, 0f));
				Line(new float3(0f - radius, radius, 0f), new float3(0f - radius, num, 0f));
				Line(new float3(0f, radius, radius), new float3(0f, num, radius));
				Line(new float3(0f, radius, 0f - radius), new float3(0f, num, 0f - radius));
			}
			PopMatrix();
		}
		PopColor();
	}

	public void WireSphere(float3 position, float radius, Color color)
	{
		PushColor(color);
		SphereOutline(position, radius);
		Circle(position, new float3(1f, 0f, 0f), radius);
		Circle(position, new float3(0f, 1f, 0f), radius);
		Circle(position, new float3(0f, 0f, 1f), radius);
		PopColor();
	}

	[BurstDiscard]
	public void Polyline(List<Vector3> points, bool cycle, Color color)
	{
		PushColor(color);
		for (int i = 0; i < points.Count - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Count > 1)
		{
			Line(points[points.Count - 1], points[0]);
		}
		PopColor();
	}

	[BurstDiscard]
	public void Polyline(List<Vector3> points, Color color)
	{
		Polyline(points, cycle: false, color);
	}

	[BurstDiscard]
	public void Polyline(Vector3[] points, bool cycle, Color color)
	{
		PushColor(color);
		for (int i = 0; i < points.Length - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Length > 1)
		{
			Line(points[^1], points[0]);
		}
		PopColor();
	}

	[BurstDiscard]
	public void Polyline(Vector3[] points, Color color)
	{
		Polyline(points, cycle: false, color);
	}

	[BurstDiscard]
	public void Polyline(float3[] points, bool cycle, Color color)
	{
		PushColor(color);
		for (int i = 0; i < points.Length - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Length > 1)
		{
			Line(points[^1], points[0]);
		}
		PopColor();
	}

	[BurstDiscard]
	public void Polyline(float3[] points, Color color)
	{
		Polyline(points, cycle: false, color);
	}

	public void Polyline(NativeArray<float3> points, bool cycle, Color color)
	{
		PushColor(color);
		for (int i = 0; i < points.Length - 1; i++)
		{
			Line(points[i], points[i + 1]);
		}
		if (cycle && points.Length > 1)
		{
			Line(points[points.Length - 1], points[0]);
		}
		PopColor();
	}

	public void Polyline(NativeArray<float3> points, Color color)
	{
		Polyline(points, cycle: false, color);
	}

	public void DashedLine(float3 a, float3 b, float dash, float gap, Color color)
	{
		PushColor(color);
		PolylineWithSymbol polylineWithSymbol = new PolylineWithSymbol(SymbolDecoration.None, gap, 0f, dash + gap);
		polylineWithSymbol.MoveTo(ref this, a);
		polylineWithSymbol.MoveTo(ref this, b);
		PopColor();
	}

	public void DashedPolyline(List<Vector3> points, float dash, float gap, Color color)
	{
		PushColor(color);
		PolylineWithSymbol polylineWithSymbol = new PolylineWithSymbol(SymbolDecoration.None, gap, 0f, dash + gap);
		for (int i = 0; i < points.Count; i++)
		{
			polylineWithSymbol.MoveTo(ref this, points[i]);
		}
		PopColor();
	}

	public void WireBox(float3 center, float3 size, Color color)
	{
		Reserve<Color32, BoxData>();
		Add(Command.WireBox | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new BoxData
		{
			center = center,
			size = size
		});
	}

	public void WireBox(float3 center, quaternion rotation, float3 size, Color color)
	{
		PushColor(color);
		PushMatrix(float4x4.TRS(center, rotation, size));
		WireBox(float3.zero, new float3(1f, 1f, 1f));
		PopMatrix();
		PopColor();
	}

	public void WireBox(Bounds bounds, Color color)
	{
		WireBox(bounds.center, bounds.size, color);
	}

	public void WireMesh(Mesh mesh, Color color)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException();
		}
		PushColor(color);
		Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
		Mesh.MeshData rawMeshData = meshDataArray[0];
		JobWireMesh.JobWireMeshFunctionPointer(ref rawMeshData, ref this);
		meshDataArray.Dispose();
		PopColor();
	}

	public unsafe void WireMesh(NativeArray<float3> vertices, NativeArray<int> triangles, Color color)
	{
		PushColor(color);
		JobWireMesh.WireMesh((float3*)vertices.GetUnsafeReadOnlyPtr(), (int*)triangles.GetUnsafeReadOnlyPtr(), vertices.Length, triangles.Length, ref this);
		PopColor();
	}

	public void SolidMesh(Mesh mesh, Color color)
	{
		SolidMeshInternal(mesh, temporary: false, color);
	}

	public void Cross(float3 position, float size, Color color)
	{
		PushColor(color);
		size *= 0.5f;
		Line(position - new float3(size, 0f, 0f), position + new float3(size, 0f, 0f));
		Line(position - new float3(0f, size, 0f), position + new float3(0f, size, 0f));
		Line(position - new float3(0f, 0f, size), position + new float3(0f, 0f, size));
		PopColor();
	}

	public void Cross(float3 position, Color color)
	{
		Cross(position, 1f, color);
	}

	[Obsolete("Use Draw.xz.Cross instead")]
	public void CrossXZ(float3 position, float size, Color color)
	{
		PushColor(color);
		size *= 0.5f;
		Line(position - new float3(size, 0f, 0f), position + new float3(size, 0f, 0f));
		Line(position - new float3(0f, 0f, size), position + new float3(0f, 0f, size));
		PopColor();
	}

	[Obsolete("Use Draw.xz.Cross instead")]
	public void CrossXZ(float3 position, Color color)
	{
		CrossXZ(position, 1f, color);
	}

	[Obsolete("Use Draw.xy.Cross instead")]
	public void CrossXY(float3 position, float size, Color color)
	{
		PushColor(color);
		size *= 0.5f;
		Line(position - new float3(size, 0f, 0f), position + new float3(size, 0f, 0f));
		Line(position - new float3(0f, size, 0f), position + new float3(0f, size, 0f));
		PopColor();
	}

	[Obsolete("Use Draw.xy.Cross instead")]
	public void CrossXY(float3 position, Color color)
	{
		CrossXY(position, 1f, color);
	}

	public void Bezier(float3 p0, float3 p1, float3 p2, float3 p3, Color color)
	{
		PushColor(color);
		float3 a = p0;
		for (int i = 1; i <= 20; i++)
		{
			float t = (float)i / 20f;
			float3 @float = EvaluateCubicBezier(p0, p1, p2, p3, t);
			Line(a, @float);
			a = @float;
		}
		PopColor();
	}

	public void CatmullRom(List<Vector3> points, Color color)
	{
		if (points.Count < 2)
		{
			return;
		}
		PushColor(color);
		if (points.Count == 2)
		{
			Line(points[0], points[1]);
		}
		else
		{
			int count = points.Count;
			CatmullRom(points[0], points[0], points[1], points[2]);
			for (int i = 0; i + 3 < count; i++)
			{
				CatmullRom(points[i], points[i + 1], points[i + 2], points[i + 3]);
			}
			CatmullRom(points[count - 3], points[count - 2], points[count - 1], points[count - 1]);
		}
		PopColor();
	}

	public void CatmullRom(float3 p0, float3 p1, float3 p2, float3 p3, Color color)
	{
		PushColor(color);
		float3 p4 = (-p0 + 6f * p1 + 1f * p2) * (1f / 6f);
		float3 p5 = (p1 + 6f * p2 - p3) * (1f / 6f);
		Bezier(p1, p4, p5, p2);
		PopColor();
	}

	public void Arrow(float3 from, float3 to, Color color)
	{
		ArrowRelativeSizeHead(from, to, DEFAULT_UP, 0.2f, color);
	}

	public void Arrow(float3 from, float3 to, float3 up, float headSize, Color color)
	{
		PushColor(color);
		float num = math.lengthsq(to - from);
		if (num > 1E-06f)
		{
			ArrowRelativeSizeHead(from, to, up, headSize * math.rsqrt(num));
		}
		PopColor();
	}

	public void ArrowRelativeSizeHead(float3 from, float3 to, float3 up, float headFraction, Color color)
	{
		PushColor(color);
		Line(from, to);
		float3 @float = to - from;
		float3 float2 = math.cross(@float, up);
		if (math.all(float2 == 0f))
		{
			float2 = math.cross(new float3(1f, 0f, 0f), @float);
		}
		if (math.all(float2 == 0f))
		{
			float2 = math.cross(new float3(0f, 1f, 0f), @float);
		}
		float2 = math.normalizesafe(float2) * math.length(@float);
		Line(to, to - (@float + float2) * headFraction);
		Line(to, to - (@float - float2) * headFraction);
		PopColor();
	}

	public void Arrowhead(float3 center, float3 direction, float radius, Color color)
	{
		Arrowhead(center, direction, DEFAULT_UP, radius, color);
	}

	public void Arrowhead(float3 center, float3 direction, float3 up, float radius, Color color)
	{
		if (!math.all(direction == 0f))
		{
			PushColor(color);
			direction = math.normalizesafe(direction);
			float3 @float = math.cross(direction, up);
			float3 float2 = center - radius * 0.5f * 0.5f * direction;
			float3 float3 = float2 + radius * direction;
			float3 float4 = float2 - radius * 0.5f * direction + radius * 0.866025f * @float;
			float3 float5 = float2 - radius * 0.5f * direction - radius * 0.866025f * @float;
			Line(float3, float4);
			Line(float4, float2);
			Line(float2, float5);
			Line(float5, float3);
			PopColor();
		}
	}

	public void ArrowheadArc(float3 origin, float3 direction, float offset, float width, Color color)
	{
		if (math.any(direction))
		{
			if (offset < 0f)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset != 0f)
			{
				PushColor(color);
				Quaternion q = Quaternion.LookRotation(direction, DEFAULT_UP);
				PushMatrix(Matrix4x4.TRS(origin, q, Vector3.one));
				float num = MathF.PI / 2f - width * (MathF.PI / 360f);
				float num2 = MathF.PI / 2f + width * (MathF.PI / 360f);
				CircleXZInternal(float3.zero, offset, num, num2);
				float3 a = new float3(math.cos(num), 0f, math.sin(num)) * offset;
				float3 b = new float3(math.cos(num2), 0f, math.sin(num2)) * offset;
				float3 @float = new float3(0f, 0f, 1.4142f * offset);
				Line(a, @float);
				Line(@float, b);
				PopMatrix();
				PopColor();
			}
		}
	}

	public void ArrowheadArc(float3 origin, float3 direction, float offset, Color color)
	{
		ArrowheadArc(origin, direction, offset, 60f, color);
	}

	public void WireGrid(float3 center, quaternion rotation, int2 cells, float2 totalSize, Color color)
	{
		PushColor(color);
		cells = math.max(cells, new int2(1, 1));
		PushMatrix(float4x4.TRS(center, rotation, new Vector3(totalSize.x, 0f, totalSize.y)));
		int x = cells.x;
		int y = cells.y;
		for (int i = 0; i <= x; i++)
		{
			Line(new float3((float)i / (float)x - 0.5f, 0f, -0.5f), new float3((float)i / (float)x - 0.5f, 0f, 0.5f));
		}
		for (int j = 0; j <= y; j++)
		{
			Line(new float3(-0.5f, 0f, (float)j / (float)y - 0.5f), new float3(0.5f, 0f, (float)j / (float)y - 0.5f));
		}
		PopMatrix();
		PopColor();
	}

	public void WireTriangle(float3 a, float3 b, float3 c, Color color)
	{
		PushColor(color);
		Line(a, b);
		Line(b, c);
		Line(c, a);
		PopColor();
	}

	[Obsolete("Use Draw.xz.WireRectangle instead")]
	public void WireRectangleXZ(float3 center, float2 size, Color color)
	{
		WireRectangle(center, quaternion.identity, size, color);
	}

	public void WireRectangle(float3 center, quaternion rotation, float2 size, Color color)
	{
		WirePlane(center, rotation, size, color);
	}

	[Obsolete("Use Draw.xy.WireRectangle instead")]
	public void WireRectangle(Rect rect, Color color)
	{
		xy.WireRectangle(rect, color);
	}

	public void WireTriangle(float3 center, quaternion rotation, float radius, Color color)
	{
		WirePolygon(center, 3, rotation, radius, color);
	}

	public void WirePentagon(float3 center, quaternion rotation, float radius, Color color)
	{
		WirePolygon(center, 5, rotation, radius, color);
	}

	public void WireHexagon(float3 center, quaternion rotation, float radius, Color color)
	{
		WirePolygon(center, 6, rotation, radius, color);
	}

	public void WirePolygon(float3 center, int vertices, quaternion rotation, float radius, Color color)
	{
		PushColor(color);
		PushMatrix(float4x4.TRS(center, rotation, new float3(radius, radius, radius)));
		float3 a = new float3(0f, 0f, 1f);
		for (int i = 1; i <= vertices; i++)
		{
			float x = MathF.PI * 2f * ((float)i / (float)vertices);
			float3 @float = new float3(math.sin(x), 0f, math.cos(x));
			Line(a, @float);
			a = @float;
		}
		PopMatrix();
		PopColor();
	}

	[Obsolete("Use Draw.xy.SolidRectangle instead")]
	public void SolidRectangle(Rect rect, Color color)
	{
		xy.SolidRectangle(rect, color);
	}

	public void SolidPlane(float3 center, float3 normal, float2 size, Color color)
	{
		PushColor(color);
		if (math.any(normal))
		{
			SolidPlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
		}
		PopColor();
	}

	public void SolidPlane(float3 center, quaternion rotation, float2 size, Color color)
	{
		PushMatrix(float4x4.TRS(center, rotation, new float3(size.x, 0f, size.y)));
		Reserve<Color32, BoxData>();
		Add(Command.Box | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new BoxData
		{
			center = 0,
			size = 1
		});
		PopMatrix();
	}

	public void WirePlane(float3 center, float3 normal, float2 size, Color color)
	{
		PushColor(color);
		if (math.any(normal))
		{
			WirePlane(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
		}
		PopColor();
	}

	public void WirePlane(float3 center, quaternion rotation, float2 size, Color color)
	{
		Reserve<Color32, PlaneData>();
		Add(Command.WirePlane | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new PlaneData
		{
			center = center,
			rotation = rotation,
			size = size
		});
	}

	public void PlaneWithNormal(float3 center, float3 normal, float2 size, Color color)
	{
		PushColor(color);
		if (math.any(normal))
		{
			PlaneWithNormal(center, Quaternion.LookRotation(calculateTangent(normal), normal), size);
		}
		PopColor();
	}

	public void PlaneWithNormal(float3 center, quaternion rotation, float2 size, Color color)
	{
		PushColor(color);
		SolidPlane(center, rotation, size);
		WirePlane(center, rotation, size);
		ArrowRelativeSizeHead(center, center + math.mul(rotation, new float3(0f, 1f, 0f)) * 0.5f, math.mul(rotation, new float3(0f, 0f, 1f)), 0.2f);
		PopColor();
	}

	public void SolidTriangle(float3 a, float3 b, float3 c, Color color)
	{
		Reserve<Color32, TriangleData>();
		Add(Command.SolidTriangle | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new TriangleData
		{
			a = a,
			b = b,
			c = c
		});
	}

	public void SolidBox(float3 center, float3 size, Color color)
	{
		Reserve<Color32, BoxData>();
		Add(Command.Box | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new BoxData
		{
			center = center,
			size = size
		});
	}

	public void SolidBox(Bounds bounds, Color color)
	{
		SolidBox(bounds.center, bounds.size, color);
	}

	public void SolidBox(float3 center, quaternion rotation, float3 size, Color color)
	{
		PushColor(color);
		PushMatrix(float4x4.TRS(center, rotation, size));
		SolidBox(float3.zero, Vector3.one);
		PopMatrix();
		PopColor();
	}

	public void Label3D(float3 position, quaternion rotation, string text, float size, Color color)
	{
		Label3D(position, rotation, text, size, LabelAlignment.MiddleLeft, color);
	}

	public void Label3D(float3 position, quaternion rotation, string text, float size, LabelAlignment alignment, Color color)
	{
		AssertBufferExists();
		DrawingData drawingData = gizmos.Target as DrawingData;
		Reserve<Color32, TextData3D>();
		Add(Command.Text3D | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new TextData3D
		{
			center = position,
			rotation = rotation,
			numCharacters = text.Length,
			size = size,
			alignment = alignment
		});
		Reserve(UnsafeUtility.SizeOf<ushort>() * text.Length);
		foreach (char c in text)
		{
			ushort value = (ushort)drawingData.fontData.GetIndex(c);
			Add(value);
		}
	}

	public void Label2D(float3 position, string text, float sizeInPixels, Color color)
	{
		Label2D(position, text, sizeInPixels, LabelAlignment.MiddleLeft, color);
	}

	public void Label2D(float3 position, string text, Color color)
	{
		Label2D(position, text, 14f, color);
	}

	public void Label2D(float3 position, string text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
		AssertBufferExists();
		DrawingData drawingData = gizmos.Target as DrawingData;
		Reserve<Color32, TextData>();
		Add(Command.Text | Command.PushColorInline);
		Add(ConvertColor(color));
		Add(new TextData
		{
			center = position,
			numCharacters = text.Length,
			sizeInPixels = sizeInPixels,
			alignment = alignment
		});
		Reserve(UnsafeUtility.SizeOf<ushort>() * text.Length);
		foreach (char c in text)
		{
			ushort value = (ushort)drawingData.fontData.GetIndex(c);
			Add(value);
		}
	}

	public void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels, Color color)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
	}

	public void Label2D(float3 position, ref FixedString32Bytes text, Color color)
	{
		Label2D(position, ref text, 14f, color);
	}

	public void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels, Color color)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
	}

	public void Label2D(float3 position, ref FixedString64Bytes text, Color color)
	{
		Label2D(position, ref text, 14f, color);
	}

	public void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels, Color color)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
	}

	public void Label2D(float3 position, ref FixedString128Bytes text, Color color)
	{
		Label2D(position, ref text, 14f, color);
	}

	public void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels, Color color)
	{
		Label2D(position, ref text, sizeInPixels, LabelAlignment.MiddleLeft, color);
	}

	public void Label2D(float3 position, ref FixedString512Bytes text, Color color)
	{
		Label2D(position, ref text, 14f, color);
	}

	public unsafe void Label2D(float3 position, ref FixedString32Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
		PopColor();
	}

	public unsafe void Label2D(float3 position, ref FixedString64Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
		PopColor();
	}

	public unsafe void Label2D(float3 position, ref FixedString128Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
		PopColor();
	}

	public unsafe void Label2D(float3 position, ref FixedString512Bytes text, float sizeInPixels, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label2D(position, text.GetUnsafePtr(), text.Length, sizeInPixels, alignment);
		PopColor();
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size, Color color)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size, Color color)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size, Color color)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
	}

	public void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size, Color color)
	{
		Label3D(position, rotation, ref text, size, LabelAlignment.MiddleLeft, color);
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString32Bytes text, float size, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
		PopColor();
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString64Bytes text, float size, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
		PopColor();
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString128Bytes text, float size, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
		PopColor();
	}

	public unsafe void Label3D(float3 position, quaternion rotation, ref FixedString512Bytes text, float size, LabelAlignment alignment, Color color)
	{
		PushColor(color);
		Label3D(position, rotation, text.GetUnsafePtr(), text.Length, size, alignment);
		PopColor();
	}
}
