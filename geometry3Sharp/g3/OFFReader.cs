using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

internal class OFFReader : IMeshReader
{
	private Dictionary<string, int> warningCount = new Dictionary<string, int>();

	public event ParsingMessagesHandler warningEvent;

	public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder)
	{
		return new IOReadResult(IOCode.FormatNotSupportedError, "binary read not supported for OFF format");
	}

	public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder)
	{
		if (!reader.ReadLine().StartsWith("OFF"))
		{
			return new IOReadResult(IOCode.FileParsingError, "ascii OFF file must start with OFF header");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (reader.Peek() >= 0)
		{
			string text = reader.ReadLine();
			num3++;
			string[] array = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0 && !array[0].StartsWith("#"))
			{
				if (array.Length != 3)
				{
					return new IOReadResult(IOCode.FileParsingError, "first non-comment line of OFF must be vertex/tri/edge counts, found: " + text);
				}
				num = int.Parse(array[0]);
				num2 = int.Parse(array[1]);
				break;
			}
		}
		builder.AppendNewMesh(bHaveVtxNormals: false, bHaveVtxColors: false, bHaveVtxUVs: false, bHaveFaceGroups: false);
		int num4 = 0;
		while (num4 < num && reader.Peek() > 0)
		{
			string text2 = reader.ReadLine();
			num3++;
			string[] array2 = text2.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length != 0 && !array2[0].StartsWith("#"))
			{
				if (array2.Length != 3)
				{
					emit_warning("found invalid OFF vertex line: " + text2);
				}
				double x = double.Parse(array2[0]);
				double y = double.Parse(array2[1]);
				double z = double.Parse(array2[2]);
				builder.AppendVertex(x, y, z);
				num4++;
			}
		}
		if (num4 < num)
		{
			return new IOReadResult(IOCode.FileParsingError, $"File specified {num} vertices but only found {num4}");
		}
		int num5 = 0;
		while (num5 < num2 && reader.Peek() > 0)
		{
			string text3 = reader.ReadLine();
			num3++;
			string[] array3 = text3.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
			if (array3.Length != 0 && !array3[0].StartsWith("#"))
			{
				if (array3.Length < 4)
				{
					emit_warning("found invalid OFF triangle line: " + text3);
				}
				if (int.Parse(array3[0]) != 3)
				{
					emit_warning("found non-triangle polygon in OFF, currently unsupported: " + text3);
				}
				int i = int.Parse(array3[1]);
				int j = int.Parse(array3[2]);
				int k = int.Parse(array3[3]);
				builder.AppendTriangle(i, j, k);
				num5++;
			}
		}
		if (num5 < num2)
		{
			emit_warning($"File specified {num2} triangles but only found {num5}");
		}
		return new IOReadResult(IOCode.Ok, "");
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
