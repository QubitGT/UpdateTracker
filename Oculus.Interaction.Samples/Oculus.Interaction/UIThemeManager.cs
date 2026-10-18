using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction;

public class UIThemeManager : MonoBehaviour
{
	[SerializeField]
	private UITheme[] _themes;

	[SerializeField]
	private int _currentThemeIndex;

	public UITheme[] Themes => _themes;

	public int CurrentThemeIndex => _currentThemeIndex;

	private void Start()
	{
		ApplyTheme(_currentThemeIndex);
	}

	public void ApplyCurrentTheme()
	{
		ApplyTheme(_currentThemeIndex);
	}

	public void ApplyTheme(int index)
	{
		if (index < 0 || index >= _themes.Length)
		{
			Debug.LogError("Theme index out of range.");
			return;
		}
		_currentThemeIndex = index;
		UITheme uITheme = _themes[index];
		Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
		foreach (Animator animator in componentsInChildren)
		{
			if (animator.CompareTag("QDSUIPrimaryButton"))
			{
				animator.runtimeAnimatorController = uITheme.acPrimaryButton;
			}
			else if (animator.CompareTag("QDSUISecondaryButton"))
			{
				animator.runtimeAnimatorController = uITheme.acSecondaryButton;
			}
			else if (animator.CompareTag("QDSUIBorderlessButton"))
			{
				animator.runtimeAnimatorController = uITheme.acBorderlessButton;
			}
			else if (animator.CompareTag("QDSUIDestructiveButton"))
			{
				animator.runtimeAnimatorController = uITheme.acDestructiveButton;
			}
			else if (animator.CompareTag("QDSUIToggleButton"))
			{
				animator.runtimeAnimatorController = uITheme.acToggleButton;
			}
			else if (animator.CompareTag("QDSUIToggleBorderlessButton"))
			{
				animator.runtimeAnimatorController = uITheme.acToggleBorderlessButton;
			}
			else if (animator.CompareTag("QDSUIToggleSwitch"))
			{
				animator.runtimeAnimatorController = uITheme.acToggleSwitch;
			}
			else if (animator.CompareTag("QDSUIToggleCheckboxRadio"))
			{
				animator.runtimeAnimatorController = uITheme.acToggleCheckboxRadio;
			}
			else if (animator.CompareTag("QDSUITextInputField"))
			{
				animator.runtimeAnimatorController = uITheme.acTextInputField;
			}
			animator.Update(0f);
		}
		Image[] componentsInChildren2 = GetComponentsInChildren<Image>();
		foreach (Image image in componentsInChildren2)
		{
			if (image.CompareTag("QDSUIIcon"))
			{
				image.color = uITheme.textPrimaryColor;
			}
			else if (image.CompareTag("QDSUIAccentColor"))
			{
				image.color = uITheme.primaryButton.normal;
			}
			else if (!image.CompareTag("QDSUISharedThemeColor") && !image.CompareTag("QDSUIDestructiveButton") && !image.CompareTag("QDSUIBorderlessButton") && !image.CompareTag("QDSUIToggleBorderlessButton") && !image.CompareTag("QDSUISecondaryButton") && !image.CompareTag("QDSUIToggleButton"))
			{
				if (image.CompareTag("QDSUISection"))
				{
					image.color = uITheme.sectionPlateColor;
				}
				else if (image.CompareTag("QDSUITooltip"))
				{
					image.color = uITheme.tooltipColor;
				}
				else if (!image.CompareTag("QDSUITextInputField"))
				{
					image.color = uITheme.backplateColor;
				}
			}
			if (uITheme.ThemeVersion == 2)
			{
				if (image.CompareTag("QDSUIBackplateGradient"))
				{
					image.color = uITheme.backplateColor;
					image.material = uITheme.backplateGradientMaterial;
				}
				if (image.CompareTag("QDSUITextInvertedColor"))
				{
					image.color = uITheme.textPrimaryInvertedColor;
				}
			}
		}
		SpriteRenderer[] componentsInChildren3 = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].color = uITheme.tooltipColor;
		}
		TextMeshProUGUI[] componentsInChildren4 = GetComponentsInChildren<TextMeshProUGUI>();
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren4)
		{
			textMeshProUGUI.font = uITheme.textFontMedium;
			if (!textMeshProUGUI.CompareTag("QDSUISharedThemeColor") && !textMeshProUGUI.CompareTag("QDSUIDestructiveButton"))
			{
				if (textMeshProUGUI.CompareTag("QDSUITextSecondaryColor"))
				{
					textMeshProUGUI.color = uITheme.textSecondaryColor;
				}
				else
				{
					textMeshProUGUI.color = uITheme.textPrimaryColor;
				}
			}
			if (uITheme.ThemeVersion == 2)
			{
				if (textMeshProUGUI.CompareTag("QDSUITextInvertedColor"))
				{
					textMeshProUGUI.color = uITheme.textPrimaryInvertedColor;
				}
				if (textMeshProUGUI.CompareTag("QDSUITextSecondaryInvertedColor"))
				{
					textMeshProUGUI.color = uITheme.textSecondaryInvertedColor;
				}
			}
		}
	}
}
