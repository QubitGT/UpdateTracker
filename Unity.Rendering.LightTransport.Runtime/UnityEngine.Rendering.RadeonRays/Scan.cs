namespace UnityEngine.Rendering.RadeonRays;

internal class Scan
{
	private readonly ComputeShader shaderScan;

	private readonly int kernelScan;

	private readonly ComputeShader shaderReduce;

	private readonly int kernelReduce;

	private const uint kKeysPerThread = 4u;

	private const uint kGroupSize = 256u;

	private const uint kKeysPerGroup = 1024u;

	public Scan(RadeonRaysShaders shaders)
	{
		shaderScan = shaders.blockScan;
		kernelScan = shaderScan.FindKernel("BlockScanAdd");
		shaderReduce = shaders.blockReducePart;
		kernelReduce = shaderReduce.FindKernel("BlockReducePart");
	}

	public void Execute(CommandBuffer cmd, GraphicsBuffer buffer, uint inputKeysOffset, uint outputKeysOffset, uint scratchDataOffset, uint size)
	{
		if (size > 1024)
		{
			uint num = Common.CeilDivide(size, 1024u);
			SetState(cmd, shaderReduce, kernelReduce, size, buffer, inputKeysOffset, scratchDataOffset, outputKeysOffset);
			cmd.DispatchCompute(shaderReduce, kernelReduce, (int)num, 1, 1);
			if (num > 1024)
			{
				uint num2 = Common.CeilDivide(num, 1024u);
				SetState(cmd, shaderReduce, kernelReduce, num, buffer, scratchDataOffset, scratchDataOffset + num, scratchDataOffset);
				cmd.DispatchCompute(shaderReduce, kernelReduce, (int)num2, 1, 1);
				Common.EnableKeyword(cmd, shaderScan, "ADD_PART_SUM", enable: false);
				SetState(cmd, shaderScan, kernelScan, num2, buffer, scratchDataOffset + num, scratchDataOffset, scratchDataOffset + num);
				cmd.DispatchCompute(shaderScan, kernelScan, 1, 1, 1);
			}
			Common.EnableKeyword(cmd, shaderScan, "ADD_PART_SUM", num > 1024);
			SetState(cmd, shaderScan, kernelScan, num, buffer, scratchDataOffset, scratchDataOffset + num, scratchDataOffset);
			uint threadGroupsX = Common.CeilDivide(num, 1024u);
			cmd.DispatchCompute(shaderScan, kernelScan, (int)threadGroupsX, 1, 1);
		}
		Common.EnableKeyword(cmd, shaderScan, "ADD_PART_SUM", size > 1024);
		SetState(cmd, shaderScan, kernelScan, size, buffer, inputKeysOffset, scratchDataOffset, outputKeysOffset);
		uint threadGroupsX2 = Common.CeilDivide(size, 1024u);
		cmd.DispatchCompute(shaderScan, kernelScan, (int)threadGroupsX2, 1, 1);
	}

	private void SetState(CommandBuffer cmd, ComputeShader shader, int kernelIndex, uint size, GraphicsBuffer buffer, uint inputKeysOffset, uint scratchDataOffset, uint outputKeysOffset)
	{
		cmd.SetComputeIntParam(shader, SID.g_constants_num_keys, (int)size);
		cmd.SetComputeIntParam(shader, SID.g_constants_input_keys_offset, (int)inputKeysOffset);
		cmd.SetComputeIntParam(shader, SID.g_constants_part_sums_offset, (int)scratchDataOffset);
		cmd.SetComputeIntParam(shader, SID.g_constants_output_keys_offset, (int)outputKeysOffset);
		cmd.SetComputeBufferParam(shader, kernelIndex, SID.g_buffer, buffer);
	}

	public static ulong GetScratchDataSizeInDwords(uint size)
	{
		if (size <= 1024)
		{
			return 0uL;
		}
		uint num = Common.CeilDivide(size, 1024u);
		if (num <= 1024)
		{
			return num;
		}
		uint num2 = Common.CeilDivide(num, 1024u);
		return num + num2;
	}
}
