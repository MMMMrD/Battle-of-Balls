using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    //速度
    [SerializeField] private float _speed = 100;
    
    //大小
    [SerializeField] private float _size = 2;

    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    public float Size
    {
        get { return _size; }
        set { _size = value; }
    }
}
