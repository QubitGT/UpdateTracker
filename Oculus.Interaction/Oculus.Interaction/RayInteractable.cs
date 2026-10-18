using System;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction;

public class RayInteractable : PointerInteractable<RayInteractor, RayInteractable>
{
	[Tooltip("The mesh used as the interactive surface for the ray.")]
	[SerializeField]
	[Interface(typeof(ISurface), new Type[] { })]
	private UnityEngine.Object _surface;

	[Tooltip("Defines the boundaries of the raycast. All RayInteractables must be inside this surface for the raycast to reach them.")]
	[SerializeField]
	[Optional]
	[Interface(typeof(ISurface), new Type[] { })]
	private UnityEngine.Object _selectSurface;

	private ISurface SelectSurface;

	[Tooltip("An IMovementProvider that determines how the interactable moves when selected.")]
	[SerializeField]
	[Optional]
	[Interface(typeof(IMovementProvider), new Type[] { })]
	private UnityEngine.Object _movementProvider;

	[Tooltip("The score used when comparing two interactables to determine which one should be selected. Each interactable has its own score, and the highest scoring interactable will be selected.")]
	[SerializeField]
	[Optional]
	private int _tiebreakerScore;

	public ISurface Surface { get; private set; }

	private IMovementProvider MovementProvider { get; set; }

	public int TiebreakerScore
	{
		get
		{
			return _tiebreakerScore;
		}
		set
		{
			_tiebreakerScore = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Surface = _surface as ISurface;
		SelectSurface = _selectSurface as ISurface;
		MovementProvider = _movementProvider as IMovementProvider;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (!(_selectSurface != null))
		{
			SelectSurface = Surface;
			_selectSurface = SelectSurface as MonoBehaviour;
		}
		this.EndStart(ref _started);
	}

	public bool Raycast(Ray ray, out SurfaceHit hit, in float maxDistance, bool selectSurface)
	{
		return (selectSurface ? SelectSurface : Surface).Raycast(in ray, out hit, maxDistance);
	}

	public IMovement GenerateMovement(in Pose to, in Pose source)
	{
		if (MovementProvider == null)
		{
			return null;
		}
		IMovement movement = MovementProvider.CreateMovement();
		movement.StopAndSetPose(source);
		movement.MoveTo(to);
		return movement;
	}

	public void InjectAllRayInteractable(ISurface surface)
	{
		InjectSurface(surface);
	}

	public void InjectSurface(ISurface surface)
	{
		Surface = surface;
		_surface = surface as UnityEngine.Object;
	}

	public void InjectOptionalSelectSurface(ISurface surface)
	{
		SelectSurface = surface;
		_selectSurface = surface as UnityEngine.Object;
	}

	public void InjectOptionalMovementProvider(IMovementProvider provider)
	{
		_movementProvider = provider as UnityEngine.Object;
		MovementProvider = provider;
	}
}
