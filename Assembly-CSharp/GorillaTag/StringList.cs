using UnityEngine;

namespace GorillaTag;

[CreateAssetMenu(fileName = "New String List", menuName = "ScriptableObjects/Data/String List")]
public class StringList : ScriptableObject
{
	[SerializeField]
	private string[] strings;

	public string[] Strings => strings;
}
