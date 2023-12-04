using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.Pool;

public enum ItemEvent
{
    WaitToActive,
}

public class ItemControl : Singleton<ItemControl>
{
    [Header("场景可食用物体参数")]
    [SerializeField] private int itemCount;

    [SerializeField] private float bigItemPercent;
    [SerializeField] private float midItemPercent;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject midItemPrefab;
    [SerializeField] private GameObject bigItemPrefab;
    public static BoxCollider2D _boxCol;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        _boxCol = GetComponent<BoxCollider2D>();
        // EventManager.Instance?.AddListener(GameStateEvent.GameStart, InstantiateItem); 
        EventManager.Instance?.AddListener(ItemEvent.WaitToActive, WaitToActive);
        EventManager.Instance?.AddListener(GameStateEvent.GameStart, InstantiateItem);
        
        InstantiateItem();
    }

    void InstantiateItem()
    {
        if(itemPrefab == null || midItemPrefab == null || bigItemPrefab == null) return;
        for(int i = 0; i < itemCount; i ++)
        {
            GameObject item = GetItem();
            Vector3 pos = RandomPoint();
            item.transform.position = pos;
        }
    }

    //获取物体
    GameObject GetItem()
    {
        GameObject item;
        float num = Random.Range(0f, 1f);
        
        if(num > bigItemPercent + midItemPercent) item = ObjectPool.Instance.GetObject(itemPrefab);
        else if (num > bigItemPercent) item = ObjectPool.Instance?.GetObject(midItemPrefab);
        else item = ObjectPool.Instance.GetObject(bigItemPrefab);
        
        return item;
    }
    
    public Vector3 RandomPoint()
    {
        float x = Random.Range(-_boxCol.size.x / 2 + 1, _boxCol.size.x / 2 - 1);
        float y = Random.Range(-_boxCol.size.y / 2 + 1, _boxCol.size.y / 2 - 1);

        return new Vector2(x, y);
    }
    
    public Vector3 RandomPointPadding(float size)
    {
        float x = Random.Range(-_boxCol.size.x / 2 + 1 + size, _boxCol.size.x / 2 - 1 - size);
        float y = Random.Range(-_boxCol.size.y / 2 + 1 + size, _boxCol.size.y / 2 - 1 - size);

        return new Vector2(x, y);
    }

    //等待一定时间后重新启用
    void WaitToActive()
    {
        StartCoroutine(WaitToActiveCoroutine());
    }

    IEnumerator WaitToActiveCoroutine()
    {
        yield return new WaitForSeconds(3);
        GameObject gameObject = GetItem();
        gameObject.transform.position = RandomPoint();
    }
}
