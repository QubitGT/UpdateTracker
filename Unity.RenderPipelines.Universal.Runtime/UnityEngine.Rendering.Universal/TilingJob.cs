using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

[BurstCompile(FloatMode = FloatMode.Default, DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct TilingJob : IJobFor
{
	[ReadOnly]
	public NativeArray<VisibleLight> lights;

	[ReadOnly]
	public NativeArray<VisibleReflectionProbe> reflectionProbes;

	[NativeDisableParallelForRestriction]
	public NativeArray<InclusiveRange> tileRanges;

	public int itemsPerTile;

	public int rangesPerItem;

	public Fixed2<float4x4> worldToViews;

	public float2 tileScale;

	public float2 tileScaleInv;

	public Fixed2<float> viewPlaneBottoms;

	public Fixed2<float> viewPlaneTops;

	public Fixed2<float4> viewToViewportScaleBiases;

	public int2 tileCount;

	public float near;

	public bool isOrthographic;

	private InclusiveRange m_TileYRange;

	private int m_Offset;

	private int m_ViewIndex;

	private float2 m_CenterOffset;

	private static readonly float3[] k_CubePoints = new float3[8]
	{
		new float3(-1f, -1f, -1f),
		new float3(-1f, -1f, 1f),
		new float3(-1f, 1f, -1f),
		new float3(-1f, 1f, 1f),
		new float3(1f, -1f, -1f),
		new float3(1f, -1f, 1f),
		new float3(1f, 1f, -1f),
		new float3(1f, 1f, 1f)
	};

	private static readonly int4[] k_CubeLineIndices = new int4[4]
	{
		new int4(0, 4, 2, 1),
		new int4(3, 7, 1, 2),
		new int4(5, 1, 7, 4),
		new int4(6, 2, 4, 7)
	};

	public void Execute(int jobIndex)
	{
		int num = jobIndex % itemsPerTile;
		m_ViewIndex = jobIndex / itemsPerTile;
		m_Offset = jobIndex * rangesPerItem;
		m_TileYRange = new InclusiveRange(short.MaxValue, short.MinValue);
		for (int i = 0; i < rangesPerItem; i++)
		{
			tileRanges[m_Offset + i] = new InclusiveRange(short.MaxValue, short.MinValue);
		}
		if (num < lights.Length)
		{
			if (isOrthographic)
			{
				TileLightOrthographic(num);
			}
			else
			{
				TileLight(num);
			}
		}
		else
		{
			TileReflectionProbe(num);
		}
	}

	private void TileLight(int lightIndex)
	{
		VisibleLight light = lights[lightIndex];
		if (light.lightType != LightType.Point && light.lightType != 0)
		{
			return;
		}
		float4x4 float4x = light.localToWorldMatrix;
		float3 lightPositionVS = math.mul(worldToViews[m_ViewIndex], math.float4(float4x.c3.xyz, 1f)).xyz;
		lightPositionVS.z *= -1f;
		if (lightPositionVS.z >= near)
		{
			ExpandY(lightPositionVS);
		}
		float3 lightDirectionVS = math.normalize(math.mul(worldToViews[m_ViewIndex], math.float4(float4x.c2.xyz, 0f)).xyz);
		lightDirectionVS.z *= -1f;
		float x = math.radians(light.spotAngle * 0.5f);
		float range = light.range;
		float num = square(range);
		float cosHalfAngle = math.cos(x);
		float coneHeight = cosHalfAngle * range;
		float clipRadius = math.sqrt(num - square(near - lightPositionVS.z));
		GetSphereHorizon(lightPositionVS.yz, range, near, clipRadius, out var p2, out var p3);
		float3 @float = math.float3(lightPositionVS.x, p2);
		float3 float2 = math.float3(lightPositionVS.x, p3);
		if (SpherePointIsValid(@float))
		{
			ExpandY(@float);
		}
		if (SpherePointIsValid(float2))
		{
			ExpandY(float2);
		}
		GetSphereHorizon(lightPositionVS.xz, range, near, clipRadius, out var p4, out var p5);
		float3 float3 = math.float3(p4.x, lightPositionVS.y, p4.y);
		float3 float4 = math.float3(p5.x, lightPositionVS.y, p5.y);
		if (SpherePointIsValid(float3))
		{
			ExpandY(float3);
		}
		if (SpherePointIsValid(float4))
		{
			ExpandY(float4);
		}
		if (light.lightType == LightType.Spot)
		{
			float num2 = math.sqrt(range * range - coneHeight * coneHeight);
			float3 float5 = lightPositionVS + lightDirectionVS * coneHeight;
			float3 float6 = ((math.abs(math.abs(lightDirectionVS.x) - 1f) < 1E-06f) ? math.float3(0f, 1f, 0f) : math.normalize(math.cross(lightDirectionVS, math.float3(1f, 0f, 0f))));
			float3 float7 = math.cross(lightDirectionVS, float6);
			GetProjectedCircleHorizon(float5.yz, num2, float6.yz, float7.yz, out var uv, out var uv2);
			float3 positionVS = float5 + uv.x * float6 + uv.y * float7;
			float3 positionVS2 = float5 + uv2.x * float6 + uv2.y * float7;
			if (positionVS.z >= near)
			{
				ExpandY(positionVS);
			}
			if (positionVS2.z >= near)
			{
				ExpandY(positionVS2);
			}
			float3 float8 = ((math.abs(math.abs(lightDirectionVS.y) - 1f) < 1E-06f) ? math.float3(1f, 0f, 0f) : math.normalize(math.cross(lightDirectionVS, math.float3(0f, 1f, 0f))));
			float3 float9 = math.cross(lightDirectionVS, float8);
			GetProjectedCircleHorizon(float5.xz, num2, float8.xz, float9.xz, out var uv3, out var uv4);
			float3 positionVS3 = float5 + uv3.x * float8 + uv3.y * float9;
			float3 positionVS4 = float5 + uv4.x * float8 + uv4.y * float9;
			if (positionVS3.z >= near)
			{
				ExpandY(positionVS3);
			}
			if (positionVS4.z >= near)
			{
				ExpandY(positionVS4);
			}
			if (GetCircleClipPoints(float5, lightDirectionVS, num2, near, out var p6, out var p7))
			{
				ExpandY(p6);
				ExpandY(p7);
			}
			float num3 = num2 * math.sqrt(1f - square(lightDirectionVS.z));
			bool flag = near >= math.min(float5.z - num3, lightPositionVS.z) && near <= math.max(float5.z + num3, lightPositionVS.z);
			float3 x2 = math.cross(lightDirectionVS, lightPositionVS);
			x2 = ((math.csum(x2) != 0f) ? math.normalize(x2) : math.float3(1f, 0f, 0f));
			float3 float10 = math.cross(lightDirectionVS, x2);
			if (flag)
			{
				float r = num2 / coneHeight;
				float2 float11 = FindNearConicTangentTheta(lightPositionVS.yz, lightDirectionVS.yz, r, x2.yz, float10.yz);
				float3 float12 = EvaluateNearConic(near, lightPositionVS, lightDirectionVS, r, x2, float10, float11.x);
				float3 float13 = EvaluateNearConic(near, lightPositionVS, lightDirectionVS, r, x2, float10, float11.y);
				if (ConicPointIsValid(float12))
				{
					ExpandY(float12);
				}
				if (ConicPointIsValid(float13))
				{
					ExpandY(float13);
				}
				float2 float14 = FindNearConicTangentTheta(lightPositionVS.xz, lightDirectionVS.xz, r, x2.xz, float10.xz);
				float3 float15 = EvaluateNearConic(near, lightPositionVS, lightDirectionVS, r, x2, float10, float14.x);
				float3 float16 = EvaluateNearConic(near, lightPositionVS, lightDirectionVS, r, x2, float10, float14.y);
				if (ConicPointIsValid(float15))
				{
					ExpandY(float15);
				}
				if (ConicPointIsValid(float16))
				{
					ExpandY(float16);
				}
			}
			GetConeSideTangentPoints(lightPositionVS, lightDirectionVS, cosHalfAngle, num2, coneHeight, range, x2, float10, out var l, out var l2);
			float3 y = math.float3(0f, 1f, viewPlaneBottoms[m_ViewIndex]);
			float num4 = math.dot(-lightPositionVS, y) / math.dot(l, y);
			float3 positionVS5 = lightPositionVS + l * num4;
			if (num4 >= 0f && num4 <= 1f && positionVS5.z >= near)
			{
				ExpandY(positionVS5);
			}
			float3 y2 = math.float3(0f, 1f, viewPlaneTops[m_ViewIndex]);
			float num5 = math.dot(-lightPositionVS, y2) / math.dot(l, y2);
			float3 positionVS6 = lightPositionVS + l * num5;
			if (num5 >= 0f && num5 <= 1f && positionVS6.z >= near)
			{
				ExpandY(positionVS6);
			}
			m_TileYRange.Clamp(0, (short)(tileCount.y - 1));
			for (int i = m_TileYRange.start + 1; i <= m_TileYRange.end; i++)
			{
				InclusiveRange empty = InclusiveRange.empty;
				float num6 = math.lerp(viewPlaneBottoms[m_ViewIndex], viewPlaneTops[m_ViewIndex], (float)i * tileScaleInv.y);
				float3 y3 = math.float3(0f, 1f, 0f - num6);
				float num7 = math.dot(-lightPositionVS, y3) / math.dot(l, y3);
				float3 positionVS7 = lightPositionVS + l * num7;
				if (num7 >= 0f && num7 <= 1f && positionVS7.z >= near)
				{
					empty.Expand((short)math.clamp(ViewToTileSpace(positionVS7).x, 0f, tileCount.x - 1));
				}
				float num8 = math.dot(-lightPositionVS, y3) / math.dot(l2, y3);
				float3 positionVS8 = lightPositionVS + l2 * num8;
				if (num8 >= 0f && num8 <= 1f && positionVS8.z >= near)
				{
					empty.Expand((short)math.clamp(ViewToTileSpace(positionVS8).x, 0f, tileCount.x - 1));
				}
				if (IntersectCircleYPlane(num6, float5, lightDirectionVS, float6, float7, num2, out var p8, out var p9))
				{
					if (p8.z >= near)
					{
						empty.Expand((short)math.clamp(ViewToTileSpace(p8).x, 0f, tileCount.x - 1));
					}
					if (p9.z >= near)
					{
						empty.Expand((short)math.clamp(ViewToTileSpace(p9).x, 0f, tileCount.x - 1));
					}
				}
				if (flag)
				{
					float y4 = num6 * near;
					float r2 = num2 / coneHeight;
					float2 float17 = FindNearConicYTheta(near, lightPositionVS, lightDirectionVS, r2, x2, float10, y4);
					float3 float18 = math.float3(EvaluateNearConic(near, lightPositionVS, lightDirectionVS, r2, x2, float10, float17.x).x, y4, near);
					float3 float19 = math.float3(EvaluateNearConic(near, lightPositionVS, lightDirectionVS, r2, x2, float10, float17.y).x, y4, near);
					if (ConicPointIsValid(float18))
					{
						empty.Expand((short)math.clamp(ViewToTileSpace(float18).x, 0f, tileCount.x - 1));
					}
					if (ConicPointIsValid(float19))
					{
						empty.Expand((short)math.clamp(ViewToTileSpace(float19).x, 0f, tileCount.x - 1));
					}
				}
				int num9 = m_Offset + 1 + i;
				tileRanges[num9] = InclusiveRange.Merge(tileRanges[num9], empty);
				tileRanges[num9 - 1] = InclusiveRange.Merge(tileRanges[num9 - 1], empty);
			}
		}
		m_TileYRange.Clamp(0, (short)(tileCount.y - 1));
		for (int j = m_TileYRange.start + 1; j <= m_TileYRange.end; j++)
		{
			InclusiveRange empty2 = InclusiveRange.empty;
			float y5 = math.lerp(viewPlaneBottoms[m_ViewIndex], viewPlaneTops[m_ViewIndex], (float)j * tileScaleInv.y);
			GetSphereYPlaneHorizon(lightPositionVS, range, near, clipRadius, y5, out var left, out var right);
			if (SpherePointIsValid(left))
			{
				empty2.Expand((short)math.clamp(ViewToTileSpace(left).x, 0f, tileCount.x - 1));
			}
			if (SpherePointIsValid(right))
			{
				empty2.Expand((short)math.clamp(ViewToTileSpace(right).x, 0f, tileCount.x - 1));
			}
			int num10 = m_Offset + 1 + j;
			tileRanges[num10] = InclusiveRange.Merge(tileRanges[num10], empty2);
			tileRanges[num10 - 1] = InclusiveRange.Merge(tileRanges[num10 - 1], empty2);
		}
		tileRanges[m_Offset] = m_TileYRange;
		bool ConicPointIsValid(float3 p)
		{
			if (math.dot(math.normalize(p - lightPositionVS), lightDirectionVS) >= 0f)
			{
				return math.dot(p - lightPositionVS, lightDirectionVS) <= coneHeight;
			}
			return false;
		}
		bool SpherePointIsValid(float3 p)
		{
			if (light.lightType != LightType.Point)
			{
				return math.dot(math.normalize(p - lightPositionVS), lightDirectionVS) >= cosHalfAngle;
			}
			return true;
		}
	}

	private void TileLightOrthographic(int lightIndex)
	{
		VisibleLight light = lights[lightIndex];
		float4x4 float4x = light.localToWorldMatrix;
		float3 lightPosVS = math.mul(worldToViews[m_ViewIndex], math.float4(float4x.c3.xyz, 1f)).xyz;
		lightPosVS.z *= -1f;
		ExpandOrthographic(lightPosVS);
		float3 lightDirVS = math.mul(worldToViews[m_ViewIndex], math.float4(float4x.c2.xyz, 0f)).xyz;
		lightDirVS.z *= -1f;
		lightDirVS = math.normalize(lightDirVS);
		float x = math.radians(light.spotAngle * 0.5f);
		float range = light.range;
		float num = square(range);
		float cosHalfAngle = math.cos(x);
		float num2 = cosHalfAngle * range;
		float num3 = square(num2);
		float num4 = 1f / num2;
		float num5 = square(num4);
		float3 @float = lightPosVS - math.float3(0f, range, 0f);
		float3 float2 = lightPosVS + math.float3(0f, range, 0f);
		float3 float3 = lightPosVS - math.float3(range, 0f, 0f);
		float3 float4 = lightPosVS + math.float3(range, 0f, 0f);
		if (SpherePointIsValid(@float))
		{
			ExpandOrthographic(@float);
		}
		if (SpherePointIsValid(float2))
		{
			ExpandOrthographic(float2);
		}
		if (SpherePointIsValid(float3))
		{
			ExpandOrthographic(float3);
		}
		if (SpherePointIsValid(float4))
		{
			ExpandOrthographic(float4);
		}
		float3 float5 = lightPosVS + lightDirVS * num2;
		float num6 = math.sqrt(num - num3);
		float num7 = square(num6);
		float3 float6 = math.normalize(math.float3(0f, 1f, 0f) - lightDirVS * lightDirVS.y);
		float3 float7 = math.normalize(math.float3(1f, 0f, 0f) - lightDirVS * lightDirVS.x);
		float3 positionVS = float5 - float6 * num6;
		float3 positionVS2 = float5 + float6 * num6;
		if (light.lightType == LightType.Spot)
		{
			float3 positionVS3 = float5 - float7 * num6;
			float3 positionVS4 = float5 + float7 * num6;
			ExpandOrthographic(positionVS);
			ExpandOrthographic(positionVS2);
			ExpandOrthographic(positionVS3);
			ExpandOrthographic(positionVS4);
		}
		m_TileYRange.Clamp(0, (short)(tileCount.y - 1));
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = 0f;
		if (light.lightType == LightType.Spot)
		{
			float num12 = num2 + num7 * num4;
			float x2 = math.sqrt(square(num7) * num5 + num7);
			float num13 = math.rcp(math.lengthsq(lightDirVS.xy));
			float2 float8 = (0f - num7) * num4 * num13 * lightDirVS.xy;
			float2 float9 = math.sqrt((square(x2) - math.lengthsq(float8)) * num13) * math.float2(lightDirVS.y, 0f - lightDirVS.x);
			float2 float10 = lightPosVS.xy + num12 * lightDirVS.xy + float8;
			float2 float11 = float10 - float9;
			float2 float12 = float10 + float9;
			num8 = float11.x - lightPosVS.x;
			num9 = math.rcp(float11.y - lightPosVS.y);
			num10 = float12.x - lightPosVS.x;
			num11 = math.rcp(float12.y - lightPosVS.y);
		}
		for (int i = m_TileYRange.start + 1; i <= m_TileYRange.end; i++)
		{
			InclusiveRange range2 = InclusiveRange.empty;
			float num14 = math.lerp(viewPlaneBottoms[m_ViewIndex], viewPlaneTops[m_ViewIndex], (float)i * tileScaleInv.y);
			float num15 = math.sqrt(num - square(num14 - lightPosVS.y));
			float3 p2 = math.float3(lightPosVS.x - num15, num14, lightPosVS.z);
			float3 p3 = math.float3(lightPosVS.x + num15, num14, lightPosVS.z);
			if (SpherePointIsValid(p2))
			{
				ExpandRangeOrthographic(ref range2, p2.x);
			}
			if (SpherePointIsValid(p3))
			{
				ExpandRangeOrthographic(ref range2, p3.x);
			}
			if (light.lightType == LightType.Spot)
			{
				if (num14 >= positionVS.y && num14 <= positionVS2.y)
				{
					float num16 = (num14 - float5.y) / float6.y;
					float num17 = float5.x + num16 * float6.x;
					float num18 = (0f - lightDirVS.z) / math.length(math.float3(0f - lightDirVS.z, 0f, lightDirVS.x));
					float num19 = math.sqrt(square(num6) - square(num16));
					float xVS = num17 - num19 * num18;
					float xVS2 = num17 + num19 * num18;
					ExpandRangeOrthographic(ref range2, xVS);
					ExpandRangeOrthographic(ref range2, xVS2);
				}
				float num20 = num14 - lightPosVS.y;
				float num21 = num20 * num9;
				float num22 = num20 * num11;
				if (num21 >= 0f && num21 <= 1f)
				{
					ExpandRangeOrthographic(ref range2, lightPosVS.x + num21 * num8);
				}
				if (num22 >= 0f && num22 <= 1f)
				{
					ExpandRangeOrthographic(ref range2, lightPosVS.x + num22 * num10);
				}
			}
			int num23 = m_Offset + 1 + i;
			tileRanges[num23] = InclusiveRange.Merge(tileRanges[num23], range2);
			tileRanges[num23 - 1] = InclusiveRange.Merge(tileRanges[num23 - 1], range2);
		}
		tileRanges[m_Offset] = m_TileYRange;
		bool SpherePointIsValid(float3 p)
		{
			if (light.lightType != LightType.Point)
			{
				return math.dot(math.normalize(p - lightPosVS), lightDirVS) >= cosHalfAngle;
			}
			return true;
		}
	}

	private void TileReflectionProbe(int index)
	{
		VisibleReflectionProbe visibleReflectionProbe = reflectionProbes[index - lights.Length];
		float3 @float = visibleReflectionProbe.bounds.center;
		float3 float2 = visibleReflectionProbe.bounds.extents;
		NativeArray<float3> nativeArray = new NativeArray<float3>(k_CubePoints.Length, Allocator.Temp);
		NativeArray<float2> nativeArray2 = new NativeArray<float2>(k_CubePoints.Length + k_CubeLineIndices.Length * 3, Allocator.Temp);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < k_CubePoints.Length; i++)
		{
			float3 xyz = math.mul(worldToViews[m_ViewIndex], math.float4(@float + float2 * k_CubePoints[i], 1f)).xyz;
			xyz.z *= -1f;
			nativeArray[i] = xyz;
			if (xyz.z >= near)
			{
				float2 value = (isOrthographic ? xyz.xy : (xyz.xy / xyz.z));
				int num3 = num++;
				nativeArray2[num3] = value;
				if (value.x < nativeArray2[num2].x)
				{
					num2 = num3;
				}
			}
		}
		for (int j = 0; j < k_CubeLineIndices.Length; j++)
		{
			int4 @int = k_CubeLineIndices[j];
			float3 start = nativeArray[@int.x];
			for (int k = 0; k < 3; k++)
			{
				float3 end = nativeArray[@int[k + 1]];
				if ((!(start.z < near) || !(end.z < near)) && (start.z < near || end.z < near))
				{
					float t = (near - start.z) / (end.z - start.z);
					float3 float3 = math.lerp(start, end, t);
					float2 value2 = (isOrthographic ? float3.xy : (float3.xy / float3.z));
					int num4 = num++;
					nativeArray2[num4] = value2;
					if (value2.x < nativeArray2[num2].x)
					{
						num2 = num4;
					}
				}
			}
		}
		NativeArray<float2> nativeArray3 = new NativeArray<float2>(num, Allocator.Temp);
		int num5 = 0;
		if (num > 0)
		{
			int num6 = num2;
			do
			{
				float2 float4 = nativeArray2[num6];
				ExpandY(math.float3(float4, 1f));
				nativeArray3[num5++] = float4;
				int num7 = 0;
				float2 float5 = nativeArray2[num7] - float4;
				for (int l = 0; l < num; l++)
				{
					float2 float6 = nativeArray2[l] - float4;
					float num8 = math.determinant(math.float2x2(float5, float6));
					if (num7 == num6 || num8 > 0f || (num8 == 0f && math.lengthsq(float6) > math.lengthsq(float5)))
					{
						num7 = l;
						float5 = float6;
					}
				}
				num6 = num7;
			}
			while (num6 != num2 && num5 < num);
			m_TileYRange.Clamp(0, (short)(tileCount.y - 1));
			for (int m = m_TileYRange.start + 1; m <= m_TileYRange.end; m++)
			{
				InclusiveRange empty = InclusiveRange.empty;
				float num9 = math.lerp(viewPlaneBottoms[m_ViewIndex], viewPlaneTops[m_ViewIndex], (float)m * tileScaleInv.y);
				for (int n = 0; n < num5; n++)
				{
					float2 float7 = nativeArray3[n];
					float2 float8 = nativeArray3[(n + 1) % num5];
					float num10 = (num9 - float7.y) / (float8.y - float7.y);
					if (!(num10 < 0f) && !(num10 > 1f))
					{
						float3 positionVS = math.float3(math.lerp(float7.x, float8.x, num10), num9, 1f);
						empty.Expand((short)math.clamp((isOrthographic ? ViewToTileSpaceOrthographic(positionVS) : ViewToTileSpace(positionVS)).x, 0f, tileCount.x - 1));
					}
				}
				int num11 = m_Offset + 1 + m;
				tileRanges[num11] = InclusiveRange.Merge(tileRanges[num11], empty);
				tileRanges[num11 - 1] = InclusiveRange.Merge(tileRanges[num11 - 1], empty);
			}
			tileRanges[m_Offset] = m_TileYRange;
		}
		nativeArray3.Dispose();
		nativeArray2.Dispose();
		nativeArray.Dispose();
	}

	private float2 ViewToTileSpace(float3 positionVS)
	{
		return (positionVS.xy / positionVS.z * viewToViewportScaleBiases[m_ViewIndex].xy + viewToViewportScaleBiases[m_ViewIndex].zw) * tileScale;
	}

	private float2 ViewToTileSpaceOrthographic(float3 positionVS)
	{
		return (positionVS.xy * viewToViewportScaleBiases[m_ViewIndex].xy + viewToViewportScaleBiases[m_ViewIndex].zw) * tileScale;
	}

	private void ExpandY(float3 positionVS)
	{
		float2 @float = ViewToTileSpace(positionVS);
		int num = (int)@float.y;
		int num2 = (int)@float.x;
		m_TileYRange.Expand((short)math.clamp(num, 0, tileCount.y - 1));
		if (num >= 0 && num < tileCount.y && num2 >= 0 && num2 < tileCount.x)
		{
			InclusiveRange value = tileRanges[m_Offset + 1 + num];
			value.Expand((short)num2);
			tileRanges[m_Offset + 1 + num] = value;
		}
	}

	private void ExpandOrthographic(float3 positionVS)
	{
		float2 @float = ViewToTileSpaceOrthographic(positionVS);
		int num = (int)@float.y;
		int num2 = (int)@float.x;
		m_TileYRange.Expand((short)math.clamp(num, 0, tileCount.y - 1));
		if (num >= 0 && num < tileCount.y && num2 >= 0 && num2 < tileCount.x)
		{
			InclusiveRange value = tileRanges[m_Offset + 1 + num];
			value.Expand((short)num2);
			tileRanges[m_Offset + 1 + num] = value;
		}
	}

	private void ExpandRangeOrthographic(ref InclusiveRange range, float xVS)
	{
		range.Expand((short)math.clamp(ViewToTileSpaceOrthographic(xVS).x, 0f, tileCount.x - 1));
	}

	private static float square(float x)
	{
		return x * x;
	}

	private static void GetSphereHorizon(float2 center, float radius, float near, float clipRadius, out float2 p0, out float2 p1)
	{
		float2 @float = math.normalize(center);
		float num = math.length(center);
		float num2 = math.sqrt(num * num - radius * radius);
		float num3 = num2 * radius / num;
		float2 float2 = @float * (num2 * num3 / radius);
		p0 = math.float2(float.MinValue, 1f);
		p1 = math.float2(float.MaxValue, 1f);
		if (center.y - radius < near)
		{
			p0 = math.float2(center.x + clipRadius, near);
			p1 = math.float2(center.x - clipRadius, near);
		}
		float2 float3 = float2 + math.float2(0f - @float.y, @float.x) * num3;
		if (square(num) >= square(radius) && float3.y >= near)
		{
			if (float3.x > p0.x)
			{
				p0 = float3;
			}
			if (float3.x < p1.x)
			{
				p1 = float3;
			}
		}
		float2 float4 = float2 + math.float2(@float.y, 0f - @float.x) * num3;
		if (square(num) >= square(radius) && float4.y >= near)
		{
			if (float4.x > p0.x)
			{
				p0 = float4;
			}
			if (float4.x < p1.x)
			{
				p1 = float4;
			}
		}
	}

	private static void GetSphereYPlaneHorizon(float3 center, float sphereRadius, float near, float clipRadius, float y, out float3 left, out float3 right)
	{
		float num = y * near;
		float num2 = math.sqrt(square(clipRadius) - square(num - center.y));
		left = math.float3(center.x - num2, num, near);
		right = math.float3(center.x + num2, num, near);
		float3 @float = math.normalize(math.float3(0f, y, 1f));
		float3 float2 = math.float3(1f, 0f, 0f);
		float x = math.abs(math.dot(math.normalize(math.float3(0f, 1f, 0f - y)), center));
		float2 float3 = math.float2(math.dot(center, @float), math.dot(center, float2));
		float num3 = math.length(float3);
		float2 float4 = float3 / num3;
		float num4 = math.sqrt(square(sphereRadius) - square(x));
		if (square(x) <= square(sphereRadius) && square(num4) <= square(num3))
		{
			float num5 = math.sqrt(square(num3) - square(num4));
			float num6 = num5 * num4 / num3;
			float2 float5 = float4 * (num5 * num6 / num4);
			float2 float6 = float5 + math.float2(float4.y, 0f - float4.x) * num6;
			float2 float7 = float5 + math.float2(0f - float4.y, float4.x) * num6;
			float3 float8 = float6.x * @float + float6.y * float2;
			if (float8.z >= near)
			{
				left = float8;
			}
			float3 float9 = float7.x * @float + float7.y * float2;
			if (float9.z >= near)
			{
				right = float9;
			}
		}
	}

	private static bool GetCircleClipPoints(float3 circleCenter, float3 circleNormal, float circleRadius, float near, out float3 p0, out float3 p1)
	{
		float3 @float = math.normalize(math.cross(circleNormal, math.float3(0f, 0f, 1f)));
		float3 float2 = math.cross(@float, circleNormal);
		float num = (near - circleCenter.z) / float2.z;
		float3 float3 = circleCenter + float2 * num;
		float num2 = math.sqrt(square(circleRadius) - square(num));
		p0 = float3 + @float * num2;
		p1 = float3 - @float * num2;
		return math.abs(num) <= circleRadius;
	}

	private static (float, float) IntersectEllipseLine(float a, float b, float3 line)
	{
		float num = math.rcp(square(line.y) * square(b));
		float num2 = 1f / square(a) + square(line.x) * num;
		float num3 = 2f * line.x * line.z * num;
		float num4 = square(line.z) * num - 1f;
		float num5 = math.sqrt(num3 * num3 - 4f * num2 * num4);
		float item = (0f - num3 + num5) / (2f * num2);
		float item2 = (0f - num3 - num5) / (2f * num2);
		return (item, item2);
	}

	private static void GetProjectedCircleHorizon(float2 center, float radius, float2 U, float2 V, out float2 uv1, out float2 uv2)
	{
		float num = math.length(V);
		if (num < 1E-06f)
		{
			uv1 = math.float2(radius, 0f);
			uv2 = math.float2(0f - radius, 0f);
			return;
		}
		float num2 = math.length(U);
		float num3 = math.rcp(num2);
		float num4 = math.rcp(num);
		float2 y = U * num3;
		float2 y2 = V * num4;
		float num5 = num2 * radius;
		float num6 = num * radius;
		float2 @float = math.float2(math.dot(-center, y), math.dot(-center, y2));
		float3 line = math.float3(@float.x / square(num5), @float.y / square(num6), -1f);
		(float, float) tuple = IntersectEllipseLine(num5, num6, line);
		float item = tuple.Item1;
		float item2 = tuple.Item2;
		uv1 = math.float2(item * num3, ((0f - line.x) / line.y * item - line.z / line.y) * num4);
		uv2 = math.float2(item2 * num3, ((0f - line.x) / line.y * item2 - line.z / line.y) * num4);
	}

	private static bool IntersectCircleYPlane(float y, float3 circleCenter, float3 circleNormal, float3 circleU, float3 circleV, float circleRadius, out float3 p1, out float3 p2)
	{
		p1 = (p2 = 0);
		float num = math.dot(circleCenter, circleNormal);
		float3 x = math.float3(1f, y, 1f) * num / math.dot(math.float3(1f, y, 1f), circleNormal) - circleCenter;
		float2 @float = math.float2(math.dot(x, circleU), math.dot(x, circleV));
		float3 x2 = math.float3(-1f, y, 1f) * num / math.dot(math.float3(-1f, y, 1f), circleNormal) - circleCenter;
		float2 float2 = math.normalize(math.float2(math.dot(x2, circleU), math.dot(x2, circleV)) - @float);
		float2 float3 = math.float2(float2.y, 0f - float2.x);
		float num2 = math.dot(@float, float3);
		float2 float4 = float3 * num2;
		if (num2 > circleRadius)
		{
			return false;
		}
		float num3 = math.sqrt(circleRadius * circleRadius - num2 * num2);
		float2 float5 = float4 + num3 * float2;
		float2 float6 = float4 - num3 * float2;
		p1 = circleCenter + float5.x * circleU + float5.y * circleV;
		p2 = circleCenter + float6.x * circleU + float6.y * circleV;
		return true;
	}

	private static void GetConeSideTangentPoints(float3 vertex, float3 axis, float cosHalfAngle, float circleRadius, float coneHeight, float range, float3 circleU, float3 circleV, out float3 l1, out float3 l2)
	{
		l1 = (l2 = 0);
		if (!(math.dot(math.normalize(-vertex), axis) >= cosHalfAngle))
		{
			float num = 0f - math.dot(vertex, axis);
			if (num == 0f)
			{
				num = 1E-06f;
			}
			float num2 = ((num < 0f) ? (-1f) : 1f);
			float3 @float = vertex + axis * num;
			float num3 = math.abs(num) * circleRadius / coneHeight;
			float3 float2 = math.float3(math.float2(math.dot(circleU, -@float), math.dot(circleV, -@float)), 0f - square(num3));
			float2 float3 = math.float2(-1f, (0f - float2.x) / float2.y * -1f - float2.z / float2.y);
			float2 float4 = math.normalize(math.float2(1f, (0f - float2.x) / float2.y * 1f - float2.z / float2.y) - float3);
			float2 float5 = math.float2(float4.y, 0f - float4.x);
			float num4 = math.dot(float3, float5);
			float2 float6 = float5 * num4;
			float num5 = math.sqrt(num3 * num3 - num4 * num4);
			float2 float7 = float6 + num5 * float4;
			float2 float8 = float6 - num5 * float4;
			float3 float9 = math.normalize(@float + float7.x * circleU + float7.y * circleV - vertex) * num2;
			float3 float10 = math.normalize(@float + float8.x * circleU + float8.y * circleV - vertex) * num2;
			l1 = float9 * range;
			l2 = float10 * range;
		}
	}

	private static float3 EvaluateNearConic(float near, float3 o, float3 d, float r, float3 u, float3 v, float theta)
	{
		float num = (near - o.z) / (d.z + r * u.z * math.cos(theta) + r * v.z * math.sin(theta));
		return math.float3(o.xy + num * (d.xy + r * u.xy * math.cos(theta) + r * v.xy * math.sin(theta)), near);
	}

	private static float2 FindNearConicTangentTheta(float2 o, float2 d, float r, float2 u, float2 v)
	{
		float num = math.sqrt(square(d.x) * square(u.y) + square(d.x) * square(v.y) - 2f * d.x * d.y * u.x * u.y - 2f * d.x * d.y * v.x * v.y + square(d.y) * square(u.x) + square(d.y) * square(v.x) - square(r) * square(u.x) * square(v.y) + 2f * square(r) * u.x * u.y * v.x * v.y - square(r) * square(u.y) * square(v.x));
		float num2 = d.x * v.y - d.y * v.x - r * u.x * v.y + r * u.y * v.x;
		return 2f * math.atan(((0f - d.x) * u.y + d.y * u.x + math.float2(1f, -1f) * num) / num2);
	}

	private static float2 FindNearConicYTheta(float near, float3 o, float3 d, float r, float3 u, float3 v, float y)
	{
		float num = math.sqrt((0f - square(d.y)) * square(o.z) + 2f * square(d.y) * o.z * near - square(d.y) * square(near) + 2f * d.y * d.z * o.y * o.z - 2f * d.y * d.z * o.y * near - 2f * d.y * d.z * o.z * y + 2f * d.y * d.z * y * near - square(d.z) * square(o.y) + 2f * square(d.z) * o.y * y - square(d.z) * square(y) + square(o.y) * square(r) * square(u.z) + square(o.y) * square(r) * square(v.z) - 2f * o.y * o.z * square(r) * u.y * u.z - 2f * o.y * o.z * square(r) * v.y * v.z - 2f * o.y * y * square(r) * square(u.z) - 2f * o.y * y * square(r) * square(v.z) + 2f * o.y * square(r) * u.y * u.z * near + 2f * o.y * square(r) * v.y * v.z * near + square(o.z) * square(r) * square(u.y) + square(o.z) * square(r) * square(v.y) + 2f * o.z * y * square(r) * u.y * u.z + 2f * o.z * y * square(r) * v.y * v.z - 2f * o.z * square(r) * square(u.y) * near - 2f * o.z * square(r) * square(v.y) * near + square(y) * square(r) * square(u.z) + square(y) * square(r) * square(v.z) - 2f * y * square(r) * u.y * u.z * near - 2f * y * square(r) * v.y * v.z * near + square(r) * square(u.y) * square(near) + square(r) * square(v.y) * square(near));
		float num2 = d.y * o.z - d.y * near - d.z * o.y + d.z * y + o.y * r * u.z - o.z * r * u.y - y * r * u.z + r * u.y * near;
		return 2f * math.atan((r * (o.y * v.z - o.z * v.y - y * v.z + v.y * near) + math.float2(1f, -1f) * num) / num2);
	}
}
