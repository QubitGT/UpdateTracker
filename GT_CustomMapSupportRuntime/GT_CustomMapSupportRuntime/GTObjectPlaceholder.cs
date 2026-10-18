using System.Collections.Generic;
using CustomMapSupport;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class GTObjectPlaceholder : MonoBehaviour
{
	public enum ECustomMapCosmeticItem
	{
		Item_A,
		Item_B,
		Item_C,
		Item_D,
		Item_E,
		Item_F,
		Item_G,
		Item_H,
		Item_I,
		Item_J,
		Item_K,
		Item_L
	}

	public GTObject PlaceholderObject;

	public bool useDefaultPlaceholder = true;

	public bool useCustomMesh;

	public float maxDistanceBeforeRespawn = 180f;

	public float maxSpeed = 30f;

	public float maxAccel = 15f;

	public AnimationCurve SpeedVSAccelCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public Vector3 localWindDirection = Vector3.up;

	public bool useWaterMesh = true;

	public float scrollTextureX;

	public float scrollTextureY;

	public float scaleTexture = 20f;

	[Tooltip("Transform for your flat Water Surface Plane. Y-Axis should point towards the Top of the water")]
	public Transform? surfacePlane;

	[Tooltip("Put any mesh colliders here that are used for your Water Surface if they aren't flat and aligned with the surfacePlane Transform")]
	public List<MeshCollider> surfaceColliders = new List<MeshCollider>();

	[Tooltip("Type of liquid for this Water Volume. This will also determine the Splash Effects that are used.")]
	public CMSZoneShaderSettings.EZoneLiquidType liquidType = CMSZoneShaderSettings.EZoneLiquidType.Water;

	[Tooltip("How fast to accelerate to the max speed of the Force Volume.\n\nExample: An acceleration of 10 would get to a max speed of 50 over 5 seconds.")]
	[Range(0f, 120f)]
	public float accel_FV;

	[Tooltip("Max depth towards the center of the volume before forcing closing velocity to 0 (-1 to not use max depth)")]
	[Range(-1f, 100f)]
	public float maxDepth_FV = -1f;

	[Tooltip("Maximum speed, in meters per second, the player can move along the direction of the volume's Y-Axis.")]
	[Range(0f, 120f)]
	public float maxSpeed_FV;

	[Tooltip("If true, all surfaces become maximum slippery while in the force volume")]
	public bool disableGrip_FV;

	public bool dampenLatVel_FV = true;

	[Tooltip("Dampen current velocity on the X axis")]
	[Range(0f, 100f)]
	public float dampenXVel_FV;

	[Tooltip("Dampen current velocity on the Z axis")]
	[Range(0f, 100f)]
	public float dampenZVel_FV;

	[Tooltip("If true, pulls player to center of the volume (towards Y-Axis)")]
	public bool applyPull_FV = true;

	[Range(0f, 500f)]
	public float pullToCenterAccel_FV;

	[Range(0f, 500f)]
	public float pullToCenterMaxSpeed_FV;

	[Tooltip("The Minimum distance before the centering force is applied")]
	[Range(0.0001f, 0.5f)]
	public float pullToCenterMinDist_FV = 0.1f;

	public AudioClip? enterClip;

	public AudioClip? exitClip;

	public AudioClip? loopClip;

	public AudioClip? loopCrescendoClip;

	[Tooltip("Creator Code that is pre-filled on this specific ATM")]
	public string defaultCreatorCode = "";

	[Range(3f, 31f)]
	public int ropeLength = 3;

	public GameObject? ropeSwingSegmentPrefab;

	public float ropeSegmentGenerationOffset = 1f;

	public List<RopeSwingSegment> ropeSwingSegments = new List<RopeSwingSegment>();

	public BezierSpline? spline;

	public GameObject? ziplineSegmentPrefab;

	public float ziplineSegmentGenerationOffset = 0.92f;

	public List<ZiplineSegment> ziplineSegments = new List<ZiplineSegment>();

	public ECustomMapCosmeticItem CosmeticItem;

	public WaterVolumeProperties GetWaterVolumeProperties()
	{
		WaterVolumeProperties result = default(WaterVolumeProperties);
		result.surfacePlane = surfacePlane;
		result.surfaceColliders = surfaceColliders;
		result.liquidType = liquidType;
		return result;
	}

	public ForceVolumeProperties GetForceVolumeProperties()
	{
		ForceVolumeProperties result = default(ForceVolumeProperties);
		result.accel = accel_FV;
		result.maxSpeed = maxSpeed_FV;
		result.maxDepth = maxDepth_FV;
		result.disableGrip = disableGrip_FV;
		result.dampenLateralVelocity = dampenLatVel_FV;
		result.dampenXVel = dampenXVel_FV;
		result.dampenZVel = dampenZVel_FV;
		result.applyPullToCenterAcceleration = applyPull_FV;
		result.pullToCenterAccel = pullToCenterAccel_FV;
		result.pullToCenterMaxSpeed = pullToCenterMaxSpeed_FV;
		result.pullToCenterMinDistance = pullToCenterMinDist_FV;
		result.enterClip = enterClip;
		result.exitClip = exitClip;
		result.loopClip = loopClip;
		result.loopCrescendoClip = loopCrescendoClip;
		return result;
	}

	public void SetForceVolumeProperties(ForceVolumeProperties props)
	{
		accel_FV = props.accel;
		maxSpeed_FV = props.maxSpeed;
		maxDepth_FV = props.maxDepth;
		disableGrip_FV = props.disableGrip;
		dampenLatVel_FV = props.dampenLateralVelocity;
		dampenXVel_FV = props.dampenXVel;
		dampenZVel_FV = props.dampenZVel;
		applyPull_FV = props.applyPullToCenterAcceleration;
		pullToCenterAccel_FV = props.pullToCenterAccel;
		pullToCenterMaxSpeed_FV = props.pullToCenterMaxSpeed;
		pullToCenterMinDist_FV = props.pullToCenterMinDistance;
		enterClip = props.enterClip;
		exitClip = props.exitClip;
		loopClip = props.loopClip;
		loopCrescendoClip = props.loopCrescendoClip;
	}
}
