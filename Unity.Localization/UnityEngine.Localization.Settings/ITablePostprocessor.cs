using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization.Settings;

public interface ITablePostprocessor
{
	void PostprocessTable(LocalizationTable table);
}
