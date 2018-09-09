using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UniRx;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;
using DG.Tweening;

public class ResultManager : MonoBehaviour
{
    [SerializeField]
    GameObject clearAnimation;
    [SerializeField]
    GameObject failedMSG;
    [SerializeField]
    GameObject nextSceneButton;
    [SerializeField]
    Button connectButton;
    [SerializeField]
    Transform crystalWings;

    private void Awake()
    {
        crystalWings.DORotate(new Vector3(0, 180, 0), 1.0f).SetLoops(-1, LoopType.Incremental).SetEase(Ease.InOutQuad);
        nextSceneButton.SetActive(false);
        if (GameState.Instance.CurrentStage.isEndless)
        {
            //Debug.Log(GameState.Instance.nowFloor+ " 層まで到達しました。");
            nextSceneButton.SetActive(true);
        }
        else
        {
            if (UserData.Instance.dungeonProgress == GameState.Instance.CurrentStage.StageNumber)
            {
                UserData.Instance.dungeonProgress++;
                SaveProgress();
            }
            else
            {
                GetStageReward();
            }
        }
    }

    void GetStageReward()
    {
        failedMSG.SetActive(false);
        ServerInterface.Instance.GetStageReward().Subscribe(rewardData =>
        {
            if (rewardData)
            {
                Invoke("EnableContinueButton", 2.0f);
                clearAnimation.SetActive(true);
               //Debug.Log("Reward OK");
            }
            else
            {
                //Error
                failedMSG.SetActive(true);
                connectButton.onClick.RemoveAllListeners();
                connectButton.onClick.AddListener(() =>
                {
                    GetStageReward();
                });
            }
        }).AddTo(this);
    }

    void EnableContinueButton()
    {
        nextSceneButton.SetActive(true);
    }

    void SaveProgress()
    {
        failedMSG.SetActive(false);
        ServerInterface.Instance.SaveDungeonProgress().Subscribe(complete =>
        {
            if (complete)
            {
                GetFirstClearReward();
            }
            else
            {
                //Error
                failedMSG.SetActive(true);
                connectButton.onClick.RemoveAllListeners();
                connectButton.onClick.AddListener(() =>
                {
                    SaveProgress();
                });
            }

        });
    }


    void GetFirstClearReward()
    {
        failedMSG.SetActive(false);
        ServerInterface.Instance.GetFirstClearReward().Subscribe(rewardData =>
        {
            if (rewardData)
            {
                GetStageReward();
            }
            else
            {
                //Error
                failedMSG.SetActive(true);
                connectButton.onClick.RemoveAllListeners();
                connectButton.onClick.AddListener(() =>
                {
                    GetFirstClearReward();
                });
            }
        });
    }


    public void PushButton()
    {
        //Move Scene
        SceneManager.LoadScene("Menu Screen");

    }
}
