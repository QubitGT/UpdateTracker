using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.Errors;
using Modio.Unity.UI.Components.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Panels;

public abstract class ModioErrorPanelBase : ModioPanelBase
{
	[Serializable]
	public class ErrorMessageResponse
	{
		public List<long> errorCode;

		public List<long> apiCode;

		public string windowTitleLocalised;

		public string windowMessageLocalised;

		public string actionPromptLocalised;

		public UnityEvent onActionPressed;
	}

	[SerializeField]
	private ModioUILocalizedText _titleLocalised;

	[SerializeField]
	private TMP_Text _errorCode;

	[SerializeField]
	private ModioUILocalizedText _errorCodeLocalised;

	[SerializeField]
	private ModioUILocalizedText _errorMessageLocalised;

	[SerializeField]
	private GameObject _showWhenActionProvided;

	[SerializeField]
	private ModioUILocalizedText _actionMessageLocalised;

	[SerializeField]
	private ErrorMessageResponse[] _errorMessageResponses;

	private Action _action;

	private bool _useLocalizedActionPrompt;

	public void OpenPanel(Error error)
	{
		OpenPanel();
		ErrorMessageResponse[] errorMessageResponses = _errorMessageResponses;
		foreach (ErrorMessageResponse errorMessageResponse in errorMessageResponses)
		{
			if (errorMessageResponse.errorCode.Contains((long)error.Code) || (errorMessageResponse.apiCode.Count != 0 && errorMessageResponse.apiCode.Contains((long)error.Code)))
			{
				if (error is RateLimitError rateLimitError)
				{
					OpenPanel(errorMessageResponse, rateLimitError.RetryAfterSeconds);
				}
				else
				{
					OpenPanel(errorMessageResponse);
				}
				return;
			}
		}
		if (_errorCode != null)
		{
			_errorCode.text = $"[Error code: {error.Code}]";
		}
		if (_errorMessageLocalised != null)
		{
			string keyIfItExists = "modio_error_description_api_" + error.Code;
			if (!_errorMessageLocalised.SetKeyIfItExists(keyIfItExists))
			{
				_errorMessageLocalised.ResetKey();
			}
		}
		if (_errorCodeLocalised != null)
		{
			_errorCodeLocalised.gameObject.SetActive(value: true);
			string text = error.Code.ToString();
			_errorCodeLocalised.SetFormatArgs(text);
		}
		if (_showWhenActionProvided != null)
		{
			_showWhenActionProvided.SetActive(value: false);
		}
		if (_titleLocalised != null)
		{
			_titleLocalised.ResetKey();
		}
		_action = null;
		ModioLog.Verbose?.Log($"Showing error for response {error}");
	}

	public void OpenPanel(string message)
	{
		OpenPanel();
		_action = null;
		if (_showWhenActionProvided != null)
		{
			_showWhenActionProvided.SetActive(value: false);
		}
		if (_titleLocalised != null)
		{
			_titleLocalised.ResetKey();
		}
		if (_errorCode != null)
		{
			_errorCode.text = message;
		}
	}

	public void OpenPanel(ErrorMessageResponse response, params object[] args)
	{
		if (_titleLocalised != null)
		{
			_titleLocalised.SetKey(response.windowTitleLocalised);
		}
		if (_errorMessageLocalised != null)
		{
			_errorMessageLocalised.SetKey(response.windowMessageLocalised, args);
		}
		if (_actionMessageLocalised != null)
		{
			_actionMessageLocalised.SetKey(response.actionPromptLocalised);
		}
		_useLocalizedActionPrompt = !string.IsNullOrEmpty(response.actionPromptLocalised);
		if (_showWhenActionProvided != null)
		{
			_showWhenActionProvided.SetActive(_useLocalizedActionPrompt);
		}
		if (_errorCodeLocalised != null)
		{
			_errorCodeLocalised.gameObject.SetActive(value: false);
		}
		_action = response.onActionPressed.Invoke;
	}

	protected override void CancelPressed()
	{
		if (!_useLocalizedActionPrompt)
		{
			_action?.Invoke();
		}
		base.CancelPressed();
	}

	public async Task MonitorTaskThenOpenPanelIfError(Task<Error> task)
	{
		try
		{
			Error error = await task;
			if ((bool)error)
			{
				OpenPanel(error);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void InvokeAction()
	{
		if (_action != null)
		{
			_action();
		}
		ClosePanel();
	}
}
