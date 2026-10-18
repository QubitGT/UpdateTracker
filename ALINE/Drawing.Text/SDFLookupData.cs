using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Drawing.Text;

internal struct SDFLookupData
{
	public NativeArray<SDFCharacter> characters;

	private Dictionary<char, int> lookup;

	public Material material;

	public const ushort Newline = ushort.MaxValue;

	public SDFLookupData(SDFFont font)
	{
		int num = 0;
		SDFCharacter value = font.characters[0];
		for (int i = 0; i < font.characters.Length; i++)
		{
			if (font.characters[i].codePoint == '?')
			{
				value = font.characters[i];
			}
			if (font.characters[i].codePoint >= '\u0080')
			{
				num++;
			}
		}
		characters = new NativeArray<SDFCharacter>(128 + num, Allocator.Persistent);
		for (int j = 0; j < characters.Length; j++)
		{
			characters[j] = value;
		}
		lookup = new Dictionary<char, int>();
		material = font.material;
		num = 0;
		for (int k = 0; k < font.characters.Length; k++)
		{
			SDFCharacter value2 = font.characters[k];
			int num2 = value2.codePoint;
			if (value2.codePoint >= '\u0080')
			{
				num2 = 128 + num;
				num++;
			}
			characters[num2] = value2;
			lookup[value2.codePoint] = num2;
		}
	}

	public int GetIndex(char c)
	{
		if (lookup.TryGetValue(c, out var value))
		{
			return value;
		}
		if (c == '\n')
		{
			return 65535;
		}
		return lookup['?'];
	}

	public void Dispose()
	{
		if (characters.IsCreated)
		{
			characters.Dispose();
		}
	}
}
