using UnityEngine;

namespace GorillaTag;

[CreateAssetMenu(fileName = "WatchableIntSO", menuName = "ScriptableObjects/Data/WatchableIntSO")]
public class WatchableIntSO : WatchableGenericSO<int>
{
	private int currentValue => base.Value;
}
