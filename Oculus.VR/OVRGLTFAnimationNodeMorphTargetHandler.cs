using System;
using UnityEngine;

public class OVRGLTFAnimationNodeMorphTargetHandler
{
	private OVRMeshData _meshData;

	public float[] Weights;

	private bool _modified;

	private OVRMeshAttributes _meshModifiableData;

	public OVRGLTFAnimationNodeMorphTargetHandler(OVRMeshData meshData)
	{
		_meshData = meshData;
		_meshModifiableData.vertices = new Vector3[_meshData.baseAttributes.vertices.Length];
		_meshModifiableData.texcoords = new Vector2[_meshData.baseAttributes.texcoords.Length];
	}

	public void Update()
	{
		if (!_modified)
		{
			return;
		}
		Array.Copy(_meshData.baseAttributes.vertices, _meshModifiableData.vertices, _meshData.baseAttributes.vertices.Length);
		Array.Copy(_meshData.baseAttributes.texcoords, _meshModifiableData.texcoords, _meshData.baseAttributes.texcoords.Length);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < _meshData.morphTargets.Length; i++)
		{
			if (_meshData.morphTargets[i].vertices != null)
			{
				flag = true;
				int num = i / 2;
				if (i % 2 == 0)
				{
					float num2 = _meshData.morphTargets[i].vertices[num].x * Weights[i];
					_meshModifiableData.vertices[num].x += num2;
				}
				else
				{
					float num3 = _meshData.morphTargets[i].vertices[num].y * Weights[i];
					_meshModifiableData.vertices[num].y += num3;
				}
			}
			if (_meshData.morphTargets[i].texcoords != null)
			{
				flag2 = true;
				int num4 = (i - 8) / 2;
				if (i % 2 == 0)
				{
					_meshModifiableData.texcoords[num4].x += _meshData.morphTargets[i].texcoords[num4].x * Weights[i];
				}
				else
				{
					_meshModifiableData.texcoords[num4].y += _meshData.morphTargets[i].texcoords[num4].y * Weights[i];
				}
			}
		}
		if (flag)
		{
			_meshData.mesh.vertices = _meshModifiableData.vertices;
			_meshData.mesh.RecalculateBounds();
		}
		if (flag2)
		{
			_meshData.mesh.uv = _meshModifiableData.texcoords;
		}
		if (flag || flag2)
		{
			_meshData.mesh.MarkModified();
		}
		_modified = false;
	}

	public void MarkModified()
	{
		_modified = true;
	}
}
