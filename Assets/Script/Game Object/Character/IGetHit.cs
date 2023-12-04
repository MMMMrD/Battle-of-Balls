using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGetHit
{
    public void SetSize(float size);
    public void SetSprite(Sprite sprite);
    public void SetDirection(Vector3 dir);
    public float GetSize();
    public float DestroyObject();
    public bool GetCanEat();
    public void SetCanEat(bool canEat);
}
