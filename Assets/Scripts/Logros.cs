using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.UI;

public class Logros : MonoBehaviour
{
    public TimeControl TimeControl;

    // Update is called once per frame
    void Update()
    {

        if (TimeControl._score < 100)
        {
            PlayGamesScript.UnlockAchievement(GPGSIds.achievement_test);
        }

    }
}
