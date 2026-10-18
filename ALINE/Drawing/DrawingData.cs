using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Drawing.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Drawing;

public class DrawingData
{
	public struct Hasher : IEquatable<Hasher>
	{
		private ulong hash;

		public static Hasher NotSupplied
		{
			get
			{
				Hasher result = default(Hasher);
				result.hash = ulong.MaxValue;
				return result;
			}
		}

		public ulong Hash => hash;

		public static Hasher Create<T>(T init)
		{
			Hasher result = default(Hasher);
			result.Add(init);
			return result;
		}

		public void Add<T>(T hash)
		{
			this.hash = (1572869 * this.hash) ^ ((ulong)hash.GetHashCode() + 12289uL);
		}

		public override int GetHashCode()
		{
			return (int)hash;
		}

		public bool Equals(Hasher other)
		{
			return hash == other.hash;
		}
	}

	internal struct ProcessedBuilderData
	{
		public enum Type
		{
			Invalid,
			Static,
			Dynamic,
			Persistent
		}

		public struct CapturedState
		{
			public Matrix4x4 matrix;

			public Color color;
		}

		public struct MeshBuffers
		{
			public UnsafeAppendBuffer splitterOutput;

			public UnsafeAppendBuffer vertices;

			public UnsafeAppendBuffer triangles;

			public UnsafeAppendBuffer solidVertices;

			public UnsafeAppendBuffer solidTriangles;

			public UnsafeAppendBuffer textVertices;

			public UnsafeAppendBuffer textTriangles;

			public UnsafeAppendBuffer capturedState;

			public Bounds bounds;

			public MeshBuffers(Allocator allocator)
			{
				splitterOutput = new UnsafeAppendBuffer(0, 4, allocator);
				vertices = new UnsafeAppendBuffer(0, 4, allocator);
				triangles = new UnsafeAppendBuffer(0, 4, allocator);
				solidVertices = new UnsafeAppendBuffer(0, 4, allocator);
				solidTriangles = new UnsafeAppendBuffer(0, 4, allocator);
				textVertices = new UnsafeAppendBuffer(0, 4, allocator);
				textTriangles = new UnsafeAppendBuffer(0, 4, allocator);
				capturedState = new UnsafeAppendBuffer(0, 4, allocator);
				bounds = default(Bounds);
			}

			public void Dispose()
			{
				splitterOutput.Dispose();
				vertices.Dispose();
				triangles.Dispose();
				solidVertices.Dispose();
				solidTriangles.Dispose();
				textVertices.Dispose();
				textTriangles.Dispose();
				capturedState.Dispose();
			}

			private static void DisposeIfLarge(ref UnsafeAppendBuffer ls)
			{
				if (ls.Length * 3 < ls.Capacity && ls.Capacity > 1024)
				{
					AllocatorManager.AllocatorHandle allocator = ls.Allocator;
					ls.Dispose();
					ls = new UnsafeAppendBuffer(0, 4, allocator);
				}
			}

			public void DisposeIfLarge()
			{
				DisposeIfLarge(ref splitterOutput);
				DisposeIfLarge(ref vertices);
				DisposeIfLarge(ref triangles);
				DisposeIfLarge(ref solidVertices);
				DisposeIfLarge(ref solidTriangles);
				DisposeIfLarge(ref textVertices);
				DisposeIfLarge(ref textTriangles);
				DisposeIfLarge(ref capturedState);
			}
		}

		public Type type;

		public BuilderData.Meta meta;

		private bool submitted;

		public NativeArray<MeshBuffers> temporaryMeshBuffers;

		private JobHandle buildJob;

		private JobHandle splitterJob;

		public List<MeshWithType> meshes;

		private static int SubmittedJobs;

		public bool isValid => type != Type.Invalid;

		public unsafe UnsafeAppendBuffer* splitterOutputPtr => &((MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr())->splitterOutput;

		public void Init(Type type, BuilderData.Meta meta)
		{
			submitted = false;
			this.type = type;
			this.meta = meta;
			if (meshes == null)
			{
				meshes = new List<MeshWithType>();
			}
			if (!temporaryMeshBuffers.IsCreated)
			{
				temporaryMeshBuffers = new NativeArray<MeshBuffers>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				temporaryMeshBuffers[0] = new MeshBuffers(Allocator.Persistent);
			}
		}

		public unsafe void SetSplitterJob(DrawingData gizmos, JobHandle splitterJob)
		{
			this.splitterJob = splitterJob;
			if (type == Type.Static)
			{
				GeometryBuilder.CameraInfo cameraInfo = new GeometryBuilder.CameraInfo(null);
				buildJob = GeometryBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), ref cameraInfo, splitterJob);
				SubmittedJobs++;
				if (SubmittedJobs % 8 == 0)
				{
					JobHandle.ScheduleBatchedJobs();
				}
			}
		}

		public unsafe void SchedulePersistFilter(int version, int lastTickVersion, float time, int sceneModeVersion)
		{
			if (type != Type.Persistent)
			{
				throw new InvalidOperationException();
			}
			if (meta.sceneModeVersion != sceneModeVersion)
			{
				meta.version = -1;
			}
			else if (meta.version < lastTickVersion || submitted)
			{
				splitterJob.Complete();
				meta.version = version;
				if (temporaryMeshBuffers[0].splitterOutput.Length == 0)
				{
					meta.version = -1;
					return;
				}
				buildJob.Complete();
				splitterJob = new PersistentFilterJob
				{
					buffer = &((MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr())->splitterOutput,
					time = time
				}.Schedule(splitterJob);
			}
		}

		public bool IsValidForCamera(Camera camera, bool allowGizmos, bool allowCameraDefault)
		{
			if (!allowGizmos && meta.isGizmos)
			{
				return false;
			}
			if (meta.cameraTargets != null)
			{
				return meta.cameraTargets.Contains(camera);
			}
			return allowCameraDefault;
		}

		public unsafe void Schedule(DrawingData gizmos, ref GeometryBuilder.CameraInfo cameraInfo)
		{
			if (type != Type.Static)
			{
				buildJob = GeometryBuilder.Build(gizmos, (MeshBuffers*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(temporaryMeshBuffers), ref cameraInfo, splitterJob);
			}
		}

		public unsafe void BuildMeshes(DrawingData gizmos)
		{
			if (type != Type.Static || !submitted)
			{
				buildJob.Complete();
				GeometryBuilder.BuildMesh(gizmos, meshes, (MeshBuffers*)temporaryMeshBuffers.GetUnsafePtr());
				submitted = true;
			}
		}

		public unsafe void CollectMeshes(List<RenderedMeshWithType> meshes)
		{
			List<MeshWithType> list = this.meshes;
			int num = 0;
			UnsafeAppendBuffer capturedState = temporaryMeshBuffers[0].capturedState;
			_ = capturedState.Length / UnsafeUtility.SizeOf<CapturedState>();
			for (int i = 0; i < list.Count; i++)
			{
				Color color;
				Matrix4x4 matrix;
				int drawingOrderIndex;
				if ((list[i].type & MeshType.Custom) != 0)
				{
					CapturedState capturedState2 = *(CapturedState*)(capturedState.Ptr + (nint)num * (nint)sizeof(CapturedState));
					color = capturedState2.color;
					matrix = capturedState2.matrix;
					num++;
					drawingOrderIndex = meta.drawOrderIndex + 1;
				}
				else
				{
					color = Color.white;
					matrix = Matrix4x4.identity;
					drawingOrderIndex = meta.drawOrderIndex;
				}
				meshes.Add(new RenderedMeshWithType
				{
					mesh = list[i].mesh,
					type = list[i].type,
					drawingOrderIndex = drawingOrderIndex,
					color = color,
					matrix = matrix
				});
			}
		}

		private void PoolMeshes(DrawingData gizmos, bool includeCustom)
		{
			if (!isValid)
			{
				throw new InvalidOperationException();
			}
			int num = 0;
			for (int i = 0; i < meshes.Count; i++)
			{
				if ((meshes[i].type & MeshType.Custom) == 0 || (includeCustom && (meshes[i].type & MeshType.Pool) != 0))
				{
					gizmos.PoolMesh(meshes[i].mesh);
					continue;
				}
				meshes[num] = meshes[i];
				num++;
			}
			meshes.RemoveRange(num, meshes.Count - num);
		}

		public void PoolDynamicMeshes(DrawingData gizmos)
		{
			if (type != Type.Static || !submitted)
			{
				PoolMeshes(gizmos, includeCustom: false);
			}
		}

		public void Release(DrawingData gizmos)
		{
			if (!isValid)
			{
				throw new InvalidOperationException();
			}
			PoolMeshes(gizmos, includeCustom: true);
			meshes.Clear();
			type = Type.Invalid;
			splitterJob.Complete();
			buildJob.Complete();
			MeshBuffers value = temporaryMeshBuffers[0];
			value.DisposeIfLarge();
			temporaryMeshBuffers[0] = value;
		}

		public void Dispose()
		{
			if (isValid)
			{
				throw new InvalidOperationException();
			}
			splitterJob.Complete();
			buildJob.Complete();
			if (temporaryMeshBuffers.IsCreated)
			{
				temporaryMeshBuffers[0].Dispose();
				temporaryMeshBuffers.Dispose();
			}
		}
	}

	internal struct SubmittedMesh
	{
		public Mesh mesh;

		public bool temporary;
	}

	[BurstCompile]
	internal struct BuilderData : IDisposable
	{
		public enum State
		{
			Free,
			Reserved,
			Initialized,
			WaitingForSplitter,
			WaitingForUserDefinedJob
		}

		public struct Meta
		{
			public Hasher hasher;

			public RedrawScope redrawScope1;

			public RedrawScope redrawScope2;

			public int version;

			public bool isGizmos;

			public int sceneModeVersion;

			public int drawOrderIndex;

			public Camera[] cameraTargets;
		}

		public struct BitPackedMeta
		{
			private uint flags;

			private const int UniqueIDBitshift = 17;

			private const int IsBuiltInFlagIndex = 16;

			private const int IndexMask = 65535;

			private const int MaxDataIndex = 65535;

			public const int UniqueIdMask = 32767;

			public int dataIndex => (int)(flags & 0xFFFF);

			public int uniqueID => (int)(flags >> 17);

			public bool isBuiltInCommandBuilder => (flags & 0x10000) != 0;

			public BitPackedMeta(int dataIndex, int uniqueID, bool isBuiltInCommandBuilder)
			{
				if (dataIndex > 65535)
				{
					throw new Exception("Too many command builders active. Are some command builders not being disposed?");
				}
				flags = (uint)(dataIndex | (uniqueID << 17)) | (isBuiltInCommandBuilder ? 65536u : 0u);
			}

			public static bool operator ==(BitPackedMeta lhs, BitPackedMeta rhs)
			{
				return lhs.flags == rhs.flags;
			}

			public static bool operator !=(BitPackedMeta lhs, BitPackedMeta rhs)
			{
				return lhs.flags != rhs.flags;
			}

			public override bool Equals(object obj)
			{
				if (obj is BitPackedMeta bitPackedMeta)
				{
					return flags == bitPackedMeta.flags;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (int)flags;
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private unsafe delegate bool AnyBuffersWrittenToDelegate(UnsafeAppendBuffer* buffers, int numBuffers);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private unsafe delegate void ResetAllBuffersToDelegate(UnsafeAppendBuffer* buffers, int numBuffers);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate bool AnyBuffersWrittenTo_000002FB_0024PostfixBurstDelegate(UnsafeAppendBuffer* buffers, int numBuffers);

		internal static class AnyBuffersWrittenTo_000002FB_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<AnyBuffersWrittenTo_000002FB_0024PostfixBurstDelegate>(AnyBuffersWrittenTo).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static bool Invoke(UnsafeAppendBuffer* buffers, int numBuffers)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						return ((delegate* unmanaged[Cdecl]<UnsafeAppendBuffer*, int, bool>)functionPointer)(buffers, numBuffers);
					}
				}
				return AnyBuffersWrittenTo_0024BurstManaged(buffers, numBuffers);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void ResetAllBuffers_000002FC_0024PostfixBurstDelegate(UnsafeAppendBuffer* buffers, int numBuffers);

		internal static class ResetAllBuffers_000002FC_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<ResetAllBuffers_000002FC_0024PostfixBurstDelegate>(ResetAllBuffers).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static void Invoke(UnsafeAppendBuffer* buffers, int numBuffers)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						((delegate* unmanaged[Cdecl]<UnsafeAppendBuffer*, int, void>)functionPointer)(buffers, numBuffers);
						return;
					}
				}
				ResetAllBuffers_0024BurstManaged(buffers, numBuffers);
			}
		}

		public BitPackedMeta packedMeta;

		public List<SubmittedMesh> meshes;

		public NativeArray<UnsafeAppendBuffer> commandBuffers;

		public bool preventDispose;

		private JobHandle splitterJob;

		private JobHandle disposeDependency;

		private AllowedDelay disposeDependencyDelay;

		private GCHandle disposeGCHandle;

		public Meta meta;

		private static int UniqueIDCounter = 0;

		private unsafe static readonly AnyBuffersWrittenToDelegate AnyBuffersWrittenToInvoke = BurstCompiler.CompileFunctionPointer<AnyBuffersWrittenToDelegate>(AnyBuffersWrittenTo).Invoke;

		private unsafe static readonly ResetAllBuffersToDelegate ResetAllBuffersToInvoke = BurstCompiler.CompileFunctionPointer<ResetAllBuffersToDelegate>(ResetAllBuffers).Invoke;

		public State state { get; private set; }

		public unsafe UnsafeAppendBuffer* bufferPtr => (UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr();

		public void Reserve(int dataIndex, bool isBuiltInCommandBuilder)
		{
			if (state != 0)
			{
				throw new InvalidOperationException();
			}
			state = State.Reserved;
			packedMeta = new BitPackedMeta(dataIndex, UniqueIDCounter++ & 0x7FFF, isBuiltInCommandBuilder);
		}

		public void Init(Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos, int drawOrderIndex, int sceneModeVersion)
		{
			if (state != State.Reserved)
			{
				throw new InvalidOperationException();
			}
			meta = new Meta
			{
				hasher = hasher,
				redrawScope1 = frameRedrawScope,
				redrawScope2 = customRedrawScope,
				isGizmos = isGizmos,
				version = 0,
				drawOrderIndex = drawOrderIndex,
				sceneModeVersion = sceneModeVersion,
				cameraTargets = null
			};
			if (meshes == null)
			{
				meshes = new List<SubmittedMesh>();
			}
			if (!commandBuffers.IsCreated)
			{
				commandBuffers = new NativeArray<UnsafeAppendBuffer>(JobsUtility.ThreadIndexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				for (int i = 0; i < commandBuffers.Length; i++)
				{
					commandBuffers[i] = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
				}
			}
			state = State.Initialized;
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
		private unsafe static bool AnyBuffersWrittenTo(UnsafeAppendBuffer* buffers, int numBuffers)
		{
			return AnyBuffersWrittenTo_000002FB_0024BurstDirectCall.Invoke(buffers, numBuffers);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
		private unsafe static void ResetAllBuffers(UnsafeAppendBuffer* buffers, int numBuffers)
		{
			ResetAllBuffers_000002FC_0024BurstDirectCall.Invoke(buffers, numBuffers);
		}

		public void SubmitWithDependency(GCHandle gcHandle, JobHandle dependency, AllowedDelay allowedDelay)
		{
			state = State.WaitingForUserDefinedJob;
			disposeDependency = dependency;
			disposeDependencyDelay = allowedDelay;
			disposeGCHandle = gcHandle;
		}

		public unsafe void Submit(DrawingData gizmos)
		{
			if (state != State.Initialized)
			{
				throw new InvalidOperationException();
			}
			if (meshes.Count == 0 && !AnyBuffersWrittenToInvoke((UnsafeAppendBuffer*)commandBuffers.GetUnsafeReadOnlyPtr(), commandBuffers.Length))
			{
				Release();
				return;
			}
			this.meta.version = gizmos.version;
			Meta meta = this.meta;
			meta.drawOrderIndex = this.meta.drawOrderIndex * 3;
			int index = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Static, meta);
			meta.drawOrderIndex = this.meta.drawOrderIndex * 3 + 1;
			int index2 = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Dynamic, meta);
			meta.drawOrderIndex = this.meta.drawOrderIndex + 1000000;
			int index3 = gizmos.processedData.Reserve(ProcessedBuilderData.Type.Persistent, meta);
			splitterJob = new StreamSplitter
			{
				inputBuffers = commandBuffers,
				staticBuffer = gizmos.processedData.Get(index).splitterOutputPtr,
				dynamicBuffer = gizmos.processedData.Get(index2).splitterOutputPtr,
				persistentBuffer = gizmos.processedData.Get(index3).splitterOutputPtr
			}.Schedule();
			gizmos.processedData.Get(index).SetSplitterJob(gizmos, splitterJob);
			gizmos.processedData.Get(index2).SetSplitterJob(gizmos, splitterJob);
			gizmos.processedData.Get(index3).SetSplitterJob(gizmos, splitterJob);
			if (meshes.Count > 0)
			{
				List<MeshWithType> list = gizmos.processedData.Get(index2).meshes;
				for (int i = 0; i < meshes.Count; i++)
				{
					list.Add(new MeshWithType
					{
						mesh = meshes[i].mesh,
						type = (MeshType.Solid | MeshType.Custom | (meshes[i].temporary ? MeshType.Pool : ((MeshType)0)))
					});
				}
				meshes.Clear();
			}
			state = State.WaitingForSplitter;
		}

		public void CheckJobDependency(DrawingData gizmos, bool allowBlocking)
		{
			if (state == State.WaitingForUserDefinedJob && (disposeDependency.IsCompleted || (allowBlocking && disposeDependencyDelay == AllowedDelay.EndOfFrame)))
			{
				disposeDependency.Complete();
				disposeDependency = default(JobHandle);
				disposeGCHandle.Free();
				state = State.Initialized;
				Submit(gizmos);
			}
		}

		public void Release()
		{
			if (state == State.Free)
			{
				throw new InvalidOperationException();
			}
			state = State.Free;
			ClearData();
		}

		private unsafe void ClearData()
		{
			disposeDependency.Complete();
			splitterJob.Complete();
			meta = default(Meta);
			disposeDependency = default(JobHandle);
			preventDispose = false;
			meshes.Clear();
			ResetAllBuffers((UnsafeAppendBuffer*)commandBuffers.GetUnsafePtr(), commandBuffers.Length);
		}

		public void Dispose()
		{
			if (state == State.WaitingForUserDefinedJob)
			{
				disposeDependency.Complete();
				disposeGCHandle.Free();
				state = State.WaitingForSplitter;
			}
			if (state == State.Reserved || state == State.Initialized || state == State.WaitingForUserDefinedJob)
			{
				Debug.LogError("Drawing data is being destroyed, but a drawing instance is still active. Are you sure you have called Dispose on all drawing instances? This will cause a memory leak!");
				return;
			}
			splitterJob.Complete();
			if (commandBuffers.IsCreated)
			{
				for (int i = 0; i < commandBuffers.Length; i++)
				{
					commandBuffers[i].Dispose();
				}
				commandBuffers.Dispose();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
		internal unsafe static bool AnyBuffersWrittenTo_0024BurstManaged(UnsafeAppendBuffer* buffers, int numBuffers)
		{
			bool flag = false;
			for (int i = 0; i < numBuffers; i++)
			{
				flag |= buffers[i].Length > 0;
			}
			return flag;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		[MonoPInvokeCallback(typeof(AnyBuffersWrittenToDelegate))]
		internal unsafe static void ResetAllBuffers_0024BurstManaged(UnsafeAppendBuffer* buffers, int numBuffers)
		{
			for (int i = 0; i < numBuffers; i++)
			{
				buffers[i].Reset();
			}
		}
	}

	internal struct BuilderDataContainer : IDisposable
	{
		private BuilderData[] data;

		public unsafe int memoryUsage
		{
			get
			{
				int num = 0;
				if (data != null)
				{
					for (int i = 0; i < data.Length; i++)
					{
						NativeArray<UnsafeAppendBuffer> commandBuffers = data[i].commandBuffers;
						for (int j = 0; j < commandBuffers.Length; j++)
						{
							num += commandBuffers[j].Capacity;
						}
						num += data[i].commandBuffers.Length * sizeof(UnsafeAppendBuffer);
					}
				}
				return num;
			}
		}

		public BuilderData.BitPackedMeta Reserve(bool isBuiltInCommandBuilder)
		{
			if (data == null)
			{
				data = new BuilderData[1];
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].state == BuilderData.State.Free)
				{
					data[i].Reserve(i, isBuiltInCommandBuilder);
					return data[i].packedMeta;
				}
			}
			BuilderData[] array = new BuilderData[data.Length * 2];
			data.CopyTo(array, 0);
			data = array;
			return Reserve(isBuiltInCommandBuilder);
		}

		public void Release(BuilderData.BitPackedMeta meta)
		{
			data[meta.dataIndex].Release();
		}

		public bool StillExists(BuilderData.BitPackedMeta meta)
		{
			int dataIndex = meta.dataIndex;
			if (data == null || dataIndex >= data.Length)
			{
				return false;
			}
			return data[dataIndex].packedMeta == meta;
		}

		public ref BuilderData Get(BuilderData.BitPackedMeta meta)
		{
			int dataIndex = meta.dataIndex;
			if (data[dataIndex].state == BuilderData.State.Free)
			{
				throw new ArgumentException("Data is not reserved");
			}
			if (data[dataIndex].packedMeta != meta)
			{
				throw new ArgumentException("This command builder has already been disposed");
			}
			return ref data[dataIndex];
		}

		public void DisposeCommandBuildersWithJobDependencies(DrawingData gizmos)
		{
			if (data != null)
			{
				for (int i = 0; i < data.Length; i++)
				{
					data[i].CheckJobDependency(gizmos, allowBlocking: false);
				}
				for (int j = 0; j < data.Length; j++)
				{
					data[j].CheckJobDependency(gizmos, allowBlocking: true);
				}
			}
		}

		public void ReleaseAllUnused()
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].state == BuilderData.State.WaitingForSplitter)
				{
					data[i].Release();
				}
			}
		}

		public void Dispose()
		{
			if (data != null)
			{
				for (int i = 0; i < data.Length; i++)
				{
					data[i].Dispose();
				}
			}
			data = null;
		}
	}

	internal struct ProcessedBuilderDataContainer
	{
		private ProcessedBuilderData[] data;

		private Dictionary<ulong, List<int>> hash2index;

		private Stack<int> freeSlots;

		private Stack<List<int>> freeLists;

		public int memoryUsage
		{
			get
			{
				int num = 0;
				if (data != null)
				{
					for (int i = 0; i < data.Length; i++)
					{
						NativeArray<ProcessedBuilderData.MeshBuffers> temporaryMeshBuffers = data[i].temporaryMeshBuffers;
						for (int j = 0; j < temporaryMeshBuffers.Length; j++)
						{
							int num2 = 0;
							num2 += temporaryMeshBuffers[j].textVertices.Capacity;
							num2 += temporaryMeshBuffers[j].textTriangles.Capacity;
							num2 += temporaryMeshBuffers[j].solidVertices.Capacity;
							num2 += temporaryMeshBuffers[j].solidTriangles.Capacity;
							num2 += temporaryMeshBuffers[j].vertices.Capacity;
							num2 += temporaryMeshBuffers[j].triangles.Capacity;
							num2 += temporaryMeshBuffers[j].capturedState.Capacity;
							num2 += temporaryMeshBuffers[j].splitterOutput.Capacity;
							num += num2;
							Debug.Log(i + ":" + j + " " + num2);
						}
					}
				}
				return num;
			}
		}

		public int Reserve(ProcessedBuilderData.Type type, BuilderData.Meta meta)
		{
			if (data == null)
			{
				data = new ProcessedBuilderData[0];
				freeSlots = new Stack<int>();
				freeLists = new Stack<List<int>>();
				hash2index = new Dictionary<ulong, List<int>>();
			}
			if (freeSlots.Count == 0)
			{
				ProcessedBuilderData[] array = new ProcessedBuilderData[math.max(4, data.Length * 2)];
				data.CopyTo(array, 0);
				for (int i = data.Length; i < array.Length; i++)
				{
					freeSlots.Push(i);
				}
				data = array;
			}
			int num = freeSlots.Pop();
			data[num].Init(type, meta);
			if (!meta.hasher.Equals(Hasher.NotSupplied))
			{
				if (!hash2index.TryGetValue(meta.hasher.Hash, out var value))
				{
					if (freeLists.Count == 0)
					{
						freeLists.Push(new List<int>());
					}
					List<int> list2 = (hash2index[meta.hasher.Hash] = freeLists.Pop());
					value = list2;
				}
				value.Add(num);
			}
			return num;
		}

		public ref ProcessedBuilderData Get(int index)
		{
			if (!data[index].isValid)
			{
				throw new ArgumentException();
			}
			return ref data[index];
		}

		private void Release(DrawingData gizmos, int i)
		{
			ulong hash = data[i].meta.hasher.Hash;
			if (!data[i].meta.hasher.Equals(Hasher.NotSupplied) && hash2index.TryGetValue(hash, out var value))
			{
				value.Remove(i);
				if (value.Count == 0)
				{
					freeLists.Push(value);
					hash2index.Remove(hash);
				}
			}
			data[i].Release(gizmos);
			freeSlots.Push(i);
		}

		public void SubmitMeshes(DrawingData gizmos, Camera camera, int versionThreshold, bool allowGizmos, bool allowCameraDefault)
		{
			if (data == null)
			{
				return;
			}
			GeometryBuilder.CameraInfo cameraInfo = new GeometryBuilder.CameraInfo(camera);
			int num = 0;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault))
				{
					num++;
					data[i].Schedule(gizmos, ref cameraInfo);
				}
			}
			JobHandle.ScheduleBatchedJobs();
			for (int j = 0; j < data.Length; j++)
			{
				if (data[j].isValid && data[j].meta.version >= versionThreshold && data[j].IsValidForCamera(camera, allowGizmos, allowCameraDefault))
				{
					data[j].BuildMeshes(gizmos);
				}
			}
		}

		public void PoolDynamicMeshes(DrawingData gizmos)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid)
				{
					data[i].PoolDynamicMeshes(gizmos);
				}
			}
		}

		public void CollectMeshes(int versionThreshold, List<RenderedMeshWithType> meshes, Camera camera, bool allowGizmos, bool allowCameraDefault)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid && data[i].meta.version >= versionThreshold && data[i].IsValidForCamera(camera, allowGizmos, allowCameraDefault))
				{
					data[i].CollectMeshes(meshes);
				}
			}
		}

		public void FilterOldPersistentCommands(int version, int lastTickVersion, float time, int sceneModeVersion)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid && data[i].type == ProcessedBuilderData.Type.Persistent)
				{
					data[i].SchedulePersistFilter(version, lastTickVersion, time, sceneModeVersion);
				}
			}
		}

		public bool SetVersion(Hasher hasher, int version)
		{
			if (data == null)
			{
				return false;
			}
			if (hash2index.TryGetValue(hasher.Hash, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					int num = value[i];
					data[num].meta.version = version;
				}
				return true;
			}
			return false;
		}

		public bool SetVersion(RedrawScope scope, int version)
		{
			if (data == null)
			{
				return false;
			}
			bool result = false;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid && (data[i].meta.redrawScope1.id == scope.id || data[i].meta.redrawScope2.id == scope.id))
				{
					data[i].meta.version = version;
					result = true;
				}
			}
			return result;
		}

		public bool SetCustomScope(Hasher hasher, RedrawScope scope)
		{
			if (data == null)
			{
				return false;
			}
			if (hash2index.TryGetValue(hasher.Hash, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					int num = value[i];
					data[num].meta.redrawScope2 = scope;
				}
				return true;
			}
			return false;
		}

		public void ReleaseDataOlderThan(DrawingData gizmos, int version)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid && data[i].meta.version < version)
				{
					Release(gizmos, i);
				}
			}
		}

		public void ReleaseAllWithHash(DrawingData gizmos, Hasher hasher)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid && data[i].meta.hasher.Hash == hasher.Hash)
				{
					Release(gizmos, i);
				}
			}
		}

		public void Dispose(DrawingData gizmos)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].isValid)
				{
					Release(gizmos, i);
				}
				data[i].Dispose();
			}
			data = null;
		}
	}

	[Flags]
	internal enum MeshType
	{
		Solid = 1,
		Lines = 2,
		Text = 4,
		Custom = 8,
		Pool = 0x10,
		BaseType = 7
	}

	internal struct MeshWithType
	{
		public Mesh mesh;

		public MeshType type;
	}

	internal struct RenderedMeshWithType
	{
		public Mesh mesh;

		public MeshType type;

		public int drawingOrderIndex;

		public Color color;

		public Matrix4x4 matrix;
	}

	private struct Range
	{
		public int start;

		public int end;
	}

	private class MeshCompareByDrawingOrder : IComparer<RenderedMeshWithType>
	{
		public int Compare(RenderedMeshWithType a, RenderedMeshWithType b)
		{
			int num = (int)(a.type & MeshType.BaseType);
			int num2 = (int)(b.type & MeshType.BaseType);
			if (num == num2)
			{
				return a.drawingOrderIndex - b.drawingOrderIndex;
			}
			return num - num2;
		}
	}

	public struct CommandBufferWrapper
	{
		public CommandBuffer cmd;

		public bool allowDisablingWireframe;

		public RasterCommandBuffer cmd2;

		public void SetWireframe(bool enable)
		{
			if (cmd != null)
			{
				cmd.SetWireframe(enable);
			}
			else if (cmd2 != null && allowDisablingWireframe)
			{
				cmd2.SetWireframe(enable);
			}
		}

		public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties)
		{
			if (cmd != null)
			{
				cmd.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
			}
			else if (cmd2 != null)
			{
				cmd2.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
			}
		}
	}

	internal BuilderDataContainer data;

	internal ProcessedBuilderDataContainer processedData;

	private List<RenderedMeshWithType> meshes = new List<RenderedMeshWithType>();

	private List<Mesh> cachedMeshes = new List<Mesh>();

	private List<Mesh> stagingCachedMeshes = new List<Mesh>();

	private int lastTimeLargestCachedMeshWasUsed;

	internal SDFLookupData fontData;

	private int currentDrawOrderIndex;

	internal int sceneModeVersion;

	public Material surfaceMaterial;

	public Material lineMaterial;

	public Material textMaterial;

	public DrawingSettings settingsAsset;

	private int lastTickVersion;

	private int lastTickVersion2;

	private HashSet<int> persistentRedrawScopes = new HashSet<int>();

	internal GCHandle gizmosHandle;

	public RedrawScope frameRedrawScope;

	private Dictionary<Camera, Range> cameraVersions = new Dictionary<Camera, Range>();

	internal static readonly ProfilerMarker MarkerScheduleJobs = new ProfilerMarker("ScheduleJobs");

	internal static readonly ProfilerMarker MarkerAwaitUserDependencies = new ProfilerMarker("Await user dependencies");

	internal static readonly ProfilerMarker MarkerSchedule = new ProfilerMarker("Schedule");

	internal static readonly ProfilerMarker MarkerBuild = new ProfilerMarker("Build");

	internal static readonly ProfilerMarker MarkerPool = new ProfilerMarker("Pool");

	internal static readonly ProfilerMarker MarkerRelease = new ProfilerMarker("Release");

	internal static readonly ProfilerMarker MarkerBuildMeshes = new ProfilerMarker("Build Meshes");

	internal static readonly ProfilerMarker MarkerCollectMeshes = new ProfilerMarker("Collect Meshes");

	internal static readonly ProfilerMarker MarkerSortMeshes = new ProfilerMarker("Sort Meshes");

	internal static readonly ProfilerMarker LeakTracking = new ProfilerMarker("RedrawScope Leak Tracking");

	private static readonly MeshCompareByDrawingOrder meshSorter = new MeshCompareByDrawingOrder();

	private Plane[] frustrumPlanes = new Plane[6];

	private MaterialPropertyBlock customMaterialProperties = new MaterialPropertyBlock();

	private int adjustedSceneModeVersion => sceneModeVersion + (Application.isPlaying ? 1000 : 0);

	private static float CurrentTime
	{
		get
		{
			if (!Application.isPlaying)
			{
				return Time.realtimeSinceStartup;
			}
			return Time.time;
		}
	}

	public DrawingSettings.Settings settingsRef
	{
		get
		{
			if (settingsAsset == null)
			{
				settingsAsset = DrawingSettings.GetSettingsAsset();
				if (settingsAsset == null)
				{
					throw new InvalidOperationException("ALINE settings could not be found");
				}
			}
			return settingsAsset.settings;
		}
	}

	public int version { get; private set; } = 1;


	private int totalMemoryUsage => data.memoryUsage + processedData.memoryUsage;

	internal int GetNextDrawOrderIndex()
	{
		currentDrawOrderIndex++;
		return currentDrawOrderIndex;
	}

	internal void PoolMesh(Mesh mesh)
	{
		stagingCachedMeshes.Add(mesh);
	}

	private void SortPooledMeshes()
	{
		cachedMeshes.Sort((Mesh a, Mesh b) => b.vertexCount - a.vertexCount);
	}

	internal Mesh GetMesh(int desiredVertexCount)
	{
		if (cachedMeshes.Count > 0)
		{
			int num = 0;
			int num2 = cachedMeshes.Count;
			while (num2 > num + 1)
			{
				int num3 = (num + num2) / 2;
				if (cachedMeshes[num3].vertexCount < desiredVertexCount)
				{
					num2 = num3;
				}
				else
				{
					num = num3;
				}
			}
			Mesh result = cachedMeshes[num];
			if (num == 0)
			{
				lastTimeLargestCachedMeshWasUsed = version;
			}
			cachedMeshes.RemoveAt(num);
			return result;
		}
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		mesh.MarkDynamic();
		return mesh;
	}

	internal void LoadFontDataIfNecessary()
	{
		if (fontData.material == null)
		{
			SDFFont font = DefaultFonts.LoadDefaultFont();
			fontData.Dispose();
			fontData = new SDFLookupData(font);
		}
	}

	private static void UpdateTime()
	{
		SharedDrawingData.BurstTime.Data = CurrentTime;
	}

	public CommandBuilder GetBuilder(bool renderInGame = false)
	{
		UpdateTime();
		return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default(RedrawScope), !renderInGame, isBuiltInCommandBuilder: false, adjustedSceneModeVersion);
	}

	internal CommandBuilder GetBuiltInBuilder(bool renderInGame = false)
	{
		UpdateTime();
		return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, default(RedrawScope), !renderInGame, isBuiltInCommandBuilder: true, adjustedSceneModeVersion);
	}

	public CommandBuilder GetBuilder(RedrawScope redrawScope, bool renderInGame = false)
	{
		UpdateTime();
		return new CommandBuilder(this, Hasher.NotSupplied, frameRedrawScope, redrawScope, !renderInGame, isBuiltInCommandBuilder: false, adjustedSceneModeVersion);
	}

	public CommandBuilder GetBuilder(Hasher hasher, RedrawScope redrawScope = default(RedrawScope), bool renderInGame = false)
	{
		if (!hasher.Equals(Hasher.NotSupplied))
		{
			DiscardData(hasher);
		}
		UpdateTime();
		return new CommandBuilder(this, hasher, frameRedrawScope, redrawScope, !renderInGame, isBuiltInCommandBuilder: false, adjustedSceneModeVersion);
	}

	private void DiscardData(Hasher hasher)
	{
		processedData.ReleaseAllWithHash(this, hasher);
	}

	internal void OnChangingPlayMode()
	{
		sceneModeVersion++;
	}

	public bool Draw(Hasher hasher)
	{
		if (hasher.Equals(Hasher.NotSupplied))
		{
			throw new ArgumentException("Invalid hash value");
		}
		return processedData.SetVersion(hasher, version);
	}

	public bool Draw(Hasher hasher, RedrawScope scope)
	{
		if (hasher.Equals(Hasher.NotSupplied))
		{
			throw new ArgumentException("Invalid hash value");
		}
		processedData.SetCustomScope(hasher, scope);
		return processedData.SetVersion(hasher, version);
	}

	internal void Draw(RedrawScope scope)
	{
		if (scope.id != 0)
		{
			processedData.SetVersion(scope, version);
		}
	}

	internal void DrawUntilDisposed(RedrawScope scope)
	{
		if (scope.id != 0)
		{
			Draw(scope);
			persistentRedrawScopes.Add(scope.id);
		}
	}

	internal void DisposeRedrawScope(RedrawScope scope)
	{
		if (scope.id != 0)
		{
			processedData.SetVersion(scope, -1);
			persistentRedrawScopes.Remove(scope.id);
		}
	}

	public void TickFramePreRender()
	{
		data.DisposeCommandBuildersWithJobDependencies(this);
		processedData.FilterOldPersistentCommands(version, lastTickVersion, CurrentTime, adjustedSceneModeVersion);
		foreach (int persistentRedrawScope in persistentRedrawScopes)
		{
			processedData.SetVersion(new RedrawScope(this, persistentRedrawScope), version);
		}
		processedData.ReleaseDataOlderThan(this, lastTickVersion2 + 1);
		lastTickVersion2 = lastTickVersion;
		lastTickVersion = version;
		currentDrawOrderIndex = 0;
		cachedMeshes.AddRange(stagingCachedMeshes);
		stagingCachedMeshes.Clear();
		SortPooledMeshes();
		if (version - lastTimeLargestCachedMeshWasUsed > 60 && cachedMeshes.Count > 0)
		{
			UnityEngine.Object.DestroyImmediate(cachedMeshes[0]);
			cachedMeshes.RemoveAt(0);
			lastTimeLargestCachedMeshWasUsed = version;
		}
	}

	public void PostRenderCleanup()
	{
		data.ReleaseAllUnused();
		version++;
	}

	private void LoadMaterials()
	{
		if (surfaceMaterial == null)
		{
			surfaceMaterial = Resources.Load<Material>("aline_surface_mat");
		}
		if (lineMaterial == null)
		{
			lineMaterial = Resources.Load<Material>("aline_outline_mat");
		}
		if (fontData.material == null)
		{
			SDFFont font = DefaultFonts.LoadDefaultFont();
			fontData.Dispose();
			fontData = new SDFLookupData(font);
		}
	}

	public DrawingData()
	{
		gizmosHandle = GCHandle.Alloc(this, GCHandleType.Weak);
		LoadMaterials();
	}

	private static int CeilLog2(int x)
	{
		return (int)math.ceil(math.log2(x));
	}

	public void Render(Camera cam, bool allowGizmos, CommandBufferWrapper commandBuffer, bool allowCameraDefault)
	{
		LoadMaterials();
		if (surfaceMaterial == null || lineMaterial == null)
		{
			return;
		}
		Plane[] planes = frustrumPlanes;
		GeometryUtility.CalculateFrustumPlanes(cam, planes);
		if (!cameraVersions.TryGetValue(cam, out var value))
		{
			Range range = default(Range);
			range.start = int.MinValue;
			range.end = int.MinValue;
			value = range;
		}
		if (value.end > lastTickVersion)
		{
			value.end = version + 1;
		}
		else
		{
			Range range = default(Range);
			range.start = value.end;
			range.end = version + 1;
			value = range;
		}
		value.start = Mathf.Max(value.start, lastTickVersion2 + 1);
		DrawingSettings.Settings settings = settingsRef;
		commandBuffer.SetWireframe(enable: false);
		if (0 == 0)
		{
			processedData.SubmitMeshes(this, cam, value.start, allowGizmos, allowCameraDefault);
			meshes.Clear();
			processedData.CollectMeshes(value.start, meshes, cam, allowGizmos, allowCameraDefault);
			processedData.PoolDynamicMeshes(this);
			meshes.Sort(meshSorter);
			int nameID = Shader.PropertyToID("_Color");
			int nameID2 = Shader.PropertyToID("_FadeColor");
			Color color = new Color(1f, 1f, 1f, settings.solidOpacity);
			Color value2 = new Color(1f, 1f, 1f, settings.solidOpacityBehindObjects);
			Color value3 = new Color(1f, 1f, 1f, settings.lineOpacity);
			Color value4 = new Color(1f, 1f, 1f, settings.lineOpacityBehindObjects);
			Color value5 = new Color(1f, 1f, 1f, settings.textOpacity);
			Color value6 = new Color(1f, 1f, 1f, settings.textOpacityBehindObjects);
			int num = 0;
			while (num < meshes.Count)
			{
				int i = num + 1;
				MeshType meshType;
				for (meshType = meshes[num].type & MeshType.BaseType; i < meshes.Count && (meshes[i].type & MeshType.BaseType) == meshType; i++)
				{
				}
				customMaterialProperties.Clear();
				Material material;
				switch (meshType)
				{
				case MeshType.Solid:
					material = surfaceMaterial;
					customMaterialProperties.SetColor(nameID, color);
					customMaterialProperties.SetColor(nameID2, value2);
					break;
				case MeshType.Lines:
					material = lineMaterial;
					customMaterialProperties.SetColor(nameID, value3);
					customMaterialProperties.SetColor(nameID2, value4);
					break;
				case MeshType.Text:
					material = fontData.material;
					customMaterialProperties.SetColor(nameID, value5);
					customMaterialProperties.SetColor(nameID2, value6);
					break;
				default:
					throw new InvalidOperationException("Invalid mesh type");
				}
				for (int j = 0; j < material.passCount; j++)
				{
					for (int k = num; k < i; k++)
					{
						RenderedMeshWithType renderedMeshWithType = meshes[k];
						if ((renderedMeshWithType.type & MeshType.Custom) != 0)
						{
							if (GeometryUtility.TestPlanesAABB(planes, TransformBoundingBox(renderedMeshWithType.matrix, renderedMeshWithType.mesh.bounds)))
							{
								customMaterialProperties.SetColor(nameID, color * renderedMeshWithType.color);
								commandBuffer.DrawMesh(renderedMeshWithType.mesh, renderedMeshWithType.matrix, material, 0, j, customMaterialProperties);
								customMaterialProperties.SetColor(nameID, color);
							}
						}
						else if (GeometryUtility.TestPlanesAABB(planes, renderedMeshWithType.mesh.bounds))
						{
							commandBuffer.DrawMesh(renderedMeshWithType.mesh, Matrix4x4.identity, material, 0, j, customMaterialProperties);
						}
					}
				}
				num = i;
			}
			meshes.Clear();
		}
		cameraVersions[cam] = value;
	}

	private static Bounds TransformBoundingBox(Matrix4x4 matrix, Bounds bounds)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Bounds result = new Bounds(matrix.MultiplyPoint(min), Vector3.zero);
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(min.x, min.y, max.z)));
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(min.x, max.y, min.z)));
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(min.x, max.y, max.z)));
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(max.x, min.y, min.z)));
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(max.x, min.y, max.z)));
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(max.x, max.y, min.z)));
		result.Encapsulate(matrix.MultiplyPoint(new Vector3(max.x, max.y, max.z)));
		return result;
	}

	public void ClearData()
	{
		gizmosHandle.Free();
		data.Dispose();
		processedData.Dispose(this);
		for (int i = 0; i < cachedMeshes.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(cachedMeshes[i]);
		}
		cachedMeshes.Clear();
		fontData.Dispose();
	}
}
