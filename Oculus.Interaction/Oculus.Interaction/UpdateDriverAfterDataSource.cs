using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class UpdateDriverAfterDataSource : MonoBehaviour, IUpdateDriver
{
	[SerializeField]
	[Interface(typeof(IUpdateDriver), new Type[] { })]
	private UnityEngine.Object _updateDriver;

	private IUpdateDriver UpdateDriver;

	[SerializeField]
	[Interface(typeof(IDataSource), new Type[] { })]
	private UnityEngine.Object _dataSource;

	private IDataSource DataSource;

	protected bool _started;

	public bool IsRootDriver { get; set; } = true;


	protected virtual void Awake()
	{
		UpdateDriver = _updateDriver as IUpdateDriver;
		DataSource = _dataSource as IDataSource;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			DataSource.InputDataAvailable += Drive;
			UpdateDriver.IsRootDriver = false;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			DataSource.InputDataAvailable -= Drive;
			UpdateDriver.IsRootDriver = true;
		}
	}

	public void Drive()
	{
		UpdateDriver.Drive();
	}

	public void InjectAllUpdateDriverAfterDataSource(IUpdateDriver updateDriver, IDataSource dataSource)
	{
		InjectUpdateDriver(updateDriver);
		InjectDataSource(dataSource);
	}

	public void InjectUpdateDriver(IUpdateDriver updateDriver)
	{
		UpdateDriver = updateDriver;
		_updateDriver = updateDriver as UnityEngine.Object;
	}

	public void InjectDataSource(IDataSource dataSource)
	{
		DataSource = dataSource;
		_dataSource = dataSource as UnityEngine.Object;
	}
}
