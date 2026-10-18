using System;
using System.Collections.Generic;

namespace g3;

public static class TilingUtil
{
	public static List<Vector2d> BoundedRegularTiling2(AxisAlignedBox2d element, AxisAlignedBox2d bounds, double spacing)
	{
		Vector2d vector2d = -element.Min;
		double width = element.Width;
		double height = element.Height;
		int num = Math.Max(1, (int)(bounds.Width / width));
		double num2 = (double)(num - 1) * spacing;
		while (num > 1 && bounds.Width - (width * (double)num + num2) < 0.0)
		{
			num--;
		}
		int num3 = Math.Max(1, (int)(bounds.Height / height));
		double num4 = (double)(num3 - 1) * spacing;
		while (num3 > 1 && bounds.Height - (height * (double)num3 + num4) < 0.0)
		{
			num3--;
		}
		List<Vector2d> list = new List<Vector2d>();
		for (int i = 0; i < num3; i++)
		{
			double y = (double)i * height + (double)i * spacing;
			for (int j = 0; j < num; j++)
			{
				double x = (double)j * width + (double)j * spacing;
				list.Add(new Vector2d(x, y) + vector2d + bounds.Min);
			}
		}
		return list;
	}

	public static List<Vector2d> BoundedCircleTiling2(AxisAlignedBox2d element, AxisAlignedBox2d bounds, double spacing)
	{
		Vector2d vector2d = -element.Min;
		double width = element.Width;
		double height = element.Height;
		if (!MathUtil.EpsilonEqual(width, height, 1.1920928955078125E-07))
		{
			throw new Exception("BoundedHexTiling2: input box is not square");
		}
		double num = width / 2.0;
		Hexagon2d obj = new Hexagon2d(element.Center, num, Hexagon2d.TopModes.Tip)
		{
			InnerRadius = num
		};
		double horzSpacing = obj.HorzSpacing;
		double vertSpacing = obj.VertSpacing;
		int num2 = Math.Max(1, (int)(bounds.Height / vertSpacing));
		double num3 = (double)(num2 - 1) * spacing;
		while (num2 > 1 && bounds.Height - (vertSpacing * (double)num2 + num3) < 0.0)
		{
			num2--;
		}
		int num4 = Math.Max(1, (int)(bounds.Width / horzSpacing));
		double num5 = (double)(num4 - 1) * spacing;
		while (num4 > 1 && bounds.Width - (horzSpacing * (double)num4 + num5) < 0.0)
		{
			num4--;
		}
		int num6 = num4;
		num5 = (double)(num6 - 1) * spacing;
		if (num2 > 0 && horzSpacing * (double)num6 + num5 + horzSpacing * 0.5 > bounds.Width)
		{
			num6--;
			num5 = (double)(num6 - 1) * spacing;
		}
		List<Vector2d> list = new List<Vector2d>();
		for (int i = 0; i < num2; i++)
		{
			double y = (double)i * vertSpacing + (double)i * spacing;
			double num7 = horzSpacing * 0.5;
			int num8 = num6;
			if (i % 2 == 0)
			{
				num7 = 0.0;
				num8 = num4;
			}
			for (int j = 0; j < num8; j++)
			{
				double x = num7 + (double)j * horzSpacing + (double)j * spacing;
				list.Add(new Vector2d(x, y) + vector2d + bounds.Min);
			}
		}
		return list;
	}
}
