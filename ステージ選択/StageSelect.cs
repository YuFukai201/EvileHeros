using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

public class StageSelect : MonoBehaviour
{
    [SerializeField] private Button[] m_areaButtons;
    [SerializeField] private Button[] m_stageButtons;
    [SerializeField] private Stage[] m_stage;

    [SerializeField] private Button m_endlessStageButton;
    [SerializeField] private Stage m_endlessStage;
    [SerializeField] GameObject m_gameModeMenu;
    [SerializeField] GameObject m_areaMenu;
    [SerializeField] GameObject m_stageMenu;
    
    const int STAGE_SEPARATION = 5;

    private void Start()
    {
        m_endlessStageButton.onClick.RemoveAllListeners();
        m_endlessStageButton.onClick.AddListener(() =>
        {
            SelectQuest(m_endlessStage);
        });
    }

    public void StageDisplay(int areaNum)
    {
        int stageNum = areaNum * STAGE_SEPARATION;
        for (int i = stageNum; i < (stageNum + STAGE_SEPARATION); i++)
        {
            if (i <= UserData.Instance.dungeonProgress)
            {
                //Debug.Log(UserData.Instance.dungeonProgress);
                m_stageButtons[i % STAGE_SEPARATION].gameObject.SetActive(true);
                int index = i;
                m_stageButtons[i % STAGE_SEPARATION].onClick.RemoveAllListeners();
                m_stageButtons[i % STAGE_SEPARATION].onClick.AddListener(() =>
                {
                    SelectQuest(m_stage[index]);
                });
            }
            else
            {
                m_stageButtons[i % STAGE_SEPARATION].gameObject.SetActive(false);
            }
        }
    }
    public void areaDisplay()
    {
        m_gameModeMenu.SetActive(false);

        for (int i = 0; i <= UserData.Instance.dungeonProgress / 5; i++)
        {
            m_areaButtons[i].gameObject.SetActive(true);
        }
    }

    public void BackFromArea()
    {
        m_gameModeMenu.SetActive(true);
        m_areaMenu.SetActive(false);
    }

    public void BackFromStageSel()
    {
        m_areaMenu.SetActive(true);
        m_stageMenu.SetActive(false);
    }

    public void OpenArea()
    {
        m_areaMenu.SetActive(true);
        m_gameModeMenu.SetActive(false);
    }

    public void OpenStageSel()
    {
        m_areaMenu.SetActive(false);
        m_stageMenu.SetActive(true);
    }

    void SelectQuest(Stage stage)
    {
#if UNITY_EDITOR
        GameState.Instance.CurrentStage = stage;
        SceneManager.LoadScene("GameScene");
        return;
#endif

        LoadingController.Instance.StartLoading();

        Debug.Log("Stage selected");
        if(stage.StaminaCost <= UserData.Instance.stamina)
        {
            ServerInterface.Instance.ConsumeStaminaObs(stage.StaminaCost).Subscribe(result =>
            {
                if(result)
                {
                    GameState.Instance.CurrentStage = stage;
                    SceneManager.LoadScene("GameScene");
                }
                else
                {
                    //Connection error msg
                    Debug.Log("Connection error");
                    LoadingController.Instance.FinishLoading();
                }
            }).AddTo(this);
        }
        else
        {
            //Not enough stamina
            Debug.Log("Not enough Stamina");
            LoadingController.Instance.FinishLoading();
        }
    }
}
