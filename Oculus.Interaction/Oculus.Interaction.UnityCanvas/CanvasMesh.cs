using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

[DisallowMultipleComponent]
public abstract class CanvasMesh : MonoBehaviour
{
	[Tooltip("Mesh construction will be driven by this texture.")]
	[SerializeField]
	protected CanvasRenderTexture _canvasRenderTexture;

	[Tooltip("The mesh filter that will be driven.")]
	[SerializeField]
	protected MeshFilter _meshFilter;

	[Tooltip("Optional mesh collider that will be driven.")]
	[SerializeField]
	[Optional]
	protected MeshCollider _meshCollider;

	protected bool _started;

	protected abstract Vector3 MeshInverseTransform(Vector3 localPosition);

	protected abstract void GenerateMesh(out List<Vector3> verts, out List<int> tris, out List<Vector2> uvs);

	public Vector3 ImposterToCanvasTransformPoint(Vector3 worldPosition)
	{
		Vector3 localPosition = _meshFilter.transform.InverseTransformPoint(worldPosition);
		Vector3 position = MeshInverseTransform(localPosition) / _canvasRenderTexture.transform.localScale.x;
		return _canvasRenderTexture.transform.TransformPoint(position);
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			UpdateImposter();
			CanvasRenderTexture canvasRenderTexture = _canvasRenderTexture;
			canvasRenderTexture.OnUpdateRenderTexture = (Action<Texture>)Delegate.Combine(canvasRenderTexture.OnUpdateRenderTexture, new Action<Texture>(HandleUpdateRenderTexture));
			if (_canvasRenderTexture.Texture != null)
			{
				HandleUpdateRenderTexture(_canvasRenderTexture.Texture);
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			CanvasRenderTexture canvasRenderTexture = _canvasRenderTexture;
			canvasRenderTexture.OnUpdateRenderTexture = (Action<Texture>)Delegate.Remove(canvasRenderTexture.OnUpdateRenderTexture, new Action<Texture>(HandleUpdateRenderTexture));
		}
	}

	protected virtual void HandleUpdateRenderTexture(Texture texture)
	{
		UpdateImposter();
	}

	protected virtual void UpdateImposter()
	{
		try
		{
			GenerateMesh(out var verts, out var tris, out var uvs);
			Mesh mesh = new Mesh();
			mesh.SetVertices(verts);
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(tris, 0);
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			_meshFilter.mesh = mesh;
			if (_meshCollider != null)
			{
				_meshCollider.sharedMesh = _meshFilter.sharedMesh;
			}
		}
		finally
		{
		}
	}

	public void InjectAllCanvasMesh(CanvasRenderTexture canvasRenderTexture, MeshFilter meshFilter)
	{
		InjectCanvasRenderTexture(canvasRenderTexture);
		InjectMeshFilter(meshFilter);
	}

	public void InjectCanvasRenderTexture(CanvasRenderTexture canvasRenderTexture)
	{
		_canvasRenderTexture = canvasRenderTexture;
	}

	public void InjectMeshFilter(MeshFilter meshFilter)
	{
		_meshFilter = meshFilter;
	}

	public void InjectOptionalMeshCollider(MeshCollider meshCollider)
	{
		_meshCollider = meshCollider;
	}
}
