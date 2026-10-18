using System;

namespace UnityEngine.ProBuilder;

[Serializable]
internal class ChangelogEntry
{
	[SerializeField]
	private SemVer m_VersionInfo;

	[SerializeField]
	private string m_ReleaseNotes;

	public SemVer versionInfo => m_VersionInfo;

	public string releaseNotes => m_ReleaseNotes;

	public ChangelogEntry(SemVer version, string releaseNotes)
	{
		m_VersionInfo = version;
		m_ReleaseNotes = releaseNotes;
	}

	public override string ToString()
	{
		return m_VersionInfo.ToString() + "\n\n" + m_ReleaseNotes;
	}
}
