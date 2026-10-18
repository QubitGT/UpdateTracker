using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class VirtualLayout : UIBehaviour
{
	public float animationSpeed;

	[SerializeField]
	private RectTransform _layoutParent;

	private List<RectTransform> _rectChildren;

	private List<RectTransform> _virtualLayoutChildren;

	protected override void OnEnable()
	{
		if (_layoutParent == null)
		{
			return;
		}
		RectTransform[] componentsInChildren = _layoutParent.gameObject.GetComponentsInChildren<RectTransform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			RectTransform rectTransform = componentsInChildren[i];
			if (Application.isPlaying)
			{
				Object.Destroy(rectTransform.gameObject);
			}
			else
			{
				Object.DestroyImmediate(rectTransform.gameObject);
			}
		}
		RectTransform[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<RectTransform>();
		_rectChildren = new List<RectTransform>();
		_virtualLayoutChildren = new List<RectTransform>();
		for (int j = 1; j < componentsInChildren2.Length; j++)
		{
			RectTransform rectTransform2 = componentsInChildren2[j];
			if (!(rectTransform2.parent != (RectTransform)base.transform))
			{
				_rectChildren.Add(rectTransform2);
				ResetChildTransform(rectTransform2);
				GameObject obj = new GameObject();
				obj.hideFlags = HideFlags.HideAndDontSave;
				obj.name = rectTransform2.name;
				obj.AddComponent<RectTransform>();
				RectTransform rectTransform3 = (RectTransform)obj.transform;
				rectTransform3.SetParent(_layoutParent, worldPositionStays: false);
				ResetChildTransform(rectTransform3);
				_virtualLayoutChildren.Add(rectTransform3);
			}
		}
		_layoutParent.ForceUpdateRectTransforms();
	}

	private void ResetChildTransform(RectTransform child)
	{
		child.localPosition = Vector3.zero;
		child.anchoredPosition = Vector2.zero;
		child.localScale = Vector3.one;
		child.localRotation = Quaternion.identity;
		child.anchorMin = Vector2.zero;
		child.anchorMax = Vector2.zero;
		child.pivot = new Vector2(0.5f, 0.5f);
	}

	protected override void OnDisable()
	{
		foreach (RectTransform virtualLayoutChild in _virtualLayoutChildren)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(virtualLayoutChild.gameObject);
			}
			else
			{
				Object.DestroyImmediate(virtualLayoutChild.gameObject);
			}
		}
	}

	private void LateUpdate()
	{
		if (_layoutParent == null)
		{
			return;
		}
		((RectTransform)base.transform).anchoredPosition = _layoutParent.anchoredPosition;
		for (int i = 0; i < _virtualLayoutChildren.Count; i++)
		{
			RectTransform rectTransform = _rectChildren[i];
			RectTransform rectTransform2 = _virtualLayoutChildren[i];
			if (Application.isPlaying)
			{
				rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, rectTransform2.anchoredPosition, animationSpeed * Time.deltaTime);
				rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, rectTransform2.sizeDelta, animationSpeed * Time.deltaTime);
			}
			else
			{
				rectTransform.anchoredPosition = rectTransform2.anchoredPosition + _layoutParent.anchoredPosition;
				rectTransform.sizeDelta = rectTransform2.sizeDelta;
			}
		}
	}

	public void InjectAllVirtualLayoutElement(RectTransform layoutParent)
	{
		_layoutParent = layoutParent;
	}
}
