using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterData))]
public class PlayerController : MonoBehaviour, IGetHit
{
    //Model，用于保存对象数据
    [Header("角色基本参数")] 
    [SerializeField]private bool canEat = true;
    private CharacterData _characterData;
    private SpriteRenderer _spriteRenderer;
    // [SerializeField]private float changeSizeSpeed = 1.0f;
    
    [Header("移动相关")]
    private float maxSpeed;
    private Vector3 direction;

    private Coroutine changeSizeCoroutine = null;

    public float Speed //Character速度，只读
    {
        get { return _characterData.Speed; }
        set
        {
            _characterData.Speed = Mathf.Clamp(value, 0.75f, 6f);
        }
    }

    public float Size
    {
        get { return _characterData.Size; }
        set
        {
            _characterData.Size = value;
        }
    }

    public bool IsCanEat
    {
        get { return canEat; }
        set { canEat = value; }
    }

    private void Awake()
    {
        InitComponent();
        maxSpeed = Speed;
    }

    // private void OnEnable()
    // {
    //     //当物体开启时重置大小
    //     InitComponent();
    // }

    private void Start()
    {
        // InitComponent();
        
        SetSprite();
        AfterEat(0);
    }

    void Update()
    {
        Movement();
        PlayerSplit();
    }

    void SetSprite()
    {
        if(_spriteRenderer.sprite != null) return;
        Sprite[] sprites = Resources.LoadAll<Sprite>("Image");
        int index = Random.Range(0, sprites.Length);
        _spriteRenderer.sprite = sprites[index];
    }

    public void SetSprite(Sprite sprite)
    {
        if(_spriteRenderer != null)
            _spriteRenderer.sprite = sprite;
    }

    //初始化所有Component
    void InitComponent()
    {
        if(_characterData == null) _characterData = GetComponent<CharacterData>();
        if(_spriteRenderer == null) _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Movement()
    {
        float x = 60.0f, y = 60.0f;
        if (ItemControl._boxCol)
        {
            x = ItemControl._boxCol.size.x;
            y = ItemControl._boxCol.size.y;
        }
        
        direction = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.A) && (transform.position.x > -x/2 + Size/2))
        {
            // transform.position = new Vector2(transform.position.x + -_characterData.Speed * Time.deltaTime, transform.position.y);
            direction += new Vector3(-1, 0);
        }
        
        if (Input.GetKey(KeyCode.D) && (transform.position.x < x/2 - Size/2))
        {
            // transform.position = new Vector2(transform.position.x + _characterData.Speed * Time.deltaTime, transform.position.y);
            direction += new Vector3(1, 0);
        }
        
        if (Input.GetKey(KeyCode.W) && (transform.position.y < y/2 - Size/2))
        {
            // transform.position = new Vector2(transform.position.x, transform.position.y  + _characterData.Speed * Time.deltaTime);
            direction += new Vector3(0, 1);
        }
        
        if (Input.GetKey(KeyCode.S) && (transform.position.y > -y/2 + Size/2))
        {
            // transform.position = new Vector2(transform.position.x, transform.position.y  + -_characterData.Speed * Time.deltaTime);
            direction += new Vector3(0, -1);
        }

        direction = direction.normalized;
        
        //标准化
        transform.position = new Vector2(transform.position.x + direction.x * Speed * Time.deltaTime,
            transform.position.y + direction.y * Speed * Time.deltaTime);

        //调用事件，对应相机移动
        //改在GameManager中进行
        // EventManager.Instance?.InvokeEvent(GameStateEvent.PlayerMove, transform.position);
    }
    
    //玩家分裂能力
    void PlayerSplit()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            float afterSize = (float)Math.Sqrt(2 * Math.Pow(Size / 2, 2));
            PlayerSplitData playerSplitData = new PlayerSplitData(transform.position,  afterSize * 2, 
                afterSize, direction, _spriteRenderer.sprite);
            Debug.Log(playerSplitData);
            EventManager.Instance?.InvokeEvent(GameStateEvent.PlayerSplit, (object)playerSplitData);
        
            SetSize(afterSize);
        }
    }

    public void SetSize(float size)
    {
        Size = size;
        AfterEat(0);
    }
    
    public void SetDirection(Vector3 dir)
    {
        StartCoroutine(MoveWithDir(dir));
    }

    IEnumerator MoveWithDir(Vector3 dir)
    {
        float num = Mathf.Pow(Size / 2, 2) * Mathf.PI / Size;
        while (num > 0.01f)
        {
            transform.position = new Vector2(transform.position.x + (dir * num).x * Speed * Time.deltaTime,
                transform.position.y + (dir * num).y * Speed * Time.deltaTime);
            num -= Time.deltaTime;
            yield return null;
        }

        IsCanEat = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.tag == "GameCharacter" || other.tag == "Item" || other.tag == "Player"))    //判断是否为可食用物体
        {
            IGetHit item = other.GetComponent<IGetHit>();
            float otherSize = item.GetSize();
            
            if (otherSize < Size && item.GetCanEat())
            {
                AfterEat(item.DestroyObject());
            }

            if (otherSize > Size && other.tag == "Item")
            {
                DestroyObject();
            }
        }
    }

    //设置物体尺寸，当数值为零时表示重置改物体大小
    void AfterEat(float count)
    {
        Size += (float)Math.Sqrt(count / Math.PI + Math.Pow(Size, 2)) - Size;
        Speed = maxSpeed / (float)(Math.PI * Math.Pow(Size, 2));
        
        //变大协程
        if(changeSizeCoroutine != null) StopCoroutine(changeSizeCoroutine);
        if(transform.localScale.x < Size)
            changeSizeCoroutine = StartCoroutine(SetSizeActiveBig());
        else if (transform.localScale.x > Size)
        {
            changeSizeCoroutine = StartCoroutine(SetSizeActiveSamll());
        }
        
        //设置摄像机大小
        
        //移动至GameManager中设置
        // EventManager.Instance?.InvokeEvent(GameStateEvent.SetCameraSize, Size);
    }

    IEnumerator SetSizeActiveBig()
    {
        while (transform.localScale.x < Size)
        {
            // transform.localScale = new Vector2(transform.localScale.x + changeSizeSpeed * Time.deltaTime, 
            //     transform.localScale.y + changeSizeSpeed * Time.deltaTime);
            this.transform.localScale += new Vector3(1,1, 0) * (Size - transform.localScale.x) * Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator SetSizeActiveSamll()
    {
        while (transform.localScale.x > Size)
        {
            // transform.localScale = new Vector2(transform.localScale.x + changeSizeSpeed * Time.deltaTime, 
            //     transform.localScale.y + changeSizeSpeed * Time.deltaTime);
            this.transform.localScale -= new Vector3(1,1, 0) * (transform.localScale.x - Size) * Time.deltaTime;
            yield return null;
        }
    }

    public bool GetCanEat()
    {
        return IsCanEat;
    }

    public void SetCanEat(bool canEat)
    {
        IsCanEat = canEat;
    }

    //IGetHit接口函数
    public float GetSize()
    {
        //TODO
        return Size;
    }

    public float DestroyObject()
    {
        //游戏结束
        GameManager.Instance.PlayerRemove(gameObject);
        //TODO:使用消融效果
        ObjectPool.Instance.PushObject(gameObject);
        return (float)Math.PI * (float)Math.Pow(Size, 2);
    }
}
