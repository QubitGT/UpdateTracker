using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace g3;

internal class DS3Reader : IMeshReader
{
	private Dictionary<string, int> warningCount = new Dictionary<string, int>();

	private string MeshName;

	private bool hasMesh;

	private bool is3ds;

	public event ParsingMessagesHandler warningEvent;

	public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder)
	{
		string text = "";
		MeshName = "";
		hasMesh = false;
		is3ds = false;
		while (true)
		{
			ushort num;
			try
			{
				num = reader.ReadUInt16();
			}
			catch
			{
				break;
			}
			text = num.ToString("X");
			uint num2 = reader.ReadUInt32();
			switch (text)
			{
			case "4D4D":
				is3ds = true;
				reader.ReadChars(10);
				break;
			case "3D3D":
				reader.ReadChars(10);
				break;
			case "4000":
			{
				List<char> list2 = new List<char>();
				while (true)
				{
					char c2 = reader.ReadChar();
					if (c2 == '\0')
					{
						break;
					}
					list2.Add(c2);
				}
				MeshName = new string(Enumerable.ToArray(list2));
				break;
			}
			case "4100":
				builder.AppendNewMesh(bHaveVtxNormals: false, bHaveVtxColors: false, bHaveVtxUVs: false, bHaveFaceGroups: false);
				if (builder.SupportsMetaData)
				{
					builder.AppendMetaData("name", MeshName);
				}
				break;
			case "4110":
			{
				ushort num5 = reader.ReadUInt16();
				for (int i = 0; i < num5; i++)
				{
					double x = reader.ReadSingle();
					double y = reader.ReadSingle();
					double z = reader.ReadSingle();
					builder.AppendVertex(x, y, z);
				}
				break;
			}
			case "4120":
			{
				ushort num7 = reader.ReadUInt16();
				for (int k = 0; k < num7; k++)
				{
					int i2 = reader.ReadInt16();
					int j2 = reader.ReadInt16();
					int k2 = reader.ReadInt16();
					reader.ReadUInt16();
					builder.AppendTriangle(i2, j2, k2);
				}
				break;
			}
			case "4130":
			{
				List<char> list = new List<char>();
				while (true)
				{
					char c = reader.ReadChar();
					if (c == '\0')
					{
						break;
					}
					list.Add(c);
				}
				new string(Enumerable.ToArray(list));
				ushort num6 = reader.ReadUInt16();
				for (int j = 0; j < num6; j++)
				{
					reader.ReadUInt16();
				}
				break;
			}
			case "4140":
			{
				ushort num3 = reader.ReadUInt16();
				for (ushort num4 = 0; num4 < num3; num4++)
				{
					Vector2f uvs = new Vector2f(reader.ReadSingle(), reader.ReadSingle());
					builder.SetVertexUV(num4, uvs);
				}
				break;
			}
			default:
				reader.ReadChars((int)(num2 - 6));
				break;
			}
		}
		if (!is3ds)
		{
			return new IOReadResult(IOCode.FileAccessError, "File is not in .3DS format");
		}
		if (!hasMesh)
		{
			return new IOReadResult(IOCode.FileParsingError, "no mesh found in file");
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder)
	{
		return new IOReadResult(IOCode.FormatNotSupportedError, "text read not supported for 3DS format");
	}

	private void emit_warning(string sMessage)
	{
		string key = sMessage.Substring(0, 15);
		int num = (warningCount.ContainsKey(key) ? warningCount[key] : 0);
		num++;
		warningCount[key] = num;
		if (num <= 10)
		{
			if (num == 10)
			{
				sMessage += " (additional message surpressed)";
			}
			this.warningEvent?.Invoke(sMessage, null);
		}
	}
}
