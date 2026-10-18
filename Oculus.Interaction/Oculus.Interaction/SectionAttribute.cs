using UnityEngine;

namespace Oculus.Interaction;

public class SectionAttribute : PropertyAttribute
{
	public string SectionName { get; private set; } = string.Empty;


	public SectionAttribute(string sectionName)
	{
		SectionName = sectionName;
	}
}
