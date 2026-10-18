using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace Liv.Lck.GorillaTag;

[RequireComponent(typeof(BoxCollider))]
public class GtColliderTriggerProcessor : MonoBehaviour
{
	[Header("Global Settings")]
	[SerializeField]
	private GtUiSettings _settings;

	[Header("Parameters")]
	[SerializeField]
	private GtColliderTriggerProcessorsGroup _group;

	[SerializeField]
	private float _tapCooldownTime = 0.25f;

	[SerializeField]
	private bool _checkTriggerFromAbove;

	[Header("Events")]
	[SerializeField]
	private UnityEvent _onTriggeredStarted;

	[SerializeField]
	private UnityEvent _onTriggeredEnded;

	private bool _canTap = true;

	private bool _isTapped;

	private GtTag _gtTag;

	private BoxCollider _boxCollider;

	public static bool IsGrabbingTablet;

	public static XRNode CurrentGrabbedHand;

	public Vector3 LastTapPosition { get; private set; }

	private void Start()
	{
		_boxCollider = GetComponent<BoxCollider>();
	}

	private GtTag GetGTag(Collider other)
	{
		GtTag component = other.GetComponent<GtTag>();
		if (component == null)
		{
			return null;
		}
		if (component.gtTagType == GtTagType.LeftHand || component.gtTagType == GtTagType.RightHand)
		{
			return component;
		}
		return null;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_gtTag != null)
		{
			return;
		}
		_gtTag = GetGTag(other);
		if (_gtTag == null || IsColliderGrabbingTablet(_gtTag))
		{
			return;
		}
		if (_group != null)
		{
			if (_group.GetCurrentTriggerProcessor() != null)
			{
				return;
			}
			_group.SetCurrentTriggerProcessor(this);
		}
		LastTapPosition = other.ClosestPoint(base.transform.position);
		if (IsTapValid(LastTapPosition) && _canTap)
		{
			_isTapped = true;
			_onTriggeredStarted.Invoke();
		}
	}

	private bool IsColliderGrabbingTablet(GtTag tag)
	{
		if (!IsGrabbingTablet)
		{
			return false;
		}
		if (tag.gtTagType == GtTagType.LeftHand && CurrentGrabbedHand == XRNode.LeftHand)
		{
			return true;
		}
		if (tag.gtTagType == GtTagType.RightHand && CurrentGrabbedHand == XRNode.RightHand)
		{
			return true;
		}
		return false;
	}

	private void OnTriggerExit(Collider other)
	{
		if (GetGTag(other) != _gtTag)
		{
			return;
		}
		_gtTag = null;
		if (_group != null)
		{
			if (_group.GetCurrentTriggerProcessor() != this)
			{
				return;
			}
			_group.SetCurrentTriggerProcessor(null);
		}
		if (_canTap)
		{
			_canTap = false;
			StartCoroutine(AllowTap());
			if (_isTapped)
			{
				_isTapped = false;
				_onTriggeredEnded.Invoke();
			}
		}
	}

	public void ResetToDefaultAfterTap()
	{
		_canTap = false;
		_isTapped = false;
		_gtTag = null;
		StartCoroutine(AllowTap());
		Invoke("SetTriggerNull", _tapCooldownTime);
	}

	public void BlockTapping()
	{
		_canTap = false;
		_isTapped = false;
		_gtTag = null;
	}

	public void ResetToDefault()
	{
		_canTap = true;
		_isTapped = false;
		_gtTag = null;
		SetTriggerNull();
	}

	public void ResetToDefaultAndTriggerButton()
	{
		_canTap = true;
		_isTapped = false;
		_gtTag = null;
		SetTriggerNull();
		_onTriggeredEnded.Invoke();
	}

	private void SetTriggerNull()
	{
		_group.SetCurrentTriggerProcessor(null);
	}

	private void OnEnable()
	{
		_canTap = true;
		_isTapped = false;
	}

	private bool IsTapValid(Vector3 tapPosition)
	{
		Vector3 vector = tapPosition - base.transform.position;
		Vector3 vector2 = Vector3.Scale(_boxCollider.size, base.transform.lossyScale);
		if (_checkTriggerFromAbove)
		{
			Vector3 rhs = vector;
			rhs.Normalize();
			float num = Vector3.Dot(base.transform.up, rhs);
			float num2 = Vector3.Dot(base.transform.forward, vector);
			if (num > 0.1f)
			{
				return num2 > 0f;
			}
			return false;
		}
		float num3 = vector2.z * 0.5f;
		return Vector3.Dot(base.transform.forward, vector) > num3;
	}

	private IEnumerator AllowTap()
	{
		yield return new WaitForSeconds(_tapCooldownTime);
		_canTap = true;
	}
}
