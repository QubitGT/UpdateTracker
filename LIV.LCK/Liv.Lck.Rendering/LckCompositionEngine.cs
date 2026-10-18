using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Liv.Lck.Rendering;

public class LckCompositionEngine : MonoBehaviour
{
	[SerializeField]
	private LckCompositionProfile _compositionProfile;

	[Tooltip("The material to use for blending if a layer does not define its own.")]
	[SerializeField]
	public Material DefaultBlendMaterial;

	public static LckCompositionEngine Instance { get; private set; }

	public bool HasActiveLayers { get; private set; }

	public List<ILckCompositionLayer> ActiveLayers { get; private set; } = new List<ILckCompositionLayer>();


	public bool IsDirty { get; set; } = true;


	private void OnEnable()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		SetDirty();
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void SetDirty()
	{
		IsDirty = true;
		HasActiveLayers = (_compositionProfile?.Layers?.Any((LckCompositionLayer layer) => layer.IsActive && layer.CurrentTexture != null)).GetValueOrDefault();
	}

	public void UpdateActiveLayers()
	{
		ActiveLayers.Clear();
		if (_compositionProfile?.Layers != null)
		{
			foreach (LckCompositionLayer layer in _compositionProfile.Layers)
			{
				if (layer != null && layer.IsActive && layer.CurrentTexture != null)
				{
					ActiveLayers.Add(layer);
				}
			}
		}
		HasActiveLayers = ActiveLayers.Count > 0;
	}
}
