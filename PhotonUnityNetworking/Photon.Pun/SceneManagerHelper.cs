using UnityEngine.SceneManagement;

namespace Photon.Pun;

public class SceneManagerHelper
{
	public static string ActiveSceneName => SceneManager.GetActiveScene().name;

	public static int ActiveSceneBuildIndex => SceneManager.GetActiveScene().buildIndex;
}
