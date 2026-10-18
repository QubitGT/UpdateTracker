using System;
using System.Collections;
using UnityEngine;

namespace Oculus.Interaction.Demo;

[RequireComponent(typeof(MeshFilter))]
public class MeshBlit : MonoBehaviour
{
	private static int MAIN_TEX = Shader.PropertyToID("_MainTex");

	public Material material;

	public RenderTexture renderTexture;

	[SerializeField]
	private float _blitsPerSecond = -1f;

	private Mesh _mesh;

	private WaitForSeconds _waitForSeconds;

	public float BlitsPerSecond
	{
		get
		{
			return _blitsPerSecond;
		}
		set
		{
			SetBlitsPerSecond(value);
		}
	}

	private Mesh Mesh
	{
		get
		{
			if (!_mesh)
			{
				return _mesh = GetComponent<MeshFilter>().sharedMesh;
			}
			return _mesh;
		}
	}

	private void OnEnable()
	{
		SetBlitsPerSecond(_blitsPerSecond);
		StartCoroutine(BlitRoutine());
		IEnumerator BlitRoutine()
		{
			while (true)
			{
				yield return _waitForSeconds;
				Blit();
			}
		}
	}

	public void Blit()
	{
		if (renderTexture == null)
		{
			throw new NullReferenceException("MeshBlit.Blit must have a RenderTexture assigned");
		}
		if (material == null)
		{
			throw new NullReferenceException("MeshBlit.Blit must have a Material assigned");
		}
		if (Mesh == null)
		{
			throw new NullReferenceException("MeshBlit.Blit's MeshFilter has no mesh");
		}
		RenderTexture temporary = RenderTexture.GetTemporary(renderTexture.descriptor);
		Graphics.Blit(renderTexture, temporary);
		material.SetTexture(MAIN_TEX, temporary);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		material.SetPass(0);
		Graphics.DrawMeshNow(Mesh, base.transform.localToWorldMatrix);
		RenderTexture.active = active;
		material.SetTexture(MAIN_TEX, null);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void SetBlitsPerSecond(float value)
	{
		_blitsPerSecond = value;
		_waitForSeconds = ((value > 0f) ? new WaitForSeconds(1f / _blitsPerSecond) : null);
	}
}
