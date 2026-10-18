using System;
using UnityEngine;

namespace Liv.Lck.Tablet;

[Serializable]
public class InitializerNotification
{
	[HideInInspector]
	public string Name;

	public NotificationType Type;

	public GameObject prefab;
}
