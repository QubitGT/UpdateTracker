using TMPro;
using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Randomize Label")]
public class RandomizeLabel : MonoBehaviour
{
	public TMP_Text label;

	public RandomStrings strings;

	public bool distinct;

	public void Randomize()
	{
		strings.distinct = distinct;
		label.text = strings.NextItem();
	}
}
