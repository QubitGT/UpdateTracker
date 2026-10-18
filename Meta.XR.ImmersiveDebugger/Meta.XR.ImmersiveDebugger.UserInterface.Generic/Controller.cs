using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Controller : MonoBehaviour
{
	private bool _visibility = true;

	private bool _refreshLayoutRequested;

	protected bool _hasRectTransform;

	private bool _layoutStyleHasChanged;

	[SerializeField]
	protected LayoutStyle _layoutStyle;

	protected List<Controller> _children;

	private RectMask2D _mask;

	private bool _transparent;

	internal Controller Owner { get; set; }

	public Transform Transform { get; protected set; }

	public RectTransform RectTransform { get; protected set; }

	protected GameObject GameObject { get; set; }

	public List<Controller> Children => _children;

	public LayoutStyle LayoutStyle
	{
		get
		{
			return _layoutStyle;
		}
		set
		{
			if (!(value == null) && !(_layoutStyle == value))
			{
				_layoutStyle = value;
				_layoutStyleHasChanged = true;
				RefreshLayout();
				LayoutStyle layoutStyle = _layoutStyle;
				if ((object)layoutStyle != null && layoutStyle.isOverlayCanvas)
				{
					UpdateRefreshLayout(force: false);
				}
			}
		}
	}

	public bool Transparent
	{
		get
		{
			return _transparent;
		}
		set
		{
			if (_transparent != value)
			{
				_transparent = value;
				OnTransparencyChanged();
			}
		}
	}

	public bool Visibility
	{
		get
		{
			return _visibility;
		}
		private set
		{
			if (_visibility != value)
			{
				_visibility = value;
				OnVisibilityChanged();
			}
		}
	}

	public event Action<Controller> OnVisibilityChangedEvent;

	protected virtual void OnTransparencyChanged()
	{
	}

	protected virtual void Setup(Controller owner)
	{
		Owner = owner;
		GameObject = base.gameObject;
		GameObject.layer = RuntimeSettings.Instance.PanelLayer;
		RectTransform = GameObject.AddComponent<RectTransform>() ?? GameObject.GetComponent<RectTransform>();
		Transform = (RectTransform ? RectTransform : GameObject.transform);
		if (Owner != this && Owner != null)
		{
			Transform.SetParent(Owner.Transform, worldPositionStays: false);
		}
		LayoutStyle = Style.Default<LayoutStyle>();
		_hasRectTransform = RectTransform != null;
	}

	internal T Append<T>(string childName) where T : Controller, new()
	{
		T val = SetupChildController<T>(childName);
		if (_children == null)
		{
			_children = new List<Controller>();
		}
		_children.Add(val);
		return val;
	}

	internal T Prepend<T>(string childName) where T : Controller, new()
	{
		T val = SetupChildController<T>(childName);
		if (_children == null)
		{
			_children = new List<Controller>();
		}
		_children.Insert(0, val);
		return val;
	}

	internal T InsertAfter<T>(string childName, Controller previous) where T : Controller, new()
	{
		T val = SetupChildController<T>(childName);
		if (_children == null)
		{
			_children = new List<Controller>();
		}
		int num = _children.IndexOf(previous);
		_children.Insert(num + 1, val);
		return val;
	}

	internal T InsertBefore<T>(string childName, Controller next) where T : Controller, new()
	{
		T val = SetupChildController<T>(childName);
		if (_children == null)
		{
			_children = new List<Controller>();
		}
		int index = _children.IndexOf(next);
		_children.Insert(index, val);
		return val;
	}

	private T SetupChildController<T>(string childName) where T : Controller, new()
	{
		T val = new GameObject(childName).AddComponent<T>();
		val.Setup(this);
		return val;
	}

	protected void Append(Controller controller)
	{
		if (_children != null && !_children.Contains(controller))
		{
			_children.Add(controller);
			controller.RefreshLayout();
		}
	}

	internal void Remove(Controller controller, bool destroy)
	{
		if (_children == null)
		{
			return;
		}
		_children.Remove(controller);
		if (destroy)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(controller.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(controller.gameObject);
			}
		}
		RefreshLayout();
	}

	protected void Clear(bool destroy)
	{
		while (_children.Count > 0)
		{
			List<Controller> children = _children;
			Remove(children[children.Count - 1], destroy);
		}
	}

	public void Hide()
	{
		Visibility = false;
	}

	public void Show()
	{
		Visibility = true;
	}

	internal void ToggleVisibility()
	{
		Visibility = !GameObject.activeSelf;
	}

	protected virtual void OnVisibilityChanged()
	{
		GameObject.SetActive(Visibility);
		this.OnVisibilityChangedEvent?.Invoke(this);
	}

	private static Vector2 GetVec2FromLayout(TextAnchor anchor)
	{
		return new Vector2((float)((int)anchor % 3) * 0.5f, 1f - (float)((int)anchor / 3) * 0.5f);
	}

	protected void UpdateRefreshLayout(bool force)
	{
		if (!force && !_refreshLayoutRequested)
		{
			return;
		}
		_refreshLayoutRequested = false;
		RefreshLayoutPreChildren();
		if (_children != null)
		{
			bool force2 = _layoutStyle.adaptHeight || _layoutStyle.autoFitChildren;
			foreach (Controller child in _children)
			{
				child.UpdateRefreshLayout(force2);
			}
		}
		RefreshLayoutPostChildren();
	}

	internal void RefreshLayout()
	{
		_refreshLayoutRequested = true;
		Owner?.RefreshLayout();
	}

	protected virtual void RefreshLayoutPreChildren()
	{
		if (!_hasRectTransform)
		{
			return;
		}
		if (_layoutStyleHasChanged)
		{
			_layoutStyleHasChanged = false;
			RectTransform.pivot = GetVec2FromLayout(_layoutStyle.pivot);
			RectTransform.anchorMin = GetVec2FromLayout(_layoutStyle.anchor);
			RectTransform.anchorMax = GetVec2FromLayout(_layoutStyle.anchor);
			switch (_layoutStyle.layout)
			{
			case LayoutStyle.Layout.Fill:
				RectTransform.anchorMin = new Vector2(0f, 0f);
				RectTransform.anchorMax = new Vector2(1f, 1f);
				break;
			case LayoutStyle.Layout.FillHorizontal:
				RectTransform.anchorMin = new Vector2(0f, RectTransform.anchorMin.y);
				RectTransform.anchorMax = new Vector2(1f, RectTransform.anchorMax.y);
				break;
			case LayoutStyle.Layout.FillVertical:
				RectTransform.anchorMin = new Vector2(RectTransform.anchorMin.x, 0f);
				RectTransform.anchorMax = new Vector2(RectTransform.anchorMax.x, 1f);
				break;
			}
			if (_layoutStyle.masks)
			{
				if ((object)_mask == null)
				{
					_mask = GameObject.AddComponent<RectMask2D>();
				}
				_mask.enabled = true;
			}
			else if (_mask != null)
			{
				_mask.enabled = false;
			}
		}
		Extensions.SetSizeOptimized(offsetMin: new Vector2(_layoutStyle.LeftMargin, _layoutStyle.BottomMargin), offsetMax: new Vector2(0f - _layoutStyle.RightMargin, 0f - _layoutStyle.TopMargin), rectTransform: RectTransform, fixedDimensions: _layoutStyle.size, setAnchoredPosition: !_layoutStyle.isOverlayCanvas);
	}

	protected virtual void RefreshLayoutPostChildren()
	{
		if (!_hasRectTransform || !LayoutStyle.adaptHeight)
		{
			return;
		}
		float num = 0f;
		if (_children != null)
		{
			foreach (Controller child in _children)
			{
				if (child is Flex flex)
				{
					num = Mathf.Max(num, flex.SizeDeltaWithMargin.y);
				}
			}
		}
		RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, num);
	}

	private void OnDestroy()
	{
		if (Owner != null)
		{
			Owner.Remove(this, destroy: false);
		}
	}

	internal void SetHeight(float height)
	{
		if (_layoutStyle.SetHeight(height))
		{
			RefreshLayout();
		}
	}

	internal void SetWidth(float width)
	{
		if (_layoutStyle.SetWidth(width))
		{
			RefreshLayout();
		}
	}
}
