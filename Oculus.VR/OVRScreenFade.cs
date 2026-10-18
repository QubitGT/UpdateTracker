using System.Collections;
using UnityEngine;

[HelpURL("https://developer.oculus.com/reference/unity/latest/class_o_v_r_screen_fade")]
public class OVRScreenFade : MonoBehaviour
{
	[Tooltip("Fade duration")]
	public float fadeTime = 2f;

	[Tooltip("Screen color at maximum fade")]
	public Color fadeColor = new Color(0.01f, 0.01f, 0.01f, 1f);

	public bool fadeOnStart = true;

	public int renderQueue = 5000;

	private float explicitFadeAlpha;

	private float animatedFadeAlpha;

	private float uiFadeAlpha;

	private MeshRenderer fadeRenderer;

	private MeshFilter fadeMesh;

	private Material fadeMaterial;

	private bool isFading;

	public static OVRScreenFade instance { get; private set; }

	public float currentAlpha => Mathf.Max(explicitFadeAlpha, animatedFadeAlpha, uiFadeAlpha);

	private void Start()
	{
		if (base.gameObject.name.StartsWith("OculusMRC_"))
		{
			Object.Destroy(this);
			return;
		}
		fadeMaterial = new Material(Shader.Find("Oculus/Unlit Transparent Color"));
		fadeMesh = base.gameObject.AddComponent<MeshFilter>();
		fadeRenderer = base.gameObject.AddComponent<MeshRenderer>();
		Mesh mesh = new Mesh();
		fadeMesh.mesh = mesh;
		Vector3[] array = new Vector3[4];
		float num = 2f;
		float num2 = 2f;
		float z = 1f;
		array[0] = new Vector3(0f - num, 0f - num2, z);
		array[1] = new Vector3(num, 0f - num2, z);
		array[2] = new Vector3(0f - num, num2, z);
		array[3] = new Vector3(num, num2, z);
		mesh.vertices = array;
		mesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
		mesh.normals = new Vector3[4]
		{
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		explicitFadeAlpha = 0f;
		animatedFadeAlpha = 0f;
		uiFadeAlpha = 0f;
		if (fadeOnStart)
		{
			FadeIn();
		}
		instance = this;
	}

	public void FadeIn()
	{
		StartCoroutine(Fade(1f, 0f));
	}

	public void FadeOut()
	{
		StartCoroutine(Fade(0f, 1f));
	}

	private void OnLevelFinishedLoading(int level)
	{
		FadeIn();
	}

	private void OnEnable()
	{
		if (!fadeOnStart)
		{
			explicitFadeAlpha = 0f;
			animatedFadeAlpha = 0f;
			uiFadeAlpha = 0f;
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
		if (fadeRenderer != null)
		{
			Object.Destroy(fadeRenderer);
		}
		if (fadeMaterial != null)
		{
			Object.Destroy(fadeMaterial);
		}
		if (fadeMesh != null)
		{
			Object.Destroy(fadeMesh);
		}
	}

	public void SetUIFade(float level)
	{
		uiFadeAlpha = Mathf.Clamp01(level);
		SetMaterialAlpha();
	}

	public void SetExplicitFade(float level)
	{
		explicitFadeAlpha = level;
		SetMaterialAlpha();
	}

	private IEnumerator Fade(float startAlpha, float endAlpha)
	{
		float elapsedTime = 0f;
		while (elapsedTime < fadeTime)
		{
			elapsedTime += Time.deltaTime;
			animatedFadeAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / fadeTime));
			SetMaterialAlpha();
			yield return new WaitForEndOfFrame();
		}
		animatedFadeAlpha = endAlpha;
		SetMaterialAlpha();
	}

	private void SetMaterialAlpha()
	{
		Color color = fadeColor;
		color.a = currentAlpha;
		isFading = color.a > 0f;
		if (fadeMaterial != null)
		{
			fadeMaterial.color = color;
			fadeMaterial.renderQueue = renderQueue;
			fadeRenderer.material = fadeMaterial;
			fadeRenderer.enabled = isFading;
		}
	}
}
