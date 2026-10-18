using System.Collections.Generic;
using UnityEngine;

namespace Drawing.Examples;

public class CurveEditor : MonoBehaviour
{
	private class CurvePoint
	{
		public Vector2 position;

		public Vector2 controlPoint0;

		public Vector2 controlPoint1;
	}

	private List<CurvePoint> curves = new List<CurvePoint>();

	private Camera cam;

	public Color curveColor;

	private void Awake()
	{
		cam = Camera.main;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			curves.Add(new CurvePoint
			{
				position = Input.mousePosition,
				controlPoint0 = Vector2.zero,
				controlPoint1 = Vector2.zero
			});
		}
		if (curves.Count > 0 && Input.GetKey(KeyCode.Mouse0) && ((Vector2)Input.mousePosition - curves[curves.Count - 1].position).magnitude > 4f)
		{
			CurvePoint curvePoint = curves[curves.Count - 1];
			curvePoint.controlPoint1 = (Vector2)Input.mousePosition - curvePoint.position;
			curvePoint.controlPoint0 = -curvePoint.controlPoint1;
		}
		Render();
	}

	private void Render()
	{
		using CommandBuilder commandBuilder = DrawingManager.GetBuilder(renderInGame: true);
		using (commandBuilder.InScreenSpace(cam))
		{
			for (int i = 0; i < curves.Count; i++)
			{
				commandBuilder.xy.Circle((Vector3)curves[i].position, 2f, Color.blue);
			}
			for (int j = 0; j < curves.Count - 1; j++)
			{
				Vector2 position = curves[j].position;
				Vector2 vector = position + curves[j].controlPoint1;
				Vector2 position2 = curves[j + 1].position;
				Vector2 vector2 = position2 + curves[j + 1].controlPoint0;
				commandBuilder.Bezier((Vector3)position, (Vector3)vector, (Vector3)vector2, (Vector3)position2, curveColor);
			}
		}
	}
}
