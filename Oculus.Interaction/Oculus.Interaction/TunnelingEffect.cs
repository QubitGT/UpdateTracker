using System;
using UnityEngine;

namespace Oculus.Interaction;

public class TunnelingEffect : MonoBehaviour
{
	[Header("Mask Setup")]
	[SerializeField]
	private Transform _leftEyeAnchor;

	[SerializeField]
	private Transform _rightEyeAnchor;

	[SerializeField]
	private Camera _centerEyeCamera;

	[SerializeField]
	private MeshFilter _meshFilter;

	[SerializeField]
	[Optional]
	private Vector3 _aimingDirection;

	[SerializeField]
	private bool _useAimingTarget;

	[Header("Mask State")]
	[SerializeField]
	private float _planeDistance;

	[Header("Mask Properties")]
	[SerializeField]
	private Color _maskOuterColor = Color.black;

	[SerializeField]
	private Color _maskInnerColor = Color.black;

	[SerializeField]
	[Range(0f, 360f)]
	private float _userFOV = 360f;

	[SerializeField]
	[Range(0f, 180f)]
	private float _featheredFOV = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _alphaStrength = 1f;

	private readonly int _maskColorInnerID = Shader.PropertyToID("_ColorInner");

	private readonly int _maskColorOuterID = Shader.PropertyToID("_ColorOuter");

	private readonly int _maskDirectionID = Shader.PropertyToID("_Direction");

	private readonly int _minRadiusID = Shader.PropertyToID("_MinRadius");

	private readonly int _maxRadiusID = Shader.PropertyToID("_MaxRadius");

	private readonly int _alphaID = Shader.PropertyToID("_Alpha");

	private Mesh _maskMesh;

	private Transform _meshTransform;

	private MeshRenderer _meshRenderer;

	private MaterialPropertyBlock _materialPropertyBlock;

	protected bool _started;

	private static readonly Vector3[] _vertices = new Vector3[4]
	{
		new Vector3(-1f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(-1f, -1f, 0f),
		new Vector3(1f, -1f, 0f)
	};

	private static readonly Vector3[] _uv0 = new Vector3[4]
	{
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f)
	};

	private static readonly int[] _triangles = new int[6] { 0, 1, 3, 0, 3, 2 };

	public Vector3 AimingDirection
	{
		get
		{
			return _aimingDirection;
		}
		set
		{
			_aimingDirection = value;
		}
	}

	public bool UseAimingTarget
	{
		get
		{
			return _useAimingTarget;
		}
		set
		{
			_useAimingTarget = value;
		}
	}

	public float PlaneDistance
	{
		get
		{
			return _planeDistance;
		}
		set
		{
			_planeDistance = value;
		}
	}

	public Color MaskOuterColor
	{
		get
		{
			return _maskOuterColor;
		}
		set
		{
			_maskOuterColor = value;
		}
	}

	public Color MaskInnerColor
	{
		get
		{
			return _maskInnerColor;
		}
		set
		{
			_maskInnerColor = value;
		}
	}

	public float UserFOV
	{
		get
		{
			return _userFOV;
		}
		set
		{
			_userFOV = value;
		}
	}

	public float ExtraFeatheredFOV
	{
		get
		{
			return _featheredFOV;
		}
		set
		{
			_featheredFOV = value;
		}
	}

	public float AlphaStrength
	{
		get
		{
			return _alphaStrength;
		}
		set
		{
			_alphaStrength = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_meshTransform = _meshFilter.gameObject.transform;
		_meshRenderer = _meshFilter.GetComponent<MeshRenderer>();
		_maskMesh = new Mesh();
		_maskMesh.SetVertices(_vertices);
		_maskMesh.SetTriangles(_triangles, 0);
		_maskMesh.SetUVs(0, _uv0);
		_maskMesh.name = "Tunnel";
		_meshFilter.sharedMesh = _maskMesh;
		_materialPropertyBlock = new MaterialPropertyBlock();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_meshRenderer.enabled = true;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_meshRenderer.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if ((object)_meshRenderer != null && (object)_meshTransform != null)
		{
			Transform obj = base.transform;
			Pose pose = _centerEyeCamera.transform.GetPose();
			obj.SetPose(in pose);
			float num = Mathf.Tan(MathF.PI / 180f * _centerEyeCamera.fieldOfView / 2f) * _planeDistance * 2f;
			float num2 = num * _centerEyeCamera.aspect;
			num2 += GetIPD();
			Vector2 vector = new Vector2(num2, num);
			vector *= 1.2f;
			_meshTransform.localPosition = new Vector3(0f, 0f, _planeDistance);
			_meshTransform.localScale = new Vector3(vector.x * 0.5f, vector.y * 0.5f, 1f);
			float num3 = UserFOV * 0.5f * (MathF.PI / 180f);
			float value = Mathf.Cos(num3);
			float value2 = Mathf.Cos(num3 - ExtraFeatheredFOV * (MathF.PI / 180f));
			_materialPropertyBlock.SetFloat(_alphaID, _alphaStrength);
			_materialPropertyBlock.SetFloat(_minRadiusID, value2);
			_materialPropertyBlock.SetFloat(_maxRadiusID, value);
			_materialPropertyBlock.SetColor(_maskColorInnerID, _maskInnerColor);
			_materialPropertyBlock.SetColor(_maskColorOuterID, _maskOuterColor);
			Vector3 vector2 = (_useAimingTarget ? _aimingDirection : _centerEyeCamera.transform.forward);
			_materialPropertyBlock.SetVector(_maskDirectionID, vector2.normalized);
			_meshRenderer.SetPropertyBlock(_materialPropertyBlock);
		}
	}

	private float GetIPD()
	{
		return Vector3.Distance(_leftEyeAnchor.position, _rightEyeAnchor.position);
	}

	public void InjectAllTunnelingEffect(Transform leftEyeAnchor, Transform rightEyeAnchor, Camera centerEyeCamera, MeshFilter meshFilter)
	{
		InjectLeftEyeAnchor(leftEyeAnchor);
		InjectRightEyeAnchor(rightEyeAnchor);
		InjectCenterEyeCamera(centerEyeCamera);
		InjectMeshFilter(meshFilter);
	}

	public void InjectLeftEyeAnchor(Transform leftEyeAnchor)
	{
		_leftEyeAnchor = leftEyeAnchor;
	}

	public void InjectRightEyeAnchor(Transform rightEyeAnchor)
	{
		_rightEyeAnchor = rightEyeAnchor;
	}

	public void InjectCenterEyeCamera(Camera centerEyeCamera)
	{
		_centerEyeCamera = centerEyeCamera;
	}

	public void InjectMeshFilter(MeshFilter meshFilter)
	{
		_meshFilter = meshFilter;
	}
}
