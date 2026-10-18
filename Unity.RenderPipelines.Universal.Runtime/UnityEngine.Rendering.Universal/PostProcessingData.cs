namespace UnityEngine.Rendering.Universal;

public struct PostProcessingData
{
	private ContextContainer frameData;

	internal UniversalPostProcessingData universalPostProcessingData => frameData.Get<UniversalPostProcessingData>();

	public ref ColorGradingMode gradingMode => ref frameData.Get<UniversalPostProcessingData>().gradingMode;

	public ref int lutSize => ref frameData.Get<UniversalPostProcessingData>().lutSize;

	public ref bool useFastSRGBLinearConversion => ref frameData.Get<UniversalPostProcessingData>().useFastSRGBLinearConversion;

	public ref bool supportScreenSpaceLensFlare => ref frameData.Get<UniversalPostProcessingData>().supportScreenSpaceLensFlare;

	public ref bool supportDataDrivenLensFlare => ref frameData.Get<UniversalPostProcessingData>().supportDataDrivenLensFlare;

	internal PostProcessingData(ContextContainer frameData)
	{
		this.frameData = frameData;
	}
}
