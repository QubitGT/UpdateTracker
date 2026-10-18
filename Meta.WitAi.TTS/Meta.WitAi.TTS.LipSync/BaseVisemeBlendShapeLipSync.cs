using System;
using System.Collections.Generic;
using System.Text;
using Meta.WitAi.Attributes;
using Meta.WitAi.TTS.Data;
using UnityEngine;

namespace Meta.WitAi.TTS.LipSync;

public abstract class BaseVisemeBlendShapeLipSync : MonoBehaviour, ILipsyncAnimator
{
	[Serializable]
	public struct VisemeBlendShapeData
	{
		public Viseme viseme;

		public VisemeBlendShapeWeight[] weights;
	}

	[Serializable]
	public struct VisemeBlendShapeWeight
	{
		[DropDown("GetBlendShapeNames", true, false, true, true, null, true)]
		public string blendShapeId;

		public float weight;
	}

	public float blendShapeWeightScale = 1f;

	public VisemeBlendShapeData[] VisemeBlendShapes;

	private Dictionary<Viseme, int> _visemeLookup = new Dictionary<Viseme, int>();

	private Dictionary<string, int> _blendShapeLookup = new Dictionary<string, int>();

	private List<string> _blendShapeNames = new List<string>();

	public abstract SkinnedMeshRenderer SkinnedMeshRenderer { get; }

	protected virtual void Reset()
	{
		if (VisemeBlendShapes != null && VisemeBlendShapes.Length != 0)
		{
			return;
		}
		List<VisemeBlendShapeData> list = new List<VisemeBlendShapeData>();
		foreach (Viseme value in Enum.GetValues(typeof(Viseme)))
		{
			list.Add(new VisemeBlendShapeData
			{
				viseme = value
			});
		}
		VisemeBlendShapes = list.ToArray();
	}

	protected virtual void Awake()
	{
		RefreshBlendShapeLookup();
	}

	public void RefreshBlendShapeLookup()
	{
		if (VisemeBlendShapes == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		_visemeLookup.Clear();
		_blendShapeLookup.Clear();
		for (int i = 0; i < VisemeBlendShapes.Length; i++)
		{
			Viseme viseme = VisemeBlendShapes[i].viseme;
			if (_visemeLookup.ContainsKey(viseme))
			{
				stringBuilder.AppendLine($"{viseme} Viseme already set (VisemeBlendShapes[{i}] ignored)");
				continue;
			}
			_visemeLookup[viseme] = i;
			VisemeBlendShapeWeight[] weights = VisemeBlendShapes[i].weights;
			for (int j = 0; j < weights.Length; j++)
			{
				VisemeBlendShapeWeight visemeBlendShapeWeight = weights[j];
				if (!string.IsNullOrEmpty(visemeBlendShapeWeight.blendShapeId) && !_blendShapeLookup.ContainsKey(visemeBlendShapeWeight.blendShapeId))
				{
					_blendShapeLookup[visemeBlendShapeWeight.blendShapeId] = -1;
				}
			}
		}
		foreach (Viseme value in Enum.GetValues(typeof(Viseme)))
		{
			if (!_visemeLookup.ContainsKey(value))
			{
				stringBuilder.AppendLine($"{value} Viseme missing texture");
			}
		}
		GetBlendShapeNames();
		for (int k = 0; k < _blendShapeNames.Count; k++)
		{
			if (_blendShapeLookup.ContainsKey(_blendShapeNames[k]))
			{
				_blendShapeLookup[_blendShapeNames[k]] = k;
			}
		}
		if (stringBuilder.Length > 0)
		{
			VLog.E(GetType().Name, $"Setup Warnings:\n{stringBuilder}");
		}
	}

	public void OnVisemeStarted(Viseme viseme)
	{
	}

	public void OnVisemeFinished(Viseme viseme)
	{
	}

	public virtual void OnVisemeLerp(Viseme fromEvent, Viseme toEvent, float percentage)
	{
		if (SkinnedMeshRenderer == null)
		{
			VLog.E(GetType().Name, "Skinned Mesh Renderer unassigned");
		}
		if (_blendShapeLookup == null)
		{
			return;
		}
		foreach (string key in _blendShapeLookup.Keys)
		{
			int num = _blendShapeLookup[key];
			if (num != -1)
			{
				float num2;
				if (percentage >= 1f)
				{
					num2 = GetBlendShapeWeight(fromEvent, key);
				}
				else if (percentage <= 0f)
				{
					num2 = GetBlendShapeWeight(fromEvent, key);
				}
				else
				{
					float blendShapeWeight = GetBlendShapeWeight(fromEvent, key);
					float blendShapeWeight2 = GetBlendShapeWeight(toEvent, key);
					num2 = Mathf.Lerp(blendShapeWeight, blendShapeWeight2, percentage);
				}
				SkinnedMeshRenderer.SetBlendShapeWeight(num, num2 * blendShapeWeightScale);
			}
		}
	}

	public float GetBlendShapeWeight(Viseme viseme, string blendShapeName)
	{
		if (_visemeLookup.TryGetValue(viseme, out var value) && value >= 0 && value < VisemeBlendShapes.Length)
		{
			VisemeBlendShapeData visemeBlendShapeData = VisemeBlendShapes[value];
			for (int i = 0; i < visemeBlendShapeData.weights.Length; i++)
			{
				if (string.Equals(visemeBlendShapeData.weights[i].blendShapeId, blendShapeName))
				{
					return visemeBlendShapeData.weights[i].weight;
				}
			}
		}
		return 0f;
	}

	public string[] GetBlendShapeNames()
	{
		if (_blendShapeNames == null)
		{
			_blendShapeNames = new List<string>();
		}
		if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh != null && _blendShapeNames.Count != SkinnedMeshRenderer.sharedMesh.blendShapeCount)
		{
			_blendShapeNames.Clear();
			for (int i = 0; i < SkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
			{
				_blendShapeNames.Add(SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i));
			}
		}
		return _blendShapeNames.ToArray();
	}
}
