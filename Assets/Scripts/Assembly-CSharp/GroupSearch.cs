using UnityEngine;

public abstract class GroupSearch : MonoBehaviour
{
	private Collider[] overlapTestColliders;

	protected virtual void Awake()
	{
		overlapTestColliders = new Collider[10];
	}

	public ToolGroup FindGroup(GroupableTool thisGroupableTool = null, ToolGroup thisGroup = null)
	{
		ToolGroup result = null;
		int num = UpdateOverlapTest(false);
		for (int i = 0; i < num; i++)
		{
			Tool colliderTool = overlapTestColliders[i].GetColliderTool();
			ToolGroup toolGroup = ((!(colliderTool != null)) ? null : colliderTool.GetComponent<ToolGroup>());
			if (toolGroup != null && toolGroup != thisGroup && toolGroup.Size > 0 && (thisGroupableTool == null || toolGroup.CanAddTool(thisGroupableTool)) && (thisGroup == null || toolGroup.CanAddGroup(thisGroup)))
			{
				result = toolGroup;
				break;
			}
		}
		return result;
	}

	public GroupableTool FindGroupableTool(GroupableTool thisGroupableTool = null)
	{
		GroupableTool result = null;
		int num = UpdateOverlapTest(true);
		for (int i = 0; i < num; i++)
		{
			Tool colliderTool = overlapTestColliders[i].GetColliderTool();
			GroupableTool groupableTool = ((!(colliderTool != null)) ? null : colliderTool.GetComponent<GroupableTool>());
			if (groupableTool != null && groupableTool != thisGroupableTool && !groupableTool.IsInGroup && (thisGroupableTool == null || groupableTool.CanAddTool(thisGroupableTool)))
			{
				result = groupableTool;
				break;
			}
		}
		return result;
	}

	protected abstract void GetOverlapGeometry(out Vector3 center, out Vector3 size);

	protected int UpdateOverlapTest(bool testTriggers)
	{
		Vector3 center;
		Vector3 size;
		GetOverlapGeometry(out center, out size);
		center = base.transform.TransformPoint(center);
		size = base.transform.lossyScale.MultiplyComponents(size);
		return Physics.OverlapBoxNonAlloc(center, size / 2f, overlapTestColliders, base.transform.rotation, 226894848, (!testTriggers) ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
	}

	private void OnDrawGizmos()
	{
		Vector3 center;
		Vector3 size;
		GetOverlapGeometry(out center, out size);
		Gizmos.color = Color.red;
		Matrix4x4 matrix = Matrix4x4.TRS(base.transform.TransformPoint(center), base.transform.rotation, base.transform.lossyScale.MultiplyComponents(size));
		Gizmos.matrix = matrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = Matrix4x4.identity;
	}
}
