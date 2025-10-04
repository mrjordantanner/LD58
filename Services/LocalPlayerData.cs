using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/Player Data")]
public class LocalPlayerData : ScriptableObject
{
    [ReadOnly] public string PlayerName;
    [ReadOnly] public int PlayerScore;
    [ReadOnly] public int PlayerBestScore;
    [ReadOnly] public int Replays;
    [ReadOnly] public float TotalTimeElapsed;

    public bool newHighScore;

    [Header("Default Values")]
    public string defaultPlayerName = "Player";

    public void ResetGameSessionData()
    {
        newHighScore = false;
        PlayerScore = 0;
    }

    public async Task ResetAllToDefaults()
    {
        ResetGameSessionData();

        PlayerName = defaultPlayerName;
        PlayerBestScore = 0;
        Replays = 0;
        TotalTimeElapsed = 0;

        await PlayerData.Instance.SaveAllAsync();
    }

}
