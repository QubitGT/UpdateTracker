using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

[StructLayout(LayoutKind.Sequential)]
public class HandPinchData
{
	private const int NumHandJoints = 24;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 72, ArraySubType = UnmanagedType.R4)]
	private readonly float[] _jointPositions;

	public HandPinchData()
	{
		int num = 72;
		_jointPositions = new float[num];
	}

	public void SetJoints(IReadOnlyList<Pose> poses)
	{
		int num = 0;
		for (int i = 0; i < 24; i++)
		{
			Vector3 position = poses[i].position;
			_jointPositions[num++] = position.x;
			_jointPositions[num++] = position.y;
			_jointPositions[num++] = position.z;
		}
	}

	public void SetJoints(IReadOnlyList<Vector3> positions)
	{
		int num = 0;
		for (int i = 0; i < 24; i++)
		{
			Vector3 vector = positions[i];
			_jointPositions[num++] = vector.x;
			_jointPositions[num++] = vector.y;
			_jointPositions[num++] = vector.z;
		}
	}
}
