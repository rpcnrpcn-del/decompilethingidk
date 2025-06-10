using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CubeMapGenerator : MonoBehaviour
{
	public int Resolution = 2048;

	public int AntiAliasLevel = 8;
}
