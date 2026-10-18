using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_room_guardian")]
[Feature(Feature.Scene)]
public class RoomGuardian : MonoBehaviour
{
	[Tooltip("Material to use for the Guardian effect")]
	public Material GuardianMaterial;

	[Tooltip("This is how far, in meters, the player must be form a surface for the Guardian to become visible (in other words, it blends `_GuardianFade` from 0 to 1). The position of the user is calculated as a point 0.2m above the ground. This is to catch tripping hazards, as well as walls.")]
	public float GuardianDistance = 1f;

	private void Start()
	{
		OVRPlugin.eyeFovPremultipliedAlphaModeEnabled = false;
		OVRTelemetry.Start(651901100, 0, -1L).Send();
	}

	private void Update()
	{
		if (!(GuardianMaterial == null))
		{
			MRUKRoom mRUKRoom = MRUK.Instance?.GetCurrentRoom();
			if ((bool)mRUKRoom)
			{
				bool num = mRUKRoom.IsPositionInRoom(Camera.main.transform.position);
				Vector3 vector = new Vector3(Camera.main.transform.position.x, 0.2f, Camera.main.transform.position.z);
				Vector3 surfacePosition;
				MRUKAnchor closestAnchor;
				float num2 = mRUKRoom.TryGetClosestSurfacePosition(vector, out surfacePosition, out closestAnchor, new LabelFilter(~(MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING)));
				bool flag = !mRUKRoom.IsPositionInSceneVolume(vector);
				float value = ((num && flag) ? Mathf.Clamp01(1f - num2 / GuardianDistance) : 1f);
				GuardianMaterial.SetFloat("_GuardianFade", value);
				Color color = (num ? Color.green : Color.red);
				Debug.DrawLine(vector, surfacePosition, color);
			}
		}
	}
}
