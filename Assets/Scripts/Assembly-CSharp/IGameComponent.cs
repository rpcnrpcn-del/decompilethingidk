public interface IGameComponent
{
	void OnAwake(GameManager gameManager);

	void OnDestroy();

	void OnStart();

	void OnUpdate();

	void InitializeLocalPlayer(bool localPlayerIsSpectator);

	void ResetLocalPlayer(bool localPlayerIsSpectator);

	void OnPlayerDisconnected(PhotonPlayer player);
}
