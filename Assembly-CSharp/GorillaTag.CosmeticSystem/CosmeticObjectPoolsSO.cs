using System.Collections.Generic;
using UnityEngine;

namespace GorillaTag.CosmeticSystem;

[CreateAssetMenu(fileName = "CosmeticObjectPoolsSO", menuName = "Gorilla Tag/CosmeticObjectPoolsSO")]
public class CosmeticObjectPoolsSO : ScriptableObject
{
	[SerializeField]
	public List<SinglePool> pools = new List<SinglePool>();
}
