using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using OVRSimpleJSON;
using UnityEngine;

public class OVRGLTFAccessor : IDisposable
{
	private struct GLTFAccessor
	{
		public OVRGLTFType Type;

		public OVRGLTFComponentType ComponentType;

		public int ComponentTypeStride;

		public int BufferViewIndex;

		public int ByteOffset;

		public int Count;

		public JSONNode Min;

		public JSONNode Max;
	}

	private struct GLTFBufferView
	{
		public int BufferIndex;

		public int ByteOffset;

		public int ByteLength;

		public int ByteStride;
	}

	private struct GLTFBuffer
	{
		public int ByteLength;
	}

	private readonly List<GLTFAccessor> _accessors = new List<GLTFAccessor>();

	private readonly List<GLTFBufferView> _bufferViews = new List<GLTFBufferView>();

	private readonly List<GLTFBuffer> _buffers = new List<GLTFBuffer>();

	private readonly Stream _binaryChunk;

	private readonly int _binaryChunkLength;

	private readonly int _binaryChunkStart;

	private readonly BinaryReader _reader;

	private GLTFAccessor _activeGltfAccessor;

	private GLTFBufferView _activeBufferView;

	private GLTFBuffer _activeBuffer;

	private int _activeBufferOffset;

	private bool _requireStrideSeek;

	public static bool TryCreate(JSONNode accessorsRoot, JSONNode bufferViewsRoot, JSONNode buffersRoot, Stream binaryChunk, out OVRGLTFAccessor dataAccessor)
	{
		BinaryReader binaryReader = new BinaryReader(binaryChunk, Encoding.UTF8, leaveOpen: true);
		uint binaryChunkLength = binaryReader.ReadUInt32();
		if (binaryReader.ReadUInt32() != 5130562)
		{
			Debug.LogError("Read chunk does not match type.");
			dataAccessor = null;
			return false;
		}
		dataAccessor = new OVRGLTFAccessor(accessorsRoot, bufferViewsRoot, buffersRoot, binaryReader, (int)binaryChunk.Position, (int)binaryChunkLength);
		return true;
	}

	private OVRGLTFAccessor(JSONNode accessorsRoot, JSONNode bufferViewsRoot, JSONNode buffersRoot, BinaryReader binaryChunkReader, int binaryChinkStart, int binaryChunkLength)
	{
		_reader = binaryChunkReader;
		_binaryChunk = binaryChunkReader.BaseStream;
		_binaryChunkLength = binaryChunkLength;
		_binaryChunkStart = binaryChinkStart;
		foreach (JSONNode child in accessorsRoot.Children)
		{
			GLTFAccessor item = default(GLTFAccessor);
			JSONNode.Enumerator enumerator2 = child.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, JSONNode> current2 = enumerator2.Current;
				switch (current2.Key)
				{
				case "bufferView":
					item.BufferViewIndex = current2.Value.AsInt;
					break;
				case "byteOffset":
					item.ByteOffset = current2.Value.AsInt;
					break;
				case "componentType":
					item.ComponentType = (OVRGLTFComponentType)current2.Value.AsInt;
					item.ComponentTypeStride = GetStrideForType(item.ComponentType);
					break;
				case "count":
					item.Count = current2.Value.AsInt;
					break;
				case "type":
					item.Type = ToOVRType(current2.Value.Value);
					break;
				case "max":
					item.Max = current2.Value;
					break;
				case "min":
					item.Min = current2.Value;
					break;
				case "sparse":
					Debug.LogWarning("Sparse accessors unsupported");
					break;
				}
			}
			_accessors.Add(item);
		}
		foreach (JSONNode child2 in bufferViewsRoot.Children)
		{
			GLTFBufferView item2 = default(GLTFBufferView);
			JSONNode.Enumerator enumerator2 = child2.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, JSONNode> current4 = enumerator2.Current;
				switch (current4.Key)
				{
				case "bufferIndex":
					item2.BufferIndex = current4.Value.AsInt;
					break;
				case "byteOffset":
					item2.ByteOffset = current4.Value.AsInt;
					break;
				case "byteLength":
					item2.ByteLength = current4.Value.AsInt;
					break;
				case "byteStride":
					item2.ByteStride = current4.Value.AsInt;
					break;
				}
			}
			_bufferViews.Add(item2);
		}
		foreach (JSONNode child3 in buffersRoot.Children)
		{
			GLTFBuffer item3 = default(GLTFBuffer);
			JSONNode.Enumerator enumerator2 = child3.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				KeyValuePair<string, JSONNode> current6 = enumerator2.Current;
				if (current6.Key == "byteLength")
				{
					item3.ByteLength = current6.Value.AsInt;
				}
			}
			_buffers.Add(item3);
		}
	}

	private static OVRGLTFType ToOVRType(string type)
	{
		switch (type)
		{
		case "SCALAR":
			return OVRGLTFType.SCALAR;
		case "VEC2":
			return OVRGLTFType.VEC2;
		case "VEC3":
			return OVRGLTFType.VEC3;
		case "VEC4":
			return OVRGLTFType.VEC4;
		case "MAT4":
			return OVRGLTFType.MAT4;
		default:
			Debug.LogError("Unsupported accessor type.");
			return OVRGLTFType.NONE;
		}
	}

	public void Seek(int accessorIndex, bool onlyBufferView = false)
	{
		if (accessorIndex >= _accessors.Count)
		{
			return;
		}
		_activeGltfAccessor = _accessors[accessorIndex];
		_activeBufferView = _bufferViews[_activeGltfAccessor.BufferViewIndex];
		_activeBuffer = _buffers[_activeBufferView.BufferIndex];
		_requireStrideSeek = _activeBufferView.ByteStride != 0 && _activeBufferView.ByteStride != _activeGltfAccessor.ComponentTypeStride;
		if (_binaryChunkLength != _activeBuffer.ByteLength)
		{
			Debug.LogError("Chunk length is not equal to buffer length.");
			return;
		}
		_activeBufferOffset = _binaryChunkStart + _activeBufferView.ByteOffset;
		if (!onlyBufferView)
		{
			_activeBufferOffset += _activeGltfAccessor.ByteOffset;
		}
		_binaryChunk.Seek(_activeBufferOffset, SeekOrigin.Begin);
	}

	private void SeekStride(int strideIndex)
	{
		if (_requireStrideSeek && strideIndex != 0)
		{
			if (strideIndex >= _activeGltfAccessor.Count)
			{
				Debug.LogError("Invalid seek index for data");
				return;
			}
			int byteStride = _activeBufferView.ByteStride;
			_binaryChunk.Seek(_activeBufferOffset + byteStride * strideIndex, SeekOrigin.Begin);
		}
	}

	public float[] ReadFloat()
	{
		float[] array = new float[_activeGltfAccessor.Count];
		if (_activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
			}
		}
		return array;
	}

	public int[] ReadInt()
	{
		int[] array = new int[_activeGltfAccessor.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadAsInt(_reader, _activeGltfAccessor.ComponentType);
		}
		return array;
	}

	public Vector2[] ReadVector2()
	{
		Vector2[] array = new Vector2[_activeGltfAccessor.Count];
		if (!_requireStrideSeek && _activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				SeekStride(i);
				array[i].x = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[i].y = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
			}
		}
		return array;
	}

	public Vector3[] ReadVector3(Vector3 conversionScale)
	{
		Vector3[] array = new Vector3[_activeGltfAccessor.Count];
		if (!_requireStrideSeek && _activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Scale(conversionScale);
			}
		}
		else
		{
			for (int j = 0; j < array.Length; j++)
			{
				SeekStride(j);
				array[j].x = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].y = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].z = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].Scale(conversionScale);
			}
		}
		return array;
	}

	public Vector4[] ReadVector4(Vector4 conversionScale)
	{
		Vector4[] array = new Vector4[_activeGltfAccessor.Count];
		if (!_requireStrideSeek && _activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Scale(conversionScale);
			}
		}
		else
		{
			for (int j = 0; j < array.Length; j++)
			{
				SeekStride(j);
				array[j].x = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].y = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].z = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].w = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				array[j].Scale(conversionScale);
			}
		}
		return array;
	}

	private static int ReadAsInt(BinaryReader reader, OVRGLTFComponentType type)
	{
		return type switch
		{
			OVRGLTFComponentType.NONE => 0, 
			OVRGLTFComponentType.BYTE => reader.ReadSByte(), 
			OVRGLTFComponentType.UNSIGNED_BYTE => reader.ReadByte(), 
			OVRGLTFComponentType.SHORT => reader.ReadInt16(), 
			OVRGLTFComponentType.UNSIGNED_SHORT => reader.ReadUInt16(), 
			OVRGLTFComponentType.UNSIGNED_INT => (int)reader.ReadUInt32(), 
			OVRGLTFComponentType.FLOAT => (int)reader.ReadSingle(), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	private static float ReadAsFloat(BinaryReader reader, OVRGLTFComponentType type)
	{
		return type switch
		{
			OVRGLTFComponentType.NONE => 0f, 
			OVRGLTFComponentType.BYTE => reader.ReadSByte(), 
			OVRGLTFComponentType.UNSIGNED_BYTE => (int)reader.ReadByte(), 
			OVRGLTFComponentType.SHORT => reader.ReadInt16(), 
			OVRGLTFComponentType.UNSIGNED_SHORT => (int)reader.ReadUInt16(), 
			OVRGLTFComponentType.UNSIGNED_INT => reader.ReadUInt32(), 
			OVRGLTFComponentType.FLOAT => reader.ReadSingle(), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public Color[] ReadColor()
	{
		if (_activeGltfAccessor.Type != OVRGLTFType.VEC4 && _activeGltfAccessor.Type != OVRGLTFType.VEC3)
		{
			Debug.LogError("Tried to read non-color type as a color array." + _activeGltfAccessor.Type);
			return Array.Empty<Color>();
		}
		Color[] array = new Color[_activeGltfAccessor.Count];
		if (!_requireStrideSeek && _activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT && _activeGltfAccessor.Type == OVRGLTFType.VEC4)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				SeekStride(i);
				if (_activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
				{
					array[i].r = _reader.ReadSingle();
					array[i].g = _reader.ReadSingle();
					array[i].b = _reader.ReadSingle();
					array[i].a = ((_activeGltfAccessor.Type == OVRGLTFType.VEC4) ? _reader.ReadSingle() : 1f);
				}
				else
				{
					float maxValueForType = GetMaxValueForType(_activeGltfAccessor.ComponentType);
					array[i].r = (float)ReadAsInt(_reader, _activeGltfAccessor.ComponentType) / maxValueForType;
					array[i].g = (float)ReadAsInt(_reader, _activeGltfAccessor.ComponentType) / maxValueForType;
					array[i].b = (float)ReadAsInt(_reader, _activeGltfAccessor.ComponentType) / maxValueForType;
					array[i].a = ((_activeGltfAccessor.Type == OVRGLTFType.VEC4) ? ((float)ReadAsInt(_reader, _activeGltfAccessor.ComponentType) / maxValueForType) : 1f);
				}
			}
		}
		return array;
	}

	public void ReadWeights(ref BoneWeight[] resultsBoneWeights)
	{
		if (_activeGltfAccessor.Type != OVRGLTFType.VEC4)
		{
			Debug.LogError("Tried to read bone weights data as a non-vec4 array.");
			return;
		}
		if (resultsBoneWeights == null)
		{
			resultsBoneWeights = new BoneWeight[_activeGltfAccessor.Count];
		}
		for (int i = 0; i < resultsBoneWeights.Length; i++)
		{
			SeekStride(i);
			resultsBoneWeights[i].weight0 = _reader.ReadSingle();
			resultsBoneWeights[i].weight1 = _reader.ReadSingle();
			resultsBoneWeights[i].weight2 = _reader.ReadSingle();
			resultsBoneWeights[i].weight3 = _reader.ReadSingle();
			float num = resultsBoneWeights[i].weight0 + resultsBoneWeights[i].weight1 + resultsBoneWeights[i].weight2 + resultsBoneWeights[i].weight3;
			if (!Mathf.Approximately(num, 0f))
			{
				resultsBoneWeights[i].weight0 /= num;
				resultsBoneWeights[i].weight1 /= num;
				resultsBoneWeights[i].weight2 /= num;
				resultsBoneWeights[i].weight3 /= num;
			}
		}
	}

	public void ReadJoints(ref BoneWeight[] resultsBoneWeights)
	{
		if (_activeGltfAccessor.Type != OVRGLTFType.VEC4)
		{
			Debug.LogError("Tried to read bone weights data as a non-vec4 array.");
			return;
		}
		if (resultsBoneWeights == null)
		{
			resultsBoneWeights = new BoneWeight[_activeGltfAccessor.Count];
		}
		for (int i = 0; i < resultsBoneWeights.Length; i++)
		{
			SeekStride(i);
			resultsBoneWeights[i].boneIndex0 = ReadAsInt(_reader, _activeGltfAccessor.ComponentType);
			resultsBoneWeights[i].boneIndex1 = ReadAsInt(_reader, _activeGltfAccessor.ComponentType);
			resultsBoneWeights[i].boneIndex2 = ReadAsInt(_reader, _activeGltfAccessor.ComponentType);
			resultsBoneWeights[i].boneIndex3 = ReadAsInt(_reader, _activeGltfAccessor.ComponentType);
		}
	}

	public Quaternion[] ReadQuaterion(Vector4 gltfToUnitySpaceRotation)
	{
		if (_activeGltfAccessor.Type != OVRGLTFType.VEC4)
		{
			Debug.LogError("Tried to read bone weights data as a non-vec4 array.");
			return Array.Empty<Quaternion>();
		}
		Quaternion[] array = new Quaternion[_activeGltfAccessor.Count];
		if (!_requireStrideSeek && _activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
			for (int i = 0; i < array.Length; i++)
			{
				array[i].x *= gltfToUnitySpaceRotation.x;
				array[i].y *= gltfToUnitySpaceRotation.y;
				array[i].z *= gltfToUnitySpaceRotation.z;
				array[i].w *= gltfToUnitySpaceRotation.w;
			}
		}
		else
		{
			for (int j = 0; j < array.Length; j++)
			{
				SeekStride(j);
				array[j].x = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType) * gltfToUnitySpaceRotation.x;
				array[j].y = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType) * gltfToUnitySpaceRotation.y;
				array[j].z = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType) * gltfToUnitySpaceRotation.z;
				array[j].w = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType) * gltfToUnitySpaceRotation.w;
			}
		}
		return array;
	}

	public Matrix4x4[] ReadMatrix4x4(Vector3 conversionScale)
	{
		if (_activeGltfAccessor.Type != OVRGLTFType.MAT4)
		{
			Debug.LogError("Tried to read non-vec3 data as a vec3 array.");
			return Array.Empty<Matrix4x4>();
		}
		Matrix4x4 matrix4x = Matrix4x4.Scale(conversionScale);
		Matrix4x4[] array = new Matrix4x4[_activeGltfAccessor.Count];
		if (!_requireStrideSeek && _activeGltfAccessor.ComponentType == OVRGLTFComponentType.FLOAT)
		{
			_binaryChunk.Read(MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array)));
			for (int i = 0; i < _activeGltfAccessor.Count; i++)
			{
				array[i] = matrix4x * array[i] * matrix4x;
			}
		}
		else
		{
			for (int j = 0; j < _activeGltfAccessor.Count; j++)
			{
				SeekStride(j);
				for (int k = 0; k < 16; k++)
				{
					array[j][k] = ReadAsFloat(_reader, _activeGltfAccessor.ComponentType);
				}
				array[j] = matrix4x * array[j] * matrix4x;
			}
		}
		return array;
	}

	private int GetStrideForType(OVRGLTFComponentType type)
	{
		switch (type)
		{
		case OVRGLTFComponentType.BYTE:
			return 1;
		case OVRGLTFComponentType.UNSIGNED_BYTE:
			return 1;
		case OVRGLTFComponentType.SHORT:
			return 2;
		case OVRGLTFComponentType.UNSIGNED_SHORT:
			return 2;
		case OVRGLTFComponentType.UNSIGNED_INT:
			return 4;
		case OVRGLTFComponentType.FLOAT:
			return 4;
		default:
			Debug.LogWarning("GetStrideForType called with unsupported component type " + type);
			return 0;
		}
	}

	private float GetMaxValueForType(OVRGLTFComponentType type)
	{
		switch (type)
		{
		case OVRGLTFComponentType.BYTE:
			return 127f;
		case OVRGLTFComponentType.UNSIGNED_BYTE:
			return 255f;
		case OVRGLTFComponentType.SHORT:
			return 32767f;
		case OVRGLTFComponentType.UNSIGNED_SHORT:
			return 65535f;
		case OVRGLTFComponentType.UNSIGNED_INT:
			return 4.2949673E+09f;
		case OVRGLTFComponentType.FLOAT:
			return float.MaxValue;
		default:
			Debug.LogWarning("GetMaxValueForType called with unsupported component type " + type);
			return 1f;
		}
	}

	public byte[] ReadBuffer(int bufferViewIndex)
	{
		_activeBufferView = _bufferViews[bufferViewIndex];
		_activeBuffer = _buffers[_activeBufferView.BufferIndex];
		_binaryChunk.Seek(_binaryChunkStart, SeekOrigin.Begin);
		_binaryChunk.Seek(_activeBufferView.ByteOffset, SeekOrigin.Current);
		return _reader.ReadBytes(_activeBufferView.ByteLength);
	}

	public void Dispose()
	{
		_reader.Dispose();
	}

	public int GetDataCount()
	{
		return _activeGltfAccessor.Count;
	}
}
