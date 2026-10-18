using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drawing;

[BurstCompile]
internal struct StreamSplitter : IJob
{
	public NativeArray<UnsafeAppendBuffer> inputBuffers;

	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeAppendBuffer* staticBuffer;

	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeAppendBuffer* dynamicBuffer;

	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeAppendBuffer* persistentBuffer;

	internal static readonly int PushCommands;

	internal static readonly int PopCommands;

	internal static readonly int MetaCommands;

	internal static readonly int DynamicCommands;

	internal static readonly int StaticCommands;

	internal static readonly int[] CommandSizes;

	static StreamSplitter()
	{
		PushCommands = 557069;
		PopCommands = 1114130;
		MetaCommands = PushCommands | PopCommands;
		DynamicCommands = 0x2607C0 | MetaCommands;
		StaticCommands = 0x7820 | MetaCommands;
		CommandSizes = new int[22];
		CommandSizes[0] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<Color32>();
		CommandSizes[1] = UnsafeUtility.SizeOf<CommandBuilder.Command>();
		CommandSizes[2] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<float4x4>();
		CommandSizes[3] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<float4x4>();
		CommandSizes[4] = UnsafeUtility.SizeOf<CommandBuilder.Command>();
		CommandSizes[5] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.LineData>();
		CommandSizes[7] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.CircleXZData>();
		CommandSizes[10] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.SphereData>();
		CommandSizes[6] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.CircleData>();
		CommandSizes[8] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.CircleData>();
		CommandSizes[9] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.CircleXZData>();
		CommandSizes[11] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.BoxData>();
		CommandSizes[12] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.PlaneData>();
		CommandSizes[13] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.BoxData>();
		CommandSizes[14] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.TriangleData>();
		CommandSizes[15] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.PersistData>();
		CommandSizes[16] = UnsafeUtility.SizeOf<CommandBuilder.Command>();
		CommandSizes[17] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.TextData>();
		CommandSizes[18] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.TextData3D>();
		CommandSizes[19] = UnsafeUtility.SizeOf<CommandBuilder.Command>() + UnsafeUtility.SizeOf<CommandBuilder.LineWidthData>();
		CommandSizes[20] = UnsafeUtility.SizeOf<CommandBuilder.Command>();
		CommandSizes[21] = UnsafeUtility.SizeOf<CommandBuilder.Command>();
	}

	public unsafe void Execute()
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		NativeArray<int> nativeArray = new NativeArray<int>(32, Allocator.Temp);
		NativeArray<int> nativeArray2 = new NativeArray<int>(32, Allocator.Temp);
		NativeArray<int> nativeArray3 = new NativeArray<int>(32, Allocator.Temp);
		UnsafeAppendBuffer unsafeAppendBuffer = *staticBuffer;
		UnsafeAppendBuffer unsafeAppendBuffer2 = *dynamicBuffer;
		UnsafeAppendBuffer unsafeAppendBuffer3 = *persistentBuffer;
		unsafeAppendBuffer.Reset();
		unsafeAppendBuffer2.Reset();
		unsafeAppendBuffer3.Reset();
		for (int i = 0; i < inputBuffers.Length; i++)
		{
			int num4 = 0;
			int num5 = 0;
			UnsafeAppendBuffer.Reader reader = inputBuffers[i].AsReader();
			if (unsafeAppendBuffer.Capacity < unsafeAppendBuffer.Length + reader.Size)
			{
				unsafeAppendBuffer.SetCapacity(math.ceilpow2(unsafeAppendBuffer.Length + reader.Size));
			}
			if (unsafeAppendBuffer2.Capacity < unsafeAppendBuffer2.Length + reader.Size)
			{
				unsafeAppendBuffer2.SetCapacity(math.ceilpow2(unsafeAppendBuffer2.Length + reader.Size));
			}
			if (unsafeAppendBuffer3.Capacity < unsafeAppendBuffer3.Length + reader.Size)
			{
				unsafeAppendBuffer3.SetCapacity(math.ceilpow2(unsafeAppendBuffer3.Length + reader.Size));
			}
			*staticBuffer = unsafeAppendBuffer;
			*dynamicBuffer = unsafeAppendBuffer2;
			*persistentBuffer = unsafeAppendBuffer3;
			while (reader.Offset < reader.Size)
			{
				CommandBuilder.Command command = *(CommandBuilder.Command*)(reader.Ptr + reader.Offset);
				int num6 = 1 << (int)(command & (CommandBuilder.Command)255);
				int num7 = CommandSizes[(int)(command & (CommandBuilder.Command)255)] + (((command & CommandBuilder.Command.PushColorInline) != 0) ? UnsafeUtility.SizeOf<Color32>() : 0);
				bool flag = (num6 & MetaCommands) != 0;
				if ((command & (CommandBuilder.Command)255) == CommandBuilder.Command.Text)
				{
					CommandBuilder.TextData textData = *(CommandBuilder.TextData*)(reader.Ptr + reader.Offset + num7 - sizeof(CommandBuilder.TextData));
					num7 += textData.numCharacters * UnsafeUtility.SizeOf<ushort>();
				}
				else if ((command & (CommandBuilder.Command)255) == CommandBuilder.Command.Text3D)
				{
					CommandBuilder.TextData3D textData3D = *(CommandBuilder.TextData3D*)(reader.Ptr + reader.Offset + num7 - sizeof(CommandBuilder.TextData3D));
					num7 += textData3D.numCharacters * UnsafeUtility.SizeOf<ushort>();
				}
				if ((num6 & DynamicCommands) != 0 && num5 == 0)
				{
					if (!flag)
					{
						num2 = unsafeAppendBuffer2.Length;
					}
					UnsafeUtility.MemCpy(unsafeAppendBuffer2.Ptr + unsafeAppendBuffer2.Length, reader.Ptr + reader.Offset, num7);
					unsafeAppendBuffer2.Length += num7;
				}
				if ((num6 & StaticCommands) != 0 && num5 == 0)
				{
					if (!flag)
					{
						num = unsafeAppendBuffer.Length;
					}
					UnsafeUtility.MemCpy(unsafeAppendBuffer.Ptr + unsafeAppendBuffer.Length, reader.Ptr + reader.Offset, num7);
					unsafeAppendBuffer.Length += num7;
				}
				if ((num6 & MetaCommands) != 0 || num5 > 0)
				{
					if (num5 > 0 && !flag)
					{
						num3 = unsafeAppendBuffer3.Length;
					}
					UnsafeUtility.MemCpy(unsafeAppendBuffer3.Ptr + unsafeAppendBuffer3.Length, reader.Ptr + reader.Offset, num7);
					unsafeAppendBuffer3.Length += num7;
				}
				if ((num6 & PushCommands) != 0)
				{
					nativeArray[num4] = unsafeAppendBuffer.Length - num7;
					nativeArray2[num4] = unsafeAppendBuffer2.Length - num7;
					nativeArray3[num4] = unsafeAppendBuffer3.Length - num7;
					num4++;
					if ((command & (CommandBuilder.Command)255) == CommandBuilder.Command.PushPersist)
					{
						num5++;
					}
					if (num4 >= 32)
					{
						return;
					}
				}
				else if ((num6 & PopCommands) != 0)
				{
					num4--;
					if (num4 < 0)
					{
						return;
					}
					if (num < nativeArray[num4])
					{
						unsafeAppendBuffer.Length = nativeArray[num4];
					}
					if (num2 < nativeArray2[num4])
					{
						unsafeAppendBuffer2.Length = nativeArray2[num4];
					}
					if (num3 < nativeArray3[num4])
					{
						unsafeAppendBuffer3.Length = nativeArray3[num4];
					}
					if ((command & (CommandBuilder.Command)255) == CommandBuilder.Command.PopPersist)
					{
						num5--;
						if (num5 < 0)
						{
							return;
						}
					}
				}
				reader.Offset += num7;
			}
			if (num4 != 0 || reader.Offset != reader.Size)
			{
				return;
			}
		}
		*staticBuffer = unsafeAppendBuffer;
		*dynamicBuffer = unsafeAppendBuffer2;
		*persistentBuffer = unsafeAppendBuffer3;
	}
}
