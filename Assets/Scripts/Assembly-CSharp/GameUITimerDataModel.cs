public class GameUITimerDataModel
{
	public float GameTimeRemaining;

	public float GameDuration;

	public float GameStateTransitionTimeRemaining;

	public static void DeepCopy(GameUITimerDataModel source, GameUITimerDataModel dest)
	{
		dest.GameTimeRemaining = source.GameTimeRemaining;
		dest.GameDuration = source.GameDuration;
		dest.GameStateTransitionTimeRemaining = source.GameStateTransitionTimeRemaining;
	}
}
