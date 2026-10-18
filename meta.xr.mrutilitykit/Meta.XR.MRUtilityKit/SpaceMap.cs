using System;
using System.Collections;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
[Feature(Feature.Scene)]
public class SpaceMap : MonoBehaviour
{
	[Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public MRUK.RoomFilter CreateOnStart = MRUK.RoomFilter.CurrentRoomOnly;

	[Tooltip("Texture requirements: Read/Write enabled, RGBA 32 bit format. Texture suggestions: Wrap Mode = Clamped, size small (<128x128)")]
	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public Texture2D TextureMap;

	private Bounds MapBounds;

	private Color[,] Pixels;

	private int PixelDimensions = 128;

	[Tooltip("The gradient of the generated map.")]
	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public Gradient MapGradient = new Gradient();

	[Tooltip("How far inside the room the left end of the Texture Gradient should appear. 0 is at the surface, negative is inside the room.")]
	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public float InnerBorder = -0.5f;

	[Tooltip("How far outside the room the right end of the Texture Gradient should appear. 0 is at the surface, positive is outside the room.")]
	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public float OuterBorder;

	[Tooltip("How much the texture map should extend from the room bounds, in meters. Should ideally be greater than or equal to outerPosition.")]
	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public float MapBorder;

	private const string MATERIAL_PROPERTY_NAME = "_SpaceMap";

	private const string PARAMETER_PROPERTY_NAME = "_SpaceMapParams";

	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public Vector2 Offset => new Vector2(MapBounds.center.x, MapBounds.center.z);

	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public Vector2 Scale => new Vector2(Mathf.Max(MapBounds.size.x, MapBounds.size.z) + MapBorder * 2f, Mathf.Max(MapBounds.size.x, MapBounds.size.z) + MapBorder * 2f);

	private void Start()
	{
		if (!MRUK.Instance || CreateOnStart == MRUK.RoomFilter.None)
		{
			return;
		}
		MRUK.Instance.RegisterSceneLoadedCallback(delegate
		{
			switch (CreateOnStart)
			{
			case MRUK.RoomFilter.AllRooms:
				CalculateMap();
				break;
			case MRUK.RoomFilter.CurrentRoomOnly:
				CalculateMap(MRUK.Instance.GetCurrentRoom());
				break;
			}
		});
	}

	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU's 'StartSpaceMap' method instead.", false)]
	public void CalculateMap(MRUKRoom room = null)
	{
		if (TextureMap == null)
		{
			Debug.LogWarning("No texture specified for Space Map");
			return;
		}
		InitializeMapValues(room);
		StartCoroutine(CalculatePixels(room));
		Shader.SetGlobalTexture("_SpaceMap", TextureMap);
		Shader.SetGlobalVector("_SpaceMapParams", new Vector4(Scale.x, Scale.y, Offset.x, Offset.y));
	}

	private void InitializeMapValues(MRUKRoom room)
	{
		PixelDimensions = TextureMap.width;
		Pixels = new Color[PixelDimensions, PixelDimensions];
		if (room != null)
		{
			MapBounds = room.GetRoomBounds();
		}
		else
		{
			MapBounds = default(Bounds);
			foreach (MRUKRoom room2 in MRUK.Instance.Rooms)
			{
				MapBounds.Encapsulate(room2.GetRoomBounds());
			}
		}
		base.transform.position = new Vector3(MapBounds.center.x, MapBounds.min.y, MapBounds.center.z);
		base.transform.localScale = new Vector3(Scale.x, MapBounds.size.y, Scale.y);
	}

	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public float GetSurfaceDistance(MRUKRoom room, Vector3 worldPosition)
	{
		float num = float.PositiveInfinity;
		float num2 = 1f;
		if (room != null)
		{
			num = room.TryGetClosestSurfacePosition(worldPosition, out var _, out var _, new LabelFilter(~(MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING)));
			num2 = (room.IsPositionInRoom(worldPosition, testVerticalBounds: false) ? 1 : (-1));
		}
		else
		{
			foreach (MRUKRoom room2 in MRUK.Instance.Rooms)
			{
				Vector3 surfacePosition2;
				MRUKAnchor closestAnchor2;
				float num3 = room2.TryGetClosestSurfacePosition(worldPosition, out surfacePosition2, out closestAnchor2, new LabelFilter(~(MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING)));
				if (num3 < num)
				{
					num = num3;
					num2 = (room2.IsPositionInRoom(worldPosition) ? 1 : (-1));
				}
			}
		}
		return num * num2;
	}

	private IEnumerator CalculatePixels(MRUKRoom room)
	{
		float num = 0.5f / (float)PixelDimensions;
		float num2 = Mathf.Max(MapBounds.size.x, MapBounds.size.z) + MapBorder * 2f;
		for (int i = 0; i < PixelDimensions; i++)
		{
			for (int j = 0; j < PixelDimensions; j++)
			{
				float num3 = (float)i / (float)PixelDimensions - 0.5f + num;
				float num4 = (float)j / (float)PixelDimensions - 0.5f + num;
				Vector3 worldPosition = new Vector3(num3 * num2 + MapBounds.center.x, 0f, num4 * num2 + MapBounds.center.z);
				float time = Mathf.Clamp01((0f - GetSurfaceDistance(room, worldPosition) - InnerBorder) / (OuterBorder - InnerBorder));
				Color color = MapGradient.Evaluate(time);
				Pixels[i, j] = color;
				TextureMap.SetPixel(i, j, color);
			}
		}
		TextureMap.Apply();
		yield return null;
	}

	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU instead.", false)]
	public void ResetFreespace()
	{
		for (int i = 0; i < PixelDimensions; i++)
		{
			for (int j = 0; j < PixelDimensions; j++)
			{
				Pixels[i, j] = Color.black;
			}
		}
	}

	[Obsolete("SpaceMap is deprecated. Please use SpaceMapGPU's 'GetColorAtPosition' method instead.", false)]
	public Color GetColorAtPosition(Vector3 worldPosition, bool getBilinear = true)
	{
		if (getBilinear)
		{
			Vector2 pixelFromWorldPosition = GetPixelFromWorldPosition(worldPosition, normalizedUV: true);
			return TextureMap.GetPixelBilinear(pixelFromWorldPosition.x, pixelFromWorldPosition.y);
		}
		Vector2 pixelFromWorldPosition2 = GetPixelFromWorldPosition(worldPosition);
		int x = Mathf.FloorToInt(pixelFromWorldPosition2.x);
		int y = Mathf.FloorToInt(pixelFromWorldPosition2.y);
		return TextureMap.GetPixel(x, y);
	}

	private Vector2 GetPixelFromWorldPosition(Vector3 worldPosition, bool normalizedUV = false)
	{
		Vector3 vector = worldPosition - MapBounds.center;
		Vector2 vector2 = new Vector2(vector.x / MapBounds.size.x + 0.5f, vector.z / MapBounds.size.z + 0.5f);
		if (!normalizedUV)
		{
			return vector2 * PixelDimensions;
		}
		return vector2;
	}
}
