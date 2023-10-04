using UnityEngine;
public class PlayGamesScript : MonoBehaviour
{
    /*
    void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        SingIn();
    }

    void SingIn()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                Debug.Log("Inicio de sesion correcto");
            }
            else
            {
                Debug.Log("Inicio de sesion fallido");
            }
        });
    }

    #region Achievements
    public static void UnlockAchievement(string id)
    {
        Social.ReportProgress(id, 100, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Logro " + id + " desbloqueado");
            }
            else
            {
                Debug.Log("Logro " + id + " fallado");
            }
        });
    }

    public static void ShowAchievementsUI()
    {
        Social.ShowAchievementsUI();
    }
    #endregion

    #region Leaderboards
    public static void AddScoreToLeaderboard(string leaderboardId, long score)
    {
        Social.ReportScore(score, leaderboardId, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Puntuación " + score + " en el leaderboard " + leaderboardId + " enviada");
            }
            else
            {
                Debug.Log("Puntuación " + score + " en el leaderboard " + leaderboardId + " fallada");
            }
        });
    }
    #endregion
    */
}
