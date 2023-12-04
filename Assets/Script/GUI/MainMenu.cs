using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startBtn;

    [SerializeField] private Button quitBtn;
    // Start is called before the first frame update
    void Start()
    {
        startBtn.onClick.AddListener(StartGame);
        quitBtn.onClick.AddListener(QuitGame);
    }

    void StartGame()
    {
        //加载场景
        SceneController.Instance?.LoadSceneWithIndex(SceneManager.GetActiveScene().buildIndex + 1, GameStateEvent.GameStart);
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
