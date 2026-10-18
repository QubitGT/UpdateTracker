using UnityEngine;

namespace Meta.WitAi;

public class Comment : MonoBehaviour
{
	[SerializeField]
	internal string title;

	[TextArea]
	[SerializeField]
	internal string comment;

	[SerializeField]
	internal bool lockComment;
}
