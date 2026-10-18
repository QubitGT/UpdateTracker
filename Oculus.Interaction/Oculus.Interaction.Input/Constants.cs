using UnityEngine;

namespace Oculus.Interaction.Input;

public static class Constants
{
	public const int NUM_HAND_JOINTS = 26;

	public const int NUM_FINGERS = 5;

	public static readonly Vector3 RightProximal = Vector3.back;

	public static readonly Vector3 RightDistal = Vector3.forward;

	public static readonly Vector3 RightPinkySide = Vector3.right;

	public static readonly Vector3 RightThumbSide = Vector3.left;

	public static readonly Vector3 RightPalmar = Vector3.down;

	public static readonly Vector3 RightDorsal = Vector3.up;

	public static readonly Vector3 LeftProximal = Vector3.back;

	public static readonly Vector3 LeftDistal = Vector3.forward;

	public static readonly Vector3 LeftPinkySide = Vector3.left;

	public static readonly Vector3 LeftThumbSide = Vector3.right;

	public static readonly Vector3 LeftPalmar = Vector3.down;

	public static readonly Vector3 LeftDorsal = Vector3.up;
}
