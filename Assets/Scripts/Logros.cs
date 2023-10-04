using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.UI;

public class Logros : MonoBehaviour
{
    public TimeControl TimeControl;

    void Update()
    {

        if (TimeControl.score < 100)
        {
            //PlayGamesScript.UnlockAchievement(GPGSIds.achievement_test);
        }

    }
}
