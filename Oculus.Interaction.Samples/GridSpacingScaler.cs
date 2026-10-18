using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
public class GridSpacingScaler : MonoBehaviour
{
	public enum Axis
	{
		Horizontal,
		Vertical
	}

	public Axis scaleAxis;

	public Vector2 minSpacing;

	private GridLayoutGroup _gridLayoutGroup;

	private RectTransform _rectTransform;

	private void Start()
	{
		_rectTransform = base.transform as RectTransform;
		if (_rectTransform == null)
		{
			Debug.LogError("GameObject Transform is not a Rect Transform");
		}
		_gridLayoutGroup = base.gameObject.GetComponent<GridLayoutGroup>();
		if (_gridLayoutGroup == null)
		{
			Debug.LogError("GameObject does not include a GridLayoutGroup");
		}
	}

	private void LateUpdate()
	{
		float num = Mathf.Floor(_rectTransform.rect.size[(int)scaleAxis]);
		float num2 = minSpacing[(int)scaleAxis];
		float num3 = _gridLayoutGroup.cellSize[(int)scaleAxis];
		int num4 = ((scaleAxis == Axis.Horizontal) ? _gridLayoutGroup.padding.horizontal : _gridLayoutGroup.padding.vertical);
		if (!(num3 + num2 <= 0f))
		{
			int num5 = Mathf.Max(1, Mathf.FloorToInt((num - (float)num4 + num2 + 0.001f) / (num3 + num2)));
			float num6 = num - (float)num5 * num3;
			_gridLayoutGroup.constraint = ((scaleAxis == Axis.Horizontal) ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount);
			_gridLayoutGroup.constraintCount = num5;
			if (num5 > 1)
			{
				float f = num6 / (float)(num5 - 1);
				Vector2 spacing = _gridLayoutGroup.spacing;
				spacing[(int)scaleAxis] = Mathf.Floor(f);
				_gridLayoutGroup.spacing = spacing;
			}
		}
	}
}
