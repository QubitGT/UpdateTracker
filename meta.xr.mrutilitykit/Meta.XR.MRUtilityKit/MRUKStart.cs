using System;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MRUtilityKit;

[Obsolete("This class is now obsolete, please register events directly with the MRUK class", true)]
[Feature(Feature.Scene)]
public class MRUKStart : MonoBehaviour
{
	public UnityEvent sceneLoadedEvent = new UnityEvent();

	public UnityEvent<MRUKRoom> roomCreatedEvent = new UnityEvent<MRUKRoom>();

	public UnityEvent<MRUKRoom> roomUpdatedEvent = new UnityEvent<MRUKRoom>();

	public UnityEvent<MRUKRoom> roomRemovedEvent = new UnityEvent<MRUKRoom>();

	private void Start()
	{
		if (!MRUK.Instance)
		{
			Debug.LogWarning("Couldn't find instance of MRUK");
			return;
		}
		MRUK.Instance.RegisterSceneLoadedCallback(delegate
		{
			sceneLoadedEvent?.Invoke();
		});
		MRUK.Instance.RegisterRoomCreatedCallback(delegate(MRUKRoom room)
		{
			roomCreatedEvent?.Invoke(room);
		});
		MRUK.Instance.RegisterRoomRemovedCallback(delegate(MRUKRoom room)
		{
			roomRemovedEvent?.Invoke(room);
		});
		MRUK.Instance.RegisterRoomUpdatedCallback(delegate(MRUKRoom room)
		{
			roomUpdatedEvent?.Invoke(room);
		});
	}
}
