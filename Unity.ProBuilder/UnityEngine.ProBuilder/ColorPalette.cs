using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[Serializable]
internal sealed class ColorPalette : ScriptableObject, IHasDefault
{
	[FormerlySerializedAs("colors")]
	[SerializeField]
	private List<Color> m_Colors;

	public Color current { get; set; }

	public ReadOnlyCollection<Color> colors => new ReadOnlyCollection<Color>(m_Colors);

	public Color this[int i]
	{
		get
		{
			return m_Colors[i];
		}
		set
		{
			m_Colors[i] = value;
		}
	}

	public int Count => m_Colors.Count;

	public void SetColors(IEnumerable<Color> colors)
	{
		if (colors == null)
		{
			throw new ArgumentNullException("colors");
		}
		m_Colors = colors.ToList();
	}

	public void SetDefaultValues()
	{
		m_Colors = new List<Color>
		{
			new Color(0f, 0.122f, 0.247f, 1f),
			new Color(0f, 0.455f, 0.851f, 1f),
			new Color(0.498f, 0.859f, 1f, 1f),
			new Color(0.224f, 0.8f, 0.8f, 1f),
			new Color(0.239f, 0.6f, 0.439f, 1f),
			new Color(0.18f, 0.8f, 0.251f, 1f),
			new Color(0.004f, 1f, 0.439f, 1f),
			new Color(1f, 0.863f, 0f, 1f),
			new Color(1f, 0.522f, 0.106f, 1f),
			new Color(1f, 0.255f, 0.212f, 1f),
			new Color(0.522f, 0.078f, 0.294f, 1f),
			new Color(0.941f, 0.071f, 0.745f, 1f),
			new Color(0.694f, 0.051f, 0.788f, 1f),
			new Color(0.067f, 0.067f, 0.067f, 1f),
			new Color(0.667f, 0.667f, 0.667f, 1f),
			new Color(0.867f, 0.867f, 0.867f, 1f)
		};
	}
}
