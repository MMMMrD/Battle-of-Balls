using System.Collections;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    [SerializeField]private float changeSizeSpeed = 1.0f;
    
    //设定摄像机缩放的权重
    [SerializeField]
    [Range(0.2f, 1.2f)]private float weight = 0.4f;

    private Coroutine changeBigSizeCoroutine = null;
    private Coroutine changeSmallSizeCoroutine = null;
    
    private void Start()
    {
        _camera = GetComponent<Camera>();
        EventManager.Instance.AddListener(GameStateEvent.PlayerMove, FollowObject);
        EventManager.Instance.AddListener(GameStateEvent.SetCameraSize, SetCameraSize);
    }

    void FollowObject(object data)
    {
        UnityEngine.Vector3 pos = (UnityEngine.Vector3)data;
        transform.position = new UnityEngine.Vector3(pos.x, pos.y, transform.position.z);
    }

    void SetCameraSize(object data)
    {
        float size = (float)data;
        if(changeBigSizeCoroutine != null) StopCoroutine(changeBigSizeCoroutine);
        if(changeSmallSizeCoroutine != null) StopCoroutine(changeSmallSizeCoroutine);
        
        if(_camera.orthographicSize < (4f - 2f * weight + size * weight))
            changeBigSizeCoroutine = StartCoroutine(SetCameraBigSizeActive(size));
        else if(_camera.orthographicSize > (4f - 2f * weight + size * weight))
            changeSmallSizeCoroutine = StartCoroutine(SetCameraSmallSizeActive(size));
        
    }

    IEnumerator SetCameraBigSizeActive(float size)
    {
        //设置相机持续变大
        while (_camera.orthographicSize < (4f - 2f * weight + size * weight))
        {
            _camera.orthographicSize += changeSizeSpeed * Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator SetCameraSmallSizeActive(float size)
    {
        //设置相机持续变大
        while (_camera.orthographicSize > (4f - 2f * weight + size * weight))
        {
            _camera.orthographicSize -= changeSizeSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
