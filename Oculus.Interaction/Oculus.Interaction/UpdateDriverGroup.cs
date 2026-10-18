using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class UpdateDriverGroup : MonoBehaviour, IUpdateDriver
{
	[SerializeField]
	[Interface(typeof(IUpdateDriver), new Type[] { })]
	private List<UnityEngine.Object> _updateDrivers;

	protected List<IUpdateDriver> Drivers;

	[SerializeField]
	[Min(1f)]
	private int _iterations = 3;

	public bool IsRootDriver { get; set; } = true;


	public int Iterations
	{
		get
		{
			return _iterations;
		}
		set
		{
			_iterations = value;
		}
	}

	protected virtual void Awake()
	{
		Drivers = _updateDrivers.ConvertAll((UnityEngine.Object mono) => mono as IUpdateDriver);
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if (IsRootDriver)
		{
			Drive();
		}
	}

	public void Drive()
	{
		for (int i = 0; i < _iterations; i++)
		{
			foreach (IUpdateDriver driver in Drivers)
			{
				driver.Drive();
			}
		}
	}

	public void InjectAllUpdateDriverGroup(List<IUpdateDriver> updateDrivers)
	{
		InjectUpdateDrivers(updateDrivers);
	}

	public void InjectUpdateDrivers(List<IUpdateDriver> updateDrivers)
	{
		Drivers = updateDrivers;
		_updateDrivers = updateDrivers.ConvertAll((IUpdateDriver driver) => driver as UnityEngine.Object);
	}
}
