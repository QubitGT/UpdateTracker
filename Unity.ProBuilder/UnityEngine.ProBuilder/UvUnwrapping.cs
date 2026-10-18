using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

internal static class UvUnwrapping
{
	internal struct UVTransform
	{
		public Vector2 translation;

		public float rotation;

		public Vector2 scale;

		public override string ToString()
		{
			return translation.ToString("F2") + ", " + rotation + ", " + scale.ToString("F2");
		}
	}

	private static List<Vector2> s_UVTransformProjectionBuffer = new List<Vector2>(8);

	private static Vector2 s_TempVector2 = Vector2.zero;

	private static readonly List<int> s_IndexBuffer = new List<int>(64);

	internal static void SetAutoUV(ProBuilderMesh mesh, Face[] faces, bool auto)
	{
		if (auto)
		{
			SetAutoAndAlignUnwrapParamsToUVs(mesh, faces.Where((Face x) => x.manualUV));
			return;
		}
		foreach (Face obj in faces)
		{
			obj.textureGroup = -1;
			obj.manualUV = true;
		}
	}

	internal static void SetAutoAndAlignUnwrapParamsToUVs(ProBuilderMesh mesh, IEnumerable<Face> facesToConvert)
	{
		Vector2[] dst = mesh.textures.ToArray();
		Face[] array = (facesToConvert as Face[]) ?? facesToConvert.ToArray();
		Face[] array2 = array;
		foreach (Face obj in array2)
		{
			obj.uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
			obj.elementGroup = -1;
			obj.textureGroup = -1;
			obj.manualUV = false;
		}
		mesh.RefreshUV(array);
		Vector2[] texturesInternal = mesh.texturesInternal;
		array2 = array;
		foreach (Face face in array2)
		{
			UVTransform uVTransform = CalculateDelta(texturesInternal, face.indexesInternal, dst, face.indexesInternal);
			AutoUnwrapSettings uv = face.uv;
			uv.offset = -uVTransform.translation;
			uv.rotation = uVTransform.rotation;
			uv.scale = uVTransform.scale;
			face.uv = uv;
		}
		mesh.RefreshUV(array);
	}

	internal static AutoUnwrapSettings GetAutoUnwrapSettings(ProBuilderMesh mesh, Face face)
	{
		if (!face.manualUV)
		{
			return new AutoUnwrapSettings(face.uv);
		}
		UVTransform uVTransform = GetUVTransform(mesh, face);
		AutoUnwrapSettings defaultAutoUnwrapSettings = AutoUnwrapSettings.defaultAutoUnwrapSettings;
		defaultAutoUnwrapSettings.offset = uVTransform.translation;
		defaultAutoUnwrapSettings.rotation = 360f - uVTransform.rotation;
		defaultAutoUnwrapSettings.scale /= uVTransform.scale;
		return defaultAutoUnwrapSettings;
	}

	internal static UVTransform GetUVTransform(ProBuilderMesh mesh, Face face)
	{
		Projection.PlanarProject(mesh.positionsInternal, face.indexesInternal, Math.Normal(mesh, face), s_UVTransformProjectionBuffer);
		return CalculateDelta(mesh.texturesInternal, face.indexesInternal, s_UVTransformProjectionBuffer, null);
	}

	private static int GetIndex(IList<int> collection, int index)
	{
		return collection?[index] ?? index;
	}

	internal static UVTransform CalculateDelta(IList<Vector2> src, IList<int> srcIndices, IList<Vector2> dst, IList<int> dstIndices)
	{
		Vector2 vector = src[GetIndex(srcIndices, 1)] - src[GetIndex(srcIndices, 0)];
		Vector2 vector2 = dst[GetIndex(dstIndices, 1)] - dst[GetIndex(dstIndices, 0)];
		float num = Vector2.Angle(vector2, vector);
		if (Vector2.Dot(Vector2.Perpendicular(vector2), vector) < 0f)
		{
			num = 360f - num;
		}
		Vector2 vector3 = ((dstIndices == null) ? Bounds2D.Center(dst) : Bounds2D.Center(dst, dstIndices));
		Vector2 rotatedSize = GetRotatedSize(dst, dstIndices, vector3, 0f - num);
		Bounds2D bounds2D = ((srcIndices == null) ? new Bounds2D(src) : new Bounds2D(src, srcIndices));
		Vector2 vector4 = rotatedSize.DivideBy(bounds2D.size);
		Vector2 vector5 = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector6 = new Vector2(float.MinValue, float.MinValue);
		int num2 = srcIndices?.Count ?? src.Count;
		for (int i = 0; i < num2; i++)
		{
			int index = GetIndex(srcIndices, i);
			Vector2 vector7 = src[index].RotateAroundPoint(bounds2D.center, num);
			vector5.x = Mathf.Min(vector5.x, vector7.x);
			vector5.y = Mathf.Min(vector5.y, vector7.y);
			vector6.x = Mathf.Max(vector6.x, vector7.x);
			vector6.y = Mathf.Max(vector6.y, vector7.y);
		}
		Vector2 vector8 = (vector5 + vector6) * 0.5f * vector4;
		UVTransform result = default(UVTransform);
		result.translation = vector3 - vector8;
		result.rotation = num;
		result.scale = rotatedSize.DivideBy(bounds2D.size);
		return result;
	}

	private static Vector2 GetRotatedSize(IList<Vector2> points, IList<int> indices, Vector2 center, float rotation)
	{
		int num = indices?.Count ?? points.Count;
		Vector2 vector = points[GetIndex(indices, 0)].RotateAroundPoint(center, rotation);
		float num2 = vector.x;
		float num3 = vector.y;
		float num4 = num2;
		float num5 = num3;
		for (int i = 1; i < num; i++)
		{
			Vector2 vector2 = points[GetIndex(indices, i)].RotateAroundPoint(center, rotation);
			float x = vector2.x;
			float y = vector2.y;
			if (x < num2)
			{
				num2 = x;
			}
			if (x > num4)
			{
				num4 = x;
			}
			if (y < num3)
			{
				num3 = y;
			}
			if (y > num5)
			{
				num5 = y;
			}
		}
		return new Vector2(num4 - num2, num5 - num3);
	}

	internal static void Unwrap(ProBuilderMesh mesh, Face face, Vector3 projection = default(Vector3))
	{
		Projection.PlanarProject(mesh, face, (projection != Vector3.zero) ? projection : Vector3.zero);
		ApplyUVSettings(mesh.texturesInternal, face.distinctIndexesInternal, face.uv);
	}

	internal static void CopyUVs(ProBuilderMesh mesh, Face source, Face dest)
	{
		Vector2[] texturesInternal = mesh.texturesInternal;
		int[] distinctIndexesInternal = source.distinctIndexesInternal;
		int[] distinctIndexesInternal2 = dest.distinctIndexesInternal;
		for (int i = 0; i < distinctIndexesInternal.Length; i++)
		{
			texturesInternal[distinctIndexesInternal2[i]].x = texturesInternal[distinctIndexesInternal[i]].x;
			texturesInternal[distinctIndexesInternal2[i]].y = texturesInternal[distinctIndexesInternal[i]].y;
		}
	}

	internal static void ProjectTextureGroup(ProBuilderMesh mesh, int group, AutoUnwrapSettings unwrapSettings)
	{
		Projection.PlanarProject(mesh, group, unwrapSettings);
		s_IndexBuffer.Clear();
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			if (face.textureGroup == group)
			{
				s_IndexBuffer.AddRange(face.distinctIndexesInternal);
			}
		}
		ApplyUVSettings(mesh.texturesInternal, s_IndexBuffer, unwrapSettings);
	}

	private static void ApplyUVSettings(Vector2[] uvs, IList<int> indexes, AutoUnwrapSettings uvSettings)
	{
		int count = indexes.Count;
		Bounds2D bounds2D = new Bounds2D(uvs, indexes);
		switch (uvSettings.fill)
		{
		case AutoUnwrapSettings.Fill.Fit:
		{
			float num = Mathf.Max(bounds2D.size.x, bounds2D.size.y);
			ScaleUVs(uvs, indexes, new Vector2(num, num), bounds2D);
			bounds2D.center /= num;
			break;
		}
		case AutoUnwrapSettings.Fill.Stretch:
			ScaleUVs(uvs, indexes, bounds2D.size, bounds2D);
			bounds2D.center /= bounds2D.size;
			break;
		}
		if (uvSettings.scale.x != 1f || uvSettings.scale.y != 1f || uvSettings.rotation != 0f)
		{
			Vector2 vector = bounds2D.center * uvSettings.scale;
			Vector2 vector2 = bounds2D.center - vector;
			Vector2 origin = vector;
			for (int i = 0; i < count; i++)
			{
				uvs[indexes[i]] -= vector2;
				uvs[indexes[i]] = uvs[indexes[i]].ScaleAroundPoint(origin, uvSettings.scale);
				uvs[indexes[i]] = uvs[indexes[i]].RotateAroundPoint(origin, uvSettings.rotation);
			}
		}
		if (!uvSettings.useWorldSpace && uvSettings.anchor != AutoUnwrapSettings.Anchor.None)
		{
			ApplyUVAnchor(uvs, indexes, uvSettings.anchor);
		}
		if (uvSettings.flipU || uvSettings.flipV || uvSettings.swapUV)
		{
			for (int j = 0; j < count; j++)
			{
				float num2 = uvs[indexes[j]].x;
				float num3 = uvs[indexes[j]].y;
				if (uvSettings.flipU)
				{
					num2 = 0f - num2;
				}
				if (uvSettings.flipV)
				{
					num3 = 0f - num3;
				}
				if (!uvSettings.swapUV)
				{
					uvs[indexes[j]].x = num2;
					uvs[indexes[j]].y = num3;
				}
				else
				{
					uvs[indexes[j]].x = num3;
					uvs[indexes[j]].y = num2;
				}
			}
		}
		for (int k = 0; k < indexes.Count; k++)
		{
			uvs[indexes[k]].x -= uvSettings.offset.x;
			uvs[indexes[k]].y -= uvSettings.offset.y;
		}
	}

	private static void ScaleUVs(Vector2[] uvs, IList<int> indexes, Vector2 scale, Bounds2D bounds)
	{
		Vector2 center = bounds.center;
		Vector2 vector = center / scale;
		Vector2 vector2 = center - vector;
		center = vector;
		for (int i = 0; i < indexes.Count; i++)
		{
			Vector2 vector3 = uvs[indexes[i]] - vector2;
			vector3.x = (vector3.x - center.x) / scale.x + center.x;
			vector3.y = (vector3.y - center.y) / scale.y + center.y;
			uvs[indexes[i]] = vector3;
		}
	}

	private static void ApplyUVAnchor(Vector2[] uvs, IList<int> indexes, AutoUnwrapSettings.Anchor anchor)
	{
		s_TempVector2.x = 0f;
		s_TempVector2.y = 0f;
		Vector2 vector = Math.SmallestVector2(uvs, indexes);
		Vector2 vector2 = Math.LargestVector2(uvs, indexes);
		switch (anchor)
		{
		case AutoUnwrapSettings.Anchor.UpperLeft:
		case AutoUnwrapSettings.Anchor.MiddleLeft:
		case AutoUnwrapSettings.Anchor.LowerLeft:
			s_TempVector2.x = vector.x;
			break;
		case AutoUnwrapSettings.Anchor.UpperRight:
		case AutoUnwrapSettings.Anchor.MiddleRight:
		case AutoUnwrapSettings.Anchor.LowerRight:
			s_TempVector2.x = vector2.x - 1f;
			break;
		default:
			s_TempVector2.x = vector.x + (vector2.x - vector.x) * 0.5f - 0.5f;
			break;
		}
		switch (anchor)
		{
		case AutoUnwrapSettings.Anchor.UpperLeft:
		case AutoUnwrapSettings.Anchor.UpperCenter:
		case AutoUnwrapSettings.Anchor.UpperRight:
			s_TempVector2.y = vector2.y - 1f;
			break;
		case AutoUnwrapSettings.Anchor.MiddleLeft:
		case AutoUnwrapSettings.Anchor.MiddleCenter:
		case AutoUnwrapSettings.Anchor.MiddleRight:
			s_TempVector2.y = vector.y + (vector2.y - vector.y) * 0.5f - 0.5f;
			break;
		default:
			s_TempVector2.y = vector.y;
			break;
		}
		int count = indexes.Count;
		for (int i = 0; i < count; i++)
		{
			uvs[indexes[i]].x -= s_TempVector2.x;
			uvs[indexes[i]].y -= s_TempVector2.y;
		}
	}

	internal static void UpgradeAutoUVScaleOffset(ProBuilderMesh mesh)
	{
		Vector2[] src = mesh.textures.ToArray();
		mesh.RefreshUV(mesh.facesInternal);
		Vector2[] texturesInternal = mesh.texturesInternal;
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			if (!face.manualUV)
			{
				UVTransform uVTransform = CalculateDelta(src, face.indexesInternal, texturesInternal, face.indexesInternal);
				AutoUnwrapSettings uv = face.uv;
				uv.offset += uVTransform.translation;
				face.uv = uv;
			}
		}
	}
}
