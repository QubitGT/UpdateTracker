using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Meta.WitAi.Utilities;

public class EventSystemInstantiator : MonoBehaviour
{
	public void Awake()
	{
		base.gameObject.GetOrAddComponent<EventSystem>();
		base.gameObject.GetOrAddComponent<InputSystemUIInputModule>();
	}
}
