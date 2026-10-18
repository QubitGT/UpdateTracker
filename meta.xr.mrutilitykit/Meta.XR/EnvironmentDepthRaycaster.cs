using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Meta.XR.EnvironmentDepth;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Meta.XR;

[AddComponentMenu("")]
[DefaultExecutionOrder(-48)]
internal class EnvironmentDepthRaycaster : MonoBehaviour
{
	private static readonly int EnvironmentDepthTextureId = Shader.PropertyToID("_EnvironmentDepthTexture");

	private static readonly int EnvironmentDepthTextureSizeId = Shader.PropertyToID("_EnvironmentDepthTextureSize");

	private static readonly int CopiedDepthTextureId = Shader.PropertyToID("_CopiedDepthTexture");

	private static readonly int EnvironmentDepthZBufferParamsId = Shader.PropertyToID("_EnvironmentDepthZBufferParams");

	private const int TextureSize = 128;

	private const int NumEyes = 2;

	private ComputeShader _shader;

	internal EnvironmentDepthManager depthManager;

	private ComputeBuffer _computeBuffer;

	private NativeArray<float> _depthTexturePixels;

	private NativeArray<float> _gpuRequestBuffer;

	private bool _isDepthTextureAvailable;

	private AsyncGPUReadbackRequest? _currentGpuReadbackRequest;

	private RenderTexture _updatedDepthTexture;

	private readonly Matrix4x4[] _matrixVP = new Matrix4x4[2];

	private readonly Matrix4x4[] _matrixV = new Matrix4x4[2];

	private readonly Matrix4x4[] _matrixVP_inv = new Matrix4x4[2];

	private readonly Plane[][] _camFrustumPlanes = new Plane[2][]
	{
		new Plane[6],
		new Plane[6]
	};

	private Vector4 _EnvironmentDepthZBufferParams;

	private readonly DepthFrameDesc[] _depthFrameDesc = new DepthFrameDesc[2];

	private Matrix4x4 _worldToTrackingSpaceMatrix = Matrix4x4.identity;

	internal bool _warmUpRaycast;

	private int _currentEyeIndex;

	private XRDisplaySubsystem _xrDisplay;

	private void Awake()
	{
		_shader = Resources.Load<ComputeShader>("CopyDepthTexture");
		_computeBuffer = new ComputeBuffer(32768, 4);
		_depthTexturePixels = new NativeArray<float>(32768, Allocator.Persistent);
		_gpuRequestBuffer = new NativeArray<float>(32768, Allocator.Persistent);
		List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>(1);
		SubsystemManager.GetSubsystems(list);
		_xrDisplay = list.Single();
	}

	private void OnDisable()
	{
		InvalidateDepthTexture();
	}

	internal void OnDepthTextureUpdate(RenderTexture updatedDepthTexture)
	{
		_updatedDepthTexture = updatedDepthTexture;
		CreateTextureCopyRequestIfNeeded();
	}

	private void InvalidateDepthTexture()
	{
		_isDepthTextureAvailable = false;
	}

	private void OnDestroy()
	{
		depthManager.onDepthTextureUpdate -= OnDepthTextureUpdate;
		if (_currentGpuReadbackRequest.HasValue && !_currentGpuReadbackRequest.Value.done)
		{
			_currentGpuReadbackRequest.Value.WaitForCompletion();
		}
		_computeBuffer.Dispose();
		_depthTexturePixels.Dispose();
		_gpuRequestBuffer.Dispose();
	}

	private void CreateTextureCopyRequestIfNeeded()
	{
		if (_currentGpuReadbackRequest.HasValue)
		{
			return;
		}
		if (!depthManager.enabled || !depthManager.IsDepthAvailable)
		{
			InvalidateDepthTexture();
			return;
		}
		if (!_warmUpRaycast)
		{
			InvalidateDepthTexture();
			return;
		}
		RenderTexture updatedDepthTexture = _updatedDepthTexture;
		if (!(updatedDepthTexture == null))
		{
			_updatedDepthTexture = null;
			for (int i = 0; i < 2; i++)
			{
				_depthFrameDesc[i] = depthManager.frameDescriptors[i];
			}
			_worldToTrackingSpaceMatrix = depthManager.GetTrackingSpaceWorldToLocalMatrix();
			_shader.SetTexture(0, EnvironmentDepthTextureId, updatedDepthTexture);
			_shader.SetFloat(EnvironmentDepthTextureSizeId, updatedDepthTexture.width);
			_EnvironmentDepthZBufferParams = Shader.GetGlobalVector(EnvironmentDepthZBufferParamsId);
			_shader.SetVector(EnvironmentDepthZBufferParamsId, _EnvironmentDepthZBufferParams);
			_shader.SetBuffer(0, CopiedDepthTextureId, _computeBuffer);
			_shader.Dispatch(0, 1, 1, 1);
			_currentGpuReadbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref _gpuRequestBuffer, _computeBuffer);
		}
	}

	private void UpdateTextureCopyRequest()
	{
		if (!_currentGpuReadbackRequest.HasValue || !_currentGpuReadbackRequest.Value.done)
		{
			return;
		}
		if (_currentGpuReadbackRequest.Value.hasError)
		{
			Debug.LogError("AsyncGPUReadback.RequestIntoNativeArray() hasError");
		}
		else
		{
			NativeArray<float> gpuRequestBuffer = _gpuRequestBuffer;
			NativeArray<float> depthTexturePixels = _depthTexturePixels;
			_depthTexturePixels = gpuRequestBuffer;
			_gpuRequestBuffer = depthTexturePixels;
			for (int i = 0; i < 2; i++)
			{
				EnvironmentDepthUtils.CalculateDepthCameraMatrices(_depthFrameDesc[i], out var projMatrix, out var viewMatrix);
				viewMatrix *= _worldToTrackingSpaceMatrix;
				_matrixV[i] = viewMatrix;
				_matrixVP[i] = projMatrix * viewMatrix;
				GeometryUtility.CalculateFrustumPlanes(_matrixVP[i], _camFrustumPlanes[i]);
				_matrixVP_inv[i] = _matrixVP[i].inverse;
			}
			_isDepthTextureAvailable = true;
		}
		_currentGpuReadbackRequest = null;
	}

	private void Update()
	{
		if (depthManager == null)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		UpdateTextureCopyRequest();
		CreateTextureCopyRequestIfNeeded();
	}

	private Vector2Int WorldPosToNonNormalizedTextureCoords(Vector3 worldPos)
	{
		Vector4 vector = _matrixVP[_currentEyeIndex] * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f);
		Vector2 vector2 = (new Vector2(vector.x, vector.y) / vector.w + Vector2.one) * 0.5f;
		return new Vector2Int(Mathf.Clamp((int)(vector2.x * 128f), 0, 127), Mathf.Clamp((int)(vector2.y * 128f), 0, 127));
	}

	private float SampleDepthTexture(Vector2Int texCoord)
	{
		return _depthTexturePixels[texCoord.x + texCoord.y * 128 + 16384 * _currentEyeIndex];
	}

	private Vector3 WorldPosAtDepthTexCoord(Vector2Int texCoord)
	{
		float num = SampleDepthTexture(texCoord);
		float z = ((num == 0f) ? 0f : (_EnvironmentDepthZBufferParams.x / num - _EnvironmentDepthZBufferParams.y));
		Vector4 vector = new Vector4((float)texCoord.x * (1f / 128f) * 2f - 1f, (float)texCoord.y * (1f / 128f) * 2f - 1f, z, 1f);
		Vector4 vector2 = _matrixVP_inv[_currentEyeIndex] * vector;
		return vector2 / vector2.w;
	}

	private float WorldPosToLinearDepth(Vector3 worldPos)
	{
		return 0f - (_matrixV[_currentEyeIndex] * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f)).z;
	}

	private Vector3 ReconstructNormal(Vector2Int texCoord)
	{
		float centerDepth = SampleDepthTexture(texCoord);
		Vector3 centerWorldPos = WorldPosAtDepthTexCoord(texCoord);
		Vector3 lhs = ClosestDerivativeToAdjacentExtrapolations(new Vector2Int(1, 0));
		Vector3 rhs = ClosestDerivativeToAdjacentExtrapolations(new Vector2Int(0, 1));
		return -Vector3.Normalize(Vector3.Cross(lhs, rhs));
		Vector3 ClosestDerivativeToAdjacentExtrapolations(Vector2Int axis)
		{
			Vector4 vector = new Vector4(SampleDepthTexture(texCoord - axis), SampleDepthTexture(texCoord + axis), SampleDepthTexture(texCoord - axis * 2), SampleDepthTexture(texCoord + axis * 2));
			Vector2 vector2 = new Vector2(Mathf.Abs(vector.x * vector.z / (2f * vector.z - vector.x) - centerDepth), Mathf.Abs(vector.y * vector.w / (2f * vector.w - vector.y) - centerDepth));
			if (!(vector2.x > vector2.y))
			{
				return centerWorldPos - WorldPosAtDepthTexCoord(texCoord - axis);
			}
			return WorldPosAtDepthTexCoord(texCoord + axis) - centerWorldPos;
		}
	}

	internal DepthRaycastResult Raycast(Ray ray, out Vector3 position, out Vector3 normal, out float normalConfidence, float maxDistance, Eye eye, bool allowOccludedRayOrigin)
	{
		normal = default(Vector3);
		normalConfidence = 0f;
		(DepthRaycastResult, Vector3, int) tuple = Raycast(ray, maxDistance, eye, allowOccludedRayOrigin);
		position = tuple.Item2;
		if (tuple.Item1 != 0)
		{
			return tuple.Item1;
		}
		_currentEyeIndex = tuple.Item3;
		if (ReconstructNormalAtWorldPos(position, out normal, out normalConfidence))
		{
			return DepthRaycastResult.Success;
		}
		_currentEyeIndex = ((_currentEyeIndex == 0) ? 1 : 0);
		if (!ReconstructNormalAtWorldPos(position, out normal, out normalConfidence))
		{
			return DepthRaycastResult.RayOutsideOfDepthCameraFrustum;
		}
		return DepthRaycastResult.Success;
	}

	private bool ReconstructNormalAtWorldPos(Vector3 position, out Vector3 normal, out float normalConfidence)
	{
		normal = default(Vector3);
		normalConfidence = 0f;
		Vector2Int vector2Int = WorldPosToNonNormalizedTextureCoords(position);
		if (vector2Int.x < 4 || vector2Int.x >= 124 || vector2Int.y < 4 || vector2Int.y >= 124)
		{
			return false;
		}
		Span<Vector2Int> span = stackalloc Vector2Int[5]
		{
			new Vector2Int(-2, 0),
			new Vector2Int(2, 0),
			new Vector2Int(0, 0),
			new Vector2Int(0, -2),
			new Vector2Int(0, 2)
		};
		int length = span.Length;
		Span<Vector3> span2 = stackalloc Vector3[length];
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < length; i++)
		{
			Vector3 vector = ReconstructNormal(vector2Int + span[i]);
			span2[i] = vector;
			zero += vector;
		}
		zero = (normal = Vector3.Normalize(zero));
		float num = 0f;
		for (int j = 0; j < length; j++)
		{
			if (Vector3.Dot(span2[j], zero) > 0.95f)
			{
				num += 1f;
			}
		}
		normalConfidence = num / (float)length;
		return true;
	}

	internal (DepthRaycastResult status, Vector3 position, int eyeIndex) Raycast(Ray ray, float maxDistance, Eye eye, bool allowOccludedRayOrigin)
	{
		if (!_isDepthTextureAvailable)
		{
			return (status: DepthRaycastResult.NotReady, position: default(Vector3), eyeIndex: 0);
		}
		if (eye != Eye.Both)
		{
			return GetRaycastResultForEye((eye != 0) ? 1 : 0);
		}
		(DepthRaycastResult, Vector3, int) result = GetRaycastResultForEye(0);
		if (result.Item1 == DepthRaycastResult.Success)
		{
			return result;
		}
		(DepthRaycastResult, Vector3, int) result2 = GetRaycastResultForEye(1);
		if (result2.Item1 == DepthRaycastResult.Success)
		{
			return result2;
		}
		if (result.Item1 == DepthRaycastResult.HitPointOccluded && result2.Item1 == DepthRaycastResult.HitPointOccluded)
		{
			if (!(Vector3.Distance(ray.origin, result.Item2) > Vector3.Distance(ray.origin, result2.Item2)))
			{
				return result2;
			}
			return result;
		}
		return result;
		(DepthRaycastResult status, Vector3 position, int eyeIndex) GetRaycastResultForEye(int index)
		{
			if (!ClampRayOriginToCamFrustumPlanes(ref ray, _camFrustumPlanes[index], ref maxDistance))
			{
				return (status: DepthRaycastResult.RayOutsideOfDepthCameraFrustum, position: default(Vector3), eyeIndex: 0);
			}
			Plane plane = _camFrustumPlanes[index][4];
			if (Vector3.Dot(ray.direction, plane.normal) < 0f && plane.Raycast(ray, out var enter) && maxDistance > enter)
			{
				maxDistance = enter;
			}
			Vector3 position;
			return (status: RaycastInternal(ray, out position, maxDistance, index, allowOccludedRayOrigin), position: position, eyeIndex: index);
		}
	}

	private static Vector3 ClosestPointOnFirstRay(Vector3 ray1Pos, Vector3 ray1Dir, Vector3 ray2Pos, Vector3 ray2Dir)
	{
		Vector3 lhs = ray2Pos - ray1Pos;
		Vector3 vector = Vector3.Cross(ray1Dir, ray2Dir);
		float num = Vector3.Dot(Vector3.Cross(lhs, ray2Dir), vector) / Vector3.Dot(vector, vector);
		return ray1Pos + ray1Dir * num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsInBounds(Vector2Int texCoord)
	{
		if (texCoord.x >= 0 && texCoord.x < 128 && texCoord.y >= 0)
		{
			return texCoord.x < 128;
		}
		return false;
	}

	private DepthRaycastResult RaycastInternal(Ray ray, out Vector3 position, float maxDistance, int eyeIndex, bool allowOccludedRayOrigin)
	{
		position = default(Vector3);
		if (!Mathf.Approximately(ray.direction.sqrMagnitude, 1f))
		{
			Debug.LogError("ray.direction should be normalized.");
			return DepthRaycastResult.NoHit;
		}
		if (maxDistance < 0.01f)
		{
			return DepthRaycastResult.NoHit;
		}
		_currentEyeIndex = eyeIndex;
		Vector3 origin = ray.origin;
		Vector3 direction = ray.direction;
		Vector3 worldPos = origin + maxDistance * direction;
		Vector2Int texCoord = WorldPosToNonNormalizedTextureCoords(origin);
		if (!allowOccludedRayOrigin)
		{
			float num = WorldPosToLinearDepth(origin);
			float num2 = SampleDepthTexture(texCoord);
			if (num > num2)
			{
				return DepthRaycastResult.RayOccluded;
			}
		}
		Vector2Int vector2Int = WorldPosToNonNormalizedTextureCoords(worldPos);
		int num3 = vector2Int.x - texCoord.x;
		int num4 = vector2Int.y - texCoord.y;
		int num5 = Mathf.Max(Mathf.Abs(num3), Mathf.Abs(num4));
		if (num5 == 0)
		{
			float num6 = SampleDepthTexture(texCoord);
			if (num6 < maxDistance && WorldPosToLinearDepth(origin) < num6 && WorldPosToLinearDepth(worldPos) > num6)
			{
				position = ray.origin + direction * num6;
				return DepthRaycastResult.Success;
			}
			return DepthRaycastResult.NoHit;
		}
		float num7 = 1f / WorldPosToLinearDepth(origin);
		float num8 = 1f / WorldPosToLinearDepth(worldPos);
		float num9 = (float)num3 / (float)num5;
		float num10 = (float)num4 / (float)num5;
		float num11 = (num8 - num7) / (float)num5;
		float num12 = texCoord.x;
		float num13 = texCoord.y;
		float num14 = num7;
		bool flag = false;
		for (int i = 0; i <= num5; i++)
		{
			Vector2Int texCoord2 = new Vector2Int((int)num12, (int)num13);
			if (!IsInBounds(texCoord2))
			{
				return DepthRaycastResult.RayOutsideOfDepthCameraFrustum;
			}
			float num15 = SampleDepthTexture(texCoord2);
			if (num15 != 0f)
			{
				float num16 = 1f / num14;
				if (!flag)
				{
					flag = num15 > num16;
				}
				else if (num15 <= num16)
				{
					Vector2Int texCoord3 = new Vector2Int((int)(num12 - num9), (int)(num13 - num10));
					float num17 = SampleDepthTexture(texCoord3);
					Vector3 vector = WorldPosAtDepthTexCoord(texCoord3);
					Vector3 vector2 = WorldPosAtDepthTexCoord(texCoord2);
					position = ClosestPointOnFirstRay(origin, direction, vector, vector2 - vector);
					if (!(num17 - num15 > 0.3f))
					{
						return DepthRaycastResult.Success;
					}
					return DepthRaycastResult.HitPointOccluded;
				}
			}
			num12 += num9;
			num13 += num10;
			num14 += num11;
		}
		if (!flag)
		{
			return DepthRaycastResult.RayOccluded;
		}
		return DepthRaycastResult.NoHit;
	}

	private static bool ClampRayOriginToCamFrustumPlanes(ref Ray ray, Plane[] planes, ref float maxDistance)
	{
		if (GeometryUtility.TestPlanesAABB(planes, new Bounds(ray.origin, Vector3.zero)))
		{
			return true;
		}
		for (int i = 0; i < 5; i++)
		{
			Plane plane = planes[i];
			if (plane.Raycast(ray, out var enter) && GeometryUtility.TestPlanesAABB(planes, new Bounds(ray.GetPoint(enter + 0.01f), Vector3.zero)))
			{
				maxDistance -= enter;
				if (maxDistance <= 0f)
				{
					return false;
				}
				ray.origin = ray.GetPoint(enter);
				return true;
			}
		}
		return false;
	}
}
