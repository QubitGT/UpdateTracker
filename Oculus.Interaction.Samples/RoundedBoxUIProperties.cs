using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoundedBoxUIProperties : UIBehaviour, IMeshModifier
{
	private Image _image;

	public Vector4 borderRadius;

	protected override void OnEnable()
	{
		_image = base.gameObject.GetComponent<Image>();
	}

	protected override void OnDisable()
	{
		_image = null;
	}

	protected override void Start()
	{
		StartCoroutine(DelayVertexGeneration());
	}

	private IEnumerator DelayVertexGeneration()
	{
		yield return new WaitForSeconds(0.1f);
		if (_image == null)
		{
			_image = base.gameObject.GetComponent<Image>();
			if (_image == null)
			{
				yield break;
			}
		}
		_image.SetAllDirty();
	}

	public void ModifyMesh(Mesh mesh)
	{
	}

	public void ModifyMesh(VertexHelper verts)
	{
		if (_image == null)
		{
			_image = base.gameObject.GetComponent<Image>();
			if (_image == null)
			{
				return;
			}
		}
		Rect rect = ((RectTransform)base.transform).rect;
		Vector4 vector = new Vector4(rect.x, rect.y, Mathf.Abs(rect.width), Mathf.Abs(rect.height));
		UIVertex vertex = default(UIVertex);
		for (int i = 0; i < verts.currentVertCount; i++)
		{
			verts.PopulateUIVertex(ref vertex, i);
			Vector4 uv = vertex.uv0;
			uv.z = vector.z;
			uv.w = vector.w;
			vertex.uv0 = uv;
			vertex.uv1 = borderRadius * 0.5f;
			verts.SetUIVertex(vertex, i);
		}
	}
}
