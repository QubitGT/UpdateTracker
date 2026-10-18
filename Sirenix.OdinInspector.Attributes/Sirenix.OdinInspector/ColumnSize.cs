namespace Sirenix.OdinInspector;

public struct ColumnSize
{
	public ColumnType ColumnType;

	public float Value;

	public static ColumnSize Auto => new ColumnSize(ColumnType.Auto, 0f);

	public ColumnSize(ColumnType columnType, float value)
	{
		ColumnType = columnType;
		Value = value;
	}

	public static ColumnSize Percent(float percentage)
	{
		return new ColumnSize(ColumnType.Percent, percentage);
	}

	public static ColumnSize Pixel(float pixels)
	{
		return new ColumnSize(ColumnType.Pixel, pixels);
	}

	public override string ToString()
	{
		return ColumnType switch
		{
			ColumnType.Auto => "Auto", 
			ColumnType.Percent => $"{Value * 100f} %", 
			ColumnType.Pixel => $"{Value} px", 
			_ => base.ToString(), 
		};
	}
}
