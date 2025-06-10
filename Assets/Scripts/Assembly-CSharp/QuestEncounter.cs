using UnityEngine;

[CreateAssetMenu(menuName = "RecRoom Quest/Encounter", fileName = "Quest_Encounter")]
public class QuestEncounter : ScriptableObject
{
	public EnemyType[] Enemies;

	public int Size
	{
		get
		{
			return (Enemies != null) ? Enemies.Length : 0;
		}
	}
}
