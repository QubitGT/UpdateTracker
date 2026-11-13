using UnityEngine;

public class HoverboardGrindRail : MonoBehaviour
{
	[Tooltip("Rail sliding axis in this transform's local space.")]
	[SerializeField]
	private Vector3 localRailAxis = Vector3.forward;

	[Tooltip("How quickly velocity perpendicular to the rail decays, per second. Higher = stickier rail.")]
	[SerializeField]
	private float lateralDamping = 20f;

	[Tooltip("Fraction (0-1) of the lateral speed removed each step that gets redirected along the rail instead of being lost.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float lateralToForwardConversion = 0.5f;

	[Tooltip("If the perpendicular speed (player units/sec) exceeds this, the rail doesn't hold - lets the player hop off sideways. <= 0 means never break away.")]
	[SerializeField]
	private float breakawayLateralSpeed;

	public Vector3 WorldRailAxis => base.transform.TransformDirection(localRailAxis).normalized;

	public Vector3 ApplyRailConstraint(Vector3 velocity, Vector3 surfaceNormal, float scale, float deltaTime)
	{
		Vector3 vector = Vector3.ProjectOnPlane(WorldRailAxis, surfaceNormal);
		if (vector.sqrMagnitude < 1E-06f)
		{
			return velocity;
		}
		vector.Normalize();
		Vector3 vector2 = Vector3.ProjectOnPlane(velocity, surfaceNormal);
		Vector3 vector3 = velocity - vector2;
		float num = Vector3.Dot(vector2, vector);
		Vector3 vector4 = vector2 - vector * num;
		float magnitude = vector4.magnitude;
		if (breakawayLateralSpeed > 0f && magnitude > breakawayLateralSpeed * scale)
		{
			return velocity;
		}
		float num2 = Mathf.Exp((0f - lateralDamping) * deltaTime);
		float num3 = magnitude * (1f - num2);
		float num4 = ((num >= 0f) ? 1f : (-1f));
		num += num4 * num3 * lateralToForwardConversion;
		return vector * num + vector4 * num2 + vector3;
	}
}
