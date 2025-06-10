public class HorseShoe : Tool
{
	public OccupiableSpawnPoint SpawnPoint { get; private set; }

	public void Spawn(OccupiableSpawnPoint spawnPoint)
	{
		if (base.hasAuthority)
		{
			SpawnPoint = spawnPoint;
			base.transform.position = SpawnPoint.transform.position;
			base.transform.rotation = SpawnPoint.transform.rotation;
		}
	}

	public void Kill()
	{
		SpawnPoint = null;
	}
}
