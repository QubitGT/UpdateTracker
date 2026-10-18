using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.Localization.Operations;
using UnityEngine.Localization.Settings;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace UnityEngine.Localization;

[Serializable]
[UxmlObject]
public class LocalizedAsset<TObject> : LocalizedAssetBase, IDisposable where TObject : Object
{
	public delegate void ChangeHandler(TObject value);

	private class ConvertToObjectOperation : WaitForCurrentOperationAsyncOperationBase<Object>
	{
		public static readonly ObjectPool<ConvertToObjectOperation> Pool = new ObjectPool<ConvertToObjectOperation>(() => new ConvertToObjectOperation(), null, null, null, collectionCheck: false);

		private AsyncOperationHandle<TObject> m_Operation;

		public void Init(AsyncOperationHandle<TObject> operation)
		{
			AddressablesInterface.ResourceManager.Acquire(operation);
			m_Operation = operation;
			base.CurrentOperation = operation;
		}

		protected override void Execute()
		{
			if (m_Operation.IsDone)
			{
				OnCompleted(m_Operation);
			}
			else
			{
				m_Operation.Completed += OnCompleted;
			}
		}

		private void OnCompleted(AsyncOperationHandle<TObject> op)
		{
			Complete((Object)op.Result, op.Status == AsyncOperationStatus.Succeeded, (string)null);
		}

		protected override void Destroy()
		{
			AddressablesInterface.Release(m_Operation);
			Pool.Release(this);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : LocalizedAssetBase.UxmlSerializedData
	{
		public override object CreateInstance()
		{
			return new LocalizedAsset<TObject>();
		}
	}

	private CallbackArray<ChangeHandler> m_ChangeHandler;

	private Action<Locale> m_SelectedLocaleChanged;

	private Action<AsyncOperationHandle<TObject>> m_AutomaticLoadingCompleted;

	private AsyncOperationHandle<TObject> m_PreviousLoadingOperation;

	private TObject m_CurrentValue;

	public override bool WaitForCompletion
	{
		set
		{
			if (value != WaitForCompletion)
			{
				base.WaitForCompletion = value;
				if (value && CurrentLoadingOperationHandle.IsValid() && !CurrentLoadingOperationHandle.IsDone)
				{
					CurrentLoadingOperationHandle.WaitForCompletion();
				}
			}
		}
	}

	internal override bool ForceSynchronous
	{
		get
		{
			if (!WaitForCompletion)
			{
				return LocalizationSettings.AssetDatabase.AsynchronousBehaviour == AsynchronousBehaviour.ForceSynchronous;
			}
			return true;
		}
	}

	public AsyncOperationHandle<TObject> CurrentLoadingOperationHandle { get; internal set; }

	public bool HasChangeHandler => m_ChangeHandler.Length != 0;

	public event ChangeHandler AssetChanged
	{
		add
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			m_ChangeHandler.Add(value);
			if (m_ChangeHandler.Length == 1)
			{
				LocalizationSettings.ValidateSettingsExist();
				ForceUpdate();
				LocalizationSettings.SelectedLocaleChanged += m_SelectedLocaleChanged;
			}
			else if (CurrentLoadingOperationHandle.IsValid() && CurrentLoadingOperationHandle.IsDone)
			{
				value(CurrentLoadingOperationHandle.Result);
			}
		}
		remove
		{
			m_ChangeHandler.RemoveByMovingTail(value);
			if (m_ChangeHandler.Length == 0)
			{
				LocalizationSettings.SelectedLocaleChanged -= m_SelectedLocaleChanged;
				ClearLoadingOperation();
			}
		}
	}

	public LocalizedAsset()
	{
		m_SelectedLocaleChanged = HandleLocaleChange;
		m_AutomaticLoadingCompleted = AutomaticLoadingCompleted;
	}

	public AsyncOperationHandle<TObject> LoadAssetAsync()
	{
		return LoadAssetAsync<TObject>();
	}

	public override AsyncOperationHandle<T> LoadAssetAsync<T>()
	{
		LocalizationSettings.ValidateSettingsExist("Can not Load Asset.");
		return LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<T>(base.TableReference, base.TableEntryReference, base.LocaleOverride);
	}

	public override AsyncOperationHandle<Object> LoadAssetAsObjectAsync()
	{
		AsyncOperationHandle<TObject> operation = LoadAssetAsync();
		ConvertToObjectOperation convertToObjectOperation = ConvertToObjectOperation.Pool.Get();
		convertToObjectOperation.Init(operation);
		return AddressablesInterface.ResourceManager.StartOperation(convertToObjectOperation, default(AsyncOperationHandle));
	}

	public TObject LoadAsset()
	{
		return LoadAssetAsync().WaitForCompletion();
	}

	protected internal override void ForceUpdate()
	{
		if (m_ChangeHandler.Length != 0)
		{
			HandleLocaleChange(null);
		}
	}

	private void HandleLocaleChange(Locale locale)
	{
		if (base.IsEmpty)
		{
			ClearLoadingOperation();
			return;
		}
		m_PreviousLoadingOperation = CurrentLoadingOperationHandle;
		CurrentLoadingOperationHandle = LoadAssetAsync();
		AddressablesInterface.Acquire(CurrentLoadingOperationHandle);
		if (!CurrentLoadingOperationHandle.IsDone)
		{
			if (!WaitForCompletion && LocalizationSettings.AssetDatabase.AsynchronousBehaviour != AsynchronousBehaviour.ForceSynchronous)
			{
				AsyncOperationHandle<TObject> currentLoadingOperationHandle = CurrentLoadingOperationHandle;
				currentLoadingOperationHandle.Completed += m_AutomaticLoadingCompleted;
				return;
			}
			CurrentLoadingOperationHandle.WaitForCompletion();
		}
		AutomaticLoadingCompleted(CurrentLoadingOperationHandle);
	}

	private void AutomaticLoadingCompleted(AsyncOperationHandle<TObject> loadOperation)
	{
		if (loadOperation.Status != AsyncOperationStatus.Succeeded)
		{
			CurrentLoadingOperationHandle = default(AsyncOperationHandle<TObject>);
			return;
		}
		InvokeChangeHandler(loadOperation.Result);
		ClearPreviousLoadingOperation();
	}

	private void InvokeChangeHandler(TObject value)
	{
		try
		{
			m_ChangeHandler.LockForChanges();
			int length = m_ChangeHandler.Length;
			if (length == 1)
			{
				m_ChangeHandler.SingleDelegate(value);
			}
			else if (length > 1)
			{
				ChangeHandler[] multiDelegates = m_ChangeHandler.MultiDelegates;
				for (int i = 0; i < length; i++)
				{
					multiDelegates[i](value);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		m_ChangeHandler.UnlockForChanges();
	}

	internal void ClearLoadingOperation()
	{
		ClearLoadingOperation(CurrentLoadingOperationHandle);
		CurrentLoadingOperationHandle = default(AsyncOperationHandle<TObject>);
	}

	private void ClearPreviousLoadingOperation()
	{
		ClearLoadingOperation(m_PreviousLoadingOperation);
		m_PreviousLoadingOperation = default(AsyncOperationHandle<TObject>);
	}

	private void ClearLoadingOperation(AsyncOperationHandle<TObject> operationHandle)
	{
		if (operationHandle.IsValid())
		{
			if (!operationHandle.IsDone)
			{
				operationHandle.Completed -= m_AutomaticLoadingCompleted;
			}
			AddressablesInterface.Release(operationHandle);
		}
	}

	protected override void Reset()
	{
		ClearLoadingOperation();
	}

	~LocalizedAsset()
	{
		ClearLoadingOperation();
	}

	void IDisposable.Dispose()
	{
		m_ChangeHandler.Clear();
		LocalizationSettings.SelectedLocaleChanged -= m_SelectedLocaleChanged;
		ClearLoadingOperation();
		GC.SuppressFinalize(this);
	}

	protected override void Initialize()
	{
		AssetChanged += UpdateBindingValue;
	}

	protected override void Cleanup()
	{
		AssetChanged -= UpdateBindingValue;
	}

	protected override BindingResult Update(in BindingContext context)
	{
		if (base.IsEmpty)
		{
			return new BindingResult(BindingStatus.Success);
		}
		if (!CurrentLoadingOperationHandle.IsDone)
		{
			return new BindingResult(BindingStatus.Pending);
		}
		return ApplyDataBindingValue(in context, m_CurrentValue);
	}

	protected virtual BindingResult ApplyDataBindingValue(in BindingContext context, TObject value)
	{
		return SetDataBindingValue(in context, value);
	}

	internal BindingResult SetDataBindingValue<T>(in BindingContext context, T value)
	{
		VisualElement container = context.targetElement;
		BindingId vep = context.bindingId;
		PropertyPath path = vep;
		if (ConverterGroups.TrySetValueGlobal(ref container, in path, value, out var returnCode))
		{
			return new BindingResult(BindingStatus.Success);
		}
		return CreateErrorResult(in context, returnCode, typeof(TObject));
	}

	private void UpdateBindingValue(TObject value)
	{
		m_CurrentValue = value;
		MarkDirty();
	}
}
