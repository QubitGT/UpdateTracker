using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public struct DistantPointDetectorFrustums
{
	[SerializeField]
	private ConicalFrustum _selectionFrustum;

	[SerializeField]
	[Optional]
	private ConicalFrustum _deselectionFrustum;

	[SerializeField]
	[Optional]
	private ConicalFrustum _aidFrustum;

	[SerializeField]
	[Range(0f, 1f)]
	private float _aidBlending;

	public ConicalFrustum SelectionFrustum => _selectionFrustum;

	public ConicalFrustum DeselectionFrustum => _deselectionFrustum;

	public ConicalFrustum AidFrustum => _aidFrustum;

	public float AidBlending => _aidBlending;

	public DistantPointDetectorFrustums(ConicalFrustum selection, ConicalFrustum deselection, ConicalFrustum aid, float blend)
	{
		_selectionFrustum = selection;
		_deselectionFrustum = deselection;
		_aidFrustum = aid;
		_aidBlending = blend;
	}
}
