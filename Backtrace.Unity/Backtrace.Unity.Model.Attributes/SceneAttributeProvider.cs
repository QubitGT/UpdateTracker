using System.Collections.Generic;
using System.Globalization;
using UnityEngine.SceneManagement;

namespace Backtrace.Unity.Model.Attributes;

internal sealed class SceneAttributeProvider : IDynamicAttributeProvider
{
	public void GetAttributes(IDictionary<string, string> attributes)
	{
		if (attributes != null)
		{
			if (SceneManager.sceneCountInBuildSettings > 0)
			{
				attributes["scene.count.build"] = SceneManager.sceneCountInBuildSettings.ToString(CultureInfo.InvariantCulture);
			}
			attributes["scene.count"] = SceneManager.sceneCount.ToString(CultureInfo.InvariantCulture);
			Scene activeScene = SceneManager.GetActiveScene();
			attributes["scene.active"] = activeScene.name;
			attributes["scene.buildIndex"] = activeScene.buildIndex.ToString(CultureInfo.InvariantCulture);
			attributes["scene.handle"] = activeScene.handle.ToString(CultureInfo.InvariantCulture);
			attributes["scene.isDirty"] = activeScene.isDirty.ToString(CultureInfo.InvariantCulture);
			attributes["scene.isLoaded"] = activeScene.isLoaded.ToString(CultureInfo.InvariantCulture);
			attributes["scene.name"] = activeScene.name;
			attributes["scene.path"] = activeScene.path;
		}
	}
}
