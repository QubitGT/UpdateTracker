using System;
using UnityEngine;

[Serializable]
public struct InputStruct
{
	public int headRotation;

	public bool usingNewIK;

	public int bodyRotation;

	public short leftUpperArmRotation;

	public short rightUpperArmRotation;

	public long rightHandLong;

	public long leftHandLong;

	public long position;

	public int handPosition;

	public int rotation;

	public int packedFields;

	public short packedCompetitiveData;

	public Vector3 velocity;

	public int grabbedRopeIndex;

	public int ropeBoneIndex;

	public bool ropeGrabIsLeft;

	public bool ropeGrabIsBody;

	public Vector3 ropeGrabOffset;

	public bool movingSurfaceIsMonkeBlock;

	public long hoverboardPosRot;

	public short hoverboardColor;

	public long propHuntPosRot;

	public double serverTimeStamp;

	public short taggedById;

	public bool isGroundedHand;

	public bool isGroundedButt;

	public int leftHandGrabbedActorNumber;

	public bool leftGrabbedHandIsLeft;

	public int rightHandGrabbedActorNumber;

	public bool rightGrabbedHandIsLeft;

	public float lastTouchedGroundAtTime;

	public float lastHandTouchedGroundAtTime;

	public long packedGTPlayerStats;

	public int gtPlayerStatsFlags;
}
