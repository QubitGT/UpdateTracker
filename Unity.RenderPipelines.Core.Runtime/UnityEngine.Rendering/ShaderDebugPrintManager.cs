using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

public sealed class ShaderDebugPrintManager
{
	private static class Profiling
	{
		public static readonly ProfilingSampler BufferReadComplete = new ProfilingSampler("ShaderDebugPrintManager.BufferReadComplete");
	}

	private enum DebugValueType
	{
		TypeUint = 1,
		TypeInt,
		TypeFloat,
		TypeUint2,
		TypeInt2,
		TypeFloat2,
		TypeUint3,
		TypeInt3,
		TypeFloat3,
		TypeUint4,
		TypeInt4,
		TypeFloat4,
		TypeBool
	}

	private static readonly ShaderDebugPrintManager s_Instance = new ShaderDebugPrintManager();

	private const int k_FramesInFlight = 4;

	private const int k_MaxBufferElements = 16384;

	private List<GraphicsBuffer> m_OutputBuffers = new List<GraphicsBuffer>();

	private List<AsyncGPUReadbackRequest> m_ReadbackRequests = new List<AsyncGPUReadbackRequest>();

	private Action<AsyncGPUReadbackRequest> m_BufferReadCompleteAction;

	private int m_FrameCounter;

	private bool m_FrameCleared;

	private string m_OutputLine = "";

	private Action<string> m_OutputAction;

	private static readonly int m_ShaderPropertyIDInputMouse = Shader.PropertyToID("_ShaderDebugPrintInputMouse");

	private static readonly int m_ShaderPropertyIDInputFrame = Shader.PropertyToID("_ShaderDebugPrintInputFrame");

	private static readonly int m_shaderDebugOutputData = Shader.PropertyToID("shaderDebugOutputData");

	private const uint k_TypeHasTag = 128u;

	public static ShaderDebugPrintManager instance => s_Instance;

	public string outputLine => m_OutputLine;

	public Action<string> outputAction
	{
		set
		{
			m_OutputAction = value;
		}
	}

	private int DebugValueTypeToElemSize(DebugValueType type)
	{
		switch (type)
		{
		case DebugValueType.TypeUint:
		case DebugValueType.TypeInt:
		case DebugValueType.TypeFloat:
		case DebugValueType.TypeBool:
			return 1;
		case DebugValueType.TypeUint2:
		case DebugValueType.TypeInt2:
		case DebugValueType.TypeFloat2:
			return 2;
		case DebugValueType.TypeUint3:
		case DebugValueType.TypeInt3:
		case DebugValueType.TypeFloat3:
			return 3;
		case DebugValueType.TypeUint4:
		case DebugValueType.TypeInt4:
		case DebugValueType.TypeFloat4:
			return 4;
		default:
			return 0;
		}
	}

	private ShaderDebugPrintManager()
	{
		for (int i = 0; i < 4; i++)
		{
			m_OutputBuffers.Add(new GraphicsBuffer(GraphicsBuffer.Target.Structured, 16384, 4));
			m_ReadbackRequests.Add(default(AsyncGPUReadbackRequest));
		}
		m_BufferReadCompleteAction = BufferReadComplete;
		m_OutputAction = DefaultOutput;
	}

	public void SetShaderDebugPrintInputConstants(CommandBuffer cmd, ShaderDebugPrintInput input)
	{
		Vector4 value = new Vector4(input.pos.x, input.pos.y, input.leftDown ? 1 : 0, input.rightDown ? 1 : 0);
		cmd.SetGlobalVector(m_ShaderPropertyIDInputMouse, value);
		cmd.SetGlobalInt(m_ShaderPropertyIDInputFrame, m_FrameCounter);
	}

	public void SetShaderDebugPrintBindings(CommandBuffer cmd)
	{
		int index = m_FrameCounter % 4;
		if (!m_ReadbackRequests[index].done)
		{
			m_ReadbackRequests[index].WaitForCompletion();
		}
		cmd.SetGlobalBuffer(m_shaderDebugOutputData, m_OutputBuffers[index]);
		ClearShaderDebugPrintBuffer();
	}

	private void ClearShaderDebugPrintBuffer()
	{
		if (!m_FrameCleared)
		{
			int index = m_FrameCounter % 4;
			NativeArray<uint> data = new NativeArray<uint>(1, Allocator.Temp);
			data[0] = 0u;
			m_OutputBuffers[index].SetData(data, 0, 0, 1);
			m_FrameCleared = true;
		}
	}

	private unsafe void BufferReadComplete(AsyncGPUReadbackRequest request)
	{
		using (new ProfilingScope(Profiling.BufferReadComplete))
		{
			if (!request.hasError)
			{
				NativeArray<uint> data = request.GetData<uint>();
				uint num = data[0];
				if (num >= 16384)
				{
					num = 16384u;
					Debug.LogWarning("Debug Shader Print Buffer Full!");
				}
				string text = "";
				if (num != 0)
				{
					text = text + "Frame #" + m_FrameCounter + ": ";
				}
				uint* unsafePtr = (uint*)data.GetUnsafePtr();
				int num2 = 1;
				while (num2 < num)
				{
					DebugValueType debugValueType = (DebugValueType)((int)data[num2] & 0xF);
					if ((data[num2] & 0x80) == 128 && num2 + 1 < num)
					{
						uint num3 = data[num2 + 1];
						num2++;
						for (int i = 0; i < 4; i++)
						{
							char c = (char)(num3 & 0xFFu);
							if (c != 0)
							{
								text += c;
								num3 >>= 8;
							}
						}
						text += " ";
					}
					int num4 = DebugValueTypeToElemSize(debugValueType);
					if (num2 + num4 > num)
					{
						break;
					}
					num2++;
					switch (debugValueType)
					{
					case DebugValueType.TypeUint:
						text += $"{data[num2]}u";
						break;
					case DebugValueType.TypeInt:
					{
						int num5 = (int)unsafePtr[num2];
						text += num5;
						break;
					}
					case DebugValueType.TypeFloat:
					{
						float num6 = *(float*)(unsafePtr + num2);
						text += $"{num6}f";
						break;
					}
					case DebugValueType.TypeUint2:
					{
						uint* ptr9 = unsafePtr + num2;
						text += $"uint2({*ptr9}, {ptr9[1]})";
						break;
					}
					case DebugValueType.TypeInt2:
					{
						int* ptr8 = (int*)(unsafePtr + num2);
						text += $"int2({*ptr8}, {ptr8[1]})";
						break;
					}
					case DebugValueType.TypeFloat2:
					{
						float* ptr7 = (float*)(unsafePtr + num2);
						text += $"float2({*ptr7}, {ptr7[1]})";
						break;
					}
					case DebugValueType.TypeUint3:
					{
						uint* ptr6 = unsafePtr + num2;
						text += $"uint3({*ptr6}, {ptr6[1]}, {ptr6[2]})";
						break;
					}
					case DebugValueType.TypeInt3:
					{
						int* ptr5 = (int*)(unsafePtr + num2);
						text += $"int3({*ptr5}, {ptr5[1]}, {ptr5[2]})";
						break;
					}
					case DebugValueType.TypeFloat3:
					{
						float* ptr4 = (float*)(unsafePtr + num2);
						text += $"float3({*ptr4}, {ptr4[1]}, {ptr4[2]})";
						break;
					}
					case DebugValueType.TypeUint4:
					{
						uint* ptr3 = unsafePtr + num2;
						text += $"uint4({*ptr3}, {ptr3[1]}, {ptr3[2]}, {ptr3[3]})";
						break;
					}
					case DebugValueType.TypeInt4:
					{
						int* ptr2 = (int*)(unsafePtr + num2);
						text += $"int4({*ptr2}, {ptr2[1]}, {ptr2[2]}, {ptr2[3]})";
						break;
					}
					case DebugValueType.TypeFloat4:
					{
						float* ptr = (float*)(unsafePtr + num2);
						text += $"float4({*ptr}, {ptr[1]}, {ptr[2]}, {ptr[3]})";
						break;
					}
					case DebugValueType.TypeBool:
						text += ((data[num2] == 0) ? "False" : "True");
						break;
					default:
						num2 = (int)num;
						break;
					}
					num2 += num4;
					text += " ";
				}
				if (num != 0)
				{
					m_OutputLine = text;
					m_OutputAction(text);
				}
			}
			else
			{
				m_OutputLine = "Error at read back!";
				m_OutputAction("Error at read back!");
			}
		}
	}

	public void EndFrame()
	{
		int index = m_FrameCounter % 4;
		m_ReadbackRequests[index] = AsyncGPUReadback.Request(m_OutputBuffers[index], m_BufferReadCompleteAction);
		m_FrameCounter++;
		m_FrameCleared = false;
	}

	public void PrintImmediate()
	{
		int index = m_FrameCounter % 4;
		AsyncGPUReadbackRequest obj = AsyncGPUReadback.Request(m_OutputBuffers[index]);
		obj.WaitForCompletion();
		m_BufferReadCompleteAction(obj);
		m_FrameCounter++;
		m_FrameCleared = false;
	}

	public void DefaultOutput(string line)
	{
		Debug.Log(line);
	}
}
