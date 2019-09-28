using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlayerState
{
    Prepare,
    Idle,
    Step,
    Jump,
    Die
}

public enum PlayerColor
{
    Black,
    White
}

public class PlayerController : MonoBehaviour
{
    public static event System.Action PlayerDied = delegate { };
    public static event System.Action PlayerStepped = delegate { };

    public PlayerState playerState;
    public PlayerColor currentColor;
    private bool isJumpNext = false;
    private bool isHalfAnim = false;
    [Header("Timing")]
    public float inputDeleyTime;
    public float stepTime;
    public float jumpTime;
    public float idleTime;
    private float initStepTime;
    private float initJumpTime;
    private float initIdleTime;
    private float timer = 0.0f;
    [Header("PlayerStatus")]
    public float playerHeight;
    public float stepHeight;
    public float jumpHeight;

    void Awake()
    {
        initStepTime = stepTime;
        initJumpTime = jumpTime;
        initIdleTime = idleTime;
    }

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        Debug.Log("newState :" + newState + ", oldState :" + oldState);
        if (newState == GameState.Playing && oldState == GameState.Prepare)
        {
            playerState = PlayerState.Idle;
        }
        else if (newState == GameState.Playing && oldState == GameState.GameOver)
        {
            playerState = PlayerState.Idle;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.GameState == GameState.Playing && Input.GetMouseButtonDown(0))
        {
            isJumpNext = true;
            LeanTween.scale(gameObject, new Vector3(1.2f, 1.4f, 1.2f), 0.05f).setOnComplete(() =>
            {
                LeanTween.scale(gameObject, new Vector3(1.0f, 1.0f, 1.0f), 0.05f);
            });

            AudioManager.Instance.PlaySE("Click");
        }


        if (playerState != PlayerState.Prepare && playerState != PlayerState.Die)
        {
            Leap();
        }
    }

    public void Leap()
    {
        timer += Time.deltaTime;
        if (playerState == PlayerState.Idle)
        {
            if (timer >= idleTime)
            {
                if (isJumpNext)
                {
                    //大ジャンプ
                    LeanTween.moveY(gameObject, playerHeight + jumpHeight, jumpTime / 2).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
                           {
                               LeanTween.moveY(this.gameObject, playerHeight, jumpTime / 2).setEase(LeanTweenType.easeInCubic);
                           });
                    LeanTween.moveZ(gameObject, transform.position.z + 2.0f, jumpTime);

                    float rot = (currentColor == PlayerColor.White) ? 180.0f : 0.0f;
                    LeanTween.rotateZ(gameObject, rot, jumpTime);
                    //色を反転
                    currentColor = (currentColor == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

                    AudioManager.Instance.PlaySE("Jump");

                    playerState = PlayerState.Jump;
                    isJumpNext = false;
                    isHalfAnim = false;
                    timer = 0.0f;
                }
                else
                {
                    //小ジャンプ
                    LeanTween.moveY(this.gameObject, playerHeight + stepHeight, stepTime / 2).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
                        {
                            LeanTween.moveY(this.gameObject, playerHeight, stepTime / 2).setEase(LeanTweenType.easeInCubic);
                        });
                    LeanTween.moveZ(this.gameObject, transform.position.z + 1.0f, stepTime);

                    playerState = PlayerState.Step;
                    isJumpNext = false;
                    isHalfAnim = false;
                    timer = 0.0f;
                }
            }
        }

        if (playerState == PlayerState.Step)
        {
            if (timer >= stepTime)
            {
                playerState = PlayerState.Idle;
                PlayerStepped();
                timer = 0.0f;
            }
        }
        else if (playerState == PlayerState.Jump)
        {
            if (!isHalfAnim && timer >= jumpTime / 2)
            {
                PlayerStepped();
                isHalfAnim = true;
            }
            if (timer >= jumpTime)
            {
                playerState = PlayerState.Idle;
                PlayerStepped();
                timer = 0.0f;
            }
        }
    }

    public void Hide()
    {
        LeanTween.alpha(gameObject, 0.0f, 0.1f);
    }
    public void Die()
    {
        if (GameManager.Instance.GameState != GameState.GameOver)
        {
            PlayerDied();
        }
    }

    public void ChangeTimeSetting(float step, float jump, float idle)
    {
        stepTime = step;
        jumpTime = jump;
        idleTime = idle;
    }

    public void ResetTimeSetting()
    {
        stepTime = initStepTime;
        jumpTime = initJumpTime;
        idleTime = initIdleTime;
    }

    public void ResetPlayer(int playerPos = 0)
    {
        transform.SetPositionAndRotation(new Vector3(0.0f, 0.1f, playerPos), Quaternion.identity);
        LeanTween.alpha(gameObject, 1.0f, 0.0f);
        currentColor = PlayerColor.White;
        isJumpNext = false;
        isHalfAnim = false;
        timer = 0.0f;
    }
}