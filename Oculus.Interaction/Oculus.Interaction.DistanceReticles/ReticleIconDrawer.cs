using System;
using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleIconDrawer : InteractorReticle<ReticleDataIcon>
{
	[SerializeField]
	[Interface(typeof(IDistanceInteractor), new Type[] { })]
	private UnityEngine.Object _distanceInteractor;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private Transform _centerEye;

	[SerializeField]
	private Texture _defaultIcon;

	[SerializeField]
	private bool _constantScreenSize;

	private Vector3 _originalScale;

	private IDistanceInteractor DistanceInteractor { get; set; }

	public Texture DefaultIcon
	{
		get
		{
			return _defaultIcon;
		}
		set
		{
			_defaultIcon = value;
		}
	}

	public bool ConstantScreenSize
	{
		get
		{
			return _constantScreenSize;
		}
		set
		{
			_constantScreenSize = value;
		}
	}

	protected override IInteractorView Interactor { get; set; }

	protected override Component InteractableComponent => DistanceInteractor.DistanceInteractable as Component;

	protected virtual void OnValidate()
	{
		if (_renderer != null)
		{
			_renderer.sharedMaterial.mainTexture = _defaultIcon;
		}
	}

	protected virtual void Awake()
	{
		DistanceInteractor = _distanceInteractor as IDistanceInteractor;
		Interactor = DistanceInteractor;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		_originalScale = base.transform.localScale;
		this.EndStart(ref _started);
	}

	protected override void Draw(ReticleDataIcon dataIcon)
	{
		if (dataIcon != null && dataIcon.CustomIcon != null)
		{
			_renderer.material.mainTexture = dataIcon.CustomIcon;
		}
		else
		{
			_renderer.material.mainTexture = _defaultIcon;
		}
		if (!_constantScreenSize)
		{
			_renderer.transform.localScale = _originalScale * dataIcon.GetTargetSize().magnitude;
		}
		_renderer.enabled = true;
	}

	protected override void Align(ReticleDataIcon data)
	{
		base.transform.position = data.ProcessHitPoint(DistanceInteractor.HitPoint);
		if (_renderer.enabled)
		{
			Vector3 normalized = (_centerEye.position - base.transform.position).normalized;
			base.transform.LookAt(base.transform.position - normalized, Vector3.up);
			if (_constantScreenSize)
			{
				float num = Vector3.Distance(base.transform.position, _centerEye.position);
				_renderer.transform.localScale = _originalScale * num;
			}
		}
	}

	protected override void Hide()
	{
		_renderer.enabled = false;
	}

	public void InjectAllReticleIconDrawer(IDistanceInteractor distanceInteractor, Transform centerEye, MeshRenderer renderer)
	{
		InjectDistanceInteractor(distanceInteractor);
		InjectCenterEye(centerEye);
		InjectRenderer(renderer);
	}

	public void InjectDistanceInteractor(IDistanceInteractor distanceInteractor)
	{
		_distanceInteractor = distanceInteractor as UnityEngine.Object;
		DistanceInteractor = distanceInteractor;
		Interactor = distanceInteractor;
	}

	public void InjectCenterEye(Transform centerEye)
	{
		_centerEye = centerEye;
	}

	public void InjectRenderer(MeshRenderer renderer)
	{
		_renderer = renderer;
	}
}
