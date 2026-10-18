using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples;

public class SceneGroupLoader : MonoBehaviour
{
	private class SceneGroupView : MonoBehaviour
	{
		public TextMeshProUGUI GroupName;

		public RectTransform TileContainer;
	}

	private class SceneTileView : MonoBehaviour
	{
		public TextMeshProUGUI Label;

		public Image Image;

		public Toggle Toggle;

		public Image SceneMissingOverlay;
	}

	[SerializeField]
	private SceneLoader _sceneLoader;

	[SerializeField]
	private Transform _sceneGroupContainer;

	[SerializeField]
	private GameObject _missingSceneWarning;

	[Header("Group Template")]
	[SerializeField]
	private GameObject _groupTemplateParent;

	[SerializeField]
	private TextMeshProUGUI _groupTemplateLabel;

	[SerializeField]
	private RectTransform _groupTileContainer;

	[Header("Tile Template")]
	[SerializeField]
	private GameObject _tileTemplateParent;

	[SerializeField]
	private TextMeshProUGUI _tileTemplateLabel;

	[SerializeField]
	private Image _tileTemplateImage;

	[SerializeField]
	private Toggle _tileTemplateToggle;

	[SerializeField]
	private Image _tileTemplateSceneMissingOverlay;

	private void Start()
	{
		BuildSceneGroups();
	}

	private void BuildSceneGroups()
	{
		bool flag = false;
		foreach (SampleSceneGroup item in from x in FindSceneGroupAssets()
			where x.GroupEnabled
			select x into g
			where g.SceneCount > 0
			orderby g.GroupDisplayOrder
			select g)
		{
			InitializeGroupViewTemplate();
			GameObject obj = Object.Instantiate(_groupTemplateParent, _sceneGroupContainer);
			obj.name = item.GroupName;
			obj.SetActive(value: true);
			SceneGroupView component = obj.GetComponent<SceneGroupView>();
			component.GroupName.text = item.GroupName;
			foreach (SampleSceneGroup.ISceneInfo sceneMenuItem in item.GetScenes())
			{
				InitializeTileViewTemplate();
				bool flag2 = CheckSceneExists(sceneMenuItem);
				flag = flag || !flag2;
				GameObject obj2 = Object.Instantiate(_tileTemplateParent, component.TileContainer);
				obj2.name = sceneMenuItem.DisplayName;
				obj2.SetActive(value: true);
				SceneTileView component2 = obj2.GetComponent<SceneTileView>();
				component2.Label.text = sceneMenuItem.DisplayName;
				component2.Toggle.enabled = flag2;
				component2.Toggle.onValueChanged.AddListener(delegate
				{
					LoadScene(sceneMenuItem);
				});
				component2.Image.sprite = sceneMenuItem.Thumbnail;
				component2.Image.enabled = flag2;
				component2.SceneMissingOverlay.sprite = sceneMenuItem.Thumbnail;
				component2.SceneMissingOverlay.gameObject.SetActive(!flag2);
			}
		}
		_missingSceneWarning.SetActive(flag);
		void InitializeGroupViewTemplate()
		{
			if (!_groupTemplateParent.TryGetComponent<SceneGroupView>(out var component4))
			{
				component4 = _groupTemplateParent.AddComponent<SceneGroupView>();
				component4.GroupName = _groupTemplateLabel;
				component4.TileContainer = _groupTileContainer;
			}
		}
		void InitializeTileViewTemplate()
		{
			if (!_tileTemplateParent.TryGetComponent<SceneTileView>(out var component3))
			{
				component3 = _tileTemplateParent.AddComponent<SceneTileView>();
				component3.Image = _tileTemplateImage;
				component3.Label = _tileTemplateLabel;
				component3.Toggle = _tileTemplateToggle;
				component3.SceneMissingOverlay = _tileTemplateSceneMissingOverlay;
			}
		}
	}

	private void LoadScene(SampleSceneGroup.ISceneInfo sceneInfo)
	{
		_sceneLoader.Load(sceneInfo.SceneName);
	}

	private static bool CheckSceneExists(SampleSceneGroup.ISceneInfo sceneInfo)
	{
		return SceneUtility.GetBuildIndexByScenePath(sceneInfo.SceneName) >= 0;
	}

	private static IEnumerable<SampleSceneGroup> FindSceneGroupAssets()
	{
		return Resources.LoadAll<SampleSceneGroup>("");
	}
}
