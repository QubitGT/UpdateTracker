using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class MultiPartFire : MonoBehaviour
{
	public Transform? baseFire;

	public Transform? middleFire;

	public Transform? topFire;

	public float baseMultiplier;

	public float middleMultiplier;

	public float topMultiplier;

	public float bottomRange;

	public float middleRange;

	public float topRange;

	public float perlinStepBottom;

	public float perlinStepMiddle;

	public float perlinStepTop;

	public float slerp = 0.01f;

	private float lastAngleBottom;

	private float lastAngleMiddle;

	private float lastAngleTop;

	private float perlinBottom;

	private float perlinMiddle;

	private float perlinTop;

	private bool mergedBottom;

	private bool mergedMiddle;

	private bool mergedTop;

	private Vector3 tempVec;

	private float lastTime;

	private void Start()
	{
		lastAngleBottom = 0f;
		lastAngleMiddle = 0f;
		lastAngleTop = 0f;
		perlinBottom = Random.Range(0, 100);
		perlinMiddle = Random.Range(200, 300);
		perlinTop = Random.Range(400, 500);
		tempVec = new Vector3(0f, 0f, 0f);
		mergedBottom = false;
		mergedMiddle = false;
		mergedTop = false;
		lastTime = Time.time;
	}

	public void Update()
	{
		Flap(ref perlinBottom, perlinStepBottom, ref lastAngleBottom, ref baseFire, bottomRange, baseMultiplier, ref mergedBottom);
		Flap(ref perlinMiddle, perlinStepMiddle, ref lastAngleMiddle, ref middleFire, middleRange, middleMultiplier, ref mergedMiddle);
		Flap(ref perlinTop, perlinStepTop, ref lastAngleTop, ref topFire, topRange, topMultiplier, ref mergedTop);
		lastTime = Time.time;
	}

	private void Flap(ref float perlinValue, float perlinStep, ref float lastAngle, ref Transform? flameTransform, float range, float multiplier, ref bool isMerged)
	{
		if (flameTransform == null)
		{
			return;
		}
		perlinValue += perlinStep;
		lastAngle += (Time.time - lastTime) * Mathf.PerlinNoise(perlinValue, 0f);
		tempVec.x = range * Mathf.Sin(lastAngle * multiplier);
		if (Mathf.Abs(tempVec.x - flameTransform.localEulerAngles.x) > 180f)
		{
			if (tempVec.x > flameTransform.localEulerAngles.x)
			{
				tempVec.x -= 360f;
			}
			else
			{
				tempVec.x += 360f;
			}
		}
		if (isMerged)
		{
			flameTransform.localEulerAngles = tempVec;
		}
		else if (Mathf.Abs(flameTransform.localEulerAngles.x - tempVec.x) < 1f)
		{
			isMerged = true;
			flameTransform.localEulerAngles = tempVec;
		}
		else
		{
			tempVec.x = (tempVec.x - flameTransform.localEulerAngles.x) * slerp + flameTransform.localEulerAngles.x;
			flameTransform.localEulerAngles = tempVec;
		}
	}
}
