using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;

    [Range(-5.0f, 0.0f)]
    public float cameraOffset;
    private Vector3 InitPos;

    void Awake()
    {
        InitPos = transform.position - target.transform.position;

        Camera cam = gameObject.GetComponent<Camera>();

        // 理想の画面の比率
        float targetRatio = 9f / 16f;
        // 現在の画面の比率
        float currentRatio = Screen.width * 1f / Screen.height;
        // 理想と現在の比率
        float ratio = targetRatio / currentRatio;

        //カメラの描画開始位置をX座標にどのくらいずらすか
        float rectX = (1.0f - ratio) / 2f;
        //カメラの描画開始位置と表示領域の設定
        cam.rect = new Rect(rectX, 0f, ratio, 1f);
    }

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        if (pos.z - target.transform.position.z <= cameraOffset)
            pos.z = target.transform.position.z + cameraOffset;
        transform.position = Vector3.Lerp(transform.position, pos, 5.0f * Time.deltaTime);
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            transform.position = target.transform.position + InitPos;
        }
    }
}
