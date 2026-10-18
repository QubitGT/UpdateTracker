using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public abstract class DataSource<TData> : MonoBehaviour, IDataSource<TData>, IDataSource where TData : class, ICopyFrom<TData>, new()
{
	[Flags]
	public enum UpdateModeFlags
	{
		Manual = 0,
		UnityUpdate = 1,
		UnityFixedUpdate = 2,
		UnityLateUpdate = 4,
		AfterPreviousStep = 8
	}

	protected bool _started;

	private bool _requiresUpdate = true;

	[Header("Update")]
	[SerializeField]
	private UpdateModeFlags _updateMode;

	[SerializeField]
	[Interface(typeof(IDataSource), new Type[] { })]
	[Optional(OptionalAttribute.Flag.DontHide)]
	private UnityEngine.Object _updateAfter;

	private IDataSource UpdateAfter;

	private int _currentDataVersion;

	public bool Started => _started;

	public UpdateModeFlags UpdateMode => _updateMode;

	protected bool UpdateModeAfterPrevious => (_updateMode & UpdateModeFlags.AfterPreviousStep) != 0;

	public virtual int CurrentDataVersion => _currentDataVersion;

	protected abstract TData DataAsset { get; }

	public event Action InputDataAvailable = delegate
	{
	};

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (_updateAfter != null)
		{
			UpdateAfter = _updateAfter as IDataSource;
		}
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started && UpdateModeAfterPrevious && UpdateAfter != null)
		{
			UpdateAfter.InputDataAvailable += MarkInputDataRequiresUpdate;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started && UpdateAfter != null)
		{
			UpdateAfter.InputDataAvailable -= MarkInputDataRequiresUpdate;
		}
	}

	protected virtual void Update()
	{
		if ((_updateMode & UpdateModeFlags.UnityUpdate) != 0)
		{
			MarkInputDataRequiresUpdate();
		}
	}

	protected virtual void FixedUpdate()
	{
		if ((_updateMode & UpdateModeFlags.UnityFixedUpdate) != 0)
		{
			MarkInputDataRequiresUpdate();
		}
	}

	protected virtual void LateUpdate()
	{
		if ((_updateMode & UpdateModeFlags.UnityLateUpdate) != 0)
		{
			MarkInputDataRequiresUpdate();
		}
	}

	protected void ResetUpdateAfter(IDataSource updateAfter, UpdateModeFlags updateMode)
	{
		bool num = base.isActiveAndEnabled;
		if (base.isActiveAndEnabled)
		{
			OnDisable();
		}
		_updateMode = updateMode;
		UpdateAfter = updateAfter;
		_requiresUpdate = true;
		_currentDataVersion++;
		if (num)
		{
			OnEnable();
		}
	}

	public TData GetData()
	{
		if (RequiresUpdate())
		{
			UpdateData();
			_requiresUpdate = false;
		}
		return DataAsset;
	}

	protected bool RequiresUpdate()
	{
		return _requiresUpdate;
	}

	public virtual void MarkInputDataRequiresUpdate()
	{
		_requiresUpdate = true;
		_currentDataVersion++;
		this.InputDataAvailable();
	}

	protected abstract void UpdateData();

	public void InjectAllDataSource(UpdateModeFlags updateMode, IDataSource updateAfter)
	{
		InjectUpdateMode(updateMode);
		InjectUpdateAfter(updateAfter);
	}

	public void InjectUpdateMode(UpdateModeFlags updateMode)
	{
		_updateMode = updateMode;
	}

	public void InjectUpdateAfter(IDataSource updateAfter)
	{
		_updateAfter = updateAfter as UnityEngine.Object;
		UpdateAfter = updateAfter;
	}
}
