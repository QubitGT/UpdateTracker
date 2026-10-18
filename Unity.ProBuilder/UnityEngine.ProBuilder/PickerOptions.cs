namespace UnityEngine.ProBuilder;

public struct PickerOptions
{
	private static readonly PickerOptions k_Default = new PickerOptions
	{
		depthTest = true,
		rectSelectMode = RectSelectMode.Partial
	};

	public bool depthTest { get; set; }

	public RectSelectMode rectSelectMode { get; set; }

	public static PickerOptions Default => k_Default;

	public override bool Equals(object obj)
	{
		if (!(obj is PickerOptions))
		{
			return false;
		}
		return Equals((PickerOptions)obj);
	}

	public bool Equals(PickerOptions other)
	{
		if (depthTest == other.depthTest)
		{
			return rectSelectMode == other.rectSelectMode;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (depthTest.GetHashCode() * 397) ^ (int)rectSelectMode;
	}

	public static bool operator ==(PickerOptions a, PickerOptions b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PickerOptions a, PickerOptions b)
	{
		return !a.Equals(b);
	}
}
