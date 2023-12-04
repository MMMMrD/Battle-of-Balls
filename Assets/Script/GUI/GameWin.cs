using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    [SerializeField] private Button restart;
    [SerializeField] private Button returnMenu;

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void Start()
    {
        restart.onClick.AddListener(RestartGame);
        returnMenu.onClick.AddListener(ReturnMainMenu);
        EventManager.Instance?.AddListener(GameStateEvent.GameWin, () => { gameObject.SetActive(true); });

        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    //重新开始游戏
    void RestartGame()
    {
        //TODO:时间暂停
        Time.timeScale = 1;
        SceneController.Instance?.LoadSceneWithIndex(SceneManager.GetActiveScene().buildIndex, GameStateEvent.GameStart);
    }

    //返回主菜单
    void ReturnMainMenu()
    {
        //调用 SceneController 用于切换场景
        Time.timeScale = 1;
        SceneController.Instance?.LoadSceneWithIndex(0, GameStateEvent.ReturnMainMenu);
    }
}