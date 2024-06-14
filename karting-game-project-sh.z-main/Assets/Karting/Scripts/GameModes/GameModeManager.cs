using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public enum GameMode
    {
        Controller,
        Mode1,
        Mode2,
        Mode3,
        Mode4,
        Test
    }

    public GameMode currentGameMode;

    // Singleton pattern
    public static GameModeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This object is not destroyed when loading a new scene
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject); // If an instance already exists, destroy this instance in the new scene
            }
        }
    }

    public void SetGameMode1()
    {
        UnityEngine.Debug.Log("Game Mode 1 activated");
        currentGameMode = GameMode.Mode1;
    }

    public void SetGameMode2()
    {
        UnityEngine.Debug.Log("Game Mode 2 activated");
        currentGameMode = GameMode.Mode2;
    }

    public void SetGameMode3()
    {
        UnityEngine.Debug.Log("Game Mode 3 activated");
        currentGameMode = GameMode.Mode3;
    }

    public void SetGameMode4()
    {
        UnityEngine.Debug.Log("Game Mode 4 activated");
        currentGameMode = GameMode.Mode4;
    }

    public void SetGameModeTest()
    {
        UnityEngine.Debug.Log("Game Mode test activated");
        currentGameMode = GameMode.Test;
    }

    //public void SetGameModeController()
    //{
    //    UnityEngine.Debug.Log("Game Mode controller activated");
    //    currentGameMode = GameMode.Controller;
    //}
}

