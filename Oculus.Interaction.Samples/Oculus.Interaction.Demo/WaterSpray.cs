using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Demo;

public class WaterSpray : MonoBehaviour, IHandGrabUseDelegate
{
	public enum NozzleMode
	{
		Spray,
		Stream
	}

	private static class NonAlloc
	{
		public static readonly Collider[] _overlapResults = new Collider[12];

		public static readonly Dictionary<int, MeshBlit> _blits = new Dictionary<int, MeshBlit>();

		private static readonly List<MeshFilter> _meshFilters = new List<MeshFilter>();

		private static readonly HashSet<Transform> _roots = new HashSet<Transform>();

		private static MaterialPropertyBlock _block;

		public static MaterialPropertyBlock PropertyBlock
		{
			get
			{
				if (_block == null)
				{
					return _block = new MaterialPropertyBlock();
				}
				return _block;
			}
		}

		public static List<MeshFilter> GetMeshFiltersInChildren(Transform root)
		{
			root.GetComponentsInChildren(_meshFilters);
			return _meshFilters;
		}

		public static HashSet<Transform> GetRootsFromOverlapResults(int hitCount)
		{
			_roots.Clear();
			for (int i = 0; i < hitCount; i++)
			{
				Transform root = GetRoot(_overlapResults[i]);
				_roots.Add(root);
			}
			return _roots;
		}

		private static Transform GetRoot(Collider hit)
		{
			if (!hit.attachedRigidbody)
			{
				if (!hit.transform.parent)
				{
					return hit.transform;
				}
				return hit.transform.parent;
			}
			return hit.attachedRigidbody.transform;
		}

		public static void CleanUpDestroyedBlits()
		{
			if (!_blits.ContainsValue(null))
			{
				return;
			}
			foreach (int item in new List<int>(_blits.Keys))
			{
				if (_blits[item] == null)
				{
					_blits.Remove(item);
				}
			}
		}
	}

	[Header("Input")]
	[SerializeField]
	private Transform _trigger;

	[SerializeField]
	private Transform _nozzle;

	[SerializeField]
	private AnimationCurve _triggerRotationCurve;

	[SerializeField]
	private SnapAxis _axis = SnapAxis.X;

	[SerializeField]
	[Range(0f, 1f)]
	private float _releaseThresold = 0.3f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _fireThresold = 0.9f;

	[SerializeField]
	private float _triggerSpeed = 3f;

	[SerializeField]
	private AnimationCurve _strengthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Output")]
	[SerializeField]
	[Tooltip("Masks the Raycast used to find objects to make wet")]
	private LayerMask _raycastLayerMask = -1;

	[SerializeField]
	[Tooltip("The spread angle when spraying, larger values will make a larger area wet")]
	private float _spraySpreadAngle = 40f;

	[SerializeField]
	[Tooltip("The spread angle when using stream, larger values will make a larger area wet")]
	private float _streamSpreadAngle = 4f;

	[SerializeField]
	private float _sprayStrength = 1.5f;

	[SerializeField]
	private int _sprayHits = 6;

	[SerializeField]
	private float _sprayRandomness = 6f;

	[SerializeField]
	[Tooltip("The max distance of the spray, controls the raycast and shader")]
	private float _maxDistance = 2f;

	[SerializeField]
	private float _dryingSpeed = 0.1f;

	[SerializeField]
	[Tooltip("Material for applying a stamp, should using the MeshBlitStamp shader or similar")]
	private Material _sprayStampMaterial;

	[SerializeField]
	[Tooltip("When not null, will be set as the '_WetBumpMap' property on wet renderers")]
	private Texture _waterBumpOverride;

	[SerializeField]
	private UnityEvent WhenSpray;

	[SerializeField]
	private UnityEvent WhenStream;

	private static readonly int WET_MAP_PROPERTY = Shader.PropertyToID("_WetMap");

	private static readonly int STAMP_MULTIPLIER_PROPERTY = Shader.PropertyToID("_StampMultipler");

	private static readonly int SUBTRACT_PROPERTY = Shader.PropertyToID("_Subtract");

	private static readonly int WET_BUMPMAP_PROPERTY = Shader.PropertyToID("_WetBumpMap");

	private static readonly int STAMP_MATRIX_PROPERTY = Shader.PropertyToID("_StampMatrix");

	private static readonly WaitForSeconds WAIT_TIME = new WaitForSeconds(0.1f);

	private bool _wasFired;

	private float _dampedUseStrength;

	private float _lastUseTime;

	private void SprayWater()
	{
		switch (GetNozzleMode())
		{
		case NozzleMode.Spray:
			Spray();
			WhenSpray?.Invoke();
			break;
		case NozzleMode.Stream:
			Stream();
			WhenStream?.Invoke();
			break;
		}
	}

	private void UpdateTriggerRotation(float progress)
	{
		float num = _triggerRotationCurve.Evaluate(progress);
		Vector3 localEulerAngles = _trigger.localEulerAngles;
		if ((_axis & SnapAxis.X) != 0)
		{
			localEulerAngles.x = num;
		}
		if ((_axis & SnapAxis.Y) != 0)
		{
			localEulerAngles.y = num;
		}
		if ((_axis & SnapAxis.Z) != 0)
		{
			localEulerAngles.z = num;
		}
		_trigger.localEulerAngles = localEulerAngles;
	}

	private NozzleMode GetNozzleMode()
	{
		if (((int)_nozzle.localEulerAngles.z + 45) / 90 % 2 == 0)
		{
			return NozzleMode.Spray;
		}
		return NozzleMode.Stream;
	}

	private void Spray()
	{
		StartCoroutine(StampRoutine(_sprayHits, _sprayRandomness, _spraySpreadAngle, _sprayStrength));
	}

	private void Stream()
	{
		StartCoroutine(StampRoutine(_sprayHits, 0f, _streamSpreadAngle, _sprayStrength));
	}

	private IEnumerator StampRoutine(int stampCount, float randomness, float spread, float strength)
	{
		StartStamping();
		Pose originalPose = _nozzle.GetPose();
		for (int i = 0; i < stampCount; i++)
		{
			yield return WAIT_TIME;
			Pose pose = originalPose;
			pose.rotation *= Quaternion.Euler(UnityEngine.Random.Range(0f - randomness, randomness), UnityEngine.Random.Range(0f - randomness, randomness), 0f);
			Stamp(pose, _maxDistance, spread, strength);
		}
		StartDrying();
	}

	private void StartStamping()
	{
		_sprayStampMaterial.SetFloat(SUBTRACT_PROPERTY, 0f);
	}

	private void StartDrying()
	{
		_sprayStampMaterial.SetMatrix(STAMP_MATRIX_PROPERTY, Matrix4x4.zero);
		_sprayStampMaterial.SetFloat(SUBTRACT_PROPERTY, _dryingSpeed);
	}

	private void Stamp(Pose pose, float maxDistance, float angle, float strength)
	{
		_sprayStampMaterial.SetMatrix(STAMP_MATRIX_PROPERTY, CreateStampMatrix(pose, angle));
		_sprayStampMaterial.SetFloat(STAMP_MULTIPLIER_PROPERTY, strength);
		float num = Mathf.Tan(MathF.PI / 180f * angle / 2f) * maxDistance;
		Vector3 point = pose.position + pose.forward * num;
		Vector3 point2 = pose.position + pose.forward * maxDistance;
		HashSet<Transform> rootsFromOverlapResults = NonAlloc.GetRootsFromOverlapResults(Physics.OverlapCapsuleNonAlloc(point, point2, num, NonAlloc._overlapResults, _raycastLayerMask.value, QueryTriggerInteraction.Ignore));
		foreach (Transform item in rootsFromOverlapResults)
		{
			RenderSplash(item);
		}
		rootsFromOverlapResults.Clear();
	}

	private void RenderSplash(Transform rootObject)
	{
		List<MeshFilter> meshFiltersInChildren = NonAlloc.GetMeshFiltersInChildren(rootObject);
		for (int i = 0; i < meshFiltersInChildren.Count; i++)
		{
			int instanceID = meshFiltersInChildren[i].GetInstanceID();
			if (!NonAlloc._blits.ContainsKey(instanceID))
			{
				NonAlloc._blits[instanceID] = CreateMeshBlit(meshFiltersInChildren[i]);
			}
			NonAlloc._blits[instanceID].Blit();
		}
	}

	private MeshBlit CreateMeshBlit(MeshFilter meshFilter)
	{
		MeshBlit meshBlit = meshFilter.gameObject.AddComponent<MeshBlit>();
		meshBlit.material = _sprayStampMaterial;
		meshBlit.renderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.RHalf);
		meshBlit.BlitsPerSecond = 30f;
		if (meshFilter.TryGetComponent<Renderer>(out var component))
		{
			component.GetPropertyBlock(NonAlloc.PropertyBlock);
			NonAlloc.PropertyBlock.SetTexture(WET_MAP_PROPERTY, meshBlit.renderTexture);
			if ((bool)_waterBumpOverride)
			{
				NonAlloc.PropertyBlock.SetTexture(WET_BUMPMAP_PROPERTY, _waterBumpOverride);
			}
			component.SetPropertyBlock(NonAlloc.PropertyBlock);
		}
		return meshBlit;
	}

	private Matrix4x4 CreateStampMatrix(Pose pose, float angle)
	{
		Matrix4x4 inverse = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one).inverse;
		inverse.m20 *= -1f;
		inverse.m21 *= -1f;
		inverse.m22 *= -1f;
		inverse.m23 *= -1f;
		return GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(angle, 1f, 0f, _maxDistance), renderIntoTexture: true) * inverse;
	}

	private void OnDestroy()
	{
		NonAlloc.CleanUpDestroyedBlits();
	}

	public void BeginUse()
	{
		_dampedUseStrength = 0f;
		_lastUseTime = Time.realtimeSinceStartup;
	}

	public void EndUse()
	{
	}

	public float ComputeUseStrength(float strength)
	{
		float num = Time.realtimeSinceStartup - _lastUseTime;
		_lastUseTime = Time.realtimeSinceStartup;
		if (strength > _dampedUseStrength)
		{
			_dampedUseStrength = Mathf.Lerp(_dampedUseStrength, strength, _triggerSpeed * num);
		}
		else
		{
			_dampedUseStrength = strength;
		}
		float num2 = _strengthCurve.Evaluate(_dampedUseStrength);
		UpdateTriggerProgress(num2);
		return num2;
	}

	private void UpdateTriggerProgress(float progress)
	{
		UpdateTriggerRotation(progress);
		if (progress >= _fireThresold && !_wasFired)
		{
			_wasFired = true;
			SprayWater();
		}
		else if (progress <= _releaseThresold)
		{
			_wasFired = false;
		}
	}
}
