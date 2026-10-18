using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Samples.PalmMenu;

public class DominantHandGameObjectFilter : MonoBehaviour, IGameObjectFilter
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _leftHand;

	[SerializeField]
	private GameObject[] _leftHandedGameObjects;

	[SerializeField]
	private GameObject[] _rightHandedGameObjects;

	private readonly HashSet<GameObject> _leftHandedGameObjectSet = new HashSet<GameObject>();

	private readonly HashSet<GameObject> _rightHandedGameObjectSet = new HashSet<GameObject>();

	private IHand LeftHand { get; set; }

	protected virtual void Start()
	{
		GameObject[] leftHandedGameObjects = _leftHandedGameObjects;
		foreach (GameObject item in leftHandedGameObjects)
		{
			_leftHandedGameObjectSet.Add(item);
		}
		leftHandedGameObjects = _rightHandedGameObjects;
		foreach (GameObject item2 in leftHandedGameObjects)
		{
			_rightHandedGameObjectSet.Add(item2);
		}
		LeftHand = _leftHand as IHand;
	}

	public bool Filter(GameObject go)
	{
		if (LeftHand.IsDominantHand)
		{
			return _leftHandedGameObjectSet.Contains(go);
		}
		return _rightHandedGameObjectSet.Contains(go);
	}
}
