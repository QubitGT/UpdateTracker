using System;
using System.Collections.Generic;
using System.Text;
using Meta.Voice.Logging;
using Meta.WitAi.TTS.Data;
using UnityEngine;

namespace Meta.WitAi.TTS.LipSync;

public abstract class BaseTextureFlipLipSync : MonoBehaviour, ILipsyncAnimator
{
	[Serializable]
	public struct VisemeTextureData
	{
		public Viseme viseme;

		public Texture2D[] textures;
	}

	private readonly IVLogger _log = LoggerRegistry.Instance.GetLogger(LogCategory.TextToSpeech);

	[Header("Texture Settings")]
	public VisemeTextureData[] VisemeTextures;

	private Dictionary<Viseme, int> _textureLookup = new Dictionary<Viseme, int>();

	public abstract Renderer Renderer { get; }

	protected virtual void Reset()
	{
		if (VisemeTextures != null && VisemeTextures.Length != 0)
		{
			return;
		}
		List<VisemeTextureData> list = new List<VisemeTextureData>();
		foreach (Viseme value in Enum.GetValues(typeof(Viseme)))
		{
			list.Add(new VisemeTextureData
			{
				viseme = value
			});
		}
		VisemeTextures = list.ToArray();
	}

	protected virtual void Awake()
	{
		RefreshTextureLookup();
	}

	private void Start()
	{
		if (!Renderer)
		{
			_log.Warning("Texture Flip material unassigned on {0}", base.name);
		}
		SetViseme(Viseme.sil);
	}

	public void RefreshTextureLookup()
	{
		if (VisemeTextures == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		_textureLookup.Clear();
		for (int i = 0; i < VisemeTextures.Length; i++)
		{
			Viseme viseme = VisemeTextures[i].viseme;
			if (VisemeTextures[i].textures == null || VisemeTextures[i].textures.Length == 0)
			{
				stringBuilder.AppendLine($"VisemeTextures[{i}] Warning: No textures are set.");
			}
			else if (_textureLookup.ContainsKey(viseme))
			{
				stringBuilder.AppendLine($"VisemeTextures[{i}] Warning: Viseme '{viseme}' already used by VisemeTextures[{_textureLookup[viseme]}].");
			}
			else
			{
				_textureLookup[viseme] = i;
			}
		}
		CheckForMissingVisemes(stringBuilder);
		if (stringBuilder.Length > 0)
		{
			VLog.E(GetType().Name, $"Setup Warnings:\n{stringBuilder}");
		}
	}

	private void CheckForMissingVisemes(StringBuilder log)
	{
		foreach (Viseme value in Enum.GetValues(typeof(Viseme)))
		{
			if (!_textureLookup.ContainsKey(value))
			{
				log.AppendLine($"{value} Viseme missing texture");
			}
		}
	}

	private void SetViseme(Viseme v)
	{
		if (!_textureLookup.ContainsKey(v))
		{
			if (v != 0)
			{
				SetViseme(Viseme.sil);
			}
			return;
		}
		int num = _textureLookup[v];
		int num2 = 0;
		VisemeTextureData visemeTextureData = VisemeTextures[num];
		if (visemeTextureData.textures.Length > 1)
		{
			num2 = UnityEngine.Random.Range(0, visemeTextureData.textures.Length);
		}
		SetTexture(visemeTextureData.textures[num2]);
	}

	protected virtual void SetTexture(Texture2D texture)
	{
		if ((bool)Renderer)
		{
			Renderer.material.SetTexture("_MainTex", texture);
		}
	}

	public void OnVisemeStarted(Viseme viseme)
	{
		SetViseme(viseme);
	}

	public void OnVisemeFinished(Viseme viseme)
	{
	}

	public void OnVisemeLerp(Viseme oldVieseme, Viseme newViseme, float percentage)
	{
	}
}
