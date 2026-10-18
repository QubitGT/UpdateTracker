using System;
using System.Collections.Generic;
using Pathfinding.RVO;
using Pathfinding.RVO.Sampled;
using UnityEngine;

namespace Pathfinding.Examples;

[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_examples_1_1_group_controller.php")]
public class GroupController : MonoBehaviour
{
	public GUIStyle selectionBox;

	public bool adjustCamera = true;

	private Vector2 start;

	private Vector2 end;

	private bool wasDown;

	private List<RVOExampleAgent> selection = new List<RVOExampleAgent>();

	private Simulator sim;

	private Camera cam;

	private const float rad2Deg = 180f / MathF.PI;

	public void Start()
	{
		cam = Camera.main;
		RVOSimulator active = RVOSimulator.active;
		if (active == null)
		{
			base.enabled = false;
			throw new Exception("No RVOSimulator in the scene. Please add one");
		}
		sim = active.GetSimulator();
	}

	public void Update()
	{
		if (adjustCamera)
		{
			List<Agent> agents = sim.GetAgents();
			float num = 0f;
			for (int i = 0; i < agents.Count; i++)
			{
				float num2 = Mathf.Max(Mathf.Abs(agents[i].Position.x), Mathf.Abs(agents[i].Position.y));
				if (num2 > num)
				{
					num = num2;
				}
			}
			float a = num / Mathf.Tan(cam.fieldOfView * (MathF.PI / 180f) / 2f);
			float b = num / Mathf.Tan(Mathf.Atan(Mathf.Tan(cam.fieldOfView * (MathF.PI / 180f) / 2f) * cam.aspect));
			float a2 = Mathf.Max(a, b) * 1.1f;
			a2 = Mathf.Max(a2, 20f);
			a2 = Mathf.Min(a2, cam.farClipPlane - 1f);
			cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(0f, a2, 0f), Time.smoothDeltaTime * 2f);
		}
		if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Mouse0))
		{
			Order();
		}
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Input.GetKey(KeyCode.A))
		{
			Select(start, end);
			wasDown = false;
		}
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
		{
			end = Event.current.mousePosition;
			if (!wasDown)
			{
				start = end;
				wasDown = true;
			}
		}
		if (Input.GetKey(KeyCode.A))
		{
			wasDown = false;
		}
		if (wasDown)
		{
			Rect position = Rect.MinMaxRect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
			if (position.width > 4f && position.height > 4f)
			{
				GUI.Box(position, "", selectionBox);
			}
		}
	}

	public void Order()
	{
		if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hitInfo))
		{
			float num = 0f;
			for (int i = 0; i < selection.Count; i++)
			{
				num += selection[i].GetComponent<RVOController>().radius;
			}
			float num2 = num / MathF.PI;
			num2 *= 2f;
			for (int j = 0; j < selection.Count; j++)
			{
				float num3 = MathF.PI * 2f * (float)j / (float)selection.Count;
				Vector3 target = hitInfo.point + new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * num2;
				selection[j].SetTarget(target);
				selection[j].SetColor(GetColor(num3));
				selection[j].RecalculatePath();
			}
		}
	}

	public void Select(Vector2 _start, Vector2 _end)
	{
		_start.y = (float)Screen.height - _start.y;
		_end.y = (float)Screen.height - _end.y;
		Vector2 vector = Vector2.Min(_start, _end);
		Vector2 vector2 = Vector2.Max(_start, _end);
		if ((vector2 - vector).sqrMagnitude < 16f)
		{
			return;
		}
		selection.Clear();
		RVOExampleAgent[] array = UnityEngine.Object.FindObjectsByType<RVOExampleAgent>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 vector3 = cam.WorldToScreenPoint(array[i].transform.position);
			if (vector3.x > vector.x && vector3.y > vector.y && vector3.x < vector2.x && vector3.y < vector2.y)
			{
				selection.Add(array[i]);
			}
		}
	}

	public Color GetColor(float angle)
	{
		return AstarMath.HSVToRGB(angle * (180f / MathF.PI), 0.8f, 0.6f);
	}
}
