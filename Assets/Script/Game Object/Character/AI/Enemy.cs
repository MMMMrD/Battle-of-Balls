using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterData))]
public class Enemy : MonoBehaviour,IGetHit
{
    [Header("Enemy 参数")] 
    private float maxSpeed;
    public bool isFindPlayer;
    public bool canEat;
    [HideInInspector]public Transform targetPoint;
    private Vector3 moveDir;
    private CharacterData _characterData;
    private SpriteRenderer _spriteRenderer;
    private SpriteMask _spriteMask;
    // [SerializeField] private int frontSortingLayerID;
    
    [Header("检测范围")] 
    [SerializeField]private float checkDir = 2.5f;  //检测距离
    [SerializeField] private float dirAngle = -0.5f;   //检测角度
    [SerializeField]private LayerMask checkLayer;
    // private CircleCollider2D childCollider;
    
    private BaseState currentState;
    public PatrolState patrolState = new PatrolState();    //巡逻状态
    public FollowState followState = new FollowState();    //跟随状态
    public RunawayState runawayState = new RunawayState();  //逃跑状态

    private Coroutine changeSizeCoroutine = null;
    private Coroutine changeDirCoroutine = null;
    private Coroutine randomRunawayDirCoroutine = null;

    private void Start()
    {
        _characterData = GetComponent<CharacterData>();
        _spriteMask = GetComponent<SpriteMask>();
        _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        
        SetSprite();
        
        maxSpeed = Speed;
        moveDir = RandomPoint();
        
        ChangeState(patrolState);
        
        AfterEat(0);
        
        EventManager.Instance?.InvokeEvent(GameStateEvent.RegisterEnemy, this);
        transform.position = ItemControl.Instance.RandomPointPadding(Size);
    }

    private void Update()
    {
        FindTarget();
        currentState.OnUpdate(this);
    }

    public float Speed
    {
        get { return _characterData.Speed; }
        set { _characterData.Speed = value > 0.75f ? value : 0.75f; }
    }

    public float Size
    {
        get { return _characterData.Size; }
        set { _characterData.Size = value; }
    }

    void SetSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Image");
        int randomIndex = Random.Range(0, sprites.Length);
        _spriteRenderer.sprite = sprites[randomIndex];
        // _spriteRenderer.allowOcclusionWhenDynamic = false;
        // _spriteMask.frontSortingLayerID = this.frontSortingLayerID;
    }

    public void SetSprite(Sprite sprite)
    {
        return;
    }

    #region 移动相关
    //移动
    public void Movement()
    {
        if (ItemControl._boxCol != null)
        {
            float x = ItemControl._boxCol.size.x;
            float y = ItemControl._boxCol.size.y;
            if (transform.position.x < -x / 2 + Size / 2 || transform.position.x > x / 2 - Size / 2)
            {
                moveDir = new Vector2(-moveDir.x, moveDir.y);
            }
            else if(transform.position.y < -y / 2 + Size / 2 || transform.position.y > y / 2 - Size / 2)
            {
                moveDir = new Vector2(moveDir.x, -moveDir.y);
            }
        }
            
        transform.position += moveDir.normalized * Speed * Time.deltaTime;
    }
    
    //跟随移动
    public void FollowPlayer()
    {
        if(targetPoint != null) moveDir = (targetPoint.position - transform.position).normalized;
        Movement();
    }

    //逃离函数
    public void RunAway()
    {
        //在状态切换至Runaway时已经调用协程改变逃跑方向，因此目前只需要移动即可
        Movement();
    }
    
    //随机选取方向
    Vector2 RandomPoint()
    {
        float x = Random.Range(-1.0f, 1.0f);
        float y = Random.Range(-1.0f, 1.0f);

        //保证每个方向上的概率相等
        while (Math.Pow(x, 2) + Math.Pow(y, 2) > 1)
        {
            x = Random.Range(-1.0f, 1.0f);
            y = Random.Range(-1.0f, 1.0f);
        }
        
        return new Vector2(x, y).normalized;
    }
    
    #endregion
    
    #region 协程调用函数
    public void StartRandomDir()
    {
        if(gameObject.activeSelf) changeDirCoroutine = StartCoroutine(ChangeDirCoroutine());
        else StopCoroutine(changeDirCoroutine);
    }

    public void StopRandomDir()
    {
        if(changeDirCoroutine != null) StopCoroutine(changeDirCoroutine);
    }
    
    public void StartRandomRunawayDir()
    {
        if(gameObject.activeSelf) randomRunawayDirCoroutine = StartCoroutine(RunawayCoroutine());
        else if(randomRunawayDirCoroutine != null) StopCoroutine(randomRunawayDirCoroutine);
    }
    
    public void StopRandomRunawayDir()
    {
        if(randomRunawayDirCoroutine != null) StopCoroutine(randomRunawayDirCoroutine);
    }

    IEnumerator ChangeDirCoroutine()
    {
        while (true)
        {
            moveDir = RandomPoint();
            yield return new WaitForSeconds(2);
        }
    }
    
    IEnumerator RunawayCoroutine()
    {
        while (true)
        {
            Vector3 dir = (transform.position - targetPoint.position).normalized;
            //在一定范围内选取一个方向
            while (Vector3.Dot(dir, moveDir) < dirAngle)
            {
                moveDir = RandomPoint();
            }
            yield return new WaitForSeconds(1);
        }
    }
    #endregion //协程调用函数

    //寻找目标
    void FindTarget()
    {
        List<Collider2D> targets = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2 + checkDir,
            checkLayer).ToList();

        targets.Remove(gameObject.GetComponent<Collider2D>());
        
        if (targets.Count > 0)
        {
            targetPoint = targets[0].transform;
            float targetSize = targetPoint.GetComponent<IGetHit>().GetSize();
            if (gameObject.activeSelf)
            {
                StartCoroutine(ChangeToNextStateCoroutine());
            }
            if (targetSize < Random.Range(Size - 0.3f, Size + 0.3f) && targetSize != Size) canEat = true;
        }
        else
        {   
            isFindPlayer = false;
            canEat = false;
            targetPoint = null;
        }
    }
    
    //延迟转换至下一状态
    IEnumerator ChangeToNextStateCoroutine()
    {
        yield return new WaitForSeconds(0.4f);
        float ran = Random.Range(0f, 1f);
        
        if(ran > 0.7f) isFindPlayer = true;
        else canEat = false;
    }
    
    //状态机函数，改变当前状态
    public void ChangeState(BaseState state)
    {
        if(currentState != null) currentState.OnQuit(this);
        
        currentState = state;
        currentState.OnEnter(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //判断两点距离
        IGetHit item = other.GetComponent<IGetHit>();
        
        if (other.tag == "Item")
        {
            if (item.GetSize() < Size)
            {
                float count = item.DestroyObject();
                AfterEat(count);
            }
        }

        if (other.tag == "GameCharacter" || other.tag == "Player")
        {
            if (item.GetSize() < Size)
            {
                float count = item.DestroyObject();
                AfterEat(count);
            }
            else
            {
                ChangeState(runawayState);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" || other.tag == "GameCharacter")
        {
            canEat = false;
            isFindPlayer = false;
        }
    }
    
    void AfterEat(float count)
    {
        //计算质量
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

    public float GetSize()
    {
        return Size;
    }

    public float DestroyObject()
    {
        if(changeDirCoroutine != null)StopCoroutine(changeDirCoroutine);
        if (changeSizeCoroutine != null) StopCoroutine(changeSizeCoroutine);
        if(randomRunawayDirCoroutine  != null) StopCoroutine(randomRunawayDirCoroutine);
        
        EventManager.Instance?.InvokeEvent(GameStateEvent.RemoveEnemy, this);
        
        gameObject.SetActive(false);
        return (float)Math.PI * (float)Math.Pow(Size, 2);
    }

    public float GetRadius()
    {
        return transform.localScale.x / 2;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, transform.localScale.x / 2 + checkDir);
    }
    
    public void SetCanEat(bool canEat){}
    
    public bool GetCanEat()
    {
        return true;
    }
    
    public void SetSize(float size){}
    
    public void SetDirection(Vector3 dir){}
}