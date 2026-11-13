using System;
using UnityEngine;

[AddComponentMenu("Gorilla Tag/Experience Selector")]
public class ExperienceSelector : MonoBehaviour
{
	[Serializable]
	public class Experience
	{
		public string name;

		[Tooltip("Enabled when this experience is selected, disabled when any other is")]
		public GameObject[] objects = Array.Empty<GameObject>();
	}

	[SerializeField]
	private int selected;

	[SerializeField]
	private Experience[] experiences = Array.Empty<Experience>();

	public int Selected => selected;
}
