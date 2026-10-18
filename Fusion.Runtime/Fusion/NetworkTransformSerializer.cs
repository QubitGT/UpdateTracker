#define DEBUG
using System;
using UnityEngine;

namespace Fusion;

internal class NetworkTransformSerializer : NetworkBufferSerializer
{
	private const int POSITION_ACCURACY = 1024;

	private const int POSITION_BLOCK_SIZE = 4;

	private const int JUMP_OFFSET = 6;

	public static NetworkTransformSerializer Instance = new NetworkTransformSerializer();

	public unsafe override int Write(Simulation.SendContext sc, NetworkObjectMeta meta, NetworkBufferSerializerInfo info, Span<int> ptr, int word, int prev)
	{
		word -= info.Offset;
		Assert.Check(prev < word, prev, word, info.Offset);
		Assert.Check(word + 6 <= meta.WordCount, word, 6, meta.WordCount);
		sc.Buffer->WriteInt32VarLength(word - prev, 4);
		ref Span<int> reference = ref ptr;
		int num = word;
		Vector3 vector = reference.Slice(num, reference.Length - num).Read<Vector3>();
		reference = ref ptr;
		num = word + 3;
		Quaternion rot = reference.Slice(num, reference.Length - num).Read<Quaternion>();
		sc.Buffer->WriteInt32VarLength(FloatUtils.Compress(vector.x), 4);
		sc.Buffer->WriteInt32VarLength(FloatUtils.Compress(vector.y), 4);
		sc.Buffer->WriteInt32VarLength(FloatUtils.Compress(vector.z), 4);
		sc.Buffer->WriteUInt32(Maths.QuaternionCompress(rot));
		return word + 6;
	}

	public unsafe override int Skip(Simulation.RecvContext rc, int word)
	{
		rc.Buffer->ReadInt32VarLength(4);
		rc.Buffer->ReadInt32VarLength(4);
		rc.Buffer->ReadInt32VarLength(4);
		rc.Buffer->ReadUInt32();
		return word + 6;
	}

	public unsafe override int Read(Simulation.RecvContext rc, NetworkObjectMeta meta, NetworkBufferSerializerInfo info, Span<int> ptr, int word)
	{
		Assert.Check(info.Offset == 0, info.Offset, word);
		ref Span<int> reference = ref ptr;
		int num = word;
		ref Vector3 reference2 = ref reference.Slice(num, reference.Length - num).AsRef<Vector3>();
		reference = ref ptr;
		num = word + 3;
		ref Quaternion reference3 = ref reference.Slice(num, reference.Length - num).AsRef<Quaternion>();
		reference2.x = FloatUtils.Decompress(rc.Buffer->ReadInt32VarLength(4));
		reference2.y = FloatUtils.Decompress(rc.Buffer->ReadInt32VarLength(4));
		reference2.z = FloatUtils.Decompress(rc.Buffer->ReadInt32VarLength(4));
		reference3 = Maths.QuaternionDecompress(rc.Buffer->ReadUInt32());
		reference3 = reference3.normalized;
		return word + 6;
	}
}
