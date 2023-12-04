using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStop : MonoBehaviour
{
    [SerializeField] private Button restart;
    [SerializeField] private Button returnMenu;
    [SerializeField] private Button closeBtn;

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void Start()
    {
        restart.onClick.AddListener(RestartGame);
        returnMenu.onClick.AddListener(ReturnMainMenu);
        closeBtn.onClick.AddListener(() => { 
            gameObject.SetActive(false);
            Time.timeScale = 1;
        });
    }

    //重新开始游戏
    void RestartGame()
    {
        //TODO:时间暂停
        Time.timeScale = 1;
        ObjectPool.Instance.ClearObjectPool();
        SceneController.Instance?.LoadSceneWithIndex(SceneManager.GetActiveScene().buildIndex, GameStateEvent.GameStart);
    }

    //返回主菜单
    void ReturnMainMenu()
    {
        //调用 SceneController 用于切换场景
        Time.timeScale = 1;
        ObjectPool.Instance.ClearObjectPool();
        SceneController.Instance?.LoadSceneWithIndex(0, GameStateEvent.ReturnMainMenu);
    }
}