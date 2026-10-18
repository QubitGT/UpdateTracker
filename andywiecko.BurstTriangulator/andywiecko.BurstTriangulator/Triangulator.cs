using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.BurstTriangulator;

public class Triangulator : IDisposable
{
	private readonly struct Circle
	{
		public readonly float2 Center;

		public readonly float Radius;

		public readonly float RadiusSq;

		public Circle(float2 center, float radius)
		{
			float radiusSq = radius * radius;
			Center = center;
			Radius = radius;
			RadiusSq = radiusSq;
		}

		public void Deconstruct(out float2 center, out float radius)
		{
			float2 center2 = Center;
			float radius2 = Radius;
			center = center2;
			radius = radius2;
		}
	}

	private readonly struct Edge : IEquatable<Edge>, IComparable<Edge>
	{
		public readonly int IdA;

		public readonly int IdB;

		public Edge(int idA, int idB)
		{
			if (idA >= idB)
			{
				int idA2 = idB;
				int idB2 = idA;
				IdA = idA2;
				IdB = idB2;
			}
			else
			{
				int idB2 = idA;
				int idA2 = idB;
				IdA = idB2;
				IdB = idA2;
			}
		}

		public static implicit operator Edge((int a, int b) ids)
		{
			return new Edge(ids.a, ids.b);
		}

		public void Deconstruct(out int idA, out int idB)
		{
			int idA2 = IdA;
			int idB2 = IdB;
			idA = idA2;
			idB = idB2;
		}

		public bool Equals(Edge other)
		{
			if (IdA == other.IdA)
			{
				return IdB == other.IdB;
			}
			return false;
		}

		public bool Contains(int id)
		{
			if (IdA != id)
			{
				return IdB == id;
			}
			return true;
		}

		public bool ContainsCommonPointWith(Edge other)
		{
			if (!Contains(other.IdA))
			{
				return Contains(other.IdB);
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (IdA >= IdB)
			{
				return IdA * IdA + IdA + IdB;
			}
			return IdB * IdB + IdA;
		}

		public int CompareTo(Edge other)
		{
			if (IdA == other.IdA)
			{
				return IdB.CompareTo(other.IdB);
			}
			return IdA.CompareTo(other.IdA);
		}

		public override string ToString()
		{
			return $"({IdA}, {IdB})";
		}
	}

	public enum Status
	{
		OK,
		ERR
	}

	public enum Preprocessor
	{
		None,
		COM,
		PCA
	}

	[Serializable]
	public class RefinementThresholds
	{
		[field: SerializeField]
		public float Area { get; set; } = 1f;


		[field: SerializeField]
		public float Angle { get; set; } = math.radians(5f);

	}

	[Serializable]
	public class TriangulationSettings
	{
		[field: SerializeField]
		public RefinementThresholds RefinementThresholds { get; } = new RefinementThresholds();


		[field: SerializeField]
		public int BatchCount { get; set; } = 64;


		[Obsolete]
		public float MinimumAngle
		{
			get
			{
				return RefinementThresholds.Angle;
			}
			set
			{
				RefinementThresholds.Angle = value;
			}
		}

		[Obsolete]
		public float MinimumArea
		{
			get
			{
				return RefinementThresholds.Area;
			}
			set
			{
				RefinementThresholds.Area = value;
			}
		}

		[Obsolete]
		public float MaximumArea
		{
			get
			{
				return RefinementThresholds.Area;
			}
			set
			{
				RefinementThresholds.Area = value;
			}
		}

		[field: SerializeField]
		public bool RefineMesh { get; set; }

		[field: SerializeField]
		public bool ConstrainEdges { get; set; }

		[field: SerializeField]
		public bool ValidateInput { get; set; } = true;


		[field: SerializeField]
		public bool RestoreBoundary { get; set; }

		[field: SerializeField]
		public int SloanMaxIters { get; set; } = 1000000;


		[field: SerializeField]
		public float ConcentricShellsParameter { get; set; } = 0.001f;


		[field: SerializeField]
		public Preprocessor Preprocessor { get; set; }
	}

	public class InputData
	{
		public NativeArray<float2> Positions { get; set; }

		public NativeArray<int> ConstraintEdges { get; set; }

		public NativeArray<float2> HoleSeeds { get; set; }
	}

	public class OutputData
	{
		private readonly Triangulator owner;

		public NativeList<float2> Positions => owner.outputPositions;

		public NativeList<int> Triangles => owner.triangles;

		public NativeReference<Status> Status => owner.status;

		public OutputData(Triangulator triangulator)
		{
			owner = triangulator;
		}
	}

	[BurstCompile]
	private struct ValidateInputPositionsJob : IJob
	{
		[ReadOnly]
		private NativeArray<float2> positions;

		private NativeReference<Status> status;

		public ValidateInputPositionsJob(Triangulator triangulator)
		{
			positions = triangulator.Input.Positions;
			status = triangulator.status;
		}

		public void Execute()
		{
			if (positions.Length < 3)
			{
				Debug.LogError("[Triangulator]: Positions.Length is less then 3!");
				status.Value |= Status.ERR;
			}
			for (int i = 0; i < positions.Length; i++)
			{
				if (!PointValidation(i))
				{
					Debug.LogError($"[Triangulator]: Positions[{i}] does not contain finite value: {positions[i]}!");
					status.Value |= Status.ERR;
				}
				if (!PointPointValidation(i))
				{
					status.Value |= Status.ERR;
				}
			}
		}

		private bool PointValidation(int i)
		{
			return math.all(math.isfinite(positions[i]));
		}

		private bool PointPointValidation(int i)
		{
			float2 @float = positions[i];
			for (int j = i + 1; j < positions.Length; j++)
			{
				float2 float2 = positions[j];
				if (math.all(@float == float2))
				{
					Debug.LogError($"[Triangulator]: Positions[{i}] and [{j}] are duplicated with value: {@float}!");
					return false;
				}
			}
			return true;
		}
	}

	[BurstCompile]
	private struct PCATransformationJob : IJob
	{
		[ReadOnly]
		private NativeArray<float2> positions;

		private NativeReference<float2> scaleRef;

		private NativeReference<float2> comRef;

		private NativeReference<float2> cRef;

		private NativeReference<float2x2> URef;

		private NativeList<float2> localPositions;

		public PCATransformationJob(Triangulator triangulator)
		{
			positions = triangulator.tmpInputPositions;
			scaleRef = triangulator.scale;
			comRef = triangulator.com;
			cRef = triangulator.pcaCenter;
			URef = triangulator.pcaMatrix;
			localPositions = triangulator.localPositions;
		}

		public void Execute()
		{
			int length = positions.Length;
			float2 @float = 0;
			foreach (float2 position in positions)
			{
				@float += position;
			}
			@float /= (float)length;
			comRef.Value = @float;
			float2x2 zero = float2x2.zero;
			foreach (float2 position2 in positions)
			{
				float2 value = position2 - @float;
				localPositions.Add(in value);
				zero += Kron(value, value);
			}
			zero /= (float)length;
			Eigen(zero, out var eigval, out var eigvec);
			URef.Value = eigvec;
			for (int i = 0; i < length; i++)
			{
				localPositions[i] = math.mul(math.transpose(eigvec), localPositions[i]);
			}
			float2 float2 = float.MaxValue;
			float2 float3 = float.MinValue;
			foreach (float2 localPosition in localPositions)
			{
				float2 = math.min(localPosition, float2);
				float3 = math.max(localPosition, float3);
			}
			eigval = (cRef.Value = 0.5f * (float2 + float3));
			float2 float5 = eigval;
			eigval = (scaleRef.Value = 2f / (float3 - float2));
			float2 float7 = eigval;
			for (int j = 0; j < length; j++)
			{
				float2 float8 = localPositions[j];
				localPositions[j] = (float8 - float5) * float7;
			}
		}
	}

	[BurstCompile]
	private struct PCATransformationHolesJob : IJob
	{
		[ReadOnly]
		private NativeArray<float2> holeSeeds;

		private NativeList<float2> localHoleSeeds;

		private NativeReference<float2>.ReadOnly scaleRef;

		private NativeReference<float2>.ReadOnly comRef;

		private NativeReference<float2>.ReadOnly cRef;

		private NativeReference<float2x2>.ReadOnly URef;

		public PCATransformationHolesJob(Triangulator triangulator)
		{
			holeSeeds = triangulator.tmpInputHoleSeeds;
			localHoleSeeds = triangulator.localHoleSeeds;
			scaleRef = triangulator.scale.AsReadOnly();
			comRef = triangulator.com.AsReadOnly();
			cRef = triangulator.pcaCenter.AsReadOnly();
			URef = triangulator.pcaMatrix.AsReadOnly();
		}

		public void Execute()
		{
			float2 value = comRef.Value;
			float2 value2 = scaleRef.Value;
			float2 value3 = cRef.Value;
			float2x2 a = math.transpose(URef.Value);
			localHoleSeeds.Resize(holeSeeds.Length, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < holeSeeds.Length; i++)
			{
				float2 @float = holeSeeds[i];
				localHoleSeeds[i] = value2 * (math.mul(a, @float - value) - value3);
			}
		}
	}

	[BurstCompile]
	private struct PCAInverseTransformationJob : IJobParallelForDefer
	{
		private NativeArray<float2> positions;

		private NativeReference<float2>.ReadOnly comRef;

		private NativeReference<float2>.ReadOnly scaleRef;

		private NativeReference<float2>.ReadOnly cRef;

		private NativeReference<float2x2>.ReadOnly URef;

		public PCAInverseTransformationJob(Triangulator triangulator)
		{
			positions = triangulator.Output.Positions.AsDeferredJobArray();
			comRef = triangulator.com.AsReadOnly();
			scaleRef = triangulator.scale.AsReadOnly();
			cRef = triangulator.pcaCenter.AsReadOnly();
			URef = triangulator.pcaMatrix.AsReadOnly();
		}

		public JobHandle Schedule(Triangulator triangulator, JobHandle dependencies)
		{
			return this.Schedule(triangulator.Output.Positions, triangulator.Settings.BatchCount, dependencies);
		}

		public void Execute(int i)
		{
			float2 @float = positions[i];
			float2 value = comRef.Value;
			float2 value2 = scaleRef.Value;
			float2 value3 = cRef.Value;
			float2x2 value4 = URef.Value;
			positions[i] = math.mul(value4, @float / value2 + value3) + value;
		}
	}

	[BurstCompile]
	private struct InitialLocalTransformationJob : IJob
	{
		[ReadOnly]
		private NativeArray<float2> positions;

		private NativeReference<float2> comRef;

		private NativeReference<float2> scaleRef;

		private NativeList<float2> localPositions;

		public InitialLocalTransformationJob(Triangulator triangulator)
		{
			positions = triangulator.tmpInputPositions;
			comRef = triangulator.com;
			scaleRef = triangulator.scale;
			localPositions = triangulator.localPositions;
		}

		public void Execute()
		{
			float2 @float = 0;
			float2 float2 = 0;
			float2 float3 = 0;
			foreach (float2 position in positions)
			{
				@float = math.min(position, @float);
				float2 = math.max(position, float2);
				float3 += position;
			}
			float3 /= (float)positions.Length;
			comRef.Value = float3;
			scaleRef.Value = 1f / math.cmax(math.max(math.abs(float2 - float3), math.abs(@float - float3)));
			localPositions.Resize(positions.Length, NativeArrayOptions.UninitializedMemory);
		}
	}

	[BurstCompile]
	private struct CalculateLocalHoleSeedsJob : IJob
	{
		[ReadOnly]
		private NativeArray<float2> holeSeeds;

		private NativeList<float2> localHoleSeeds;

		private NativeReference<float2>.ReadOnly comRef;

		private NativeReference<float2>.ReadOnly scaleRef;

		public CalculateLocalHoleSeedsJob(Triangulator triangulator)
		{
			holeSeeds = triangulator.tmpInputHoleSeeds;
			localHoleSeeds = triangulator.localHoleSeeds;
			comRef = triangulator.com.AsReadOnly();
			scaleRef = triangulator.scale.AsReadOnly();
		}

		public void Execute()
		{
			float2 value = comRef.Value;
			float2 value2 = scaleRef.Value;
			localHoleSeeds.Resize(holeSeeds.Length, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < holeSeeds.Length; i++)
			{
				localHoleSeeds[i] = value2 * (holeSeeds[i] - value);
			}
		}
	}

	[BurstCompile]
	private struct CalculateLocalPositionsJob : IJobParallelForDefer
	{
		private NativeReference<float2>.ReadOnly comRef;

		private NativeReference<float2>.ReadOnly scaleRef;

		private NativeArray<float2> localPositions;

		[ReadOnly]
		private NativeArray<float2> positions;

		public CalculateLocalPositionsJob(Triangulator triangulator)
		{
			comRef = triangulator.com.AsReadOnly();
			scaleRef = triangulator.scale.AsReadOnly();
			localPositions = triangulator.localPositions.AsDeferredJobArray();
			positions = triangulator.tmpInputPositions;
		}

		public JobHandle Schedule(Triangulator triangulator, JobHandle dependencies)
		{
			return this.Schedule(triangulator.localPositions, triangulator.Settings.BatchCount, dependencies);
		}

		public void Execute(int i)
		{
			float2 @float = positions[i];
			float2 value = comRef.Value;
			float2 value2 = scaleRef.Value;
			localPositions[i] = value2 * (@float - value);
		}
	}

	[BurstCompile]
	private struct LocalToWorldTransformationJob : IJobParallelForDefer
	{
		private NativeArray<float2> positions;

		private NativeReference<float2>.ReadOnly comRef;

		private NativeReference<float2>.ReadOnly scaleRef;

		public LocalToWorldTransformationJob(Triangulator triangulator)
		{
			positions = triangulator.Output.Positions.AsDeferredJobArray();
			comRef = triangulator.com.AsReadOnly();
			scaleRef = triangulator.scale.AsReadOnly();
		}

		public JobHandle Schedule(Triangulator triangulator, JobHandle dependencies)
		{
			return this.Schedule(triangulator.Output.Positions, triangulator.Settings.BatchCount, dependencies);
		}

		public void Execute(int i)
		{
			float2 @float = positions[i];
			float2 value = comRef.Value;
			float2 value2 = scaleRef.Value;
			positions[i] = @float / value2 + value;
		}
	}

	[BurstCompile]
	private struct ClearDataJob : IJob
	{
		private NativeReference<float2> scaleRef;

		private NativeReference<float2> comRef;

		private NativeReference<float2> cRef;

		private NativeReference<float2x2> URef;

		private NativeList<float2> outputPositions;

		private NativeList<int> triangles;

		private NativeList<int> halfedges;

		private NativeList<Circle> circles;

		private NativeList<Edge> constraintEdges;

		private NativeReference<Status> status;

		public ClearDataJob(Triangulator triangulator)
		{
			outputPositions = triangulator.outputPositions;
			triangles = triangulator.triangles;
			halfedges = triangulator.halfedges;
			circles = triangulator.circles;
			constraintEdges = triangulator.constraintEdges;
			status = triangulator.status;
			scaleRef = triangulator.scale;
			comRef = triangulator.com;
			cRef = triangulator.pcaCenter;
			URef = triangulator.pcaMatrix;
		}

		public void Execute()
		{
			outputPositions.Clear();
			triangles.Clear();
			halfedges.Clear();
			circles.Clear();
			constraintEdges.Clear();
			status.Value = Status.OK;
			scaleRef.Value = 1;
			comRef.Value = 0;
			cRef.Value = 0;
			URef.Value = float2x2.identity;
		}
	}

	[BurstCompile]
	private struct DelaunayTriangulationJob : IJob
	{
		private struct DistComparer : IComparer<int>
		{
			private NativeArray<float> dist;

			public DistComparer(NativeArray<float> dist)
			{
				this.dist = dist;
			}

			public int Compare(int x, int y)
			{
				return dist[x].CompareTo(dist[y]);
			}
		}

		private NativeReference<Status> status;

		[ReadOnly]
		private NativeArray<float2> inputPositions;

		private NativeList<float2> outputPositions;

		private NativeList<int> triangles;

		private NativeList<int> halfedges;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<float2> positions;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> ids;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<float> dists;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> hullNext;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> hullPrev;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> hullTri;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> hullHash;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> EDGE_STACK;

		private int hullStart;

		private int trianglesLen;

		private int hashSize;

		private float2 c;

		public DelaunayTriangulationJob(Triangulator triangulator)
		{
			status = triangulator.status;
			inputPositions = triangulator.Input.Positions;
			outputPositions = triangulator.outputPositions;
			triangles = triangulator.triangles;
			halfedges = triangulator.halfedges;
			positions = default(NativeArray<float2>);
			ids = default(NativeArray<int>);
			dists = default(NativeArray<float>);
			hullNext = default(NativeArray<int>);
			hullPrev = default(NativeArray<int>);
			hullTri = default(NativeArray<int>);
			hullHash = default(NativeArray<int>);
			EDGE_STACK = default(NativeArray<int>);
			hullStart = int.MaxValue;
			trianglesLen = 0;
			hashSize = 0;
			c = float.MaxValue;
		}

		private readonly int HashKey(float2 p)
		{
			return (int)math.floor(pseudoAngle(p.x - c.x, p.y - c.y) * (float)hashSize) % hashSize;
			static float pseudoAngle(float dx, float dy)
			{
				float num = dx / (math.abs(dx) + math.abs(dy));
				return ((dy > 0f) ? (3f - num) : (1f + num)) / 4f;
			}
		}

		public void Execute()
		{
			if (status.Value == Status.ERR)
			{
				return;
			}
			outputPositions.CopyFrom(in inputPositions);
			positions = outputPositions.AsArray();
			int length = positions.Length;
			int num = math.max(2 * length - 5, 0);
			triangles.Length = 3 * num;
			halfedges.Length = 3 * num;
			hashSize = (int)math.ceil(math.sqrt(length));
			using (hullPrev = new NativeArray<int>(length, Allocator.Temp))
			{
				using (hullNext = new NativeArray<int>(length, Allocator.Temp))
				{
					using (hullTri = new NativeArray<int>(length, Allocator.Temp))
					{
						using (hullHash = new NativeArray<int>(hashSize, Allocator.Temp))
						{
							using (ids = new NativeArray<int>(length, Allocator.Temp))
							{
								using (dists = new NativeArray<float>(length, Allocator.Temp))
								{
									using (EDGE_STACK = new NativeArray<int>(512, Allocator.Temp))
									{
										float2 @float = float.MaxValue;
										float2 float2 = float.MinValue;
										for (int i = 0; i < positions.Length; i++)
										{
											float2 y = positions[i];
											@float = math.min(@float, y);
											float2 = math.max(float2, y);
											ids[i] = i;
										}
										float2 x = 0.5f * (@float + float2);
										int num2 = int.MaxValue;
										int num3 = int.MaxValue;
										int num4 = int.MaxValue;
										float num5 = float.MaxValue;
										for (int j = 0; j < positions.Length; j++)
										{
											float num6 = math.distancesq(x, positions[j]);
											if (num6 < num5)
											{
												num2 = j;
												num5 = num6;
											}
										}
										float2 float3 = positions[num2];
										num5 = float.MaxValue;
										for (int k = 0; k < positions.Length; k++)
										{
											if (k != num2)
											{
												float num7 = math.distancesq(float3, positions[k]);
												if (num7 < num5)
												{
													num3 = k;
													num5 = num7;
												}
											}
										}
										float2 float4 = positions[num3];
										float num8 = float.MaxValue;
										for (int l = 0; l < positions.Length; l++)
										{
											if (l != num2 && l != num3)
											{
												float2 float5 = positions[l];
												float num9 = CircumRadiusSq(float3, float4, float5);
												if (num9 < num8)
												{
													num4 = l;
													num8 = num9;
												}
											}
										}
										float2 float6 = positions[num4];
										if (num8 == float.MaxValue)
										{
											Debug.LogError("[Triangulator]: Provided input is not supported!");
											status.Value |= Status.ERR;
											return;
										}
										int num11;
										if (Orient2dFast(float3, float4, float6) < 0f)
										{
											int num10 = num4;
											num11 = num3;
											num3 = num10;
											num4 = num11;
											float2 float7 = float6;
											float2 float8 = float4;
											float4 = float7;
											float6 = float8;
										}
										c = CircumCenter(float3, float4, float6);
										for (int m = 0; m < positions.Length; m++)
										{
											dists[m] = math.distancesq(c, positions[m]);
										}
										ids.Sort(new DistComparer(dists));
										hullStart = num2;
										ref NativeArray<int> reference = ref hullNext;
										int index = num2;
										num11 = (hullPrev[num4] = num3);
										reference[index] = num11;
										ref NativeArray<int> reference2 = ref hullNext;
										int index2 = num3;
										num11 = (hullPrev[num2] = num4);
										reference2[index2] = num11;
										ref NativeArray<int> reference3 = ref hullNext;
										int index3 = num4;
										num11 = (hullPrev[num3] = num2);
										reference3[index3] = num11;
										hullTri[num2] = 0;
										hullTri[num3] = 1;
										hullTri[num4] = 2;
										hullHash[HashKey(float3)] = num2;
										hullHash[HashKey(float4)] = num3;
										hullHash[HashKey(float6)] = num4;
										AddTriangle(num2, num3, num4, -1, -1, -1);
										for (int n = 0; n < ids.Length; n++)
										{
											int num15 = ids[n];
											if (num15 == num2 || num15 == num3 || num15 == num4)
											{
												continue;
											}
											float2 float9 = positions[num15];
											int num16 = 0;
											for (int num17 = 0; num17 < hashSize; num17++)
											{
												int num18 = HashKey(float9);
												num16 = hullHash[(num18 + num17) % hashSize];
												if (num16 != -1 && num16 != hullNext[num16])
												{
													break;
												}
											}
											num16 = hullPrev[num16];
											int num19 = num16;
											int num20 = hullNext[num19];
											while (Orient2dFast(float9, positions[num19], positions[num20]) >= 0f)
											{
												num19 = num20;
												if (num19 == num16)
												{
													num19 = int.MaxValue;
													break;
												}
												num20 = hullNext[num19];
											}
											if (num19 == int.MaxValue)
											{
												continue;
											}
											int num21 = AddTriangle(num19, num15, hullNext[num19], -1, -1, hullTri[num19]);
											hullTri[num15] = Legalize(num21 + 2);
											hullTri[num19] = num21;
											int num22 = hullNext[num19];
											num20 = hullNext[num22];
											while (Orient2dFast(float9, positions[num22], positions[num20]) < 0f)
											{
												num21 = AddTriangle(num22, num15, num20, hullTri[num15], -1, hullTri[num22]);
												hullTri[num15] = Legalize(num21 + 2);
												hullNext[num22] = num22;
												num22 = num20;
												num20 = hullNext[num22];
											}
											if (num19 == num16)
											{
												num20 = hullPrev[num19];
												while (Orient2dFast(float9, positions[num20], positions[num19]) < 0f)
												{
													num21 = AddTriangle(num20, num15, num19, -1, hullTri[num19], hullTri[num20]);
													Legalize(num21 + 2);
													hullTri[num20] = num21;
													hullNext[num19] = num19;
													num19 = num20;
													num20 = hullPrev[num19];
												}
											}
											num11 = (hullPrev[num15] = num19);
											hullStart = num11;
											ref NativeArray<int> reference4 = ref hullNext;
											int index4 = num19;
											num11 = (hullPrev[num22] = num15);
											reference4[index4] = num11;
											hullNext[num15] = num22;
											hullHash[HashKey(float9)] = num15;
											hullHash[HashKey(positions[num19])] = num19;
										}
										triangles.Length = trianglesLen;
										halfedges.Length = trianglesLen;
									}
								}
							}
						}
					}
				}
			}
		}

		private int Legalize(int a)
		{
			int num = 0;
			int num4;
			while (true)
			{
				int num2 = halfedges[a];
				int num3 = a - a % 3;
				num4 = num3 + (a + 2) % 3;
				if (num2 == -1)
				{
					if (num == 0)
					{
						break;
					}
					a = EDGE_STACK[--num];
					continue;
				}
				int num5 = num2 - num2 % 3;
				int index = num3 + (a + 1) % 3;
				int num6 = num5 + (num2 + 2) % 3;
				int num7 = triangles[num4];
				int index2 = triangles[a];
				int index3 = triangles[index];
				int num8 = triangles[num6];
				if (InCircle(positions[num7], positions[index2], positions[index3], positions[num8]))
				{
					triangles[a] = num8;
					triangles[num2] = num7;
					int num9 = halfedges[num6];
					if (num9 == -1)
					{
						int num10 = hullStart;
						do
						{
							if (hullTri[num10] == num6)
							{
								hullTri[num10] = a;
								break;
							}
							num10 = hullPrev[num10];
						}
						while (num10 != hullStart);
					}
					Link(a, num9);
					Link(num2, halfedges[num4]);
					Link(num4, num6);
					int value = num5 + (num2 + 1) % 3;
					if (num < EDGE_STACK.Length)
					{
						EDGE_STACK[num++] = value;
					}
				}
				else
				{
					if (num == 0)
					{
						break;
					}
					a = EDGE_STACK[--num];
				}
			}
			return num4;
		}

		private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
		{
			int num = trianglesLen;
			triangles[num] = i0;
			triangles[num + 1] = i1;
			triangles[num + 2] = i2;
			Link(num, a);
			Link(num + 1, b);
			Link(num + 2, c);
			trianglesLen += 3;
			return num;
		}

		private void Link(int a, int b)
		{
			halfedges[a] = b;
			if (b != -1)
			{
				halfedges[b] = a;
			}
		}
	}

	[BurstCompile]
	private struct ValidateInputConstraintEdges : IJob
	{
		[ReadOnly]
		private NativeArray<int> constraints;

		[ReadOnly]
		private NativeArray<float2> positions;

		private NativeReference<Status> status;

		public ValidateInputConstraintEdges(Triangulator triangulator)
		{
			constraints = triangulator.Input.ConstraintEdges;
			positions = triangulator.Input.Positions;
			status = triangulator.status;
		}

		public void Execute()
		{
			if (constraints.Length % 2 == 1)
			{
				Debug.LogError("[Triangulator]: Constraint input buffer does not contain even number of elements!");
				status.Value |= Status.ERR;
				return;
			}
			for (int i = 0; i < constraints.Length / 2; i++)
			{
				if (!EdgePositionsRangeValidation(i) || !EdgeValidation(i) || !EdgePointValidation(i) || !EdgeEdgeValidation(i))
				{
					status.Value |= Status.ERR;
					break;
				}
			}
		}

		private bool EdgePositionsRangeValidation(int i)
		{
			int num = constraints[2 * i];
			int num2 = constraints[2 * i + 1];
			int num3 = num;
			int num4 = num2;
			int length = positions.Length;
			if (num3 >= length || num3 < 0 || num4 >= length || num4 < 0)
			{
				Debug.LogError($"[Triangulator]: ConstraintEdges[{i}] = ({num3}, {num4}) is out of range Positions.Length = {length}!");
				return false;
			}
			return true;
		}

		private bool EdgeValidation(int i)
		{
			int num = constraints[2 * i];
			int num2 = constraints[2 * i + 1];
			int num3 = num;
			int num4 = num2;
			if (num3 == num4)
			{
				Debug.LogError($"[Triangulator]: ConstraintEdges[{i}] is length zero!");
				return false;
			}
			return true;
		}

		private bool EdgePointValidation(int i)
		{
			int num = constraints[2 * i];
			int num2 = constraints[2 * i + 1];
			int num3 = num;
			int num4 = num2;
			float2 @float = positions[num3];
			float2 float2 = positions[num4];
			float2 b = @float;
			float2 b2 = float2;
			for (int j = 0; j < positions.Length; j++)
			{
				if (j != num3 && j != num4 && PointLineSegmentIntersection(positions[j], b, b2))
				{
					Debug.LogError($"[Triangulator]: ConstraintEdges[{i}] and Positions[{j}] are collinear!");
					return false;
				}
			}
			return true;
		}

		private bool EdgeEdgeValidation(int i)
		{
			for (int j = i + 1; j < constraints.Length / 2; j++)
			{
				if (!ValidatePair(i, j))
				{
					return false;
				}
			}
			return true;
		}

		private bool ValidatePair(int i, int j)
		{
			int num = constraints[2 * i];
			int num2 = constraints[2 * i + 1];
			int num3 = num;
			int num4 = num2;
			num = constraints[2 * j];
			int num5 = constraints[2 * j + 1];
			int num6 = num;
			int num7 = num5;
			if ((num3 == num6 && num4 == num7) || (num3 == num7 && num4 == num6))
			{
				Debug.LogError($"[Triangulator]: ConstraintEdges[{i}] and [{j}] are equivalent!");
				return false;
			}
			if (num3 == num6 || num3 == num7 || num4 == num6 || num4 == num7)
			{
				return true;
			}
			float2 @float = positions[num3];
			float2 float2 = positions[num4];
			float2 float3 = positions[num6];
			float2 float4 = positions[num7];
			float2 a = @float;
			float2 a2 = float2;
			float2 b = float3;
			float2 b2 = float4;
			if (EdgeEdgeIntersection(a, a2, b, b2))
			{
				Debug.LogError($"[Triangulator]: ConstraintEdges[{i}] and [{j}] intersect!");
				return false;
			}
			return true;
		}
	}

	[BurstCompile]
	private struct ConstrainEdgesJob : IJob
	{
		private NativeReference<Status> status;

		[ReadOnly]
		private NativeArray<float2> outputPositions;

		private NativeArray<int> triangles;

		[ReadOnly]
		private NativeArray<int> inputConstraintEdges;

		private NativeList<Edge> internalConstraints;

		private NativeArray<int> halfedges;

		private readonly int maxIters;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<int> intersections;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<int> unresolvedIntersections;

		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> pointToHalfedge;

		public ConstrainEdgesJob(Triangulator triangulator)
		{
			status = triangulator.status;
			outputPositions = triangulator.outputPositions.AsDeferredJobArray();
			triangles = triangulator.triangles.AsDeferredJobArray();
			maxIters = triangulator.Settings.SloanMaxIters;
			inputConstraintEdges = triangulator.Input.ConstraintEdges;
			internalConstraints = triangulator.constraintEdges;
			halfedges = triangulator.halfedges.AsDeferredJobArray();
			intersections = default(NativeList<int>);
			unresolvedIntersections = default(NativeList<int>);
			pointToHalfedge = default(NativeArray<int>);
		}

		public void Execute()
		{
			if (status.Value != 0)
			{
				return;
			}
			using (intersections = new NativeList<int>(Allocator.Temp))
			{
				using (unresolvedIntersections = new NativeList<int>(Allocator.Temp))
				{
					using (pointToHalfedge = new NativeArray<int>(outputPositions.Length, Allocator.Temp))
					{
						for (int i = 0; i < triangles.Length; i++)
						{
							pointToHalfedge[triangles[i]] = i;
						}
						BuildInternalConstraints();
						foreach (Edge internalConstraint in internalConstraints)
						{
							TryApplyConstraint(internalConstraint);
						}
					}
				}
			}
		}

		private void BuildInternalConstraints()
		{
			internalConstraints.Length = inputConstraintEdges.Length / 2;
			for (int i = 0; i < internalConstraints.Length; i++)
			{
				internalConstraints[i] = new Edge(inputConstraintEdges[2 * i], inputConstraintEdges[2 * i + 1]);
			}
		}

		private void TryApplyConstraint(Edge c)
		{
			intersections.Clear();
			unresolvedIntersections.Clear();
			CollectIntersections(c);
			int iter = 0;
			while ((status.Value & Status.ERR) != Status.ERR)
			{
				NativeList<int> nativeList = unresolvedIntersections;
				NativeList<int> nativeList2 = intersections;
				intersections = nativeList;
				unresolvedIntersections = nativeList2;
				TryResolveIntersections(c, ref iter);
				if (unresolvedIntersections.IsEmpty)
				{
					break;
				}
			}
		}

		private void TryResolveIntersections(Edge c, ref int iter)
		{
			for (int i = 0; i < intersections.Length; i++)
			{
				if (IsMaxItersExceeded(iter++, maxIters))
				{
					return;
				}
				int value = intersections[i];
				int num = NextHalfedge(value);
				int value2 = NextHalfedge(num);
				int num2 = halfedges[value];
				int num3 = NextHalfedge(NextHalfedge(num2));
				int index = triangles[value];
				int index2 = triangles[num];
				int num4 = triangles[value2];
				int num5 = triangles[num3];
				float2 @float = outputPositions[index];
				float2 float2 = outputPositions[num5];
				float2 float3 = outputPositions[index2];
				float2 float4 = outputPositions[num4];
				float2 a = @float;
				float2 b = float2;
				float2 c2 = float3;
				float2 d = float4;
				if (!IsConvexQuadrilateral(a, b, c2, d))
				{
					unresolvedIntersections.Add(in value);
					continue;
				}
				triangles[value] = num5;
				triangles[num2] = num4;
				pointToHalfedge[num5] = value;
				pointToHalfedge[num4] = num2;
				int num6 = halfedges[num3];
				halfedges[value] = num6;
				if (num6 != -1)
				{
					halfedges[num6] = value;
				}
				int num7 = halfedges[value2];
				halfedges[num2] = num7;
				if (num7 != -1)
				{
					halfedges[num7] = num2;
				}
				halfedges[value2] = num3;
				halfedges[num3] = value2;
				for (int j = i + 1; j < intersections.Length; j++)
				{
					int num8 = intersections[j];
					intersections[j] = ((num8 == value2) ? num2 : ((num8 == num3) ? value : num8));
				}
				for (int k = 0; k < unresolvedIntersections.Length; k++)
				{
					int num9 = unresolvedIntersections[k];
					unresolvedIntersections[k] = ((num9 == value2) ? num2 : ((num9 == num3) ? value : num9));
				}
				Edge e = new Edge(num4, num5);
				if (EdgeEdgeIntersection(c, e))
				{
					unresolvedIntersections.Add(in value2);
				}
			}
			intersections.Clear();
		}

		private bool EdgeEdgeIntersection(Edge e1, Edge e2)
		{
			float2 @float = outputPositions[e1.IdA];
			float2 float2 = outputPositions[e1.IdB];
			float2 a = @float;
			float2 a2 = float2;
			@float = outputPositions[e2.IdA];
			float2 float3 = outputPositions[e2.IdB];
			float2 b = @float;
			float2 b2 = float3;
			if (!e1.ContainsCommonPointWith(e2))
			{
				return Triangulator.EdgeEdgeIntersection(a, a2, b, b2);
			}
			return false;
		}

		private void CollectIntersections(Edge edge)
		{
			int num = -1;
			Edge edge2 = edge;
			edge2.Deconstruct(out var idA, out var idB);
			int index = idA;
			int num2 = idB;
			int num3 = pointToHalfedge[index];
			int num4 = num3;
			do
			{
				int value = NextHalfedge(num4);
				if (triangles[value] == num2)
				{
					break;
				}
				int index2 = NextHalfedge(value);
				if (EdgeEdgeIntersection(edge, new Edge(triangles[value], triangles[index2])))
				{
					unresolvedIntersections.Add(in value);
					num = halfedges[value];
					break;
				}
				num4 = halfedges[index2];
			}
			while (num4 != -1 && num4 != num3);
			num4 = halfedges[num3];
			if (num == -1 && num4 != -1)
			{
				num4 = NextHalfedge(num4);
				do
				{
					int value2 = NextHalfedge(num4);
					if (triangles[value2] == num2)
					{
						break;
					}
					int index3 = NextHalfedge(value2);
					if (EdgeEdgeIntersection(edge, new Edge(triangles[value2], triangles[index3])))
					{
						unresolvedIntersections.Add(in value2);
						num = halfedges[value2];
						break;
					}
					num4 = halfedges[num4];
					if (num4 == -1)
					{
						break;
					}
					num4 = NextHalfedge(num4);
				}
				while (num4 != num3);
			}
			while (num != -1)
			{
				int num5 = num;
				num = -1;
				int value3 = NextHalfedge(num5);
				int value4 = NextHalfedge(value3);
				if (triangles[value4] != num2)
				{
					if (EdgeEdgeIntersection(edge, new Edge(triangles[value3], triangles[value4])))
					{
						unresolvedIntersections.Add(in value3);
						num = halfedges[value3];
					}
					else if (EdgeEdgeIntersection(edge, new Edge(triangles[value4], triangles[num5])))
					{
						unresolvedIntersections.Add(in value4);
						num = halfedges[value4];
					}
					continue;
				}
				break;
			}
		}

		private bool IsMaxItersExceeded(int iter, int maxIters)
		{
			if (iter >= maxIters)
			{
				Debug.LogError("[Triangulator]: Sloan max iterations exceeded! This may suggest that input data is hard to resolve by Sloan's algorithm. It usually happens when the scale of the input positions is not uniform. Please try to post-process input data or increase SloanMaxIters value.");
				status.Value |= Status.ERR;
				return true;
			}
			return false;
		}
	}

	[BurstCompile]
	private struct RefineMeshJob : IJob
	{
		private int initialPointsCount;

		private readonly bool restoreBoundary;

		private readonly float maximumArea2;

		private readonly float minimumAngle;

		private readonly float D;

		private NativeReference<float2>.ReadOnly scaleRef;

		private NativeReference<Status>.ReadOnly status;

		private NativeList<int> triangles;

		private NativeList<float2> outputPositions;

		private NativeList<Circle> circles;

		private NativeList<int> halfedges;

		[NativeDisableContainerSafetyRestriction]
		private NativeQueue<int> trianglesQueue;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<int> badTriangles;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<int> pathPoints;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<int> pathHalfedges;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<bool> visitedTriangles;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<Edge> constraints;

		[NativeDisableContainerSafetyRestriction]
		private NativeList<bool> constrainedHalfedges;

		public RefineMeshJob(Triangulator triangulator, NativeList<Edge> constraints)
		{
			restoreBoundary = triangulator.Settings.RestoreBoundary;
			maximumArea2 = 2f * triangulator.Settings.RefinementThresholds.Area;
			minimumAngle = triangulator.Settings.RefinementThresholds.Angle;
			D = triangulator.Settings.ConcentricShellsParameter;
			scaleRef = triangulator.scale.AsReadOnly();
			status = triangulator.status.AsReadOnly();
			triangles = triangulator.triangles;
			outputPositions = triangulator.outputPositions;
			circles = triangulator.circles;
			halfedges = triangulator.halfedges;
			initialPointsCount = 0;
			trianglesQueue = default(NativeQueue<int>);
			badTriangles = default(NativeList<int>);
			pathPoints = default(NativeList<int>);
			pathHalfedges = default(NativeList<int>);
			visitedTriangles = default(NativeList<bool>);
			constrainedHalfedges = default(NativeList<bool>);
			this.constraints = constraints;
		}

		public void Execute()
		{
			if (status.Value != 0)
			{
				return;
			}
			initialPointsCount = outputPositions.Length;
			circles.Length = triangles.Length / 3;
			int idA;
			int idB;
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				idB = triangles[3 * i];
				idA = triangles[3 * i + 1];
				int num = triangles[3 * i + 2];
				int i2 = idB;
				int j = idA;
				int k = num;
				circles[i] = CalculateCircumCircle(i2, j, k, outputPositions.AsArray());
			}
			using (trianglesQueue = new NativeQueue<int>(Allocator.Temp))
			{
				using (badTriangles = new NativeList<int>(triangles.Length / 3, Allocator.Temp))
				{
					using (pathPoints = new NativeList<int>(Allocator.Temp))
					{
						using (pathHalfedges = new NativeList<int>(Allocator.Temp))
						{
							using (visitedTriangles = new NativeList<bool>(triangles.Length / 3, Allocator.Temp))
							{
								using (constrainedHalfedges = new NativeList<bool>(triangles.Length, Allocator.Temp)
								{
									Length = triangles.Length
								})
								{
									using NativeList<int> heQueue = new NativeList<int>(triangles.Length, Allocator.Temp);
									using NativeList<int> tQueue = new NativeList<int>(triangles.Length, Allocator.Temp);
									NativeList<Edge> nativeList = default(NativeList<Edge>);
									if (!constraints.IsCreated)
									{
										nativeList = (constraints = new NativeList<Edge>(Allocator.Temp));
										for (int l = 0; l < halfedges.Length; l++)
										{
											if (halfedges[l] == -1)
											{
												ref NativeList<Edge> reference = ref constraints;
												Edge value = new Edge(triangles[l], triangles[NextHalfedge(l)]);
												reference.Add(in value);
											}
										}
									}
									if (!restoreBoundary)
									{
										for (int m = 0; m < halfedges.Length; m++)
										{
											Edge value2 = new Edge(triangles[m], triangles[NextHalfedge(m)]);
											if (halfedges[m] == -1 && !constraints.Contains(value2))
											{
												constraints.Add(in value2);
											}
										}
									}
									for (int n = 0; n < constrainedHalfedges.Length; n++)
									{
										for (int num2 = 0; num2 < constraints.Length; num2++)
										{
											constraints[num2].Deconstruct(out idA, out idB);
											int num3 = idA;
											int num4 = idB;
											idB = triangles[n];
											int num5 = triangles[NextHalfedge(n)];
											int num6 = idB;
											int num7 = num5;
											if (num6 >= num7)
											{
												int num8 = num7;
												idB = num6;
												num6 = num8;
												num7 = idB;
											}
											else
											{
												int num9 = num6;
												idB = num7;
												num6 = num9;
												num7 = idB;
											}
											if (num3 == num6 && num4 == num7)
											{
												constrainedHalfedges[n] = true;
												break;
											}
										}
									}
									for (int value3 = 0; value3 < constrainedHalfedges.Length; value3++)
									{
										if (constrainedHalfedges[value3] && IsEncroached(value3))
										{
											heQueue.Add(in value3);
										}
									}
									SplitEncroachedEdges(heQueue, default(NativeList<int>));
									for (int value4 = 0; value4 < triangles.Length / 3; value4++)
									{
										if (IsBadTriangle(value4))
										{
											tQueue.Add(in value4);
										}
									}
									for (int num10 = 0; num10 < tQueue.Length; num10++)
									{
										int num11 = tQueue[num10];
										if (num11 != -1)
										{
											SplitTriangle(num11, heQueue, tQueue);
										}
									}
									if (nativeList.IsCreated)
									{
										nativeList.Dispose();
									}
								}
							}
						}
					}
				}
			}
		}

		private void SplitEncroachedEdges(NativeList<int> heQueue, NativeList<int> tQueue)
		{
			for (int i = 0; i < heQueue.Length; i++)
			{
				int num = heQueue[i];
				if (num != -1)
				{
					SplitEdge(num, heQueue, tQueue);
				}
			}
			heQueue.Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsEncroached(int he0)
		{
			int num = NextHalfedge(he0);
			int index = NextHalfedge(num);
			float2 @float = outputPositions[triangles[he0]];
			float2 float2 = outputPositions[triangles[num]];
			float2 float3 = outputPositions[triangles[index]];
			return math.dot(@float - float3, float2 - float3) <= 0f;
		}

		private void SplitEdge(int he, NativeList<int> heQueue, NativeList<int> tQueue)
		{
			int num = triangles[he];
			int num2 = triangles[NextHalfedge(he)];
			int num3 = num;
			int num4 = num2;
			float2 @float = outputPositions[num3];
			float2 float2 = outputPositions[num4];
			float2 float3 = @float;
			float2 float4 = float2;
			float2 p;
			if ((num3 < initialPointsCount && num4 < initialPointsCount) || (num3 >= initialPointsCount && num4 >= initialPointsCount))
			{
				p = 0.5f * (float3 + float4);
			}
			else
			{
				float num5 = math.distance(float3, float4);
				int num6 = (int)math.round(math.log2(0.5f * num5 / D));
				float num7 = D / num5 * (float)(1 << num6);
				num7 = ((num3 < initialPointsCount) ? num7 : (1f - num7));
				p = (1f - num7) * float3 + num7 * float4;
			}
			constrainedHalfedges[he] = false;
			int num8 = halfedges[he];
			if (num8 != -1)
			{
				constrainedHalfedges[num8] = false;
			}
			if (halfedges[he] != -1)
			{
				UnsafeInsertPointBulk(p, he / 3, heQueue, tQueue);
				int num9 = triangles.Length - 3;
				int value = -1;
				int value2 = -1;
				while (value == -1 || value2 == -1)
				{
					int num10 = NextHalfedge(num9);
					if (triangles[num10] == num3)
					{
						value = num9;
					}
					if (triangles[num10] == num4)
					{
						value2 = num9;
					}
					int index = NextHalfedge(num10);
					num9 = halfedges[index];
				}
				if (IsEncroached(value))
				{
					heQueue.Add(in value);
				}
				int value3 = halfedges[value];
				if (IsEncroached(value3))
				{
					heQueue.Add(in value3);
				}
				if (IsEncroached(value2))
				{
					heQueue.Add(in value2);
				}
				int value4 = halfedges[value2];
				if (IsEncroached(value4))
				{
					heQueue.Add(in value4);
				}
				constrainedHalfedges[value] = true;
				constrainedHalfedges[value3] = true;
				constrainedHalfedges[value2] = true;
				constrainedHalfedges[value4] = true;
			}
			else
			{
				UnsafeInsertPointBoundary(p, he, heQueue, tQueue);
				int num11 = 3 * (pathPoints.Length - 1);
				int value5 = halfedges.Length - 1;
				int value6 = halfedges.Length - num11;
				if (IsEncroached(value5))
				{
					heQueue.Add(in value5);
				}
				if (IsEncroached(value6))
				{
					heQueue.Add(in value6);
				}
				constrainedHalfedges[value5] = true;
				constrainedHalfedges[value6] = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool IsBadTriangle(int tId)
		{
			int num = triangles[3 * tId];
			int num2 = triangles[3 * tId + 1];
			int num3 = triangles[3 * tId + 2];
			int i = num;
			int j = num2;
			int k = num3;
			float2 value = scaleRef.Value;
			NativeArray<float2> source = outputPositions.AsArray();
			if (!(Area2(i, j, k, source) > maximumArea2 * value.x * value.y))
			{
				return AngleIsTooSmall(tId, minimumAngle);
			}
			return true;
		}

		private void SplitTriangle(int tId, NativeList<int> heQueue, NativeList<int> tQueue)
		{
			Circle circle = circles[tId];
			NativeList<int> nativeList = new NativeList<int>(Allocator.Temp);
			int num;
			for (int i = 0; i < constrainedHalfedges.Length; i++)
			{
				if (!constrainedHalfedges[i])
				{
					continue;
				}
				num = triangles[i];
				int num2 = triangles[NextHalfedge(i)];
				int num3 = num;
				int num4 = num2;
				if (halfedges[i] == -1 || num3 < num4)
				{
					float2 @float = outputPositions[num3];
					float2 float2 = outputPositions[num4];
					float2 float3 = @float;
					float2 float4 = float2;
					if (math.dot(float3 - circle.Center, float4 - circle.Center) <= 0f)
					{
						nativeList.Add(in i);
					}
				}
			}
			if (nativeList.IsEmpty)
			{
				UnsafeInsertPointBulk(circle.Center, tId, heQueue, tQueue);
				return;
			}
			float2 value = scaleRef.Value;
			num = triangles[3 * tId];
			int num5 = triangles[3 * tId + 1];
			int num6 = triangles[3 * tId + 2];
			int i2 = num;
			int j = num5;
			int k = num6;
			NativeArray<float2> source = outputPositions.AsArray();
			if (Area2(i2, j, k, source) > maximumArea2 * value.x * value.y)
			{
				foreach (int item in nativeList.AsReadOnly())
				{
					int value2 = item;
					heQueue.Add(in value2);
				}
			}
			if (!heQueue.IsEmpty)
			{
				tQueue.Add(in tId);
				SplitEncroachedEdges(heQueue, tQueue);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool AngleIsTooSmall(int tId, float minimumAngle)
		{
			int num = triangles[3 * tId];
			int num2 = triangles[3 * tId + 1];
			int num3 = triangles[3 * tId + 2];
			int index = num;
			int index2 = num2;
			int index3 = num3;
			float2 @float = outputPositions[index];
			float2 float2 = outputPositions[index2];
			float2 float3 = outputPositions[index3];
			float2 float4 = @float;
			float2 float5 = float2;
			float2 float6 = float3;
			float2 float7 = float5 - float4;
			float2 float8 = float6 - float5;
			float2 float9 = float4 - float6;
			return math.any(math.abs(math.float3(Angle(float7, -float9), Angle(float8, -float7), Angle(float9, -float8))) < minimumAngle);
		}

		private int UnsafeInsertPointCommon(float2 p, int initTriangle)
		{
			int length = outputPositions.Length;
			outputPositions.Add(in p);
			badTriangles.Clear();
			trianglesQueue.Clear();
			pathPoints.Clear();
			pathHalfedges.Clear();
			visitedTriangles.Clear();
			visitedTriangles.Length = triangles.Length / 3;
			trianglesQueue.Enqueue(initTriangle);
			badTriangles.Add(in initTriangle);
			visitedTriangles[initTriangle] = true;
			RecalculateBadTriangles(p);
			return length;
		}

		private void UnsafeInsertPointBulk(float2 p, int initTriangle, NativeList<int> heQueue = default(NativeList<int>), NativeList<int> tQueue = default(NativeList<int>))
		{
			int pId = UnsafeInsertPointCommon(p, initTriangle);
			BuildStarPolygon();
			ProcessBadTriangles(heQueue, tQueue);
			BuildNewTrianglesForStar(pId, heQueue, tQueue);
		}

		private void UnsafeInsertPointBoundary(float2 p, int initHe, NativeList<int> heQueue = default(NativeList<int>), NativeList<int> tQueue = default(NativeList<int>))
		{
			int pId = UnsafeInsertPointCommon(p, initHe / 3);
			BuildAmphitheaterPolygon(initHe);
			ProcessBadTriangles(heQueue, tQueue);
			BuildNewTrianglesForAmphitheater(pId, heQueue, tQueue);
		}

		private void RecalculateBadTriangles(float2 p)
		{
			int item;
			while (trianglesQueue.TryDequeue(out item))
			{
				VisitEdge(p, 3 * item);
				VisitEdge(p, 3 * item + 1);
				VisitEdge(p, 3 * item + 2);
			}
		}

		private void VisitEdge(float2 p, int t0)
		{
			int num = halfedges[t0];
			if (num == -1 || constrainedHalfedges[num])
			{
				return;
			}
			int value = num / 3;
			if (!visitedTriangles[value])
			{
				Circle circle = circles[value];
				if (math.distancesq(circle.Center, p) <= circle.RadiusSq)
				{
					badTriangles.Add(in value);
					trianglesQueue.Enqueue(value);
					visitedTriangles[value] = true;
				}
			}
		}

		private void BuildAmphitheaterPolygon(int initHe)
		{
			int num = initHe;
			int num2 = triangles[num];
			int value2;
			while (true)
			{
				num = NextHalfedge(num);
				if (triangles[num] == num2)
				{
					break;
				}
				int value = halfedges[num];
				if (value == -1 || !badTriangles.Contains(value / 3))
				{
					ref NativeList<int> reference = ref pathPoints;
					value2 = triangles[num];
					reference.Add(in value2);
					pathHalfedges.Add(in value);
				}
				else
				{
					num = value;
				}
			}
			ref NativeList<int> reference2 = ref pathPoints;
			value2 = triangles[initHe];
			reference2.Add(in value2);
			ref NativeList<int> reference3 = ref pathHalfedges;
			value2 = -1;
			reference3.Add(in value2);
		}

		private void BuildStarPolygon()
		{
			int num = -1;
			for (int i = 0; i < badTriangles.Length; i++)
			{
				int num2 = badTriangles[i];
				for (int j = 0; j < 3; j++)
				{
					int num3 = 3 * num2 + j;
					int value = halfedges[num3];
					if (value == -1 || !badTriangles.Contains(value / 3))
					{
						ref NativeList<int> reference = ref pathPoints;
						int value2 = triangles[num3];
						reference.Add(in value2);
						pathHalfedges.Add(in value);
						num = num3;
						break;
					}
				}
				if (num != -1)
				{
					break;
				}
			}
			int num4 = num;
			int num5 = pathPoints[0];
			while (true)
			{
				num4 = NextHalfedge(num4);
				if (triangles[num4] != num5)
				{
					int value3 = halfedges[num4];
					if (value3 == -1 || !badTriangles.Contains(value3 / 3))
					{
						ref NativeList<int> reference2 = ref pathPoints;
						int value2 = triangles[num4];
						reference2.Add(in value2);
						pathHalfedges.Add(in value3);
					}
					else
					{
						num4 = value3;
					}
					continue;
				}
				break;
			}
		}

		private void ProcessBadTriangles(NativeList<int> heQueue, NativeList<int> tQueue)
		{
			badTriangles.Sort();
			for (int num = badTriangles.Length - 1; num >= 0; num--)
			{
				int num2 = badTriangles[num];
				triangles.RemoveAt(3 * num2 + 2);
				triangles.RemoveAt(3 * num2 + 1);
				triangles.RemoveAt(3 * num2);
				circles.RemoveAt(num2);
				RemoveHalfedge(3 * num2 + 2, 0);
				RemoveHalfedge(3 * num2 + 1, 1);
				RemoveHalfedge(3 * num2, 2);
				constrainedHalfedges.RemoveAt(3 * num2 + 2);
				constrainedHalfedges.RemoveAt(3 * num2 + 1);
				constrainedHalfedges.RemoveAt(3 * num2);
				for (int i = 3 * num2; i < halfedges.Length; i++)
				{
					int num3 = halfedges[i];
					if (num3 != -1)
					{
						halfedges[(num3 < 3 * num2) ? num3 : i] -= 3;
					}
				}
				for (int j = 0; j < pathHalfedges.Length; j++)
				{
					if (pathHalfedges[j] > 3 * num2 + 2)
					{
						pathHalfedges[j] -= 3;
					}
				}
				if (heQueue.IsCreated)
				{
					for (int k = 0; k < heQueue.Length; k++)
					{
						int num4 = heQueue[k];
						if (num4 == 3 * num2 || num4 == 3 * num2 + 1 || num4 == 3 * num2 + 2)
						{
							heQueue[k] = -1;
						}
						else if (num4 > 3 * num2 + 2)
						{
							heQueue[k] -= 3;
						}
					}
				}
				if (tQueue.IsCreated)
				{
					for (int l = 0; l < tQueue.Length; l++)
					{
						int num5 = tQueue[l];
						if (num5 == num2)
						{
							tQueue[l] = -1;
						}
						else if (num5 > num2)
						{
							tQueue[l]--;
						}
					}
				}
			}
		}

		private void RemoveHalfedge(int he, int offset)
		{
			int num = halfedges[he];
			int num2 = ((num > he) ? (num - offset) : num);
			if (num2 > -1)
			{
				halfedges[num2] = -1;
			}
			halfedges.RemoveAt(he);
		}

		private void BuildNewTrianglesForStar(int pId, NativeList<int> heQueue, NativeList<int> tQueue)
		{
			int length = triangles.Length;
			triangles.Length += 3 * pathPoints.Length;
			circles.Length += pathPoints.Length;
			for (int i = 0; i < pathPoints.Length - 1; i++)
			{
				triangles[length + 3 * i] = pId;
				triangles[length + 3 * i + 1] = pathPoints[i];
				triangles[length + 3 * i + 2] = pathPoints[i + 1];
				circles[length / 3 + i] = CalculateCircumCircle(pId, pathPoints[i], pathPoints[i + 1], outputPositions.AsArray());
			}
			ref NativeList<int> reference = ref triangles;
			reference[reference.Length - 3] = pId;
			ref NativeList<int> reference2 = ref triangles;
			int index = reference2.Length - 2;
			ref NativeList<int> reference3 = ref pathPoints;
			reference2[index] = reference3[reference3.Length - 1];
			ref NativeList<int> reference4 = ref triangles;
			reference4[reference4.Length - 1] = pathPoints[0];
			ref NativeList<Circle> reference5 = ref circles;
			int index2 = reference5.Length - 1;
			ref NativeList<int> reference6 = ref pathPoints;
			reference5[index2] = CalculateCircumCircle(pId, reference6[reference6.Length - 1], pathPoints[0], outputPositions.AsArray());
			int length2 = halfedges.Length;
			halfedges.Length += 3 * pathPoints.Length;
			constrainedHalfedges.Length += 3 * pathPoints.Length;
			for (int j = 0; j < pathPoints.Length - 1; j++)
			{
				int num = pathHalfedges[j];
				halfedges[3 * j + 1 + length2] = num;
				if (num != -1)
				{
					halfedges[num] = 3 * j + 1 + length2;
					constrainedHalfedges[3 * j + 1 + length2] = constrainedHalfedges[num];
				}
				else
				{
					constrainedHalfedges[3 * j + 1 + length2] = true;
				}
				halfedges[3 * j + 2 + length2] = 3 * j + 3 + length2;
				halfedges[3 * j + 3 + length2] = 3 * j + 2 + length2;
			}
			ref NativeList<int> reference7 = ref pathHalfedges;
			int num2 = reference7[reference7.Length - 1];
			halfedges[length2 + 3 * (pathPoints.Length - 1) + 1] = num2;
			if (num2 != -1)
			{
				halfedges[num2] = length2 + 3 * (pathPoints.Length - 1) + 1;
				constrainedHalfedges[length2 + 3 * (pathPoints.Length - 1) + 1] = constrainedHalfedges[num2];
			}
			else
			{
				constrainedHalfedges[length2 + 3 * (pathPoints.Length - 1) + 1] = true;
			}
			halfedges[length2] = length2 + 3 * (pathPoints.Length - 1) + 2;
			halfedges[length2 + 3 * (pathPoints.Length - 1) + 2] = length2;
			if (!heQueue.IsCreated)
			{
				return;
			}
			for (int k = 0; k < pathPoints.Length - 1; k++)
			{
				int value = length2 + 3 * k + 1;
				if (constrainedHalfedges[value] && IsEncroached(value))
				{
					heQueue.Add(in value);
				}
				else if (tQueue.IsCreated && IsBadTriangle(value / 3))
				{
					int value2 = value / 3;
					tQueue.Add(in value2);
				}
			}
		}

		private void BuildNewTrianglesForAmphitheater(int pId, NativeList<int> heQueue, NativeList<int> tQueue)
		{
			int length = triangles.Length;
			triangles.Length += 3 * (pathPoints.Length - 1);
			circles.Length += pathPoints.Length - 1;
			for (int i = 0; i < pathPoints.Length - 1; i++)
			{
				triangles[length + 3 * i] = pId;
				triangles[length + 3 * i + 1] = pathPoints[i];
				triangles[length + 3 * i + 2] = pathPoints[i + 1];
				circles[length / 3 + i] = CalculateCircumCircle(pId, pathPoints[i], pathPoints[i + 1], outputPositions.AsArray());
			}
			int length2 = halfedges.Length;
			halfedges.Length += 3 * (pathPoints.Length - 1);
			constrainedHalfedges.Length += 3 * (pathPoints.Length - 1);
			for (int j = 0; j < pathPoints.Length - 2; j++)
			{
				int num = pathHalfedges[j];
				halfedges[3 * j + 1 + length2] = num;
				if (num != -1)
				{
					halfedges[num] = 3 * j + 1 + length2;
					constrainedHalfedges[3 * j + 1 + length2] = constrainedHalfedges[num];
				}
				else
				{
					constrainedHalfedges[3 * j + 1 + length2] = true;
				}
				halfedges[3 * j + 2 + length2] = 3 * j + 3 + length2;
				halfedges[3 * j + 3 + length2] = 3 * j + 2 + length2;
			}
			ref NativeList<int> reference = ref pathHalfedges;
			int num2 = reference[reference.Length - 2];
			halfedges[length2 + 3 * (pathPoints.Length - 2) + 1] = num2;
			if (num2 != -1)
			{
				halfedges[num2] = length2 + 3 * (pathPoints.Length - 2) + 1;
				constrainedHalfedges[length2 + 3 * (pathPoints.Length - 2) + 1] = constrainedHalfedges[num2];
			}
			else
			{
				constrainedHalfedges[length2 + 3 * (pathPoints.Length - 2) + 1] = true;
			}
			halfedges[length2] = -1;
			halfedges[length2 + 3 * (pathPoints.Length - 2) + 2] = -1;
			if (!heQueue.IsCreated)
			{
				return;
			}
			for (int k = 0; k < pathPoints.Length - 1; k++)
			{
				int value = length2 + 3 * k + 1;
				if (constrainedHalfedges[value] && IsEncroached(value))
				{
					heQueue.Add(in value);
				}
				else if (tQueue.IsCreated && IsBadTriangle(value / 3))
				{
					int value2 = value / 3;
					tQueue.Add(in value2);
				}
			}
		}
	}

	private interface IPlantingSeedJobMode<TSelf>
	{
		bool PlantBoundarySeed { get; }

		bool PlantHolesSeed { get; }

		NativeArray<float2> HoleSeeds { get; }

		TSelf Create(Triangulator triangulator);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct PlantBoundary : IPlantingSeedJobMode<PlantBoundary>
	{
		public bool PlantBoundarySeed => true;

		public bool PlantHolesSeed => false;

		public NativeArray<float2> HoleSeeds => default(NativeArray<float2>);

		public PlantBoundary Create(Triangulator _)
		{
			return default(PlantBoundary);
		}
	}

	private struct PlantHoles : IPlantingSeedJobMode<PlantHoles>
	{
		private NativeArray<float2> holeSeeds;

		public readonly bool PlantBoundarySeed => false;

		public readonly bool PlantHolesSeed => true;

		public readonly NativeArray<float2> HoleSeeds => holeSeeds;

		public PlantHoles Create(Triangulator triangulator)
		{
			PlantHoles result = default(PlantHoles);
			result.holeSeeds = triangulator.Input.HoleSeeds;
			return result;
		}
	}

	private struct PlantBoundaryAndHoles : IPlantingSeedJobMode<PlantBoundaryAndHoles>
	{
		private NativeArray<float2> holeSeeds;

		public readonly bool PlantBoundarySeed => true;

		public readonly bool PlantHolesSeed => true;

		public readonly NativeArray<float2> HoleSeeds => holeSeeds;

		public PlantBoundaryAndHoles Create(Triangulator triangulator)
		{
			PlantBoundaryAndHoles result = default(PlantBoundaryAndHoles);
			result.holeSeeds = triangulator.Input.HoleSeeds;
			return result;
		}
	}

	[BurstCompile]
	private struct PlantingSeedsJob<T> : IJob where T : struct, IPlantingSeedJobMode<T>
	{
		[ReadOnly]
		private NativeArray<float2> positions;

		private readonly T mode;

		private NativeReference<Status>.ReadOnly status;

		private NativeList<int> triangles;

		private NativeList<float2> outputPositions;

		private NativeList<Circle> circles;

		private NativeList<Edge> constraintEdges;

		private NativeList<int> halfedges;

		public PlantingSeedsJob(Triangulator triangulator)
		{
			positions = triangulator.Input.Positions;
			mode = default(T).Create(triangulator);
			status = triangulator.status.AsReadOnly();
			triangles = triangulator.triangles;
			outputPositions = triangulator.outputPositions;
			circles = triangulator.circles;
			constraintEdges = triangulator.constraintEdges;
			halfedges = triangulator.halfedges;
		}

		public void Execute()
		{
			if (status.Value != 0)
			{
				return;
			}
			if (circles.Length != triangles.Length / 3)
			{
				circles.Length = triangles.Length / 3;
				for (int i = 0; i < triangles.Length / 3; i++)
				{
					int num = triangles[3 * i];
					int num2 = triangles[3 * i + 1];
					int num3 = triangles[3 * i + 2];
					int i2 = num;
					int j = num2;
					int k = num3;
					circles[i] = CalculateCircumCircle(i2, j, k, outputPositions.AsArray());
				}
			}
			using NativeArray<bool> visitedTriangles = new NativeArray<bool>(triangles.Length / 3, Allocator.Temp);
			using NativeList<int> badTriangles = new NativeList<int>(triangles.Length / 3, Allocator.Temp);
			using NativeQueue<int> trianglesQueue = new NativeQueue<int>(Allocator.Temp);
			PlantSeeds(visitedTriangles, badTriangles, trianglesQueue);
			using NativeHashSet<int> potentialPointsToRemove = new NativeHashSet<int>(3 * badTriangles.Length, Allocator.Temp);
			GeneratePotentialPointsToRemove(positions.Length, potentialPointsToRemove, badTriangles);
			RemoveBadTriangles(badTriangles);
			RemovePoints(potentialPointsToRemove);
		}

		private void PlantSeeds(NativeArray<bool> visitedTriangles, NativeList<int> badTriangles, NativeQueue<int> trianglesQueue)
		{
			if (mode.PlantBoundarySeed)
			{
				for (int i = 0; i < halfedges.Length; i++)
				{
					if (halfedges[i] == -1 && !visitedTriangles[i / 3] && !constraintEdges.Contains(new Edge(triangles[i], triangles[NextHalfedge(i)])))
					{
						PlantSeed(i / 3, visitedTriangles, badTriangles, trianglesQueue);
					}
				}
			}
			if (!mode.PlantHolesSeed)
			{
				return;
			}
			foreach (float2 holeSeed in mode.HoleSeeds)
			{
				int num = FindTriangle(holeSeed);
				if (num != -1)
				{
					PlantSeed(num, visitedTriangles, badTriangles, trianglesQueue);
				}
			}
		}

		private void PlantSeed(int tId, NativeArray<bool> visitedTriangles, NativeList<int> badTriangles, NativeQueue<int> trianglesQueue)
		{
			if (!visitedTriangles[tId])
			{
				visitedTriangles[tId] = true;
				trianglesQueue.Enqueue(tId);
				badTriangles.Add(in tId);
				while (trianglesQueue.TryDequeue(out tId))
				{
					int num = triangles[3 * tId];
					int num2 = triangles[3 * tId + 1];
					int num3 = triangles[3 * tId + 2];
					int num4 = num;
					int num5 = num2;
					int num6 = num3;
					TryEnqueue(new Edge(num4, num5), 3 * tId, constraintEdges, halfedges);
					TryEnqueue(new Edge(num5, num6), 3 * tId + 1, constraintEdges, halfedges);
					TryEnqueue(new Edge(num6, num4), 3 * tId + 2, constraintEdges, halfedges);
				}
			}
			void TryEnqueue(Edge e, int he, NativeList<Edge> constraintEdges, NativeList<int> halfedges)
			{
				int num7 = halfedges[he];
				if (!constraintEdges.Contains(e) && num7 != -1)
				{
					int value = num7 / 3;
					if (!visitedTriangles[value])
					{
						visitedTriangles[value] = true;
						trianglesQueue.Enqueue(value);
						badTriangles.Add(in value);
					}
				}
			}
		}

		private int FindTriangle(float2 p)
		{
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				int num = triangles[3 * i];
				int num2 = triangles[3 * i + 1];
				int num3 = triangles[3 * i + 2];
				int index = num;
				int index2 = num2;
				int index3 = num3;
				float2 @float = outputPositions[index];
				float2 float2 = outputPositions[index2];
				float2 float3 = outputPositions[index3];
				float2 a = @float;
				float2 b = float2;
				float2 c = float3;
				if (PointInsideTriangle(p, a, b, c))
				{
					return i;
				}
			}
			return -1;
		}

		private void GeneratePotentialPointsToRemove(int initialPointsCount, NativeHashSet<int> potentialPointsToRemove, NativeList<int> badTriangles)
		{
			foreach (int item in badTriangles.AsReadOnly())
			{
				for (int i = 0; i < 3; i++)
				{
					int num = triangles[3 * item + i];
					if (num >= initialPointsCount)
					{
						potentialPointsToRemove.Add(num);
					}
				}
			}
		}

		private void RemoveBadTriangles(NativeList<int> badTriangles)
		{
			badTriangles.Sort();
			for (int num = badTriangles.Length - 1; num >= 0; num--)
			{
				int num2 = badTriangles[num];
				triangles.RemoveAt(3 * num2 + 2);
				triangles.RemoveAt(3 * num2 + 1);
				triangles.RemoveAt(3 * num2);
				circles.RemoveAt(num2);
				RemoveHalfedge(3 * num2 + 2, 0);
				RemoveHalfedge(3 * num2 + 1, 1);
				RemoveHalfedge(3 * num2, 2);
				for (int i = 3 * num2; i < halfedges.Length; i++)
				{
					int num3 = halfedges[i];
					if (num3 != -1)
					{
						halfedges[(num3 < 3 * num2) ? num3 : i] -= 3;
					}
				}
			}
		}

		private void RemoveHalfedge(int he, int offset)
		{
			int num = halfedges[he];
			int num2 = ((num > he) ? (num - offset) : num);
			if (num2 > -1)
			{
				halfedges[num2] = -1;
			}
			halfedges.RemoveAt(he);
		}

		private void RemovePoints(NativeHashSet<int> potentialPointsToRemove)
		{
			NativeArray<int> nativeArray = new NativeArray<int>(outputPositions.Length, Allocator.Temp);
			NativeArray<int> nativeArray2 = new NativeArray<int>(outputPositions.Length, Allocator.Temp);
			for (int i = 0; i < nativeArray.Length; i++)
			{
				nativeArray[i] = -1;
			}
			for (int j = 0; j < triangles.Length; j++)
			{
				nativeArray[triangles[j]] = j;
			}
			using NativeArray<int> array = potentialPointsToRemove.ToNativeArray(Allocator.Temp);
			array.Sort();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				int num2 = array[num];
				if (nativeArray[num2] == -1)
				{
					outputPositions.RemoveAt(num2);
					for (int k = num2; k < nativeArray2.Length; k++)
					{
						nativeArray2[k]--;
					}
				}
			}
			for (int l = 0; l < triangles.Length; l++)
			{
				triangles[l] += nativeArray2[triangles[l]];
			}
			nativeArray.Dispose();
			nativeArray2.Dispose();
		}
	}

	private NativeList<float2> outputPositions;

	private NativeList<int> triangles;

	private NativeList<int> halfedges;

	private NativeList<Circle> circles;

	private NativeList<Edge> constraintEdges;

	private NativeReference<Status> status;

	private NativeArray<float2> tmpInputPositions;

	private NativeArray<float2> tmpInputHoleSeeds;

	private NativeList<float2> localPositions;

	private NativeList<float2> localHoleSeeds;

	private NativeReference<float2> com;

	private NativeReference<float2> scale;

	private NativeReference<float2> pcaCenter;

	private NativeReference<float2x2> pcaMatrix;

	public TriangulationSettings Settings { get; } = new TriangulationSettings();


	public InputData Input { get; set; } = new InputData();


	public OutputData Output { get; }

	public Triangulator(int capacity, Allocator allocator)
	{
		outputPositions = new NativeList<float2>(capacity, allocator);
		triangles = new NativeList<int>(6 * capacity, allocator);
		halfedges = new NativeList<int>(6 * capacity, allocator);
		circles = new NativeList<Circle>(capacity, allocator);
		constraintEdges = new NativeList<Edge>(capacity, allocator);
		status = new NativeReference<Status>(Status.OK, allocator);
		localPositions = new NativeList<float2>(capacity, allocator);
		localHoleSeeds = new NativeList<float2>(capacity, allocator);
		com = new NativeReference<float2>(allocator);
		scale = new NativeReference<float2>(1, allocator);
		pcaCenter = new NativeReference<float2>(allocator);
		pcaMatrix = new NativeReference<float2x2>(float2x2.identity, allocator);
		Output = new OutputData(this);
	}

	public Triangulator(Allocator allocator)
		: this(16384, allocator)
	{
	}

	public void Dispose()
	{
		outputPositions.Dispose();
		triangles.Dispose();
		halfedges.Dispose();
		circles.Dispose();
		constraintEdges.Dispose();
		status.Dispose();
		localPositions.Dispose();
		localHoleSeeds.Dispose();
		com.Dispose();
		scale.Dispose();
		pcaCenter.Dispose();
		pcaMatrix.Dispose();
	}

	public void Run()
	{
		Schedule().Complete();
	}

	public JobHandle Schedule(JobHandle dependencies = default(JobHandle))
	{
		dependencies = new ClearDataJob(this).Schedule(dependencies);
		dependencies = Settings.Preprocessor switch
		{
			Preprocessor.PCA => SchedulePCATransformation(dependencies), 
			Preprocessor.COM => ScheduleWorldToLocalTransformation(dependencies), 
			Preprocessor.None => dependencies, 
			_ => throw new NotImplementedException(), 
		};
		if (Settings.ValidateInput)
		{
			dependencies = new ValidateInputPositionsJob(this).Schedule(dependencies);
			dependencies = (Settings.ConstrainEdges ? new ValidateInputConstraintEdges(this).Schedule(dependencies) : dependencies);
		}
		dependencies = new DelaunayTriangulationJob(this).Schedule(dependencies);
		dependencies = (Settings.ConstrainEdges ? new ConstrainEdgesJob(this).Schedule(dependencies) : dependencies);
		NativeArray<float2> holeSeeds = Input.HoleSeeds;
		if (Settings.RestoreBoundary && Settings.ConstrainEdges)
		{
			dependencies = (holeSeeds.IsCreated ? new PlantingSeedsJob<PlantBoundaryAndHoles>(this).Schedule(dependencies) : new PlantingSeedsJob<PlantBoundary>(this).Schedule(dependencies));
		}
		else if (holeSeeds.IsCreated && Settings.ConstrainEdges)
		{
			dependencies = new PlantingSeedsJob<PlantHoles>(this).Schedule(dependencies);
		}
		dependencies = (Settings.RefineMesh ? new RefineMeshJob(this, Settings.ConstrainEdges ? constraintEdges : default(NativeList<Edge>)).Schedule(dependencies) : dependencies);
		dependencies = Settings.Preprocessor switch
		{
			Preprocessor.PCA => SchedulePCAInverseTransformation(dependencies), 
			Preprocessor.COM => ScheduleLocalToWorldTransformation(dependencies), 
			Preprocessor.None => dependencies, 
			_ => throw new NotImplementedException(), 
		};
		return dependencies;
	}

	private JobHandle SchedulePCATransformation(JobHandle dependencies)
	{
		tmpInputPositions = Input.Positions;
		Input.Positions = localPositions.AsDeferredJobArray();
		if (Input.HoleSeeds.IsCreated)
		{
			tmpInputHoleSeeds = Input.HoleSeeds;
			Input.HoleSeeds = localHoleSeeds.AsDeferredJobArray();
		}
		dependencies = new PCATransformationJob(this).Schedule(dependencies);
		if (tmpInputHoleSeeds.IsCreated)
		{
			dependencies = new PCATransformationHolesJob(this).Schedule(dependencies);
		}
		return dependencies;
	}

	private JobHandle SchedulePCAInverseTransformation(JobHandle dependencies)
	{
		dependencies = new PCAInverseTransformationJob(this).Schedule(this, dependencies);
		Input.Positions = tmpInputPositions;
		tmpInputPositions = default(NativeArray<float2>);
		if (tmpInputHoleSeeds.IsCreated)
		{
			Input.HoleSeeds = tmpInputHoleSeeds;
			tmpInputHoleSeeds = default(NativeArray<float2>);
		}
		return dependencies;
	}

	private JobHandle ScheduleWorldToLocalTransformation(JobHandle dependencies)
	{
		tmpInputPositions = Input.Positions;
		Input.Positions = localPositions.AsDeferredJobArray();
		if (Input.HoleSeeds.IsCreated)
		{
			tmpInputHoleSeeds = Input.HoleSeeds;
			Input.HoleSeeds = localHoleSeeds.AsDeferredJobArray();
		}
		dependencies = new InitialLocalTransformationJob(this).Schedule(dependencies);
		if (tmpInputHoleSeeds.IsCreated)
		{
			dependencies = new CalculateLocalHoleSeedsJob(this).Schedule(dependencies);
		}
		dependencies = new CalculateLocalPositionsJob(this).Schedule(this, dependencies);
		return dependencies;
	}

	private JobHandle ScheduleLocalToWorldTransformation(JobHandle dependencies)
	{
		dependencies = new LocalToWorldTransformationJob(this).Schedule(this, dependencies);
		Input.Positions = tmpInputPositions;
		tmpInputPositions = default(NativeArray<float2>);
		if (tmpInputHoleSeeds.IsCreated)
		{
			Input.HoleSeeds = tmpInputHoleSeeds;
			tmpInputHoleSeeds = default(NativeArray<float2>);
		}
		return dependencies;
	}

	private static int NextHalfedge(int he)
	{
		if (he % 3 != 2)
		{
			return he + 1;
		}
		return he - 2;
	}

	private static float Angle(float2 a, float2 b)
	{
		return math.atan2(Cross(a, b), math.dot(a, b));
	}

	private static float Area2(int i, int j, int k, ReadOnlySpan<float2> positions)
	{
		float2 @float = positions[i];
		float2 float2 = positions[j];
		float2 float3 = positions[k];
		float2 float4 = @float;
		float2 float5 = float2;
		float2 float6 = float3;
		float2 a = float5 - float4;
		float2 b = float6 - float4;
		return math.abs(Cross(a, b));
	}

	private static float Cross(float2 a, float2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	private static Circle CalculateCircumCircle(int i, int j, int k, NativeArray<float2> positions)
	{
		float2 @float = positions[i];
		float2 float2 = positions[j];
		float2 float3 = positions[k];
		float2 a = @float;
		float2 b = float2;
		float2 c = float3;
		return new Circle(CircumCenter(a, b, c), CircumRadius(a, b, c));
	}

	private static float CircumRadius(float2 a, float2 b, float2 c)
	{
		return math.distance(CircumCenter(a, b, c), a);
	}

	private static float CircumRadiusSq(float2 a, float2 b, float2 c)
	{
		return math.distancesq(CircumCenter(a, b, c), a);
	}

	private static float2 CircumCenter(float2 a, float2 b, float2 c)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		float num3 = c.x - a.x;
		float num4 = c.y - a.y;
		float num5 = num * num + num2 * num2;
		float num6 = num3 * num3 + num4 * num4;
		float num7 = 0.5f / (num * num4 - num2 * num3);
		float x = a.x + (num4 * num5 - num2 * num6) * num7;
		float y = a.y + (num * num6 - num3 * num5) * num7;
		return new float2(x, y);
	}

	private static float Orient2dFast(float2 a, float2 b, float2 c)
	{
		return (a.y - c.y) * (b.x - c.x) - (a.x - c.x) * (b.y - c.y);
	}

	private static bool InCircle(float2 a, float2 b, float2 c, float2 p)
	{
		float num = a.x - p.x;
		float num2 = a.y - p.y;
		float num3 = b.x - p.x;
		float num4 = b.y - p.y;
		float num5 = c.x - p.x;
		float num6 = c.y - p.y;
		float num7 = num * num + num2 * num2;
		float num8 = num3 * num3 + num4 * num4;
		float num9 = num5 * num5 + num6 * num6;
		return num * (num4 * num9 - num8 * num6) - num2 * (num3 * num9 - num8 * num5) + num7 * (num3 * num6 - num4 * num5) < 0f;
	}

	private static float3 Barycentric(float2 a, float2 b, float2 c, float2 p)
	{
		float2 @float = b - a;
		float2 float2 = c - a;
		float2 float3 = p - a;
		float2 a2 = @float;
		float2 b2 = float2;
		float2 float4 = float3;
		float num = 1f / Cross(a2, b2);
		float num2 = num * Cross(float4, b2);
		float num3 = num * Cross(a2, float4);
		return math.float3(1f - num2 - num3, num2, num3);
	}

	private static void Eigen(float2x2 matrix, out float2 eigval, out float2x2 eigvec)
	{
		float num = matrix[0][0];
		float num2 = matrix[1][1];
		float num3 = matrix[0][1];
		float num4 = num - num2;
		float num5 = num + num2;
		float num6 = (float)((num4 >= 0f) ? 1 : (-1)) * math.sqrt(num4 * num4 + 4f * num3 * num3);
		float x = num5 + num6;
		float y = num5 - num6;
		eigval = 0.5f * math.float2(x, y);
		float x2 = 0.5f * math.atan2(2f * num3, num4);
		eigvec = math.float2x2(math.cos(x2), 0f - math.sin(x2), math.sin(x2), math.cos(x2));
	}

	private static float2x2 Kron(float2 a, float2 b)
	{
		return math.float2x2(a * b[0], a * b[1]);
	}

	private static bool PointInsideTriangle(float2 p, float2 a, float2 b, float2 c)
	{
		return math.cmax(-Barycentric(a, b, c, p)) <= 0f;
	}

	private static float CCW(float2 a, float2 b, float2 c)
	{
		return math.sign(Cross(b - a, b - c));
	}

	private static bool PointLineSegmentIntersection(float2 a, float2 b0, float2 b1)
	{
		if (CCW(b0, b1, a) == 0f)
		{
			return math.all((a >= math.min(b0, b1)) & (a <= math.max(b0, b1)));
		}
		return false;
	}

	private static bool EdgeEdgeIntersection(float2 a0, float2 a1, float2 b0, float2 b1)
	{
		if (CCW(a0, a1, b0) != CCW(a0, a1, b1))
		{
			return CCW(b0, b1, a0) != CCW(b0, b1, a1);
		}
		return false;
	}

	private static bool IsConvexQuadrilateral(float2 a, float2 b, float2 c, float2 d)
	{
		if (CCW(a, c, b) != 0f && CCW(a, c, d) != 0f && CCW(b, d, a) != 0f && CCW(b, d, c) != 0f && CCW(a, c, b) != CCW(a, c, d))
		{
			return CCW(b, d, a) != CCW(b, d, c);
		}
		return false;
	}
}
