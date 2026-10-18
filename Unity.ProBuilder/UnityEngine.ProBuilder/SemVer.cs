using System;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityEngine.ProBuilder;

[Serializable]
internal sealed class SemVer : IEquatable<SemVer>, IComparable<SemVer>, IComparable
{
	[SerializeField]
	private int m_Major = -1;

	[SerializeField]
	private int m_Minor = -1;

	[SerializeField]
	private int m_Patch = -1;

	[SerializeField]
	private int m_Build = -1;

	[SerializeField]
	private string m_Type;

	[SerializeField]
	private string m_Metadata;

	[SerializeField]
	private string m_Date;

	public const string DefaultStringFormat = "M.m.p-t.b";

	public int major => m_Major;

	public int minor => m_Minor;

	public int patch => m_Patch;

	public int build => m_Build;

	public string type
	{
		get
		{
			if (m_Type == null)
			{
				return "";
			}
			return m_Type;
		}
	}

	public string metadata
	{
		get
		{
			if (m_Metadata == null)
			{
				return "";
			}
			return m_Metadata;
		}
	}

	public string date
	{
		get
		{
			if (m_Date == null)
			{
				return "";
			}
			return m_Date;
		}
	}

	public SemVer MajorMinorPatch => new SemVer(major, minor, patch);

	public SemVer()
	{
		m_Major = 0;
		m_Minor = 0;
		m_Patch = 0;
		m_Build = -1;
		m_Type = null;
		m_Date = null;
		m_Metadata = null;
	}

	public SemVer(string formatted, string date = null)
	{
		m_Metadata = formatted;
		m_Date = date;
		if (TryGetVersionInfo(formatted, out var version))
		{
			m_Major = version.m_Major;
			m_Minor = version.m_Minor;
			m_Patch = version.m_Patch;
			m_Build = version.m_Build;
			m_Type = version.m_Type;
			m_Metadata = version.metadata;
		}
	}

	public SemVer(int major, int minor, int patch, int build = -1, string type = null, string date = null, string metadata = null)
	{
		m_Major = major;
		m_Minor = minor;
		m_Patch = patch;
		m_Build = build;
		m_Type = type;
		m_Metadata = metadata;
		m_Date = date;
	}

	public bool IsValid()
	{
		if (major != -1 && minor != -1)
		{
			return patch != -1;
		}
		return false;
	}

	public override bool Equals(object o)
	{
		if (o is SemVer)
		{
			return Equals((SemVer)o);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 13;
		if (IsValid())
		{
			num = num * 7 + major.GetHashCode();
			num = num * 7 + minor.GetHashCode();
			num = num * 7 + patch.GetHashCode();
			num = num * 7 + build.GetHashCode();
			return num * 7 + type.GetHashCode();
		}
		if (!string.IsNullOrEmpty(metadata))
		{
			return base.GetHashCode();
		}
		return metadata.GetHashCode();
	}

	public bool Equals(SemVer version)
	{
		if ((object)version == null)
		{
			return false;
		}
		if (IsValid() != version.IsValid())
		{
			return false;
		}
		if (IsValid())
		{
			if (major == version.major && minor == version.minor && patch == version.patch && type.Equals(version.type))
			{
				return build.Equals(version.build);
			}
			return false;
		}
		if (string.IsNullOrEmpty(metadata) || string.IsNullOrEmpty(version.metadata))
		{
			return false;
		}
		return metadata.Equals(version.metadata);
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as SemVer);
	}

	private static int WrapNoValue(int value)
	{
		if (value >= 0)
		{
			return value;
		}
		return int.MaxValue;
	}

	public int CompareTo(SemVer version)
	{
		if ((object)version == null)
		{
			return 1;
		}
		if (Equals(version))
		{
			return 0;
		}
		if (major > version.major)
		{
			return 1;
		}
		if (major < version.major)
		{
			return -1;
		}
		if (minor > version.minor)
		{
			return 1;
		}
		if (minor < version.minor)
		{
			return -1;
		}
		if (WrapNoValue(patch) > WrapNoValue(version.patch))
		{
			return 1;
		}
		if (WrapNoValue(patch) < WrapNoValue(version.patch))
		{
			return -1;
		}
		if (string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(version.type))
		{
			return 1;
		}
		if (!string.IsNullOrEmpty(type) && string.IsNullOrEmpty(version.type))
		{
			return -1;
		}
		if (WrapNoValue(build) > WrapNoValue(version.build))
		{
			return 1;
		}
		if (WrapNoValue(build) < WrapNoValue(version.build))
		{
			return -1;
		}
		return 0;
	}

	public static bool operator ==(SemVer left, SemVer right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(SemVer left, SemVer right)
	{
		return !(left == right);
	}

	public static bool operator <(SemVer left, SemVer right)
	{
		if ((object)left == null)
		{
			return (object)right != null;
		}
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(SemVer left, SemVer right)
	{
		if ((object)left == null)
		{
			return false;
		}
		return left.CompareTo(right) > 0;
	}

	public static bool operator <=(SemVer left, SemVer right)
	{
		if (!(left == right))
		{
			return left < right;
		}
		return true;
	}

	public static bool operator >=(SemVer left, SemVer right)
	{
		if (!(left == right))
		{
			return left > right;
		}
		return true;
	}

	public string ToString(string format)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		char[] array = format.ToCharArray();
		foreach (char c in array)
		{
			if (flag)
			{
				stringBuilder.Append(c);
				flag = false;
				continue;
			}
			switch (c)
			{
			case '\\':
				flag = true;
				break;
			case 'M':
				stringBuilder.Append(major);
				break;
			case 'm':
				stringBuilder.Append(minor);
				break;
			case 'p':
				stringBuilder.Append(patch);
				break;
			case 'b':
				stringBuilder.Append(build);
				break;
			case 'T':
			case 't':
				stringBuilder.Append(type);
				break;
			case 'd':
				stringBuilder.Append(date);
				break;
			case 'D':
				stringBuilder.Append(metadata);
				break;
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ToString("M.m.p"));
		if (!string.IsNullOrEmpty(type))
		{
			stringBuilder.Append("-");
			stringBuilder.Append(type);
			if (build > -1)
			{
				stringBuilder.Append(".");
				stringBuilder.Append(build.ToString());
			}
		}
		if (!string.IsNullOrEmpty(date))
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(date);
		}
		return stringBuilder.ToString();
	}

	public static bool TryGetVersionInfo(string input, out SemVer version)
	{
		version = new SemVer();
		bool flag = false;
		try
		{
			Match match = Regex.Match(input, "^([0-9]+\\.[0-9]+\\.[0-9]+)");
			if (!match.Success)
			{
				return false;
			}
			string[] array = match.Value.Split('.');
			int.TryParse(array[0], out version.m_Major);
			int.TryParse(array[1], out version.m_Minor);
			int.TryParse(array[2], out version.m_Patch);
			flag = true;
			Match match2 = Regex.Match(input, "(?i)(?<=\\-)[a-z0-9\\-]+");
			if (match2.Success)
			{
				version.m_Type = match2.Value;
			}
			Match match3 = Regex.Match(input, "(?i)(?<=\\-[a-z0-9\\-]+\\.)[0-9]+");
			version.m_Build = (match3.Success ? GetBuildNumber(match3.Value) : (-1));
			Match match4 = Regex.Match(input, "(?<=\\+).+");
			if (match4.Success)
			{
				version.m_Metadata = match4.Value;
			}
		}
		catch
		{
			flag = false;
		}
		return flag;
	}

	private static int GetBuildNumber(string input)
	{
		Match match = Regex.Match(input, "[0-9]+");
		int result = 0;
		if (match.Success && int.TryParse(match.Value, out result))
		{
			return result;
		}
		return -1;
	}
}
