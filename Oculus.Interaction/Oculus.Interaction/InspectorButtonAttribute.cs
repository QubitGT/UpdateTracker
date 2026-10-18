using System;
using UnityEngine;

namespace Oculus.Interaction;

[AttributeUsage(AttributeTargets.Field)]
public class InspectorButtonAttribute : PropertyAttribute
{
	private const float BUTTON_WIDTH = 80f;

	private const float BUTTON_HEIGHT = 20f;

	public readonly string methodName;

	public readonly float buttonHeight;

	public float ButtonWidth { get; set; } = 80f;


	public InspectorButtonAttribute(string methodName)
	{
		this.methodName = methodName;
		buttonHeight = 20f;
	}

	public InspectorButtonAttribute(string methodName, float buttonHeight)
	{
		this.methodName = methodName;
		this.buttonHeight = buttonHeight;
	}
}
