using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundedBoxVideoController : MonoBehaviour
{
	private struct BoxAnimation
	{
		public RectTransform rectTransform;

		public Image image;

		public float duration;

		public float startHeight;

		public float animationMaxHeight;

		public float startVelocity;

		public float startTime;

		public float acceleration;

		public void Update(float animationTime)
		{
			float value = animationTime - startTime;
			value = Mathf.Clamp(value, 0f, duration);
			float num = value * value;
			float num2 = startVelocity * value - 0.5f * acceleration * num;
			float num3 = num2 / animationMaxHeight;
			float y = startHeight - num2;
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.y = y;
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.rotation = Quaternion.Euler(0f, 0f, num3 * 360f);
		}

		public void SetColor(Color color)
		{
			image.color = color;
		}
	}

	public Slider timeSlider;

	public float animationDuration;

	public float animationTime;

	public int cycleCount;

	public Sprite playIcon;

	public Sprite pauseIcon;

	public Image playPauseImg;

	public bool isPlaying;

	public List<Color> boxColors;

	public List<RectTransform> boxes;

	private float animationCycleDuration;

	[Header("Time Labels")]
	public TextMeshProUGUI leftLabel;

	public TextMeshProUGUI rightLabel;

	[Header("Background Material Settings")]
	public Image backgroundImage;

	public Vector2 direction;

	public Color colorA;

	public Color colorB;

	private readonly int columnDirectionID = Shader.PropertyToID("columnDirection");

	private readonly int rowDirectionID = Shader.PropertyToID("rowDirection");

	private readonly int animationTimeID = Shader.PropertyToID("animationTime");

	private readonly int colorAID = Shader.PropertyToID("colorA");

	private readonly int colorBID = Shader.PropertyToID("colorB");

	private List<BoxAnimation> animations;

	private void OnEnable()
	{
		UpdateBackgroundMaterialProperties();
	}

	private void Start()
	{
		animations = new List<BoxAnimation>();
		Vector2 size = ((RectTransform)base.transform).rect.size;
		float num = size.x / (float)boxes.Count;
		timeSlider.onValueChanged.AddListener(delegate
		{
			OnSliderValueChange();
		});
		float num2 = ((float)boxes.Count - 1f) * 0.35f + 1f;
		animationCycleDuration = animationDuration / (float)cycleCount;
		float num3 = animationCycleDuration / num2;
		float num4 = boxes[0].rect.height * 0.6f;
		float num5 = size.y * 0.5f + num4;
		float num6 = 2f * num5 / (num3 * 0.5f);
		float acceleration = num6 / (num3 * 0.5f);
		for (int i = 0; i < boxes.Count; i++)
		{
			RectTransform rectTransform = boxes[i];
			float num7 = (float)i + 0.5f;
			rectTransform.anchoredPosition = new Vector2(num7 * num, 0f);
			BoxAnimation boxAnimation = default(BoxAnimation);
			boxAnimation.duration = num3;
			boxAnimation.startHeight = num4;
			boxAnimation.animationMaxHeight = num5;
			boxAnimation.rectTransform = rectTransform;
			boxAnimation.startVelocity = num6;
			boxAnimation.acceleration = acceleration;
			boxAnimation.startTime = num3 * 0.35f * (float)i;
			boxAnimation.image = rectTransform.GetComponent<Image>();
			BoxAnimation item = boxAnimation;
			animations.Add(item);
		}
		SetPlay();
		UpdateBackgroundMaterialProperties();
	}

	public void UpdateBackgroundMaterialProperties()
	{
		Vector2 normalized = direction.normalized;
		backgroundImage.materialForRendering.SetVector(columnDirectionID, normalized);
		backgroundImage.materialForRendering.SetVector(rowDirectionID, new Vector2(0f - normalized.y, normalized.x));
		backgroundImage.materialForRendering.SetColor(colorAID, colorA.linear);
		backgroundImage.materialForRendering.SetColor(colorBID, colorB.linear);
		backgroundImage.materialForRendering.SetFloat(animationTimeID, animationTime);
	}

	public void OnSliderValueChange()
	{
		animationTime = timeSlider.value * animationDuration;
	}

	public void TogglePlayPause()
	{
		if (isPlaying)
		{
			SetPaused();
			return;
		}
		if (Mathf.Abs(animationDuration - animationTime) < 0.1f)
		{
			animationTime = 0f;
		}
		SetPlay();
	}

	private void SetPaused()
	{
		isPlaying = false;
		playPauseImg.sprite = playIcon;
	}

	private void SetPlay()
	{
		isPlaying = true;
		playPauseImg.sprite = pauseIcon;
	}

	private string FormatTime(float seconds)
	{
		int num = Mathf.FloorToInt(seconds / 60f);
		int num2 = (int)seconds % 60;
		string text = num.ToString();
		string text2 = num2.ToString("D2");
		return text + ":" + text2;
	}

	private void LateUpdate()
	{
		if (isPlaying)
		{
			animationTime += Time.deltaTime;
			timeSlider.SetValueWithoutNotify(animationTime / animationDuration);
			if (animationTime > animationDuration)
			{
				animationTime = animationDuration;
				SetPaused();
			}
		}
		else
		{
			animationTime = timeSlider.value * animationDuration;
		}
		for (int i = 0; i < animations.Count; i++)
		{
			float num = Mathf.Floor(animationTime / animationCycleDuration) % (float)boxColors.Count;
			animations[i].SetColor(boxColors[(int)num]);
			animations[i].Update(animationTime % animationCycleDuration);
		}
		float seconds = Mathf.Round(animationDuration - animationTime);
		leftLabel.SetText(FormatTime(animationTime));
		rightLabel.SetText(FormatTime(seconds));
		backgroundImage.materialForRendering.SetFloat(animationTimeID, animationTime);
	}
}
