using System;
using System.Threading.Tasks;
using Modio.Mods;
using Modio.Users;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyDiskUsage : IUserProperty, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Image _fillImage;

	[SerializeField]
	private GameObject _enableIfAvailableSpaceSupported;

	[SerializeField]
	private GameObject _disableIfAvailableSpaceSupported;

	private bool _isUpdatingUsage;

	public void Start()
	{
		if (_text != null)
		{
			_text.text = "";
		}
	}

	public void OnDestroy()
	{
	}

	public void OnEnable()
	{
		ModioClient.OnInitialized += UpdateUsage;
		Mod.AddChangeListener(ModChangeType.FileState, OnModFileStateChanged);
	}

	public void OnDisable()
	{
		ModioClient.OnInitialized -= UpdateUsage;
		Mod.RemoveChangeListener(ModChangeType.FileState, OnModFileStateChanged);
	}

	private void OnModFileStateChanged(Mod _, ModChangeType __)
	{
		UpdateUsage();
	}

	public void OnUserUpdate(UserProfile user)
	{
		if (!_isUpdatingUsage)
		{
			UpdateUsage();
		}
	}

	private async void UpdateUsage()
	{
		_isUpdatingUsage = true;
		await Task.Yield();
		long usedSpaceBytes = ModInstallationManagement.GetTotalDiskUsage(includeQueued: false);
		long reservedSpaceBytes = ModInstallationManagement.GetTotalDiskUsage(includeQueued: true);
		long num = await ModioClient.DataStorage.GetAvailableFreeSpaceForModInstall();
		bool flag = num > 0;
		if (_text != null)
		{
			string text = StringFormat.Bytes(StringFormatBytes.Suffix, reservedSpaceBytes);
			if (flag)
			{
				string text2 = StringFormat.Bytes(StringFormatBytes.Suffix, num + usedSpaceBytes);
				_text.text = text + " / " + text2;
			}
			else
			{
				_text.text = text;
			}
		}
		if (_fillImage != null)
		{
			_fillImage.fillAmount = ((reservedSpaceBytes <= 0) ? 0f : ((float)reservedSpaceBytes / (float)(num + usedSpaceBytes)));
		}
		if (_enableIfAvailableSpaceSupported != null)
		{
			_enableIfAvailableSpaceSupported.SetActive(flag);
		}
		if (_disableIfAvailableSpaceSupported != null)
		{
			_disableIfAvailableSpaceSupported.SetActive(!flag);
		}
		_isUpdatingUsage = false;
	}
}
