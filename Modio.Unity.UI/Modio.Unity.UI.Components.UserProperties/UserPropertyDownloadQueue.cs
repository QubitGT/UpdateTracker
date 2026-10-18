using System;
using System.Threading.Tasks;
using Modio.Mods;
using Modio.Users;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyDownloadQueue : IUserProperty, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private Image[] _progressBars;

	[SerializeField]
	private TMP_Text _progressPercentText;

	[SerializeField]
	private TMP_Text _progressSizesText;

	[SerializeField]
	private TMP_Text _operationCountText;

	[SerializeField]
	private TMP_Text _speedText;

	[SerializeField]
	private GameObject _disableIfNoOperations;

	[SerializeField]
	private GameObject _showForDownloadOnly;

	[SerializeField]
	private GameObject _showForInstallOnly;

	[SerializeField]
	private float _hideAfterSecondsOfInactivity = 2f;

	private int _completedOperationCount;

	private Mod _mod;

	public void OnUserUpdate(UserProfile user)
	{
	}

	public void Start()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnEnable()
	{
		if (_disableIfNoOperations != null)
		{
			_disableIfNoOperations.SetActive(value: false);
		}
		SetInstallOrDownloadState(isDownloading: false);
		Mod.AddChangeListener(ModChangeType.FileState, OnModChangeEvent);
	}

	public void OnDisable()
	{
		Mod.RemoveChangeListener(ModChangeType.FileState, OnModChangeEvent);
		if (_mod != null)
		{
			_mod.OnModUpdated -= OnModUpdated;
		}
	}

	private void OnModChangeEvent(Mod mod, ModChangeType modChangeType)
	{
		ModFileState state = mod.File.State;
		if (state != ModFileState.Downloading && state != ModFileState.Installing && state != ModFileState.Updating && state != ModFileState.Uninstalling)
		{
			if (_mod == mod)
			{
				_completedOperationCount++;
				_mod.OnModUpdated -= OnModUpdated;
				OnModUpdated();
				_mod = null;
			}
			return;
		}
		if (_mod != null)
		{
			_mod.OnModUpdated -= OnModUpdated;
		}
		_mod = mod;
		_mod.OnModUpdated += OnModUpdated;
		if (_disableIfNoOperations != null)
		{
			_disableIfNoOperations.SetActive(value: true);
		}
		OnModUpdated();
	}

	private void OnModUpdated()
	{
		float num = _mod.File.FileStateProgress;
		bool flag = _mod.File.State == ModFileState.None || _mod.File.State == ModFileState.Installed;
		bool flag2 = _mod.File.State == ModFileState.FileOperationFailed;
		if (flag)
		{
			num = 1f;
		}
		if (!flag2)
		{
			Image[] progressBars = _progressBars;
			for (int i = 0; i < progressBars.Length; i++)
			{
				progressBars[i].fillAmount = num;
			}
			if ((bool)_progressPercentText)
			{
				_progressPercentText.text = $"{num:P0}";
			}
			if ((bool)_progressSizesText)
			{
				long num2 = ((_mod.File.State == ModFileState.Downloading) ? _mod.File.ArchiveFileSize : _mod.File.FileSize);
				string text = StringFormat.Bytes(StringFormatBytes.Suffix, (long)((float)num2 * num), null, reducePrecision: true);
				string text2 = StringFormat.Bytes(StringFormatBytes.Suffix, num2, null, reducePrecision: true);
				_progressSizesText.text = text + " / " + text2;
			}
			if ((bool)_speedText)
			{
				_speedText.text = ((_mod.File.DownloadingBytesPerSecond <= 0) ? string.Empty : ("(" + StringFormat.Bytes(StringFormatBytes.Suffix, _mod.File.DownloadingBytesPerSecond, null, reducePrecision: true) + "/s)"));
			}
		}
		int pendingModOperationCount = ModInstallationManagement.PendingModOperationCount;
		if (_mod.File.State == ModFileState.Downloading)
		{
			SetInstallOrDownloadState(isDownloading: true);
		}
		else if (_mod.File.State == ModFileState.Installing || _mod.File.State == ModFileState.Uninstalling || _mod.File.State == ModFileState.Updating)
		{
			SetInstallOrDownloadState(isDownloading: false);
		}
		if (_operationCountText != null)
		{
			_operationCountText.text = $"{pendingModOperationCount}";
		}
		if ((flag || flag2) && pendingModOperationCount <= 1)
		{
			HideAfterDelay();
		}
	}

	private async void HideAfterDelay()
	{
		await Task.Delay((int)(_hideAfterSecondsOfInactivity * 1000f));
		if ((_mod == null || _mod.File.State == ModFileState.None || _mod.File.State == ModFileState.Installed || _mod.File.State == ModFileState.FileOperationFailed) && _disableIfNoOperations != null)
		{
			_disableIfNoOperations.SetActive(value: false);
		}
	}

	private void SetInstallOrDownloadState(bool isDownloading)
	{
		if (_showForDownloadOnly != null)
		{
			_showForDownloadOnly.SetActive(isDownloading);
		}
		if (_showForInstallOnly != null)
		{
			_showForInstallOnly.SetActive(!isDownloading);
		}
	}
}
