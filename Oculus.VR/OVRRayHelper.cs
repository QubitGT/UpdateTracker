using UnityEngine;

public class OVRRayHelper : MonoBehaviour
{
	public MeshRenderer Renderer;

	public Material NormalMaterial;

	public Material PinchMaterial;

	public GameObject Cursor;

	public SpriteRenderer CursorFill;

	private Vector3 _initialScale;

	public float DefaultLength;

	private Vector3 _cursorIntitialSize;

	private const float _cursorSelectedScaleFactor = 0.5f;

	private void Start()
	{
		if (Renderer != null)
		{
			_initialScale = Renderer.transform.localScale;
		}
		if (Cursor != null)
		{
			_cursorIntitialSize = Cursor.transform.localScale;
		}
	}

	public void UpdatePointerRay(OVRInputRayData rayData)
	{
		if (Renderer != null)
		{
			float num = (rayData.IsOverCanvas ? rayData.DistanceToCanvas : DefaultLength);
			Renderer.transform.localPosition = Vector3.forward * (num * 0.5f + 0.05f);
			Renderer.transform.localScale = new Vector3(_initialScale.x, num * 0.5f - 0.025f, _initialScale.z);
			Renderer.sharedMaterial = (rayData.IsActive ? PinchMaterial : NormalMaterial);
		}
		if (Cursor != null)
		{
			Cursor.SetActive(rayData.IsOverCanvas);
			Cursor.transform.localScale = Mathf.Lerp(1f, 0.5f, rayData.ActivationStrength) * _cursorIntitialSize;
			if (CursorFill != null)
			{
				float a = Mathf.Lerp(0f, 1f, rayData.ActivationStrength);
				CursorFill.color = new Color(1f, 1f, 1f, a);
			}
			if (rayData.IsOverCanvas)
			{
				Cursor.transform.position = rayData.WorldPosition + rayData.WorldNormal * 0.05f;
				Cursor.transform.forward = rayData.WorldNormal;
			}
		}
	}
}
