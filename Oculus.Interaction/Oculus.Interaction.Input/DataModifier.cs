using UnityEngine;

namespace Oculus.Interaction.Input;

public abstract class DataModifier<TData> : DataSource<TData> where TData : class, ICopyFrom<TData>, new()
{
	[Header("Data Modifier")]
	[SerializeField]
	[Interface("_modifyDataFromSource")]
	protected Object _iModifyDataFromSourceMono;

	private IDataSource<TData> _modifyDataFromSource;

	[SerializeField]
	[Tooltip("If this is false, then this modifier will simply pass through data without performing any modification. This saves on memory and computation")]
	protected bool _applyModifier = true;

	private TData _thisDataAsset;

	private TData _currentDataAsset = InvalidAsset;

	private static TData InvalidAsset { get; } = new TData();


	protected override TData DataAsset => _currentDataAsset;

	public virtual IDataSource<TData> ModifyDataFromSource
	{
		get
		{
			if (_modifyDataFromSource != null)
			{
				return _modifyDataFromSource;
			}
			return _modifyDataFromSource = _iModifyDataFromSourceMono as IDataSource<TData>;
		}
	}

	public override int CurrentDataVersion
	{
		get
		{
			if (!_applyModifier)
			{
				return ModifyDataFromSource.CurrentDataVersion;
			}
			return base.CurrentDataVersion;
		}
	}

	public void ResetSources(IDataSource<TData> modifyDataFromSource, IDataSource updateAfter, UpdateModeFlags updateMode)
	{
		ResetUpdateAfter(updateAfter, updateMode);
		_modifyDataFromSource = modifyDataFromSource;
		_currentDataAsset = InvalidAsset;
	}

	protected override void UpdateData()
	{
		if (_applyModifier)
		{
			if (_thisDataAsset == null)
			{
				_thisDataAsset = new TData();
			}
			_thisDataAsset.CopyFrom(ModifyDataFromSource.GetData());
			_currentDataAsset = _thisDataAsset;
			Apply(_currentDataAsset);
		}
		else
		{
			_currentDataAsset = ModifyDataFromSource.GetData();
		}
	}

	protected abstract void Apply(TData data);

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	public void InjectAllDataModifier(UpdateModeFlags updateMode, IDataSource updateAfter, IDataSource<TData> modifyDataFromSource, bool applyModifier)
	{
		InjectAllDataSource(updateMode, updateAfter);
		InjectModifyDataFromSource(modifyDataFromSource);
		InjectApplyModifier(applyModifier);
	}

	public void InjectModifyDataFromSource(IDataSource<TData> modifyDataFromSource)
	{
		_modifyDataFromSource = modifyDataFromSource;
		_iModifyDataFromSourceMono = modifyDataFromSource as Object;
	}

	public void InjectApplyModifier(bool applyModifier)
	{
		_applyModifier = applyModifier;
	}
}
