using UnityEngine;

public class InlineLinkAttribute : PropertyAttribute
{
	public string DocumentationURL;

	public InlineLinkAttribute(string documentationURL)
	{
		DocumentationURL = documentationURL;
	}
}
