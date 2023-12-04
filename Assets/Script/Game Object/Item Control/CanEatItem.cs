
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterData))]
public class CanEatItem : MonoBehaviour, IGetHit
{
    [SerializeField]private float num = 0.2f;
    private CharacterData _characterData;
    private SpriteRenderer _spriteRenderer;
    private List<Color> _colors = new List<Color>();

    private void OnEnable()
    {
        SetSprite();
    }

    private void Start()
    {
        _characterData = GetComponent<CharacterData>();

        SetSprite();
    }

    public float GetSize()
    {
        return _characterData.Size;
    }

    void SetSprite()
    {
        if(_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        
        Sprite[] sprites = Resources.LoadAll<Sprite>("Shape");
        int randomIndex = Random.Range(0, sprites.Length);
        _spriteRenderer.sprite = sprites[randomIndex];
        
        _colors.Add(new Color(239f/255,146f/255,173f/255));
        _colors.Add(new Color(131f/255,222f/255,135f/255));
        _colors.Add(new Color(178f/255,247f/255,138f/255));
        _colors.Add(new Color(228f/255,108f/255,107f/255));
        _colors.Add(new Color(248f/255,220f/255,133f/255));
        _colors.Add(new Color(117f/255,248f/255,223f/255));
        _colors.Add(new Color(165f/255,191f/255,230f/255));

        randomIndex = Random.Range(0, _colors.Count);
        _spriteRenderer.color = _colors[randomIndex];
    }

    public float DestroyObject()
    {
        
        ObjectPool.Instance?.PushObject(gameObject);
        //等待几秒后再次生成物体
        EventManager.Instance?.InvokeEvent(ItemEvent.WaitToActive);
        return num;
    }
    
    public void SetSize(float size)
    {
        return;
    }

    public float GetRadius()
    {
        return 0.1f;
    }
    
    public void SetCanEat(bool canEat){}

    public bool GetCanEat()
    {
        return true;
    }
    
    public void SetSprite(Sprite sprite) {}
    public void SetDirection(Vector3 dir){}
}
