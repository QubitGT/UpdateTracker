using System;
using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleDataTeleport : MonoBehaviour, IReticleData
{
	[Obsolete]
	public enum TeleportReticleMode
	{
		Hidden,
		ValidTarget,
		InvalidTarget
	}

	[SerializeField]
	[Optional]
	private Transform _snapPoint;

	[SerializeField]
	[Optional]
	private MaterialPropertyBlockEditor _materialBlock;

	private static readonly int _highlightShaderID = Shader.PropertyToID("_Highlight");

	[Tooltip("Determines if the teleport reticle is hidden or marked as either valid or invalid when hovering over this spot.")]
	[SerializeField]
	[Obsolete("Use _hideReticle instead")]
	[Optional(OptionalAttribute.Flag.Obsolete)]
	private TeleportReticleMode _reticleMode = TeleportReticleMode.ValidTarget;

	[Tooltip("Determines if the teleport reticle is hidden when hovering over this spot.")]
	[SerializeField]
	public bool _hideReticle;

	[Obsolete("Use HideReticle instead")]
	public TeleportReticleMode ReticleMode
	{
		get
		{
			return _reticleMode;
		}
		set
		{
			_reticleMode = value;
		}
	}

	public bool HideReticle
	{
		get
		{
			return _hideReticle;
		}
		set
		{
			_hideReticle = value;
		}
	}

	public Vector3 ProcessHitPoint(Vector3 hitPoint)
	{
		if (_snapPoint != null)
		{
			return _snapPoint.position;
		}
		return hitPoint;
	}

	public void Highlight(bool highlight)
	{
		if (_materialBlock != null)
		{
			_materialBlock.MaterialPropertyBlock.SetFloat(_highlightShaderID, highlight ? 1f : 0f);
			_materialBlock.UpdateMaterialPropertyBlock();
		}
	}

	public void InjectOptionalSnapPoint(Transform snapPoint)
	{
		_snapPoint = snapPoint;
	}

	public void InjectOptionalMaterialPropertyBlockEditor(MaterialPropertyBlockEditor materialBlock)
	{
		_materialBlock = materialBlock;
	}
}
