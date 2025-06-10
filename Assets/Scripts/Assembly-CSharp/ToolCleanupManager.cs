using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class ToolCleanupManager : SingletonMonoBehaviour<ToolCleanupManager>
{
	[SerializeField]
	[Tooltip("Min distance from original spawn position before a tool is considered eligible for cleanup")]
	private float minDisplacementBeforeCleanup = 0.1f;

	[SerializeField]
	[Tooltip("Min distance from the closest player before a tool is considered eligible for cleanup")]
	private float minPlayerDistanceBeforeCleanup = 3f;

	[SerializeField]
	[Tooltip("Delay before eligible tools are cleaned up")]
	private float delayUntilCleanup = 15f;

	[SerializeField]
	[Tooltip("Tools that should be excluded from cleanup")]
	private Tool[] excludedTools;

	private int index;

	private List<ToolCleanup> tools;

	private void Awake()
	{
		SingletonMonoBehaviour<ToolCleanupManager>.Instance = this;
		tools = new List<ToolCleanup>();
	}

	private void Start()
	{
		if (excludedTools == null)
		{
			excludedTools = new Tool[0];
		}
		GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			Tool[] componentsInChildren = gameObject.GetComponentsInChildren<Tool>(true);
			foreach (Tool tool in componentsInChildren)
			{
				AddToolHelper(tool);
			}
		}
		if (tools.Count == 0)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (tools != null && tools.Count != 0)
		{
			index %= tools.Count;
			if (tools[index] == null || tools[index].gameObject == null)
			{
				tools.RemoveAt(index);
				index %= tools.Count;
			}
			else
			{
				tools[index].CheckCleanup();
				index = (index + 1) % tools.Count;
			}
		}
	}

	public ToolCleanup AddTool(Tool tool)
	{
		return AddToolHelper(tool);
	}

	public void RemoveTool(Tool tool)
	{
		ToolCleanup cleanup = tool.GetComponent<ToolCleanup>();
		if (cleanup == null)
		{
			return;
		}
		int num = tools.FindIndex((ToolCleanup c) => c == cleanup);
		if (num < 0)
		{
			return;
		}
		tools.RemoveAt(num);
		if (tools.Count == 0)
		{
			index = 0;
			return;
		}
		if (num < index)
		{
			index--;
		}
		index %= tools.Count;
	}

	private ToolCleanup AddToolHelper(Tool tool)
	{
		if (!excludedTools.Contains(tool))
		{
			ToolCleanup toolCleanup = tool.GetComponent<ToolCleanup>();
			if (toolCleanup == null)
			{
				toolCleanup = tool.gameObject.AddComponent<ToolCleanup>();
				toolCleanup.MinDisplacementBeforeCleanup = minDisplacementBeforeCleanup;
				toolCleanup.MinPlayerDistanceBeforeCleanup = minPlayerDistanceBeforeCleanup;
				toolCleanup.DelayUntilCleanup = delayUntilCleanup;
			}
			if (!toolCleanup.ExcludeFromCleanup)
			{
				tools.Add(toolCleanup);
				return toolCleanup;
			}
		}
		return null;
	}
}
