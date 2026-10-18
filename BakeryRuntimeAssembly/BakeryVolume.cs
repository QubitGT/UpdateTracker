using System;
using UnityEngine;

[HelpURL("https://geom.io/bakery/wiki/index.php?title=Manual#Bakery_Volume")]
[ExecuteInEditMode]
public class BakeryVolume : MonoBehaviour
{
	public enum Encoding
	{
		Half4,
		RGBA8,
		RGBA8Mono
	}

	public enum ShadowmaskEncoding
	{
		RGBA8,
		A8
	}

	public bool enableBaking = true;

	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	public bool adaptiveRes = true;

	public float voxelsPerUnit = 0.5f;

	public int resolutionX = 16;

	public int resolutionY = 16;

	public int resolutionZ = 16;

	public Encoding encoding;

	public ShadowmaskEncoding shadowmaskEncoding;

	public bool firstLightIsAlwaysAlpha;

	public bool denoise;

	public bool isGlobal;

	public Texture3D bakedTexture0;

	public Texture3D bakedTexture1;

	public Texture3D bakedTexture2;

	public Texture3D bakedTexture3;

	public Texture3D bakedMask;

	public bool supportRotationAfterBake;

	public bool rotateAroundY;

	public bool _rotateAroundXYZ;

	public int multiVolumePriority;

	public static BakeryVolume globalVolume;

	public static bool showAll;

	private Transform tform;

	public Vector3 GetMin()
	{
		if (rotateAroundY)
		{
			Vector2 rotationY = GetRotationY();
			Vector3 vector = bounds.min - bounds.center;
			return new Vector3(vector.x * rotationY.y + vector.z * rotationY.x, vector.y, vector.x * (0f - rotationY.x) + vector.z * rotationY.y) + bounds.center;
		}
		return bounds.min;
	}

	public Vector3 GetMax()
	{
		if (rotateAroundY)
		{
			Vector2 rotationY = GetRotationY();
			Vector3 vector = bounds.max - bounds.center;
			return new Vector3(vector.x * rotationY.y + vector.z * rotationY.x, vector.y, vector.x * (0f - rotationY.x) + vector.z * rotationY.y) + bounds.center;
		}
		return bounds.max;
	}

	private Vector3 TransformPoint(Vector3 p, Vector3 center, Vector2 sc)
	{
		p -= center;
		return new Vector3(p.x * sc.y + p.z * sc.x, p.y, p.x * (0f - sc.x) + p.z * sc.y) + center;
	}

	public Vector4 GetWorldXZMinMax()
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		if (rotateAroundY)
		{
			Vector2 rotationY = GetRotationY();
			Vector3 center = bounds.center;
			Vector3 min2 = bounds.min;
			Vector3 p = new Vector3(bounds.max.x, 0f, bounds.min.z);
			Vector3 max2 = bounds.max;
			Vector3 p2 = new Vector3(bounds.min.x, 0f, bounds.max.z);
			min2 = TransformPoint(min2, center, rotationY);
			p = TransformPoint(p, center, rotationY);
			max2 = TransformPoint(max2, center, rotationY);
			p2 = TransformPoint(p2, center, rotationY);
			float x = Mathf.Min(Mathf.Min(Mathf.Min(min2.x, p.x), max2.x), p2.x);
			float y = Mathf.Min(Mathf.Min(Mathf.Min(min2.z, p.z), max2.z), p2.z);
			float z = Mathf.Max(Mathf.Max(Mathf.Max(min2.x, p.x), max2.x), p2.x);
			float w = Mathf.Max(Mathf.Max(Mathf.Max(min2.z, p.z), max2.z), p2.z);
			return new Vector4(x, y, z, w);
		}
		return new Vector4(min.x, min.z, max.x, max.z);
	}

	public Vector3 GetMaxXMinZ()
	{
		if (rotateAroundY)
		{
			Vector2 rotationY = GetRotationY();
			Vector3 vector = new Vector3(bounds.max.x, 0f, bounds.min.z) - bounds.center;
			return new Vector3(vector.x * rotationY.y + vector.z * rotationY.x, vector.y, vector.x * (0f - rotationY.x) + vector.z * rotationY.y) + bounds.center;
		}
		return bounds.max;
	}

	public Vector3 GetInvSize()
	{
		Bounds bounds = this.bounds;
		return new Vector3(1f / bounds.size.x, 1f / bounds.size.y, 1f / bounds.size.z);
	}

	public Matrix4x4 GetMatrix()
	{
		if (tform == null)
		{
			tform = base.transform;
		}
		return Matrix4x4.TRS(tform.position, tform.rotation, Vector3.one).inverse;
	}

	public Vector2 GetRotationY()
	{
		if (!rotateAroundY)
		{
			return new Vector2(0f, 1f);
		}
		if (tform == null)
		{
			tform = base.transform;
		}
		float f = tform.eulerAngles.y * (MathF.PI / 180f);
		return new Vector2(Mathf.Sin(f), Mathf.Cos(f));
	}

	public void SetGlobalParams()
	{
		Shader.SetGlobalTexture("_Volume0", bakedTexture0);
		Shader.SetGlobalTexture("_Volume1", bakedTexture1);
		Shader.SetGlobalTexture("_Volume2", bakedTexture2);
		if (bakedTexture3 != null)
		{
			Shader.SetGlobalTexture("_Volume3", bakedTexture3);
		}
		Shader.SetGlobalTexture("_VolumeMask", bakedMask);
		Shader.SetGlobalVector("_GlobalVolumeMin", GetMin());
		Shader.SetGlobalVector("_GlobalVolumeInvSize", GetInvSize());
		if (supportRotationAfterBake)
		{
			Shader.SetGlobalMatrix("_GlobalVolumeMatrix", GetMatrix());
		}
		if (rotateAroundY)
		{
			Shader.SetGlobalVector("_GlobalVolumeRY", GetRotationY());
		}
		if (bakedTexture0 != null)
		{
			Shader.SetGlobalVector("_GlobalVolumeVoxelSize", new Vector3(1f / (float)bakedTexture0.width, 1f / (float)bakedTexture0.height, 1f / (float)bakedTexture0.depth));
		}
	}

	public void UpdateBounds()
	{
		Vector3 position = base.transform.position;
		Vector3 size = bounds.size;
		bounds = new Bounds(position, size);
	}

	public void OnEnable()
	{
		if (isGlobal)
		{
			globalVolume = this;
			SetGlobalParams();
		}
	}

	private void OnDrawGizmos()
	{
		if (showAll)
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
			Transform transform = base.transform;
			if (rotateAroundY)
			{
				Quaternion quaternion = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
				Gizmos.matrix = Matrix4x4.TRS(quaternion * -transform.position + transform.position, quaternion, Vector3.one);
			}
			Gizmos.DrawWireCube(transform.position, bounds.size);
		}
	}
}
