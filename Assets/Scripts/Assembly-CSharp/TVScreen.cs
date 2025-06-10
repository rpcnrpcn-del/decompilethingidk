using UnityEngine;

public class TVScreen : MonoBehaviour
{
	[SerializeField]
	private MovieTexture[] movies;

	private Renderer renderer;

	private void Start()
	{
		renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		MovieTexture movieTexture = renderer.material.mainTexture as MovieTexture;
		if (!(movieTexture != null) || !movieTexture.isPlaying)
		{
			int num = Random.Range(0, movies.Length);
			renderer.material.mainTexture = movies[num];
			MovieTexture movieTexture2 = renderer.material.mainTexture as MovieTexture;
			movieTexture2.Stop();
			movieTexture2.Play();
		}
	}
}
