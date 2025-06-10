public interface IRecRoomSceneComponent
{
	void OnStart(RecRoomSceneManager sceneManager);

	void OnDestroy();

	void Update();

	void SetLocalPlayerSettings();
}
