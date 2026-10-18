using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class TagSet : MonoBehaviour
{
	[Tooltip("The tags that should apply to this GameObject.")]
	[SerializeField]
	private List<string> _tags;

	private readonly HashSet<string> _tagSet = new HashSet<string>();

	protected virtual void Start()
	{
		foreach (string tag in _tags)
		{
			_tagSet.Add(tag);
		}
	}

	public bool ContainsTag(string tag)
	{
		return _tagSet.Contains(tag);
	}

	public void AddTag(string tag)
	{
		_tagSet.Add(tag);
	}

	public void RemoveTag(string tag)
	{
		_tagSet.Remove(tag);
	}

	public void InjectOptionalTags(List<string> tags)
	{
		_tags = tags;
	}
}
