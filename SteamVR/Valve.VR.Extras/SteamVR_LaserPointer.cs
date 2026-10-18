using UnityEngine;

namespace Valve.VR.Extras;

public class SteamVR_LaserPointer : MonoBehaviour
{
	public SteamVR_Behaviour_Pose pose;

	public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

	public bool active = true;

	public Color color;

	public float thickness = 0.002f;

	public Color clickColor = Color.green;

	public GameObject holder;

	public GameObject pointer;

	private bool isActive;

	public bool addRigidBody;

	public Transform reference;

	private Transform previousContact;

	public event PointerEventHandler PointerIn;

	public event PointerEventHandler PointerOut;

	public event PointerEventHandler PointerClick;

	private void Start()
	{
		if (pose == null)
		{
			pose = GetComponent<SteamVR_Behaviour_Pose>();
		}
		if (pose == null)
		{
			Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);
		}
		if (interactWithUI == null)
		{
			Debug.LogError("No ui interaction action has been set on this component.", this);
		}
		holder = new GameObject();
		holder.transform.parent = base.transform;
		holder.transform.localPosition = Vector3.zero;
		holder.transform.localRotation = Quaternion.identity;
		pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
		pointer.transform.parent = holder.transform;
		pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
		pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
		pointer.transform.localRotation = Quaternion.identity;
		BoxCollider component = pointer.GetComponent<BoxCollider>();
		if (addRigidBody)
		{
			if ((bool)component)
			{
				component.isTrigger = true;
			}
			pointer.AddComponent<Rigidbody>().isKinematic = true;
		}
		else if ((bool)component)
		{
			Object.Destroy(component);
		}
		Material material = new Material(Shader.Find("Unlit/Color"));
		material.SetColor("_Color", color);
		pointer.GetComponent<MeshRenderer>().material = material;
	}

	public virtual void OnPointerIn(PointerEventArgs e)
	{
		if (this.PointerIn != null)
		{
			this.PointerIn(this, e);
		}
	}

	public virtual void OnPointerClick(PointerEventArgs e)
	{
		if (this.PointerClick != null)
		{
			this.PointerClick(this, e);
		}
	}

	public virtual void OnPointerOut(PointerEventArgs e)
	{
		if (this.PointerOut != null)
		{
			this.PointerOut(this, e);
		}
	}

	private void Update()
	{
		if (!isActive)
		{
			isActive = true;
			base.transform.GetChild(0).gameObject.SetActive(value: true);
		}
		float num = 100f;
		RaycastHit hitInfo;
		bool num2 = Physics.Raycast(new Ray(base.transform.position, base.transform.forward), out hitInfo);
		if ((bool)previousContact && previousContact != hitInfo.transform)
		{
			OnPointerOut(new PointerEventArgs
			{
				fromInputSource = pose.inputSource,
				distance = 0f,
				flags = 0u,
				target = previousContact
			});
			previousContact = null;
		}
		if (num2 && previousContact != hitInfo.transform)
		{
			OnPointerIn(new PointerEventArgs
			{
				fromInputSource = pose.inputSource,
				distance = hitInfo.distance,
				flags = 0u,
				target = hitInfo.transform
			});
			previousContact = hitInfo.transform;
		}
		if (!num2)
		{
			previousContact = null;
		}
		if (num2 && hitInfo.distance < 100f)
		{
			num = hitInfo.distance;
		}
		if (num2 && interactWithUI.GetStateUp(pose.inputSource))
		{
			PointerEventArgs e = default(PointerEventArgs);
			e.fromInputSource = pose.inputSource;
			e.distance = hitInfo.distance;
			e.flags = 0u;
			e.target = hitInfo.transform;
			OnPointerClick(e);
		}
		if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
		{
			pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, num);
			pointer.GetComponent<MeshRenderer>().material.color = clickColor;
		}
		else
		{
			pointer.transform.localScale = new Vector3(thickness, thickness, num);
			pointer.GetComponent<MeshRenderer>().material.color = color;
		}
		pointer.transform.localPosition = new Vector3(0f, 0f, num / 2f);
	}
}
