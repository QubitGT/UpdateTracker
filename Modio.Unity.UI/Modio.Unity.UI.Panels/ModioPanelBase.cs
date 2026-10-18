using System;
using Modio.Unity.UI.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Panels;

public abstract class ModioPanelBase : MonoBehaviour
{
	public enum GainedFocusCause
	{
		OpeningFromClosed,
		RegainingFocusFromStackedPanel,
		InputSuppressionChangeOnly
	}

	[SerializeField]
	private GameObject _panelToEnable;

	[SerializeField]
	private Selectable _selectOnOpen;

	[SerializeField]
	private bool _startHidden;

	[SerializeField]
	private ModioPanelBase _openOnTopOf;

	private GameObject _lastSelectedGameObject;

	public bool HasFocus { get; private set; }

	public event Action<bool> OnHasFocusChanged;

	protected virtual void Awake()
	{
		ModioPanelManager.GetInstance().RegisterPanel(this);
	}

	protected virtual void Start()
	{
		if (_startHidden && !HasFocus)
		{
			if (_panelToEnable == null)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				_panelToEnable.SetActive(value: false);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (HasFocus)
		{
			OnLostFocus();
		}
	}

	public void OpenPanel()
	{
		Transform parent = base.transform.parent;
		if (parent != null && !parent.gameObject.activeInHierarchy)
		{
			Debug.LogWarning($"Attempted to open panel {this} with disabled parent. Suppressing this to avoid lost input.");
			return;
		}
		if (_panelToEnable == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			_panelToEnable.SetActive(value: true);
		}
		if (_openOnTopOf != null && ((_openOnTopOf._panelToEnable == null) ? (!base.gameObject.activeSelf) : (!_openOnTopOf._panelToEnable.activeSelf)))
		{
			_openOnTopOf.OpenPanel();
		}
		ModioPanelManager.GetInstance().OpenPanel(this);
	}

	public void ClosePanel()
	{
		ModioPanelManager.GetInstance().ClosePanel(this);
		if (_panelToEnable == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			_panelToEnable.SetActive(value: false);
		}
	}

	public virtual void OnGainedFocus(GainedFocusCause selectionBehaviour)
	{
		HasFocus = true;
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.Cancel, CancelPressed);
		ModioUIInput.SwappedControlScheme += OnSwappedControlScheme;
		if (selectionBehaviour == GainedFocusCause.RegainingFocusFromStackedPanel && _lastSelectedGameObject != null && _lastSelectedGameObject.activeInHierarchy && !EventSystem.current.alreadySelecting)
		{
			SetSelectedGameObject(_lastSelectedGameObject);
			NewSelectionWhileFocused(_lastSelectedGameObject);
		}
		else if (selectionBehaviour != GainedFocusCause.InputSuppressionChangeOnly)
		{
			DoDefaultSelection();
		}
		this.OnHasFocusChanged?.Invoke(obj: true);
	}

	public virtual void OnLostFocus()
	{
		HasFocus = false;
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Cancel, CancelPressed);
		ModioUIInput.SwappedControlScheme -= OnSwappedControlScheme;
		this.OnHasFocusChanged?.Invoke(obj: false);
	}

	protected virtual void CancelPressed()
	{
		ClosePanel();
	}

	public virtual void DoDefaultSelection()
	{
		if (_selectOnOpen != null)
		{
			_selectOnOpen.Select();
			NewSelectionWhileFocused(_selectOnOpen.gameObject);
		}
	}

	public virtual void FocusedPanelLateUpdate()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject != null && currentSelectedGameObject.activeInHierarchy)
		{
			if ((object)_lastSelectedGameObject != currentSelectedGameObject)
			{
				NewSelectionWhileFocused(currentSelectedGameObject);
			}
		}
		else if (ModioUIInput.IsUsingGamepad)
		{
			DoDefaultSelection();
		}
	}

	public virtual void SetSelectedGameObject(GameObject selection)
	{
		EventSystem.current.SetSelectedGameObject(selection);
	}

	public void OverrideLastSelectedGameObject(GameObject selection)
	{
		_lastSelectedGameObject = selection;
	}

	protected virtual void NewSelectionWhileFocused(GameObject currentSelection)
	{
		_lastSelectedGameObject = currentSelection;
	}

	private void OnSwappedControlScheme(bool isController)
	{
		if (!isController || EventSystem.current.currentSelectedGameObject != null)
		{
			return;
		}
		float num = float.MaxValue;
		Selectable selectable = null;
		Vector3 mousePosition = UnityEngine.Input.mousePosition;
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>();
		foreach (Selectable selectable2 in componentsInChildren)
		{
			if (!selectable2.gameObject.activeInHierarchy || selectable2.navigation.mode == UnityEngine.UI.Navigation.Mode.None || !selectable2.interactable)
			{
				continue;
			}
			RectTransform rectTransform = selectable2.transform as RectTransform;
			if (!(rectTransform == null))
			{
				float sqrMagnitude = (rectTransform.TransformPoint(rectTransform.rect.center) - mousePosition).sqrMagnitude;
				if (!(sqrMagnitude > num))
				{
					selectable = selectable2;
					num = sqrMagnitude;
				}
			}
		}
		if (selectable != null)
		{
			selectable.Select();
		}
	}
}
