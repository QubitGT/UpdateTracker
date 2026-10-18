using Unity.Mathematics;

namespace Drawing;

public struct LabelAlignment
{
	public float2 relativePivot;

	public float2 pixelOffset;

	public static readonly LabelAlignment TopLeft = new LabelAlignment
	{
		relativePivot = new float2(0f, 1f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment MiddleLeft = new LabelAlignment
	{
		relativePivot = new float2(0f, 0.5f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment BottomLeft = new LabelAlignment
	{
		relativePivot = new float2(0f, 0f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment BottomCenter = new LabelAlignment
	{
		relativePivot = new float2(0.5f, 0f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment BottomRight = new LabelAlignment
	{
		relativePivot = new float2(1f, 0f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment MiddleRight = new LabelAlignment
	{
		relativePivot = new float2(1f, 0.5f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment TopRight = new LabelAlignment
	{
		relativePivot = new float2(1f, 1f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment TopCenter = new LabelAlignment
	{
		relativePivot = new float2(0.5f, 1f),
		pixelOffset = new float2(0f, 0f)
	};

	public static readonly LabelAlignment Center = new LabelAlignment
	{
		relativePivot = new float2(0.5f, 0.5f),
		pixelOffset = new float2(0f, 0f)
	};

	public LabelAlignment withPixelOffset(float x, float y)
	{
		LabelAlignment result = default(LabelAlignment);
		result.relativePivot = relativePivot;
		result.pixelOffset = new float2(x, y);
		return result;
	}
}
