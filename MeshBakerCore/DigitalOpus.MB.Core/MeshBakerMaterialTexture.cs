using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MeshBakerMaterialTexture
{
	private Texture2D _t;

	public float texelDensity;

	internal static bool readyToBuildAtlases;

	private DRect encapsulatingSamplingRect;

	public Texture2D t
	{
		set
		{
			_t = value;
		}
	}

	public DRect matTilingRect { get; private set; }

	public int isImportedAsNormalMap { get; private set; }

	public bool isNull => _t == null;

	public int width
	{
		get
		{
			if (_t != null)
			{
				return _t.width;
			}
			return 16;
		}
	}

	public int height
	{
		get
		{
			if (_t != null)
			{
				return _t.height;
			}
			return 16;
		}
	}

	public MeshBakerMaterialTexture(Texture tx, Vector2 matTilingOffset, Vector2 matTilingScale, float texelDens, int isImportedAsNormalMap)
	{
		if (tx is Texture2D)
		{
			_t = (Texture2D)tx;
		}
		else if (!(tx == null))
		{
			Debug.LogError("An error occured. Texture must be Texture2D " + tx);
		}
		matTilingRect = new DRect(matTilingOffset, matTilingScale);
		texelDensity = texelDens;
		this.isImportedAsNormalMap = isImportedAsNormalMap;
	}

	public DRect GetEncapsulatingSamplingRect()
	{
		return encapsulatingSamplingRect;
	}

	public void SetEncapsulatingSamplingRect(MB_TexSet ts, DRect r)
	{
		encapsulatingSamplingRect = r;
	}

	public Texture2D GetTexture2D()
	{
		if (!readyToBuildAtlases)
		{
			Debug.LogError("This function should not be called before Step3. For steps 1 and 2 should always call methods like isNull, width, height");
			throw new Exception("GetTexture2D called before ready to build atlases");
		}
		return _t;
	}

	public string GetTexName()
	{
		if (_t != null)
		{
			return _t.name;
		}
		return "null";
	}

	public bool AreTexturesEqual(MeshBakerMaterialTexture b)
	{
		if (_t == b._t)
		{
			return true;
		}
		return false;
	}
}
