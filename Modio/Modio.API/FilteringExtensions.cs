namespace Modio.API;

public static class FilteringExtensions
{
	public static string ClearText(this Filtering filtering)
	{
		return filtering switch
		{
			Filtering.Like => "-lk", 
			Filtering.Not => "-not", 
			Filtering.NotLike => "-not-lk", 
			Filtering.In => "-in", 
			Filtering.NotIn => "-not-in", 
			Filtering.Max => "-max", 
			Filtering.Min => "-min", 
			Filtering.BitwiseAnd => "-bitwise-and", 
			_ => string.Empty, 
		};
	}
}
