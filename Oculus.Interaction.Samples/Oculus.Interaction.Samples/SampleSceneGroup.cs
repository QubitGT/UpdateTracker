using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Samples;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Scene Group")]
public class SampleSceneGroup : ScriptableObject
{
	public interface ISceneInfo
	{
		string DisplayName { get; }

		string SceneName { get; }

		string SceneGuid { get; }

		Sprite Thumbnail { get; }
	}

	[Serializable]
	private class SceneInfo : ISceneInfo
	{
		public string DisplayName;

		public string SceneName;

		public string SceneGuid;

		public Sprite Thumbnail;

		string ISceneInfo.DisplayName => DisplayName;

		string ISceneInfo.SceneName => SceneName;

		Sprite ISceneInfo.Thumbnail => Thumbnail;

		string ISceneInfo.SceneGuid => SceneGuid;
	}

	[Tooltip("Scenes in this group will be displayed under this header in the scene menu.")]
	[SerializeField]
	private string _groupName;

	[Tooltip("Only Enabled scene groups will be shown in the scene menu.")]
	[SerializeField]
	private bool _groupEnabled = true;

	[Tooltip("Scene groups will appear in the scene menu sorted in ascending order by this value.")]
	[SerializeField]
	private int _groupDisplayOrder;

	[SerializeField]
	[HideInInspector]
	private SceneInfo[] _sceneInfos;

	public string GroupName => _groupName;

	public bool GroupEnabled => _groupEnabled;

	public int GroupDisplayOrder => _groupDisplayOrder;

	public int SceneCount => _sceneInfos.Length;

	public IEnumerable<ISceneInfo> GetScenes()
	{
		SceneInfo[] sceneInfos = _sceneInfos;
		for (int i = 0; i < sceneInfos.Length; i++)
		{
			yield return sceneInfos[i];
		}
	}
}
