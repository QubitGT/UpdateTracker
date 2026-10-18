using System;
using UnityEngine;

namespace Drawing;

public static class Palette
{
	public static class Pure
	{
		public static readonly Color Yellow = new Color(1f, 1f, 0f, 1f);

		public static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

		public static readonly Color Grey = new Color(0.5f, 0.5f, 0.5f, 1f);

		public static readonly Color Magenta = new Color(1f, 0f, 1f, 1f);

		public static readonly Color Cyan = new Color(0f, 1f, 1f, 1f);

		public static readonly Color Red = new Color(1f, 0f, 0f, 1f);

		public static readonly Color Black = new Color(0f, 0f, 0f, 1f);

		public static readonly Color White = new Color(1f, 1f, 1f, 1f);

		public static readonly Color Blue = new Color(0f, 0f, 1f, 1f);

		public static readonly Color Green = new Color(0f, 1f, 0f, 1f);
	}

	public static class Colorbrewer
	{
		public static class Set1
		{
			public static readonly Color Red = new Color(76f / 85f, 0.101960786f, 0.10980392f, 1f);

			public static readonly Color Blue = new Color(11f / 51f, 42f / 85f, 0.72156864f, 1f);

			public static readonly Color Green = new Color(0.3019608f, 35f / 51f, 0.2901961f, 1f);

			public static readonly Color Purple = new Color(0.59607846f, 26f / 85f, 0.6392157f, 1f);

			public static readonly Color Orange = new Color(1f, 0.49803922f, 0f, 1f);

			public static readonly Color Yellow = new Color(1f, 1f, 0.2f, 1f);

			public static readonly Color Brown = new Color(0.6509804f, 0.3372549f, 8f / 51f, 1f);

			public static readonly Color Pink = new Color(0.96862745f, 43f / 85f, 0.7490196f, 1f);

			public static readonly Color Grey = new Color(0.6f, 0.6f, 0.6f, 1f);
		}

		public static class Blues
		{
			private static readonly Color[] Colors = new Color[45]
			{
				new Color(0.16862746f, 28f / 51f, 38f / 51f),
				new Color(0.6509804f, 63f / 85f, 73f / 85f),
				new Color(0.16862746f, 28f / 51f, 38f / 51f),
				new Color(0.9254902f, 77f / 85f, 0.9490196f),
				new Color(0.6509804f, 63f / 85f, 73f / 85f),
				new Color(0.16862746f, 28f / 51f, 38f / 51f),
				new Color(0.94509804f, 14f / 15f, 82f / 85f),
				new Color(63f / 85f, 67f / 85f, 0.88235295f),
				new Color(0.45490196f, 0.6627451f, 69f / 85f),
				new Color(1f / 51f, 0.4392157f, 0.6901961f),
				new Color(0.94509804f, 14f / 15f, 82f / 85f),
				new Color(63f / 85f, 67f / 85f, 0.88235295f),
				new Color(0.45490196f, 0.6627451f, 69f / 85f),
				new Color(0.16862746f, 28f / 51f, 38f / 51f),
				new Color(0.015686275f, 0.3529412f, 47f / 85f),
				new Color(0.94509804f, 14f / 15f, 82f / 85f),
				new Color(0.8156863f, 0.81960785f, 46f / 51f),
				new Color(0.6509804f, 63f / 85f, 73f / 85f),
				new Color(0.45490196f, 0.6627451f, 69f / 85f),
				new Color(0.16862746f, 28f / 51f, 38f / 51f),
				new Color(0.015686275f, 0.3529412f, 47f / 85f),
				new Color(0.94509804f, 14f / 15f, 82f / 85f),
				new Color(0.8156863f, 0.81960785f, 46f / 51f),
				new Color(0.6509804f, 63f / 85f, 73f / 85f),
				new Color(0.45490196f, 0.6627451f, 69f / 85f),
				new Color(18f / 85f, 48f / 85f, 64f / 85f),
				new Color(1f / 51f, 0.4392157f, 0.6901961f),
				new Color(1f / 85f, 26f / 85f, 41f / 85f),
				new Color(1f, 0.96862745f, 0.9843137f),
				new Color(0.9254902f, 77f / 85f, 0.9490196f),
				new Color(0.8156863f, 0.81960785f, 46f / 51f),
				new Color(0.6509804f, 63f / 85f, 73f / 85f),
				new Color(0.45490196f, 0.6627451f, 69f / 85f),
				new Color(18f / 85f, 48f / 85f, 64f / 85f),
				new Color(1f / 51f, 0.4392157f, 0.6901961f),
				new Color(1f / 85f, 26f / 85f, 41f / 85f),
				new Color(1f, 0.96862745f, 0.9843137f),
				new Color(0.9254902f, 77f / 85f, 0.9490196f),
				new Color(0.8156863f, 0.81960785f, 46f / 51f),
				new Color(0.6509804f, 63f / 85f, 73f / 85f),
				new Color(0.45490196f, 0.6627451f, 69f / 85f),
				new Color(18f / 85f, 48f / 85f, 64f / 85f),
				new Color(1f / 51f, 0.4392157f, 0.6901961f),
				new Color(0.015686275f, 0.3529412f, 47f / 85f),
				new Color(0.007843138f, 0.21960784f, 0.34509805f)
			};

			public static Color GetColor(int classes, int index)
			{
				if (index < 0 || index >= classes)
				{
					throw new ArgumentOutOfRangeException("index", "Index must be less than classes and at least 0");
				}
				if (classes <= 0 || classes > 9)
				{
					throw new ArgumentOutOfRangeException("classes", "Only up to 9 classes are supported");
				}
				return Colors[(classes - 1) * classes / 2 + index];
			}
		}
	}
}
