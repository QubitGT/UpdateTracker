using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.BuildingBlocks;

public class RoomMeshEvent : MonoBehaviour
{
	public UnityEvent<MeshFilter> OnRoomMeshLoadCompleted;
}
