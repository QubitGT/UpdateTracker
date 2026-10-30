using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("GorillaTag/Super Infection/SIUI Player Quest Entry")]
public class SIUIPlayerQuestEntry : MonoBehaviour
{
	public Image background;

	public SIUIProgressBar progress;

	public TextMeshProUGUI questDescription;

	public GameObject completeOverlay;

	public GameObject questInfo;

	public GameObject noQuestAvailable;

	public GameObject newQuestTag;

	public int lastQuestId;

	public int lastQuestProgress;

	private void Awake()
	{
		lastQuestId = -1;
		lastQuestProgress = -1;
	}
}
