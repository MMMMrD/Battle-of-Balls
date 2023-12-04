using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Tools;
using Unity.VisualScripting;
using UnityEngine.Timeline;

public enum GameStateEvent{
    RegisterEnemy,
    RemoveEnemy,
    InstantiatePlayer, //加载角色
    GameStart, //游戏开始
    GameOver, //游戏结束
    GameWin,
    PlayerMove, //角色移动
    SetCameraSize,
    InstantiateItem,
    ReturnMainMenu, //返回主菜单
    PlayerSplit,
}

public class PlayerSplitData
{
    public Vector3 point;
    public float radius;
    public float size;
    public Vector3 dir;
    public Sprite sprite;
    
    public PlayerSplitData(Vector3 point, float radius, float size, Vector3 otherDir, Sprite sprite)
    {
        this.point = point;
        this.size = size;
        this.radius = radius;
        this.sprite = sprite;

        if (otherDir.x + otherDir.y + otherDir.z != 0) this.dir = otherDir;
        else this.dir = new Vector3(1, 0, 0);
    }
}

//规定一些游戏规则，以及一些重要的游戏事件
public class GameManager : Tools.Singleton<GameManager>
{
    [Header("游戏视角相关数据")]
    [SerializeField]private float moveSpeed = 2.0f;
    private Vector3 lookAtPoint = new Vector3(0,0,0);
    private Vector3 targetPoint = new Vector3(0, 0, 0);
    
    private List<IGetHit> enemyList = new List<IGetHit>();
    private List<GameObject> playerList = new List<GameObject>();
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        //添加游戏中的各种事件
        // EventManager.Instance?.AddListener(GameStateEvent.InstantiatePlayer, InstantiatePlayer);
        EventManager.Instance?.AddListener(GameStateEvent.GameStart, GameStart);
        EventManager.Instance?.AddListener(GameStateEvent.RegisterEnemy, RegisterEnemy);
        EventManager.Instance?.AddListener(GameStateEvent.RemoveEnemy, RemoveEnemy);
        EventManager.Instance?.AddListener(GameStateEvent.PlayerSplit, PlayerSplit);
        
        InstantiatePlayer();
        StartCoroutine(LookAtPointMovementCoroutine());
    }

    private void Update()
    {
        LookAtPointMovement();
        EventManager.Instance?.InvokeEvent(GameStateEvent.PlayerMove, lookAtPoint);
    }

    void GameStart()
    {
        InstantiatePlayer();
    }

    void RegisterEnemy(object data)
    {
        IGetHit enemy = (IGetHit)data;
        enemyList.Add(enemy);
    }

    void RemoveEnemy(object data)
    {
        IGetHit enemy = (IGetHit)data;
        if (enemyList.Contains(enemy)) enemyList.Remove(enemy);
        Debug.Log(enemyList.Count);
        if(enemyList.Count == 0) GameOver(true);
    }

    public void RemoveAllEnemy()
    {
        enemyList.Clear();
    }

    public void GameOver(object data)
    {
        bool win = (bool)data;
        if (win) EventManager.Instance?.InvokeEvent(GameStateEvent.GameWin);
        else EventManager.Instance?.InvokeEvent(GameStateEvent.GameOver);
    }

    void LookAtPointMovement()
    {
        targetPoint = new Vector3(0, 0, 0);
        foreach (var value in playerList)
        {
            targetPoint += value.transform.position;
        }

        targetPoint /= playerList.Count;
    }

    IEnumerator LookAtPointMovementCoroutine()
    {
        while (true)
        {
            Vector3 dir = (targetPoint - lookAtPoint);
            if (Vector3.Distance(targetPoint, lookAtPoint) > 0.01f)
            {
                lookAtPoint += dir * moveSpeed * Time.deltaTime;
            }
            yield return null;
        }
    }

    void SetMainCameraSize()
    {
        float distance = 0.0f;
        foreach (var value in playerList)
        {
            float tmpDis = Vector3.Distance(value.transform.position, targetPoint) + value.transform.localScale.x;
            distance = tmpDis > distance ? tmpDis : distance;
        }
        
        Debug.Log(distance);
        
        EventManager.Instance?.InvokeEvent(GameStateEvent.SetCameraSize, distance);
    }

    #region 玩家相关

    //生成玩家分裂物体
    public void PlayerSplit(object data)
    {
        PlayerSplitData splitData = (PlayerSplitData)data;
        GameObject playerSplitObject =  ObjectPool.Instance.GetObjectButNoSetActive((GameObject)Resources.Load("Prefabs/PlayerObject/PlayerSplitObject"));
        
        if (playerSplitObject != null)
        {
            playerSplitObject.transform.position = splitData.point;
        
            playerSplitObject.SetActive(true);
            IGetHit item = playerSplitObject.GetComponent<IGetHit>();
            item.SetCanEat(false);
            item.SetSize(splitData.size);
            item.SetSprite(splitData.sprite);
            item.SetDirection(splitData.dir.normalized);
            playerList.Add(playerSplitObject);
         
            SetMainCameraSize();
        }
    }

    public void PlayerRemove(GameObject playerObject)
    {
        if (playerList.Contains(playerObject)) playerList.Remove(playerObject);
        if(playerList.Count == 0) GameOver(false);
    }

    //生成玩家初始物体
    void InstantiatePlayer()
    {
        playerList.Clear();
        GameObject prefab = (GameObject)Resources.Load("Prefabs/PlayerObject/Player");
        GameObject playerObject = ObjectPool.Instance.GetObject(prefab);
        playerObject.transform.position = ItemControl.Instance.RandomPoint();
        lookAtPoint = playerObject.transform.position;
        targetPoint = playerObject.transform.position;
        playerList.Add(playerObject);

        SetMainCameraSize();
    }
    #endregion
}
