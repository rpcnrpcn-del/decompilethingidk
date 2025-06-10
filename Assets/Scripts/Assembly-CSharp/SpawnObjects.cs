using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
	public GameObject ObjectToSpawn;

	public float Interval;

	public float NumObjects;

	private float SpawnTimer;

	private int SpawnCounter;

	private void Start()
	{
		SpawnTimer = 0f;
		SpawnCounter = 0;
	}

	private void Update()
	{
		if ((float)SpawnCounter < NumObjects)
		{
			SpawnTimer -= Time.deltaTime;
			if (SpawnTimer < 0f)
			{
				Object.Instantiate(ObjectToSpawn, base.transform.position, base.transform.rotation);
				SpawnTimer = Interval;
				SpawnCounter++;
			}
		}
	}
}
