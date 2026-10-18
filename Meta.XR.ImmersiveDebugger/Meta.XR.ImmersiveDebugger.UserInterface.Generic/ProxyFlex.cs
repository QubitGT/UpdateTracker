using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

internal class ProxyFlex<ControllerType, ProxyControllerType> where ControllerType : Controller, new() where ProxyControllerType : ProxyController<ControllerType>, new()
{
	private readonly int _maximumNumberOfProxies;

	private readonly Dictionary<ControllerType, ProxyController<ControllerType>> _targetsDictionary = new Dictionary<ControllerType, ProxyController<ControllerType>>();

	private readonly ScrollView _scrollView;

	private readonly List<ProxyControllerType> _proxyChildren = new List<ProxyControllerType>();

	private readonly Controller _before;

	private readonly Controller _after;

	private readonly LayoutStyle _childrenLayoutStyle;

	private float _lastScroll;

	public bool Dirty { get; private set; }

	public Flex Flex => _scrollView.Flex;

	public int NumberOfProxies => _proxyChildren.Count;

	private int NumberOfControllers => Flex.Children.Count - 2;

	public ProxyFlex(int numberOfInstantiatedControllers, int maximumNumberOfProxies, LayoutStyle layoutStyle, ScrollView scrollView)
	{
		_scrollView = scrollView;
		for (int i = 0; i < numberOfInstantiatedControllers; i++)
		{
			Flex.Append<ControllerType>(i.ToString()).LayoutStyle = layoutStyle;
		}
		_maximumNumberOfProxies = maximumNumberOfProxies;
		_childrenLayoutStyle = layoutStyle;
		_before = Flex.Prepend<Controller>("before");
		_before.LayoutStyle = Style.Instantiate<LayoutStyle>("DynamicSpace");
		_after = Flex.Append<Controller>("after");
		_after.LayoutStyle = Style.Instantiate<LayoutStyle>("DynamicSpace");
	}

	public ProxyControllerType AppendProxy()
	{
		if (NumberOfProxies >= _maximumNumberOfProxies)
		{
			RemoveProxy(_proxyChildren[0]);
		}
		ProxyControllerType val = OVRObjectPool.Get<ProxyControllerType>();
		_proxyChildren.Add(val);
		Dirty = true;
		return val;
	}

	public void RemoveProxy(ProxyControllerType proxy)
	{
		_proxyChildren.Remove(proxy);
		OVRObjectPool.Return(proxy);
		Dirty = true;
	}

	public void Clear()
	{
		foreach (ProxyControllerType proxyChild in _proxyChildren)
		{
			OVRObjectPool.Return(proxyChild);
		}
		_proxyChildren.Clear();
		Dirty = true;
	}

	public void Update()
	{
		if (HasScrolledEnough())
		{
			Dirty = true;
		}
		if (Dirty)
		{
			Fill();
			Dirty = false;
		}
	}

	private bool HasScrolledEnough()
	{
		return Mathf.Abs(Flex.RectTransform.anchoredPosition.y - _lastScroll) > 1f;
	}

	private void Fill()
	{
		_lastScroll = Flex.RectTransform.anchoredPosition.y;
		float height = ComputeStartHeightFromProgress(_scrollView.Progress);
		List<Controller> children = Flex.Children;
		int itemIndexAtHeight = GetItemIndexAtHeight(height);
		int num = itemIndexAtHeight + NumberOfControllers - 1;
		int num2 = Math.Max(0, Math.Min(num - NumberOfProxies, itemIndexAtHeight));
		itemIndexAtHeight -= num2;
		num -= num2;
		int num3 = 1;
		for (int i = itemIndexAtHeight; i <= num; i++)
		{
			if (i < NumberOfProxies)
			{
				ControllerType target = children[num3++] as ControllerType;
				_proxyChildren[i].Fill(target, _targetsDictionary);
			}
		}
		float height2 = ComputeHeight(0, itemIndexAtHeight - 1);
		float height3 = ComputeHeight(num + 1, NumberOfProxies - 1);
		_before.SetHeight(height2);
		_after.SetHeight(height3);
	}

	private float ComputeTotalHeight()
	{
		return ComputeHeight(0, Math.Max(NumberOfProxies - 1, NumberOfControllers - 1));
	}

	private float ComputeTotalUsefulHeight()
	{
		return ComputeTotalHeight() - ComputeHeight(1, NumberOfControllers - 1) + Flex.LayoutStyle.spacing;
	}

	private float ComputeStartHeightFromProgress(float progress)
	{
		return (1f - progress) * ComputeTotalUsefulHeight();
	}

	private int GetItemIndexAtHeight(float height)
	{
		int val = (int)(height / (_childrenLayoutStyle.size.y + Flex.LayoutStyle.spacing));
		return Math.Max(0, val);
	}

	private float ComputeHeight(int startIndex, int endIndex)
	{
		int num = endIndex - startIndex + 1;
		float spacing = Flex.LayoutStyle.spacing;
		return (float)num * (_childrenLayoutStyle.size.y + spacing) - spacing;
	}
}
