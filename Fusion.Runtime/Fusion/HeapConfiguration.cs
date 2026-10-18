using System;
using UnityEngine;

namespace Fusion;

[Serializable]
public class HeapConfiguration
{
	private const int PageCountMin = 16;

	private const int PageCountMax = 4096;

	[InlineHelp]
	public PageSizes PageShift = PageSizes._32Kb;

	[InlineHelp]
	[Range(16f, 4096f)]
	public int PageCount = 256;

	[InlineHelp]
	[HideInInspector]
	public int GlobalsSize;

	internal Allocator.Config ToAllocatorConfig()
	{
		return new Allocator.Config(PageShift, PageCount, GlobalsSize);
	}

	public HeapConfiguration Init(int globalsSize)
	{
		HeapConfiguration heapConfiguration = (HeapConfiguration)MemberwiseClone();
		heapConfiguration.GlobalsSize = globalsSize;
		return heapConfiguration;
	}

	public override string ToString()
	{
		return $"[HeapConfiguration: {PageShift}/{PageCount}/{GlobalsSize}]";
	}
}
