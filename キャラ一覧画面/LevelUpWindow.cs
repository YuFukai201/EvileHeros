using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class LevelUpWindow : MonoBehaviour {

    [SerializeField] TextMeshProUGUI[] beforeStatus;
    [SerializeField] TextMeshProUGUI[] pulsStatusText;
    [SerializeField] PowerUp calcStatus;
    [SerializeField] TextMeshProUGUI beforeLevel;
    [SerializeField] TextMeshProUGUI afterLevel;
    [SerializeField] TextMeshProUGUI beforeCount;
    [SerializeField] TextMeshProUGUI minusCount;
    [SerializeField] CharacterDetailWindow detailWindow;

    [SerializeField] GameObject failedMSG;
    [SerializeField] Button connectButton;
    [SerializeField] TeamEdit teamEdit;

    private CharacterBase baseCharacter;
    private int charaLevel = 0;
    private int totalCount = 0;

    public void setStatusInfo(CharacterBase chara)
    {
        connectButton.onClick.RemoveAllListeners();

        baseCharacter = chara;
        charaLevel = UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level;
        totalCount = 0;

        var calcResult = calcStatus.CalcCharacterStatus(chara);

        beforeStatus[0].text = calcResult.hp.ToString();
        beforeStatus[1].text = calcResult.atk.ToString();
        beforeStatus[2].text = calcResult.def.ToString();

        pulsStatusText[0].text = "+0";
        pulsStatusText[1].text = "+0";
        pulsStatusText[2].text = "+0";

        beforeLevel.text = charaLevel.ToString();
        afterLevel.text = "+0";
        beforeCount.text = (UserData.Instance.userItems.OwnedCharacter[Int32.Parse(baseCharacter.ID.Split('c')[1])].Value - 1).ToString();
        minusCount.text = "-0";


        if (charaLevel >= UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].rarity * 10)
        {
            Debug.Log("これ以上強化できません");
        }

        gameObject.SetActive(true);
    }

    public void setPulsStatus(bool plus)
    {
        if (plus)
        {
            if (UserData.Instance.userItems.OwnedCharacter[Int32.Parse(baseCharacter.ID.Split('c')[1])].Value - 1 >= RequiredNumber(charaLevel) + totalCount)
            {
                if (charaLevel < UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].rarity * 10)
                {
                    charaLevel++;
                    totalCount += RequiredNumber(charaLevel);
                }
                else
                {
                    Debug.Log("これ以上強化できません");  
                }
            }
            else
            {
                Debug.Log("枚数が足りません");
            }
        }
        else
        {
            if (charaLevel != UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level)
            {
                totalCount -= RequiredNumber(charaLevel);
                charaLevel--;  
            }
            else
            {
                Debug.Log("これ以上減少できません");
            }
        }
        minusCount.text = "-" + totalCount;
        afterLevel.text = "+" + (charaLevel - UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level);
        pulsStatusText[0].text = "+" + (calcStatus.CalcStatusUp(baseCharacter.HpGrowValue, charaLevel) - calcStatus.CalcStatusUp(baseCharacter.HpGrowValue, UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level)).ToString();
        pulsStatusText[1].text = "+" + (calcStatus.CalcStatusUp(baseCharacter.AtkGrowValue, charaLevel) - calcStatus.CalcStatusUp(baseCharacter.AtkGrowValue, UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level)).ToString();
        pulsStatusText[2].text = "+" + (calcStatus.CalcStatusUp(baseCharacter.DefGrowValue, charaLevel) - calcStatus.CalcStatusUp(baseCharacter.DefGrowValue, UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level)).ToString();
        Debug.Log("level" + charaLevel + " :count " + totalCount);
    }

    void LevelUpProcess()
    {

        ServerInterface.Instance.UpdateCharacterStatus().Subscribe(updateDeta =>
        {
            if (updateDeta)
            {
                setStatusInfo(baseCharacter);
                detailWindow.UpdateInfomation();
                teamEdit.UpdateIconInformation();
                //Effect
                LoadingController.Instance.FinishLoading();
            }
            else
            {
                failedMSG.SetActive(true);
                LoadingController.Instance.FinishLoading();
                connectButton.onClick.RemoveAllListeners();
                connectButton.onClick.AddListener(() =>
                {
                    failedMSG.SetActive(false);
                    LevelUpProcess();
                });
            }
        });    
    }

    public void DecreaseCharacter()
    {
        if (totalCount <= 0) return;

        ServerInterface.Instance.DecreaseItemCount(
            totalCount, UserData.Instance.userItems.OwnedCharacter[Int32.Parse(baseCharacter.ID.Split('c')[1])].ID).Subscribe(resultData =>
            {
                LoadingController.Instance.StartLoading();

                if (resultData)
                {
                    UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level = charaLevel;
                    UserData.Instance.userItems.OwnedCharacter[Int32.Parse(baseCharacter.ID.Split('c')[1])].Value -= totalCount;
                    LevelUpProcess();
                }
                else
                {
                    failedMSG.SetActive(true);
                    LoadingController.Instance.FinishLoading();
                    connectButton.onClick.RemoveAllListeners();
                    connectButton.onClick.AddListener(() =>
                    {
                        failedMSG.SetActive(false);
                        DecreaseCharacter();
                    });
                }
            });
    }

    private int RequiredNumber(int level)
    {
        if (level > 40)
        {
            return 3;
        }
        else if (level > 30)
        {
            return 2;
        } 
        return 1;
    }
}
