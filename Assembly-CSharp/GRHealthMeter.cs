using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Health Meter")]
public class GRHealthMeter : MonoBehaviour
{
	public List<GRHealthMeterNode> nodes;

	private int maxHP;

	public void Setup(int maxHP)
	{
		this.maxHP = maxHP;
	}

	public void SetHP(int hp)
	{
		int num = Mathf.CeilToInt((float)hp / (float)maxHP * (float)nodes.Count);
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].SetEmpty(i >= num);
		}
	}
}
