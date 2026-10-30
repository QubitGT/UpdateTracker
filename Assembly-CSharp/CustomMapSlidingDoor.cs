using UnityEngine;

[AddComponentMenu("GorillaTag/Virtual Stump/Custom Map Sliding Door")]
public class CustomMapSlidingDoor : MonoBehaviour
{
	[Tooltip("The door to move. Defaults to this transform if left empty. Its position at startup is the closed position. Must not be static.")]
	public Transform doorTransform;

	[Tooltip("Empty transform placed where the door should be when fully open. Must not be a child of the door.")]
	public Transform openPosition;

	[Tooltip("Movement speed in meters per second.")]
	public float speed = 1.5f;

	[Tooltip("Whether the door starts open.")]
	public bool startOpen;

	[Tooltip("Optional sound played when the door starts opening.")]
	public AudioClip openSound;

	[Tooltip("Optional sound played when the door starts closing.")]
	public AudioClip closeSound;

	public AudioSource audioSource;

	private Vector3 closedWorldPosition;

	private bool isOpen;

	private bool isMoving;

	private bool initialized;

	private Vector3 TargetPosition
	{
		get
		{
			if (!isOpen)
			{
				return closedWorldPosition;
			}
			return openPosition.position;
		}
	}

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			if (doorTransform == null)
			{
				doorTransform = base.transform;
			}
			if (openPosition == null)
			{
				Debug.LogError("[CustomMapSlidingDoor] " + base.name + " has no openPosition assigned; the door will not move.", this);
			}
			closedWorldPosition = doorTransform.position;
			if (startOpen && openPosition != null)
			{
				isOpen = true;
				doorTransform.position = openPosition.position;
			}
		}
	}

	public void OpenDoor()
	{
		MoveTo(open: true, openSound);
	}

	public void CloseDoor()
	{
		MoveTo(open: false, closeSound);
	}

	public void SetOpenImmediate(bool open)
	{
		Initialize();
		if (!(openPosition == null))
		{
			isOpen = open;
			isMoving = false;
			doorTransform.position = TargetPosition;
		}
	}

	private void MoveTo(bool open, AudioClip sound)
	{
		Initialize();
		if (!(openPosition == null) && isOpen != open)
		{
			isOpen = open;
			isMoving = true;
			PlaySound(sound);
		}
	}

	private void Update()
	{
		if (isMoving)
		{
			Vector3 targetPosition = TargetPosition;
			doorTransform.position = Vector3.MoveTowards(doorTransform.position, targetPosition, speed * Time.deltaTime);
			if (doorTransform.position == targetPosition)
			{
				isMoving = false;
			}
		}
	}

	private void PlaySound(AudioClip clip)
	{
		if (clip != null && audioSource != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}
}
