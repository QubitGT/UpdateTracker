using System.Collections;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class GorillaComputerTerminal : MonoBehaviour, IBuildValidation
{
	public TextMeshPro myScreenText;

	public TextMeshPro myFunctionText;

	public MeshRenderer monitorMesh;

	private bool isFullScreen;

	private bool hasSavedScreenLayout;

	private Vector2 savedScreenTextPosition;

	private Vector2 savedScreenTextSize;

	private static readonly Vector3[] s_functionTextCorners = new Vector3[4];

	private string functionText = string.Empty;

	private bool usingMonospaceFont;

	private TMP_FontAsset savedFont;

	private float savedCharacterSpacing;

	public bool BuildValidationCheck()
	{
		if (myScreenText == null || myFunctionText == null || monitorMesh == null)
		{
			Debug.LogErrorFormat(base.gameObject, "gorilla computer terminal {0} is missing screen text, function text, or monitor mesh. this will break lots of computer stuff", base.gameObject.name);
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		if (GorillaComputer.instance == null)
		{
			StartCoroutine(OnEnable_Local());
		}
		else
		{
			Init();
		}
	}

	private IEnumerator OnEnable_Local()
	{
		yield return new WaitUntil(() => GorillaComputer.instance != null);
		yield return null;
		Init();
	}

	private void Init()
	{
		GameEvents.ScreenTextChangedEvent.AddListener(OnScreenTextChanged);
		GameEvents.FunctionSelectTextChangedEvent.AddListener(OnFunctionTextChanged);
		GameEvents.ScreenTextMaterialsEvent.AddListener(OnMaterialsChanged);
		GameEvents.LanguageEvent.AddListener(OnLanguageChanged);
		OnFunctionTextChanged(GorillaComputer.instance.functionSelectText.currentText);
		if (GorillaComputer.instance.screenText.currentMaterials != null)
		{
			monitorMesh.sharedMaterials = GorillaComputer.instance.screenText.currentMaterials;
		}
		OnLanguageChanged();
		OnScreenTextChanged(GorillaComputer.instance.screenText.currentText ?? string.Empty);
	}

	private void LateUpdate()
	{
		if (usingMonospaceFont && GorillaComputer.instance != null && myScreenText.font != GorillaComputer.instance.monospaceScreenFont)
		{
			savedFont = myScreenText.font;
			savedCharacterSpacing = myScreenText.characterSpacing;
			myScreenText.font = GorillaComputer.instance.monospaceScreenFont;
			myScreenText.characterSpacing = 0f;
		}
	}

	private void OnDisable()
	{
		GameEvents.ScreenTextChangedEvent.RemoveListener(OnScreenTextChanged);
		GameEvents.FunctionSelectTextChangedEvent.RemoveListener(OnFunctionTextChanged);
		GameEvents.ScreenTextMaterialsEvent.RemoveListener(OnMaterialsChanged);
		GameEvents.LanguageEvent.RemoveListener(OnLanguageChanged);
	}

	private void OnScreenTextChanged(string text)
	{
		SetFullScreen(GorillaComputer.instance != null && GorillaComputer.instance.IsScreenTextFullScreen);
		SetMonospaceFont(isFullScreen && CanShowInMonospaceFont(text));
		myScreenText.text = text;
	}

	private static bool CanShowInMonospaceFont(string text)
	{
		TMP_FontAsset monospaceScreenFont = GorillaComputer.instance.monospaceScreenFont;
		if (monospaceScreenFont == null)
		{
			return false;
		}
		foreach (char c in text)
		{
			if (!char.IsControl(c) && !monospaceScreenFont.HasCharacter(c))
			{
				return false;
			}
		}
		return true;
	}

	private void SetMonospaceFont(bool useMonospaceFont)
	{
		if (useMonospaceFont != usingMonospaceFont)
		{
			usingMonospaceFont = useMonospaceFont;
			if (useMonospaceFont)
			{
				savedFont = myScreenText.font;
				savedCharacterSpacing = myScreenText.characterSpacing;
				myScreenText.font = GorillaComputer.instance.monospaceScreenFont;
				myScreenText.characterSpacing = 0f;
			}
			else
			{
				myScreenText.font = savedFont;
				myScreenText.characterSpacing = savedCharacterSpacing;
			}
		}
	}

	private void SetFullScreen(bool fullScreen)
	{
		if (fullScreen == isFullScreen)
		{
			return;
		}
		isFullScreen = fullScreen;
		myFunctionText.text = (fullScreen ? string.Empty : functionText);
		RectTransform rectTransform = myScreenText.rectTransform;
		if (!fullScreen)
		{
			rectTransform.anchoredPosition = savedScreenTextPosition;
			rectTransform.sizeDelta = savedScreenTextSize;
			return;
		}
		if (!hasSavedScreenLayout)
		{
			savedScreenTextPosition = rectTransform.anchoredPosition;
			savedScreenTextSize = rectTransform.sizeDelta;
			hasSavedScreenLayout = true;
		}
		Rect rect = rectTransform.rect;
		myFunctionText.rectTransform.GetWorldCorners(s_functionTextCorners);
		Vector2 vector = rect.min;
		Vector2 vector2 = rect.max;
		for (int i = 0; i < s_functionTextCorners.Length; i++)
		{
			Vector2 rhs = rectTransform.InverseTransformPoint(s_functionTextCorners[i]);
			vector = Vector2.Min(vector, rhs);
			vector2 = Vector2.Max(vector2, rhs);
		}
		rectTransform.sizeDelta = savedScreenTextSize + (vector2 - vector - rect.size);
		Vector2 vector3 = (vector + vector2) * 0.5f - rectTransform.rect.center;
		rectTransform.position += rectTransform.TransformVector(vector3);
	}

	private void OnFunctionTextChanged(string text)
	{
		functionText = text;
		if (!isFullScreen)
		{
			myFunctionText.text = text;
		}
	}

	private void OnMaterialsChanged(Material[] materials)
	{
		monitorMesh.sharedMaterials = materials;
	}

	private void OnLanguageChanged()
	{
		if (LocalisationManager.GetFontAssetForCurrentLocale(out var result))
		{
			myFunctionText.font = result.fontAsset;
			if (usingMonospaceFont)
			{
				savedFont = result.fontAsset;
				savedCharacterSpacing = result.charSpacing;
			}
			else
			{
				myScreenText.font = result.fontAsset;
				myScreenText.characterSpacing = result.charSpacing;
			}
		}
	}
}
