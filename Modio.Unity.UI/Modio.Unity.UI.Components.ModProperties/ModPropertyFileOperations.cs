using System;
using Modio.Errors;
using Modio.Mods;
using Modio.Unity.UI.Components.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyFileOperations : IModProperty
{
	[Flags]
	private enum Operation
	{
		None = 0,
		Queued = 1,
		Downloading = 2,
		Installing = 4,
		Installed = 8,
		Updating = 0x10,
		Uninstalling = 0x20,
		FileOperationFailed = 0x40,
		InstalledByOtherUser = 0x80
	}

	[SerializeField]
	private Operation _operations = ~Operation.Installed;

	[Space]
	[SerializeField]
	private GameObject _noOperationActive;

	[SerializeField]
	private GameObject _operationActive;

	[Space]
	[SerializeField]
	private TMP_Text _operationName;

	[SerializeField]
	private ModioUILocalizedText _operationNameLocalised;

	[SerializeField]
	private TMP_Text _progressPercent;

	[SerializeField]
	private Image _progressFill;

	[SerializeField]
	private bool _invertProgressFill;

	[SerializeField]
	private TMP_Text _downloadSpeed;

	public void OnModUpdate(Mod mod)
	{
		Operation operation = mod.File.State switch
		{
			ModFileState.Queued => Operation.Queued, 
			ModFileState.Downloading => Operation.Downloading, 
			ModFileState.Installing => Operation.Installing, 
			ModFileState.Installed => mod.IsSubscribed ? Operation.Installed : Operation.InstalledByOtherUser, 
			ModFileState.Updating => Operation.Updating, 
			ModFileState.Uninstalling => Operation.Uninstalling, 
			ModFileState.FileOperationFailed => Operation.FileOperationFailed, 
			_ => Operation.None, 
		};
		bool flag = operation != 0 && _operations.HasFlag(operation);
		if (_noOperationActive != null)
		{
			_noOperationActive.gameObject.SetActive(!flag);
		}
		if (_operationActive != null)
		{
			_operationActive.gameObject.SetActive(flag);
		}
		if (!flag)
		{
			return;
		}
		if (_operationName != null)
		{
			_operationName.text = operation.ToString();
		}
		if (_operationNameLocalised != null)
		{
			string key = operation switch
			{
				Operation.None => "", 
				Operation.Queued => "modio_modstate_queued", 
				Operation.Downloading => "modio_modstate_downloading", 
				Operation.Installing => "modio_modstate_installing", 
				Operation.Installed => "modio_modstate_installed", 
				Operation.Updating => "modio_modstate_updating", 
				Operation.Uninstalling => "modio_modstate_uninstalling", 
				Operation.FileOperationFailed => (mod.File.FileStateErrorCause.Code == ErrorCode.INSUFFICIENT_SPACE) ? "modio_error_storage_header" : "modio_modstate_error", 
				Operation.InstalledByOtherUser => "modio_modstate_installed", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			_operationNameLocalised.SetKey(key);
		}
		if (_progressPercent != null)
		{
			_progressPercent.text = mod.File.FileStateProgress.ToString("P0", ModioUILocalizationManager.CultureInfo);
		}
		if (_progressFill != null)
		{
			_progressFill.fillAmount = (_invertProgressFill ? (1f - mod.File.FileStateProgress) : mod.File.FileStateProgress);
		}
		if (_downloadSpeed != null)
		{
			bool flag2 = operation == Operation.Downloading;
			if (flag2)
			{
				_downloadSpeed.text = StringFormat.BytesSuffix(mod.File.DownloadingBytesPerSecond, reducePrecision: true) + "/s";
			}
			_downloadSpeed.gameObject.SetActive(flag2);
		}
	}
}
