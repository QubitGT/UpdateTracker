using UnityEngine;

namespace Liv.Lck.UI;

[CreateAssetMenu(fileName = "LckButtonColors", menuName = "LIV/LCK/LCK Button Colors", order = 0)]
public class LckButtonColors : ScriptableObject
{
	public Color NormalColor;

	public Color HighlightedColor;

	public Color PressedColor;

	public Color SelectedColor;

	public Color DisabledColor;
}
