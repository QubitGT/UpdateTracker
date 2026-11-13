using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("GorillaTag/Cosmetics/Transferrable Object Hand Events")]
public class TransferrableObjectHandEvents : MonoBehaviour
{
	private enum HeldHand
	{
		Unknown,
		NotHeld,
		Left,
		Right
	}

	[SerializeField]
	private TransferrableObject transferrableObject;

	[SerializeField]
	private UnityEvent onHeldLeft;

	[SerializeField]
	private UnityEvent onHeldRight;

	[SerializeField]
	private UnityEvent onNotHeld;

	private HeldHand currentHand;

	private void Awake()
	{
		if (transferrableObject == null)
		{
			transferrableObject = GetComponentInParent<TransferrableObject>();
		}
	}

	private void OnEnable()
	{
		if (!(transferrableObject == null))
		{
			transferrableObject.OnPositionStateChanged += HandlePositionStateChanged;
			currentHand = HeldHand.Unknown;
			HandlePositionStateChanged(transferrableObject);
		}
	}

	private void OnDisable()
	{
		if (transferrableObject != null)
		{
			transferrableObject.OnPositionStateChanged -= HandlePositionStateChanged;
		}
	}

	private void HandlePositionStateChanged(TransferrableObject obj)
	{
		HeldHand heldHand = (obj.InLeftHand() ? HeldHand.Left : ((!obj.InRightHand()) ? HeldHand.NotHeld : HeldHand.Right));
		if (heldHand != currentHand)
		{
			currentHand = heldHand;
			switch (heldHand)
			{
			case HeldHand.Left:
				onHeldLeft?.Invoke();
				break;
			case HeldHand.Right:
				onHeldRight?.Invoke();
				break;
			default:
				onNotHeld?.Invoke();
				break;
			}
		}
	}
}
