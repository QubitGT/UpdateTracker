using UnityEngine;

public class FPSController : MonoBehaviour
{
	public delegate void OnStateChangeEventHandler();

	public float baseMoveSpeed = 4f;

	public float shiftMoveSpeed = 8f;

	public float ctrlMoveSpeed = 1f;

	public float lookHorizontal = 0.4f;

	public float lookVertical = 0.25f;

	[SerializeField]
	private Vector3 leftControllerPosOffset = new Vector3(-0.2f, -0.25f, 0.3f);

	[SerializeField]
	private Vector3 leftControllerRotationOffset = new Vector3(265f, -82f, 28f);

	[SerializeField]
	private Vector3 rightControllerPosOffset = new Vector3(0.2f, -0.25f, 0.3f);

	[SerializeField]
	private Vector3 rightControllerRotationOffset = new Vector3(263f, 318f, 485f);

	[SerializeField]
	private Vector3 noclipLeftControllerPosOffset = new Vector3(-0.3f, -0.1f, 0.65f);

	[SerializeField]
	private Vector3 noclipLeftControllerRotationOffset = new Vector3(180f, -90f, -90f);

	[SerializeField]
	private Vector3 noclipRightControllerPosOffset = new Vector3(0.3f, -0.1f, 0.65f);

	[SerializeField]
	private Vector3 noclipRightControllerRotationOffset = new Vector3(0f, -90f, -90f);

	[SerializeField]
	private bool toggleGrab;

	[SerializeField]
	private bool clampGrab;

	[Tooltip("Sweep the head, hands and body along each move and slide along whatever they hit, instead of pushing into it. Not used in noclip.")]
	[SerializeField]
	private bool collideAndSlide = true;

	[Tooltip("Tallest ledge (in m at player scale 1) that collide and slide steps up onto instead of sliding along.")]
	[SerializeField]
	private float stepHeight = 0.2f;

	[Tooltip("Rough launch speed (m/s) when clicking to throw a held game entity. Far targets get the slowest arc that still reaches them. Hold Shift to lob instead.")]
	[SerializeField]
	private float clickThrowSpeed = 15f;

	private bool controlRightHand;

	public LayerMask HandMask;

	[HideInInspector]
	public event OnStateChangeEventHandler OnStartEvent;

	public event OnStateChangeEventHandler OnStopEvent;
}
