using System;
using Unity.Mathematics;
using UnityEngine;

namespace Drawing.Examples;

public class AlineStyling : MonoBehaviour
{
	public Color gizmoColor = new Color(1f, 0.34509805f, 1f / 3f);

	public Color gizmoColor2 = new Color(0.30980393f, 0.8f, 79f / 85f);

	private void Update()
	{
		CommandBuilder ingame = Draw.ingame;
		using (ingame.InScreenSpace(Camera.main))
		{
			using (ingame.WithMatrix(Matrix4x4.TRS(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f), Quaternion.identity, new Vector3(Screen.width, Screen.width, 1f))))
			{
				for (int i = 0; i < 4; i++)
				{
					using (ingame.WithLineWidth(i * i + 1))
					{
						float x = MathF.PI / 4f * (float)(i + 1) + Time.time * (float)i;
						Vector3 vector = new Vector3(-0.3f + (float)i * 0.2f, 0f, 0f);
						float num = 0.075f;
						ingame.Line(vector + new Vector3(math.cos(x) * num, math.sin(x) * num, 0f), vector, gizmoColor);
						ingame.Line(vector, vector + new Vector3(num, 0f, 0f), gizmoColor);
						ingame.xy.Circle(vector, num, gizmoColor2);
					}
				}
			}
		}
	}
}
