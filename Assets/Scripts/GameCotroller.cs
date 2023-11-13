using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class GameCotroller : MonoBehaviour
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
