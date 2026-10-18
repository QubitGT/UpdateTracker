using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class HandSourceInjector : MonoBehaviour
{
	[Serializable]
	private class ActiveDataSource
	{
		[SerializeField]
		[Interface(typeof(IDataSource<HandDataAsset>), new Type[] { })]
		private UnityEngine.Object _source;

		[SerializeField]
		[Interface(typeof(IDataSource), new Type[] { })]
		private UnityEngine.Object _modifyAfter;

		public IDataSource<HandDataAsset> Source { get; private set; }

		public IDataSource ModifyAfter { get; private set; }

		public void Initialize()
		{
			Source = _source as IDataSource<HandDataAsset>;
			ModifyAfter = _modifyAfter as IDataSource;
			AssertField(Source, "Source");
			AssertField(ModifyAfter, "ModifyAfter");
			static void AssertField(object obj, string name)
			{
			}
		}

		public bool IsActive()
		{
			return Source.GetData().IsDataValidAndConnected;
		}
	}

	[SerializeField]
	private Hand _targetHand;

	[SerializeField]
	private ActiveDataSource[] _sources;

	private ActiveDataSource _activeDataSource;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		ActiveDataSource[] sources = _sources;
		for (int i = 0; i < sources.Length; i++)
		{
			sources[i].Initialize();
		}
		UpdateActiveSource();
		if (_activeDataSource == null)
		{
			ApplySource(_sources[0]);
		}
		this.EndStart(ref _started);
	}

	private void Update()
	{
		if (!_activeDataSource.IsActive())
		{
			UpdateActiveSource();
		}
	}

	private void UpdateActiveSource()
	{
		ActiveDataSource[] sources = _sources;
		foreach (ActiveDataSource activeDataSource in sources)
		{
			if (activeDataSource.IsActive())
			{
				ApplySource(activeDataSource);
				break;
			}
		}
	}

	private void ApplySource(ActiveDataSource activeDataSource)
	{
		_activeDataSource = activeDataSource;
		_targetHand.ResetSources(activeDataSource.Source, activeDataSource.ModifyAfter, _targetHand.UpdateMode);
	}
}
