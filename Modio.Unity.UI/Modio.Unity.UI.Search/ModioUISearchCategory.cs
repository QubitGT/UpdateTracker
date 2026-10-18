using System.Collections.Generic;
using UnityEngine;

namespace Modio.Unity.UI.Search;

public class ModioUISearchCategory : MonoBehaviour
{
	[SerializeField]
	private string _categoryLabel;

	[SerializeField]
	private string _categoryLabelLocalized;

	[SerializeField]
	private List<ModioUISearchSettings> _tabs;

	[SerializeField]
	private ModioUISearchSettings _customSearchBase;

	public string CategoryLabel => _categoryLabel;

	public string CategoryLabelLocalized => _categoryLabelLocalized;

	public IEnumerable<ModioUISearchSettings> Tabs => _tabs;

	public ModioUISearchSettings CustomSearchBase => _customSearchBase;
}
