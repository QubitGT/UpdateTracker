using System;
using Unity.Collections;
using UnityEngine.U2D;

namespace UnityEngine.Rendering.Universal;

[Serializable]
internal class ShadowShape2DProvider_SpriteRenderer : ShadowShape2DProvider
{
	private const float k_InitialTrim = 0.05f;

	private ShadowShape2D m_PersistantShapeData;

	private SpriteDrawMode m_CurrentDrawMode;

	private Vector2 m_CurrentDrawModeSize;

	private void SetFullRectShapeData(SpriteRenderer spriteRenderer, ShadowShape2D shadowShape2D)
	{
		if (spriteRenderer.drawMode != 0)
		{
			Sprite sprite = spriteRenderer.sprite;
			Vector2 size = spriteRenderer.size;
			Vector3 vector = new Vector2(size.x * sprite.pivot.x / sprite.rect.width, size.y * sprite.pivot.y / sprite.rect.height);
			Rect rect = new Rect(-vector, new Vector2(size.x, size.y));
			NativeArray<Vector3> vertices = new NativeArray<Vector3>(4, Allocator.Temp);
			NativeArray<int> indices = new NativeArray<int>(8, Allocator.Temp);
			vertices[0] = new Vector3(rect.min.x, rect.min.y);
			vertices[1] = new Vector3(rect.min.x, rect.max.y);
			vertices[2] = new Vector3(rect.max.x, rect.max.y);
			vertices[3] = new Vector3(rect.max.x, rect.min.y);
			indices[0] = 0;
			indices[1] = 1;
			indices[2] = 1;
			indices[3] = 2;
			indices[4] = 2;
			indices[5] = 3;
			indices[6] = 3;
			indices[7] = 0;
			shadowShape2D.SetShape(vertices, indices, ShadowShape2D.OutlineTopology.Lines);
			vertices.Dispose();
			indices.Dispose();
		}
	}

	private void SetPersistantShapeData(Sprite sprite, ShadowShape2D shadowShape2D, NativeSlice<Vector3> vertexSlice)
	{
		if (shadowShape2D != null)
		{
			NativeArray<ushort> indices = sprite.GetIndices();
			NativeArray<int> indices2 = new NativeArray<int>(indices.Length, Allocator.Temp);
			NativeArray<Vector3> vertices = new NativeArray<Vector3>(vertexSlice.Length, Allocator.Temp);
			for (int i = 0; i < indices2.Length; i++)
			{
				indices2[i] = indices[i];
			}
			for (int j = 0; j < vertices.Length; j++)
			{
				vertices[j] = vertexSlice[j];
			}
			shadowShape2D.SetShape(vertices, indices2, ShadowShape2D.OutlineTopology.Triangles);
			vertices.Dispose();
			indices2.Dispose();
		}
	}

	private void TryToSetPersistantShapeData(SpriteRenderer spriteRenderer, ShadowShape2D persistantShadowShape, bool force)
	{
		if (spriteRenderer != null && spriteRenderer.sprite != null)
		{
			if (spriteRenderer.drawMode != 0 && (spriteRenderer.size.x != m_CurrentDrawModeSize.x || spriteRenderer.size.y != m_CurrentDrawModeSize.y || spriteRenderer.drawMode != m_CurrentDrawMode || force))
			{
				m_CurrentDrawModeSize = spriteRenderer.size;
				SetFullRectShapeData(spriteRenderer, persistantShadowShape);
			}
			else if (spriteRenderer.drawMode != m_CurrentDrawMode || force)
			{
				Sprite sprite = spriteRenderer.sprite;
				NativeSlice<Vector3> vertexAttribute = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
				SetPersistantShapeData(sprite, m_PersistantShapeData, vertexAttribute);
			}
			m_CurrentDrawMode = spriteRenderer.drawMode;
		}
	}

	private void UpdatePersistantShapeData(SpriteRenderer spriteRenderer)
	{
		TryToSetPersistantShapeData(spriteRenderer, m_PersistantShapeData, force: true);
	}

	public override int Priority()
	{
		return 1;
	}

	public override bool IsShapeSource(Component sourceComponent)
	{
		return sourceComponent is SpriteRenderer;
	}

	public override void OnPersistantDataCreated(Component sourceComponent, ShadowShape2D persistantShadowShape)
	{
		SpriteRenderer spriteRenderer = (SpriteRenderer)sourceComponent;
		m_PersistantShapeData = persistantShadowShape as ShadowMesh2D;
		if (spriteRenderer.sprite != null)
		{
			float trimEdgeFromBounds = ShadowShapeProvider2DUtility.GetTrimEdgeFromBounds(spriteRenderer.bounds, 0.05f);
			persistantShadowShape.SetDefaultTrim(trimEdgeFromBounds);
		}
		TryToSetPersistantShapeData(spriteRenderer, persistantShadowShape, force: true);
	}

	public override void OnBeforeRender(Component sourceComponent, Bounds worldCullingBounds, ShadowShape2D persistantShadowShape)
	{
		SpriteRenderer spriteRenderer = (SpriteRenderer)sourceComponent;
		persistantShadowShape.SetFlip(spriteRenderer.flipX, spriteRenderer.flipY);
		TryToSetPersistantShapeData(spriteRenderer, persistantShadowShape, force: false);
	}

	public override void Enabled(Component sourceComponent, ShadowShape2D persistantShadowShape)
	{
		SpriteRenderer obj = (SpriteRenderer)sourceComponent;
		m_PersistantShapeData = persistantShadowShape;
		obj.RegisterSpriteChangeCallback(UpdatePersistantShapeData);
	}

	public override void Disabled(Component sourceComponent, ShadowShape2D persistantShadowShape)
	{
		((SpriteRenderer)sourceComponent).UnregisterSpriteChangeCallback(UpdatePersistantShapeData);
	}
}
