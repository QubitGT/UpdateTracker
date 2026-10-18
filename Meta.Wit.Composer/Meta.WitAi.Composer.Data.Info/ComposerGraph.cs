using System;
using UnityEngine;

namespace Meta.WitAi.Composer.Data.Info;

[Serializable]
public struct ComposerGraph
{
	[HideInInspector]
	public string canvasName;

	[Tooltip("The Context Map is a JSON object passed between the the server and the client. \nThese are the JSON paths and values present in the Context Map on the server")]
	public ContextMapPaths contextMap;

	[Tooltip("A listing of all the actions sent by the Responses in this canvas.")]
	public string[] actions;
}
