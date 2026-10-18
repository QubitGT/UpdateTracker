using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Navigation;

public class ModioGridNavigation : Selectable, ILayoutController
{
	private static readonly Queue<Selectable> PrevRow = new Queue<Selectable>();

	[SerializeField]
	private bool _getSelectablesInChildrensChildren;

	[SerializeField]
	private GameObject _fallbackSelectionToIfNoValidChildren;

	private bool _selectChildImmediately;

	private bool _needsDelayedNavigationCorrection;

	private GameObject _lastSelectedGameObject;

	private static readonly List<Selectable> ReusedSelectables = new List<Selectable>();

	private static readonly Vector3[] PrevCorners = new Vector3[4];

	private static readonly Vector3[] TransCorners = new Vector3[4];

	protected override void OnEnable()
	{
		base.OnEnable();
		_lastSelectedGameObject = null;
		LayoutRebuilder.MarkLayoutForRebuild((RectTransform)base.transform);
		_needsDelayedNavigationCorrection = true;
	}

	public void SetLayoutHorizontal()
	{
	}

	public void SetLayoutVertical()
	{
		_needsDelayedNavigationCorrection = true;
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (_needsDelayedNavigationCorrection)
		{
			RecalculateNavigation();
			_needsDelayedNavigationCorrection = false;
		}
		if (_selectChildImmediately && EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			_selectChildImmediately = false;
		}
		if (_selectChildImmediately)
		{
			_selectChildImmediately = false;
			Vector3 vector = new Vector3(-10000f, 10000f, 0f);
			if (_lastSelectedGameObject != null)
			{
				vector = _lastSelectedGameObject.transform.position;
			}
			float num = float.MaxValue;
			Selectable selectable = null;
			foreach (Transform item in base.transform)
			{
				if (!item.gameObject.activeSelf)
				{
					continue;
				}
				if (_getSelectablesInChildrensChildren)
				{
					item.GetComponentsInChildren(ReusedSelectables);
				}
				else
				{
					ReusedSelectables.Clear();
					Selectable component = item.GetComponent<Selectable>();
					if (component != null)
					{
						ReusedSelectables.Add(component);
					}
				}
				foreach (Selectable reusedSelectable in ReusedSelectables)
				{
					if (reusedSelectable == this || !reusedSelectable.interactable || reusedSelectable.navigation.mode == UnityEngine.UI.Navigation.Mode.None)
					{
						continue;
					}
					RectTransform rectTransform = reusedSelectable.transform as RectTransform;
					float num2 = float.MaxValue;
					if (rectTransform != null)
					{
						rectTransform.GetWorldCorners(TransCorners);
						Vector3[] transCorners = TransCorners;
						for (int i = 0; i < transCorners.Length; i++)
						{
							float sqrMagnitude = (transCorners[i] - vector).sqrMagnitude;
							num2 = Mathf.Min(num2, sqrMagnitude);
						}
					}
					else
					{
						num2 = (reusedSelectable.transform.position - vector).sqrMagnitude;
					}
					if (!(num2 > num))
					{
						num = num2;
						selectable = reusedSelectable;
					}
				}
			}
			if (selectable != null)
			{
				EventSystem.current.SetSelectedGameObject(selectable.gameObject);
			}
			else if (_fallbackSelectionToIfNoValidChildren != null)
			{
				EventSystem.current.SetSelectedGameObject(_fallbackSelectionToIfNoValidChildren);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(_lastSelectedGameObject);
			}
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject != null && currentSelectedGameObject.activeInHierarchy)
		{
			_lastSelectedGameObject = currentSelectedGameObject;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		_selectChildImmediately = true;
	}

	private void RecalculateNavigation()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		bool flag = true;
		int num = 0;
		PrevRow.Clear();
		foreach (Transform item in base.transform)
		{
			if (!item.gameObject.activeSelf)
			{
				continue;
			}
			if (_getSelectablesInChildrensChildren)
			{
				item.GetComponentsInChildren(ReusedSelectables);
			}
			else
			{
				ReusedSelectables.Clear();
				Selectable component = item.GetComponent<Selectable>();
				if (component != null)
				{
					ReusedSelectables.Add(component);
				}
			}
			foreach (Selectable reusedSelectable in ReusedSelectables)
			{
				if (reusedSelectable == this || !reusedSelectable.interactable || reusedSelectable.navigation.mode == UnityEngine.UI.Navigation.Mode.None)
				{
					continue;
				}
				UnityEngine.UI.Navigation navigation = reusedSelectable.navigation;
				navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
				bool flag2 = false;
				if (selectable != null)
				{
					flag2 = ((!(selectable.transform is RectTransform prevRectTransform) || !(reusedSelectable.transform is RectTransform rectTransform)) ? (selectable.transform.position.x + 1f < reusedSelectable.transform.position.x) : IsToTheRight(prevRectTransform, rectTransform));
					flag = flag && flag2;
				}
				if (flag2)
				{
					num++;
					navigation.selectOnLeft = selectable;
					if (selectable != null)
					{
						UnityEngine.UI.Navigation navigation2 = selectable.navigation;
						navigation2.selectOnRight = reusedSelectable;
						selectable.navigation = navigation2;
					}
				}
				else
				{
					while (PrevRow.Count > num && PrevRow.Count > 0)
					{
						Selectable selectable3 = PrevRow.Dequeue();
						UnityEngine.UI.Navigation navigation3 = selectable3.navigation;
						navigation3.selectOnDown = selectable;
						selectable3.navigation = navigation3;
					}
					num = 1;
					navigation.selectOnLeft = GetNeighbourInDir(MoveDirection.Left);
					if (selectable != null)
					{
						UnityEngine.UI.Navigation navigation4 = selectable.navigation;
						navigation4.selectOnRight = GetNeighbourInDir(MoveDirection.Right);
						selectable.navigation = navigation4;
					}
					selectable2 = selectable;
				}
				if (flag)
				{
					navigation.selectOnUp = GetNeighbourInDir(MoveDirection.Up);
				}
				else
				{
					Selectable selectable4 = selectable2;
					if (PrevRow.Count >= num)
					{
						selectable4 = PrevRow.Dequeue();
						UnityEngine.UI.Navigation navigation5 = selectable4.navigation;
						navigation5.selectOnDown = reusedSelectable;
						selectable4.navigation = navigation5;
					}
					navigation.selectOnUp = selectable4;
				}
				reusedSelectable.navigation = navigation;
				selectable = reusedSelectable;
				PrevRow.Enqueue(selectable);
			}
		}
		int num2 = PrevRow.Count - num;
		foreach (Selectable item2 in PrevRow)
		{
			UnityEngine.UI.Navigation navigation6 = item2.navigation;
			if (num2-- > 0)
			{
				navigation6.selectOnDown = selectable;
			}
			else
			{
				navigation6.selectOnDown = GetNeighbourInDir(MoveDirection.Down);
			}
			item2.navigation = navigation6;
		}
		if (selectable != null)
		{
			UnityEngine.UI.Navigation navigation7 = selectable.navigation;
			navigation7.selectOnRight = GetNeighbourInDir(MoveDirection.Right);
			selectable.navigation = navigation7;
		}
	}

	public void NeedsNavigationCorrection()
	{
		_needsDelayedNavigationCorrection = true;
	}

	private static bool IsToTheRight(RectTransform prevRectTransform, RectTransform rectTransform)
	{
		prevRectTransform.GetWorldCorners(PrevCorners);
		rectTransform.GetWorldCorners(TransCorners);
		Vector3 rhs = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Vector3[] prevCorners = PrevCorners;
		for (int i = 0; i < prevCorners.Length; i++)
		{
			rhs = Vector3.Max(prevCorners[i], rhs);
		}
		prevCorners = TransCorners;
		for (int i = 0; i < prevCorners.Length; i++)
		{
			if (prevCorners[i].x < rhs.x)
			{
				return false;
			}
		}
		return true;
	}

	private Selectable GetNeighbourInDir(MoveDirection moveDirection)
	{
		Selectable selectable = this;
		int num = 0;
		do
		{
			selectable = moveDirection switch
			{
				MoveDirection.Left => selectable.navigation.selectOnLeft, 
				MoveDirection.Up => selectable.navigation.selectOnUp, 
				MoveDirection.Right => selectable.navigation.selectOnRight, 
				MoveDirection.Down => selectable.navigation.selectOnDown, 
				_ => throw new ArgumentOutOfRangeException("moveDirection", moveDirection, null), 
			};
		}
		while (selectable != null && !selectable.isActiveAndEnabled && num++ < 10);
		if (selectable == this)
		{
			return null;
		}
		return selectable;
	}
}
