using UnityEngine;

public class GameController : MonoBehaviour
{
    public static bool isStart;
    public static bool isTraining;
    void Awake()
    {
        isStart = false;
        isTraining = true;
    }
    public void SetStart()
    {
        isStart = true;
    }
    public void CloseGame()
    {
        Application.Quit();
    }
    void Update()
    {
    }
}
