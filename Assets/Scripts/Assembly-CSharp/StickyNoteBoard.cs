using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StickyNoteBoard : MonoBehaviour
{
	[SerializeField]
	private List<StickyNote> notes;

	private BoxCollider boxCollider;

	private const float STACK_OFFSET = 0.001f;

	private void Awake()
	{
		boxCollider = GetComponent<BoxCollider>();
		RestackNotes();
	}

	public void GetNoteAttachPosition(Transform noteTransform, out Vector3 targetPosition, out Quaternion targetRotation)
	{
		float num = Vector3.Dot(base.transform.position - noteTransform.position, base.transform.forward) - (float)notes.Count * 0.001f;
		targetPosition = noteTransform.position + num * base.transform.forward;
		targetRotation = Quaternion.FromToRotation(noteTransform.forward, base.transform.forward) * noteTransform.rotation;
	}

	public bool WillFitOnBoard(Vector3 position, Quaternion rotation, Vector3 halfExtents)
	{
		Transform transform = boxCollider.transform;
		Vector3 vector = boxCollider.size / 2f;
		Vector3[] array = new Vector3[4]
		{
			new Vector3(0f - halfExtents.x, 0f - halfExtents.y, 0f),
			new Vector3(0f - halfExtents.x, halfExtents.y, 0f),
			new Vector3(halfExtents.x, halfExtents.y, 0f),
			new Vector3(halfExtents.x, 0f - halfExtents.y, 0f)
		};
		Vector3[] array2 = array;
		foreach (Vector2 vector2 in array2)
		{
			Vector3 vector3 = transform.InverseTransformPoint(position + rotation * vector2);
			if (Mathf.Abs(vector3.x) > vector.x || Mathf.Abs(vector3.y) > vector.y)
			{
				return false;
			}
		}
		return true;
	}

	public void MoveNoteToTop(StickyNote note)
	{
		notes.Remove(note);
		notes.Add(note);
		RestackNotes();
	}

	private void RestackNotes()
	{
		for (int i = 0; i < notes.Count; i++)
		{
			Transform transform = notes[i].transform;
			float num = Vector3.Dot(base.transform.position - transform.position, base.transform.forward) - (float)i * 0.001f;
			transform.position += num * base.transform.forward;
		}
	}
}
