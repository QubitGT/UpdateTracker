using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

public static class SelectionPicker
{
	public static Dictionary<ProBuilderMesh, HashSet<int>> PickVerticesInRect(Camera cam, Rect rect, IList<ProBuilderMesh> selectable, PickerOptions options, float pixelsPerPoint = 1f)
	{
		if (options.depthTest)
		{
			return SelectionPickerRenderer.PickVerticesInRect(cam, rect, selectable, doDepthTest: true, (int)((float)cam.pixelWidth / pixelsPerPoint), (int)((float)cam.pixelHeight / pixelsPerPoint));
		}
		Dictionary<ProBuilderMesh, HashSet<int>> dictionary = new Dictionary<ProBuilderMesh, HashSet<int>>();
		foreach (ProBuilderMesh item in selectable)
		{
			if (!item.selectable)
			{
				continue;
			}
			SharedVertex[] sharedVerticesInternal = item.sharedVerticesInternal;
			HashSet<int> hashSet = new HashSet<int>();
			Vector3[] positionsInternal = item.positionsInternal;
			Transform transform = item.transform;
			float num = cam.pixelHeight;
			for (int i = 0; i < sharedVerticesInternal.Length; i++)
			{
				Vector3 position = transform.TransformPoint(positionsInternal[sharedVerticesInternal[i][0]]);
				Vector3 point = cam.WorldToScreenPoint(position);
				if (!(point.z < cam.nearClipPlane))
				{
					point.x /= pixelsPerPoint;
					point.y = (num - point.y) / pixelsPerPoint;
					if (rect.Contains(point))
					{
						hashSet.Add(i);
					}
				}
			}
			dictionary.Add(item, hashSet);
		}
		return dictionary;
	}

	public static Dictionary<ProBuilderMesh, HashSet<Face>> PickFacesInRect(Camera cam, Rect rect, IList<ProBuilderMesh> selectable, PickerOptions options, float pixelsPerPoint = 1f)
	{
		if (options.depthTest && options.rectSelectMode == RectSelectMode.Partial)
		{
			return SelectionPickerRenderer.PickFacesInRect(cam, rect, selectable, (int)((float)cam.pixelWidth / pixelsPerPoint), (int)((float)cam.pixelHeight / pixelsPerPoint));
		}
		Dictionary<ProBuilderMesh, HashSet<Face>> dictionary = new Dictionary<ProBuilderMesh, HashSet<Face>>();
		foreach (ProBuilderMesh item in selectable)
		{
			if (!item.selectable)
			{
				continue;
			}
			HashSet<Face> hashSet = new HashSet<Face>();
			Transform transform = item.transform;
			Vector3[] positionsInternal = item.positionsInternal;
			Vector3[] array = new Vector3[item.vertexCount];
			for (int i = 0; i < item.vertexCount; i++)
			{
				array[i] = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(transform.TransformPoint(positionsInternal[i])), pixelsPerPoint);
			}
			for (int j = 0; j < item.facesInternal.Length; j++)
			{
				Face face = item.facesInternal[j];
				if (options.rectSelectMode == RectSelectMode.Complete)
				{
					if (array[face.indexesInternal[0]].z < cam.nearClipPlane || !rect.Contains(array[face.indexesInternal[0]]))
					{
						continue;
					}
					bool flag = false;
					for (int k = 1; k < face.distinctIndexesInternal.Length; k++)
					{
						int num = face.distinctIndexesInternal[k];
						if (array[num].z < cam.nearClipPlane || !rect.Contains(array[num]))
						{
							flag = true;
							break;
						}
					}
					if (!flag && (!options.depthTest || !HandleUtility.PointIsOccluded(cam, item, transform.TransformPoint(Math.Average(positionsInternal, face.distinctIndexesInternal)))))
					{
						hashSet.Add(face);
					}
					continue;
				}
				Bounds2D bounds2D = new Bounds2D(array, face.edgesInternal);
				bool flag2 = false;
				if (bounds2D.Intersects(rect))
				{
					for (int l = 0; l < face.distinctIndexesInternal.Length; l++)
					{
						if (flag2)
						{
							break;
						}
						Vector3 point = array[face.distinctIndexesInternal[l]];
						flag2 = point.z > cam.nearClipPlane && rect.Contains(point);
					}
					if (!flag2)
					{
						Vector2 vector = new Vector2(rect.xMin, rect.yMax);
						Vector2 vector2 = new Vector2(rect.xMax, rect.yMax);
						Vector2 vector3 = new Vector2(rect.xMin, rect.yMin);
						Vector2 vector4 = new Vector2(rect.xMax, rect.yMin);
						flag2 = Math.PointInPolygon(array, bounds2D, face.edgesInternal, vector);
						if (!flag2)
						{
							flag2 = Math.PointInPolygon(array, bounds2D, face.edgesInternal, vector2);
						}
						if (!flag2)
						{
							flag2 = Math.PointInPolygon(array, bounds2D, face.edgesInternal, vector4);
						}
						if (!flag2)
						{
							flag2 = Math.PointInPolygon(array, bounds2D, face.edgesInternal, vector3);
						}
						for (int m = 0; m < face.edgesInternal.Length; m++)
						{
							if (flag2)
							{
								break;
							}
							if (Math.GetLineSegmentIntersect(vector2, vector, array[face.edgesInternal[m].a], array[face.edgesInternal[m].b]))
							{
								flag2 = true;
							}
							else if (Math.GetLineSegmentIntersect(vector, vector3, array[face.edgesInternal[m].a], array[face.edgesInternal[m].b]))
							{
								flag2 = true;
							}
							else if (Math.GetLineSegmentIntersect(vector3, vector4, array[face.edgesInternal[m].a], array[face.edgesInternal[m].b]))
							{
								flag2 = true;
							}
							else if (Math.GetLineSegmentIntersect(vector4, vector, array[face.edgesInternal[m].a], array[face.edgesInternal[m].b]))
							{
								flag2 = true;
							}
						}
					}
				}
				if (flag2)
				{
					hashSet.Add(face);
				}
			}
			dictionary.Add(item, hashSet);
		}
		return dictionary;
	}

	public static Dictionary<ProBuilderMesh, HashSet<Edge>> PickEdgesInRect(Camera cam, Rect rect, IList<ProBuilderMesh> selectable, PickerOptions options, float pixelsPerPoint = 1f)
	{
		if (options.depthTest && options.rectSelectMode == RectSelectMode.Partial)
		{
			return SelectionPickerRenderer.PickEdgesInRect(cam, rect, selectable, doDepthTest: true, (int)((float)cam.pixelWidth / pixelsPerPoint), (int)((float)cam.pixelHeight / pixelsPerPoint));
		}
		Dictionary<ProBuilderMesh, HashSet<Edge>> dictionary = new Dictionary<ProBuilderMesh, HashSet<Edge>>();
		foreach (ProBuilderMesh item2 in selectable)
		{
			if (!item2.selectable)
			{
				continue;
			}
			Transform transform = item2.transform;
			HashSet<Edge> hashSet = new HashSet<Edge>();
			int i = 0;
			for (int faceCount = item2.faceCount; i < faceCount; i++)
			{
				Edge[] edgesInternal = item2.facesInternal[i].edgesInternal;
				int j = 0;
				for (int num = edgesInternal.Length; j < num; j++)
				{
					Edge item = edgesInternal[j];
					Vector3 vector = transform.TransformPoint(item2.positionsInternal[item.a]);
					Vector3 vector2 = transform.TransformPoint(item2.positionsInternal[item.b]);
					Vector3 vector3 = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(vector), pixelsPerPoint);
					Vector3 vector4 = cam.ScreenToGuiPoint(cam.WorldToScreenPoint(vector2), pixelsPerPoint);
					switch (options.rectSelectMode)
					{
					case RectSelectMode.Complete:
						if (!(vector3.z < cam.nearClipPlane) && !(vector4.z < cam.nearClipPlane) && rect.Contains(vector3) && rect.Contains(vector4) && (!options.depthTest || !HandleUtility.PointIsOccluded(cam, item2, (vector + vector2) * 0.5f)))
						{
							hashSet.Add(item);
						}
						break;
					case RectSelectMode.Partial:
						if (Math.RectIntersectsLineSegment(rect, vector3, vector4))
						{
							hashSet.Add(item);
						}
						break;
					}
				}
			}
			dictionary.Add(item2, hashSet);
		}
		return dictionary;
	}

	public static Face PickFace(Camera camera, Vector3 mousePosition, ProBuilderMesh pickable)
	{
		if (HandleUtility.FaceRaycast(camera.ScreenPointToRay(mousePosition), pickable, out var hit, float.PositiveInfinity, CullingMode.Back))
		{
			return pickable.facesInternal[hit.face];
		}
		return null;
	}
}
