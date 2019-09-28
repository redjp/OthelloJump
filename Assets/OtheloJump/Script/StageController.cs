using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum BlockStatus
{
    Hole,
    Black,
    White
}
public struct StageBlock
{
    public GameObject gameObject;
    public BlockStatus status;
}
public class StageController : MonoBehaviour
{

    private StageBlock[] stageBlock = new StageBlock[maxStageBlock];
    //ブロックの表示数
    public const int maxStageBlock = 20;
    //裏で待機しておくブロックの数
    public const int storeStageBlock = 3;
    //連続する同じ色の数
    public const int maxSameColor = 2;

    public GameObject blockPrefab;
    public float blockHeight;
    public float blockPopTime;
    public PlayerController playerController;
    private int currentPosition = 0;
    private bool isFakeRoute = false;
    private int prebSameColor = 0;
    public float fallTime;
    public float blinkTime;

    private int addScore = 0;
    private int difficulty = 1;

    [Header("SpeedUp Settings")]
    public UIManager uiManager;
    public int speedUpFreq;
    private int speechCount = 0;
    public TextMeshProUGUI speechText;

    void Awake()
    {
        for (int i = 0; i < maxStageBlock; i++)
        {
            stageBlock[i].gameObject = Instantiate(blockPrefab, new Vector3(0.0f, -blockHeight / 2, i), Quaternion.identity);
            stageBlock[i].gameObject.transform.parent = this.transform;
        }

        ResetStage();
    }
    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        PlayerController.PlayerStepped += PlayerController_PlayerStepped;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        PlayerController.PlayerStepped -= PlayerController_PlayerStepped;
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing && oldState == GameState.Prepare)
        {
            if (ScoreManager.Instance.Score < 100)
            { }
            else if (ScoreManager.Instance.Score < 300)
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(100);
                ResetStage(100);
                playerController.ResetPlayer(100);
                playerController.ChangeTimeSetting(0.6f - 0.01f * 2, 0.9f - 0.015f * 2, 0.3f - 0.02f * 2);
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 11;
            }
            else if (ScoreManager.Instance.Score < 600)
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(300);
                ResetStage(300);
                playerController.ResetPlayer(300);
                playerController.ChangeTimeSetting(0.5f, 0.75f, 0.1f);
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 31;
            }
            else if (ScoreManager.Instance.Score < 1000)
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(600);
                ResetStage(600);
                playerController.ResetPlayer(600);
                playerController.ChangeTimeSetting(0.5f, 0.75f, 0.1f);
                GameManager.Instance.gameSpeed = 1.16f;
                difficulty = 61;
            }
            else
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(1000);
                ResetStage(1000);
                playerController.ResetPlayer(1000);
                playerController.ChangeTimeSetting(0.5f, 0.75f, 0.1f);
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 101;
            }
        }
        if (newState == GameState.Playing && oldState == GameState.GameOver)
        {
            if (ScoreManager.Instance.Score < 100)
            {
                ScoreManager.Instance.Reset();
                ResetStage();
                playerController.ResetPlayer();
                playerController.ResetTimeSetting();
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 1;
            }
            else if (ScoreManager.Instance.Score < 300)
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(100);
                ResetStage(100);
                playerController.ResetPlayer(100);
                playerController.ChangeTimeSetting(0.6f - 0.01f * 2, 0.9f - 0.015f * 2, 0.3f - 0.02f * 2);
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 11;
            }
            else if (ScoreManager.Instance.Score < 600)
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(300);
                ResetStage(300);
                playerController.ResetPlayer(300);
                playerController.ChangeTimeSetting(0.5f, 0.75f, 0.1f);
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 31;
            }
            else if (ScoreManager.Instance.Score < 1000)
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(600);
                ResetStage(600);
                playerController.ResetPlayer(600);
                playerController.ChangeTimeSetting(0.5f, 0.75f, 0.1f);
                GameManager.Instance.gameSpeed = 1.16f;
                difficulty = 61;
            }
            else
            {
                ScoreManager.Instance.Reset();
                ScoreManager.Instance.AddScore(1000);
                ResetStage(1000);
                playerController.ResetPlayer(1000);
                playerController.ChangeTimeSetting(0.5f, 0.75f, 0.1f);
                GameManager.Instance.gameSpeed = 1.0f;
                difficulty = 101;
            }

            blockPopTime = playerController.jumpTime;
        }
    }

    void PlayerController_PlayerStepped()
    {
        //ステップごとの処理
        currentPosition++;
        addScore++;

        //プレイヤーの生存判定
        if (playerController.playerState == PlayerState.Idle)
        {
            switch (stageBlock[currentPosition % maxStageBlock].status)
            {
                case BlockStatus.Hole:
                    Debug.Log(currentPosition % maxStageBlock + " Hole!");
                    playerController.playerState = PlayerState.Die;
                    GameManager.Instance.PreGameOver();

                    RefleshAllBlockColor();
                    //穴に落ちる処理
                    LeanTween.moveY(playerController.gameObject, -blockHeight, fallTime).setOnComplete(() =>
                    {
                        playerController.Hide();
                        playerController.Die();
                    });

                    AudioManager.Instance.PlaySE("Fall");

                    return;
                //break;
                case BlockStatus.White:
                    if (playerController.currentColor == PlayerColor.White)
                    {
                        Debug.Log(currentPosition % maxStageBlock + " White!");
                        playerController.playerState = PlayerState.Die;
                        GameManager.Instance.PreGameOver();

                        RefleshAllBlockColor();
                        //色が違って死亡する処理
                        LeanTween.color(stageBlock[currentPosition % maxStageBlock].gameObject, Color.white, 0.0f);
                        LeanTween.color(stageBlock[currentPosition % maxStageBlock].gameObject, Color.red, blinkTime).setLoopPingPong(3).setOnComplete(() =>
                    {
                        playerController.Die();
                    });

                        AudioManager.Instance.PlaySE("Fail");

                        return;
                    }
                    break;
                case BlockStatus.Black:
                    if (playerController.currentColor == PlayerColor.Black)
                    {
                        Debug.Log(currentPosition % maxStageBlock + " Black!");
                        playerController.playerState = PlayerState.Die;
                        GameManager.Instance.PreGameOver();

                        RefleshAllBlockColor();
                        //色が違って死亡する処理
                        LeanTween.color(stageBlock[currentPosition % maxStageBlock].gameObject, Color.black, 0.0f);
                        LeanTween.color(stageBlock[currentPosition % maxStageBlock].gameObject, Color.red, blinkTime).setLoopPingPong(3).setOnComplete(() =>
                    {
                        playerController.Die();
                    });

                        AudioManager.Instance.PlaySE("Fail");

                        return;
                    }
                    break;
            }

            AudioManager.Instance.PlaySE("Step");

            ScoreManager.Instance.AddScore(addScore);
            addScore = 0;

            //加速判定
            CheckSpeedUp();
        }
        //古い床の消滅アニメーション
        DeleteOldBlock(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject);
        //新しい床の生成
        CreateNewBlock(currentPosition - 1 + maxStageBlock - storeStageBlock);

        if (GameManager.Instance.GameState == GameState.Playing)
        {
            //床の色を隠す
            HideBlockColor();
            //スピーチを流す
            StartCoroutine(CheckAndSetSpeech());
        }
    }

    void CheckSpeedUp()
    {
        if (ScoreManager.Instance.Score >= difficulty * speedUpFreq)
        {
            int i;
            switch (difficulty)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    i = difficulty;
                    playerController.ChangeTimeSetting(0.7f - 0.02f * i, 1.05f - 0.03f * i, 0.5f - 0.04f * i);
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 7:
                case 9:
                case 11:
                case 13:
                case 15:
                    i = (difficulty - 6) / 2;
                    playerController.ChangeTimeSetting(0.6f - 0.01f * i, 0.9f - 0.015f * i, 0.3f - 0.02f * i);
                    blockPopTime = playerController.jumpTime;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 18:
                case 21:
                case 24:
                case 27:
                case 30:
                    i = (difficulty - 15) / 3;
                    playerController.ChangeTimeSetting(0.55f - 0.01f * i, 0.9f - 0.015f * i, 0.2f - 0.02f * i);
                    blockPopTime = playerController.jumpTime;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 33:
                case 36:
                case 39:
                case 42:
                case 45:
                    i = (difficulty - 30) / 3;
                    GameManager.Instance.gameSpeed = 1.0f + 0.03f * i;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 49:
                case 53:
                case 57:
                case 61:
                case 65:
                    i = (difficulty - 45) / 4;
                    GameManager.Instance.gameSpeed = 1.15f + 0.03f * i;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 70:
                case 75:
                case 80:
                case 85:
                case 90:
                    i = (difficulty - 65) / 5;
                    GameManager.Instance.gameSpeed = 1.3f + 0.02f * i;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 92:
                case 94:
                case 96:
                case 98:
                    i = (difficulty - 90) / 2;
                    GameManager.Instance.gameSpeed = 1.4f - 0.08f * i;
                    break;
                case 100:
                    GameManager.Instance.gameSpeed = 1.0f;
                    AudioManager.Instance.PlayBGM("Pooh");
                    break;
                case 103:
                case 108:
                case 113:
                case 118:
                case 123:
                case 128:
                case 133:
                case 138:
                case 143:
                case 148:
                    i = (difficulty - 98) / 5;
                    GameManager.Instance.gameSpeed = 1.0f + 0.03f * i;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 150:
                    AudioManager.Instance.PlaySE("Cheer");
                    break;
                case 153:
                case 158:
                case 163:
                case 168:
                case 173:
                case 178:
                case 183:
                case 188:
                case 193:
                    i = (difficulty - 148) / 5;
                    GameManager.Instance.gameSpeed = 1.3f + 0.01f * i;
                    uiManager.SetSpeedUpAnimation();
                    break;
                case 198:
                    GameManager.Instance.gameSpeed = 1.4f;
                    uiManager.SetSpeedUpAnimation();
                    break;
            }

            difficulty++;
        }
    }

    //ブロックを灰色に変えて色を隠す
    void HideBlockColor()
    {
        if (currentPosition < 1000)
        {
        }
        else if (currentPosition < 1001)
        {
            uiManager.SetDangerAnimation();
            LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1002)
        {
            LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1075)
        {
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1100)
        { }
        else if (currentPosition < 1101)
        {
            uiManager.SetDangerAnimation();
            LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1102)
        {
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1175)
        {
            LeanTween.color(stageBlock[(currentPosition + 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1200)
        { }
        else if (currentPosition < 1201)
        {
            uiManager.SetDangerAnimation();
            LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1202)
        {
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1275)
        {
            LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1300)
        { }
        else if (currentPosition < 1301)
        {
            uiManager.SetDangerAnimation();
            LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1302)
        {
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1303)
        {
            LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 3) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1375)
        {
            LeanTween.color(stageBlock[(currentPosition + 3) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1400)
        { }
        else if (currentPosition < 1401)
        {
            uiManager.SetDangerAnimation();
            LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1402)
        {
            LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1403)
        {
            LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 3) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            LeanTween.color(stageBlock[(currentPosition + 4) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1475)
        {
            LeanTween.color(stageBlock[(currentPosition + 4) % maxStageBlock].gameObject, Color.gray, blockPopTime);
        }
        else if (currentPosition < 1500)
        {
        }
        else
        {
            if (currentPosition % 100 == 0)
            {
                uiManager.SetDangerAnimation();
                LeanTween.color(stageBlock[(currentPosition - 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
                LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            }
            else if (currentPosition % 100 == 1)
            {
                LeanTween.color(stageBlock[(currentPosition) % maxStageBlock].gameObject, Color.gray, blockPopTime);
                LeanTween.color(stageBlock[(currentPosition + 1) % maxStageBlock].gameObject, Color.gray, blockPopTime);
                LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            }
            else if (currentPosition % 100 == 2)
            {
                LeanTween.color(stageBlock[(currentPosition + 2) % maxStageBlock].gameObject, Color.gray, blockPopTime);
                LeanTween.color(stageBlock[(currentPosition + 3) % maxStageBlock].gameObject, Color.gray, blockPopTime);
                LeanTween.color(stageBlock[(currentPosition + 4) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            }
            else if (currentPosition % 100 < 75)
            {
                LeanTween.color(stageBlock[(currentPosition + 4) % maxStageBlock].gameObject, Color.gray, blockPopTime);
            }
        }
    }

    //スピーチを流す
    IEnumerator CheckAndSetSpeech()
    {
        if (speechCount == 0 && currentPosition == 5)
        {
            yield return new WaitForSeconds(0.2f);
            AudioManager.Instance.PlaySE("Speech1");
            speechText.text = "こんにちは皆さん";
            yield return new WaitForSeconds(2.0f);
            speechText.text = "この世界には白と黒、つまり対立する2つの凶暴な勢力が存在します";
            yield return new WaitForSeconds(5.9f);
            speechText.text = "あなたは2面性を使い、それらを巧みに乗り越えてください";
            yield return new WaitForSeconds(5.0f);
            speechText.text = "";
            speechCount++;
        }
        else if (currentPosition == 250)
        {
            yield return new WaitForSeconds(0.2f);
            AudioManager.Instance.PlaySE("Speech2");
            speechText.text = "これまでの流れを見るに、";
            yield return new WaitForSeconds(3.0f);
            speechText.text = "このあたりで集中力が切れてくるころかもしれません";
            yield return new WaitForSeconds(4.9f);
            speechText.text = "一度、休憩を挟んでから再開するのもいいでしょう";
            yield return new WaitForSeconds(5.2f);
            speechText.text = "残念ながらこのゲームにはポーズ機能はありませんが・・・";
            yield return new WaitForSeconds(7.0f);
            speechText.text = "";
        }
        else if (currentPosition == 950)
        {
            yield return new WaitForSeconds(0.2f);
            AudioManager.Instance.PlaySE("Speech3");
            speechText.text = "こんなところまで来るなんて・・・";
            yield return new WaitForSeconds(5.3f);
            speechText.text = "どうやら追手が来たようです";
            yield return new WaitForSeconds(4.3f);
            speechText.text = "この先、辛く苦しい戦いになりますが、どうか自分を見失わないで";
            yield return new WaitForSeconds(7.0f);
            speechText.text = "";
        }
        else if (currentPosition == 1950)
        {
            yield return new WaitForSeconds(0.2f);
            AudioManager.Instance.PlaySE("Speech1");
            speechText.text = "皆さん聞いてください";
            yield return new WaitForSeconds(2.0f);
            speechText.text = "次に加速したら、それ以降もう難易度の変化はありません";
            yield return new WaitForSeconds(5.9f);
            speechText.text = "ここまでプレイしてくれて本当にありがとうございます";
            yield return new WaitForSeconds(5.0f);
            speechText.text = "";
        }
    }

    //ブロックの色を変える
    void ApplyBlockStatus(StageBlock stageBlock)
    {
        switch (stageBlock.status)
        {
            case BlockStatus.Hole:
                if (stageBlock.gameObject.activeSelf) stageBlock.gameObject.SetActive(false);
                break;
            case BlockStatus.White:
                if (!stageBlock.gameObject.activeSelf) stageBlock.gameObject.SetActive(true);
                LeanTween.color(stageBlock.gameObject, Color.white, 0.0f);
                break;
            case BlockStatus.Black:
                if (!stageBlock.gameObject.activeSelf) stageBlock.gameObject.SetActive(true);
                LeanTween.color(stageBlock.gameObject, Color.black, 0.0f);
                break;
        }
    }

    void RefleshAllBlockColor()
    {
        for (int i = 0; i < maxStageBlock; i++)
        {
            switch (stageBlock[i].status)
            {
                case BlockStatus.White:
                    LeanTween.color(stageBlock[i].gameObject, Color.white, 0.5f);
                    break;
                case BlockStatus.Black:
                    LeanTween.color(stageBlock[i].gameObject, Color.black, 0.5f);
                    break;
            }
        }
    }

    void DeleteOldBlock(GameObject oldBlock)
    {
        LeanTween.scaleY(oldBlock, 0.0f, blockPopTime);
        LeanTween.moveY(oldBlock, -blockHeight, blockPopTime);
        LeanTween.alpha(oldBlock, 0.0f, blockPopTime).setEase(LeanTweenType.easeInCubic).setOnComplete(() =>
        {
            oldBlock.SetActive(false);
        });
    }

    void CreateNewBlock(int posZ)
    {
        if (!isFakeRoute)
        {
            if (prebSameColor >= maxSameColor || Random.value < GameManager.Instance.fakeRouteRatio)
            {
                if (Random.value < GameManager.Instance.holeRatio)
                {
                    stageBlock[posZ % maxStageBlock].status = BlockStatus.Hole;
                }
                else
                {
                    stageBlock[posZ % maxStageBlock].status = (Random.value < 0.50f) ? BlockStatus.White : BlockStatus.Black;
                }

                isFakeRoute = true;
                prebSameColor = 0;
            }
            else
            {
                stageBlock[posZ % maxStageBlock].status = stageBlock[(posZ - 1) % maxStageBlock].status;
                prebSameColor++;
            }
        }
        else
        {
            stageBlock[posZ % maxStageBlock].status = (stageBlock[(posZ - 2) % maxStageBlock].status == BlockStatus.White) ? BlockStatus.Black : BlockStatus.White;

            isFakeRoute = false;
        }

        ApplyBlockStatus(stageBlock[posZ % maxStageBlock]);

        LeanTween.scaleY(stageBlock[posZ % maxStageBlock].gameObject, blockHeight, blockPopTime);
        LeanTween.moveY(stageBlock[posZ % maxStageBlock].gameObject, -blockHeight / 2, blockPopTime);
        LeanTween.moveZ(stageBlock[posZ % maxStageBlock].gameObject, posZ, 0.0f);
        LeanTween.alpha(stageBlock[posZ % maxStageBlock].gameObject, 1.0f, blockPopTime).setEase(LeanTweenType.easeOutCubic);
    }

    public void ResetStage(int stagePos = 0)
    {
        currentPosition = stagePos;
        addScore = 0;
        isFakeRoute = false;
        prebSameColor = 0;

        int i;
        //全ブロックを stagePos から並べる
        for (i = 0; i < maxStageBlock; i++)
        {
            LeanTween.cancel(stageBlock[(i + stagePos) % maxStageBlock].gameObject);
            stageBlock[(i + stagePos) % maxStageBlock].gameObject.transform.SetPositionAndRotation(new Vector3(0.0f, -blockHeight / 2, i + stagePos), Quaternion.identity);
            stageBlock[(i + stagePos) % maxStageBlock].gameObject.transform.localScale = new Vector3(1.0f, blockHeight, 1.0f);
            stageBlock[(i + stagePos) % maxStageBlock].gameObject.SetActive(false);
        }
        //ステージの先頭を固定
        for (i = 0; i < 5; i++)
        {
            stageBlock[(i + stagePos) % maxStageBlock].status = BlockStatus.Black;
            ApplyBlockStatus(stageBlock[(i + stagePos) % maxStageBlock]);
            LeanTween.alpha(stageBlock[(i + stagePos) % maxStageBlock].gameObject, 1.0f, 0.0f);
        }
        //残りのステージを生成
        for (; i < maxStageBlock - storeStageBlock; i++)
        {
            CreateNewBlock(i + stagePos);
        }
        //待機ステージを隠す
        for (; i < maxStageBlock; i++)
        {
            LeanTween.scaleY(stageBlock[(i + stagePos) % maxStageBlock].gameObject, 0.0f, 0.0f);
            LeanTween.moveY(stageBlock[(i + stagePos) % maxStageBlock].gameObject, -blockHeight, 0.0f);
            LeanTween.alpha(stageBlock[(i + stagePos) % maxStageBlock].gameObject, 0.0f, 0.0f);
        }
    }
}
