using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class EvolutionWindow : MonoBehaviour {

    const int MAX_RARITY = 5;

    [SerializeField] TextMeshProUGUI coinText;
    [SerializeField] TextMeshProUGUI decreaseCoinText;
    [SerializeField] TextMeshProUGUI charaCountText;
    [SerializeField] TextMeshProUGUI decreaseCharaText;
    [SerializeField] Image[] itemImage;
    [SerializeField] TextMeshProUGUI[] itemCount;
    [SerializeField] Image[] rarityImage;
    [SerializeField] Button evolButton;

    [SerializeField] GameObject failedMSG;
    [SerializeField] Button connectButton;

    [SerializeField] CharacterDetailWindow detailWindow;
    [SerializeField] TeamEdit teamEdit;

    [SerializeField] int decreaseCharaValue;
    [SerializeField] int decreaseCoin;

    int characterIndex;
    int coin;
    int nextRarity;
    int charaCout;
    itemEnum[] items;

    int totalDecreaseCoin;
    int[] totalDecreaseItem;
    int totalDecreaseChara;


    [Flags]
    enum ConnectionFlag
    {
        DecreaseCoin = 1,
        DecreaseChara = 2,
        UpdateRarity = 4,
        DecreaseItem1 = 8,
        DecreaseItem2 = 16,
        DecreaseItem3 = 32,
        None = 0
    }

    ConnectionFlag connectionFlag = ConnectionFlag.None;
    ConnectionFlag cachedFailed = ConnectionFlag.None;

    ReactiveProperty<int> connectionCount = new ReactiveProperty<int>(0);

    void Start()
    {
        connectionCount.Skip(1).Subscribe(count =>
        {
            Debug.Log(items.Length);
            if (count >= items.Length + 3)
            {
                if (ContainsAll())
                {
                    connectionFlag = ConnectionFlag.None;
                    InitInformation();
                    connectionCount.Value = 0;
                    Debug.Log("はい、ありがとうございます！");
                }
                else
                {
                    int c = 0;
                    foreach (ConnectionFlag f in Enum.GetValues(typeof(ConnectionFlag)))
                    {
                        if (c >= items.Length + 3) break;
                        if (!Contains(f))
                        {
                            cachedFailed |= f;
                            connectionCount.Value--;
                        }
                        c++;
                    }

                    failedMSG.SetActive(true);
                    LoadingController.Instance.FinishLoading();
                    connectButton.onClick.RemoveAllListeners();
                    connectButton.onClick.AddListener(() =>
                    {
                        failedMSG.SetActive(false);
                        RetryFailedProcess();
                    });
                }
                LoadingController.Instance.FinishLoading();
            }

        }).AddTo(this);
    }


    public void setEvolutionInfo(CharacterBase chara)
    {
        characterIndex = Int32.Parse(chara.ID.Split('c')[1]);
        nextRarity = UserData.Instance.charaStatusList.charaStatus[characterIndex].rarity + 1;
        Debug.Log(nextRarity);

        totalDecreaseCoin = decreaseCoin * ((nextRarity == MAX_RARITY) ? 2 : 1);
        totalDecreaseChara = decreaseCharaValue * ((nextRarity == MAX_RARITY) ? 2 : 1);

        coin = UserData.Instance.userItems.OwnedCoin;
        charaCout = UserData.Instance.userItems.OwnedCharacter[characterIndex].Value - 1;
        items = new itemEnum[chara.EvolutionItems.Length];
        totalDecreaseItem = new int[chara.EvolutionItems.Length];

        coinText.text =coin.ToString();
        decreaseCoinText.text = "- " + (totalDecreaseCoin).ToString();

        charaCountText.text = charaCout.ToString();
        decreaseCharaText.text = "- " + (totalDecreaseChara).ToString();

        for (int i = 0; i < chara.EvolutionItems.Length; i++)
        {
            switch (chara.EvolutionItems[i].item)
            {
                case itemEnum.RedFragment:
                    itemImage[i].color = new Color(255, 0, 0);
                    break;
                case itemEnum.GreenFragment:
                    itemImage[i].color = new Color(0, 255, 0);
                    break;
                case itemEnum.BlueFragment:
                    itemImage[i].color = new Color(0, 0, 255);
                    break;
                default:
                    Debug.LogError("Item is null");
                    break;
            }
            totalDecreaseItem[i] = chara.EvolutionItems[i].value * ((nextRarity == MAX_RARITY) ? 2 : 1);

            items[i] = chara.EvolutionItems[i].item;
            itemCount[i].text = UserData.Instance.userItems.OwnedItem[(int)chara.EvolutionItems[i].item].Value.ToString() + " / " + totalDecreaseItem[i];
        }

        for (int i = 0; i < MAX_RARITY; i++)
        {
            if (i == nextRarity - 1 && i != MAX_RARITY - 1)
            {
                rarityImage[i + 1].gameObject.SetActive(true);
                rarityImage[i + 1].rectTransform.sizeDelta = new Vector2(30, 30);
                break;
            }
            rarityImage[i].gameObject.SetActive(true);
        }

        if (EvolutionCheck() == false)
            evolButton.interactable = false;
        else
            evolButton.interactable = true;

        gameObject.SetActive(true);
    }

    public void EvolutionProcess()
    {
        LoadingController.Instance.StartLoading();

        DecreaseCharacter();
        DecreaseCoin();
        RarityUpdate();
        for (int i = 0; i < items.Length; i++)
        {
            EvolutionDecreaseItem(i);
        }
    }

    public void InitInformation()
    {
        this.gameObject.SetActive(false);
        coin = 0;
        //items = null;
        //nextRarity = 0;
        charaCout = 0;

        foreach (var image in rarityImage)
        {
            image.gameObject.SetActive(false);
            image.rectTransform.sizeDelta = new Vector2(25, 25);
        }

        detailWindow.UpdateInfomation();
        teamEdit.UpdateIconInformation();
    }

    bool EvolutionCheck()
    {
        bool flag = true;

        if (totalDecreaseCoin > coin)
        {
            Debug.LogError("コインが足りません");
            flag = false;
        }
        if (totalDecreaseChara > charaCout)
        {
            Debug.LogError("キャラ枚数が足りません");
            flag = false;
        }
        for (int i = 0; i < items.Length; i++)
        {
            if (totalDecreaseItem[i] > UserData.Instance.userItems.OwnedItem[(int)items[i]].Value)
            {
                Debug.LogError(items[i].ToString() + "が足りません");
                flag = false;
            }
        }

        return flag;
    }

    void EvolutionDecreaseItem(int itemInd)
    {
        ServerInterface.Instance.DecreaseItemCount(totalDecreaseItem[itemInd], UserData.Instance.userItems.OwnedItem[(int)items[itemInd]].ID)
        .Subscribe(resultData =>
        {
            if (resultData)
            {
                switch (itemInd)
                {
                    case 0:
                        connectionFlag |= ConnectionFlag.DecreaseItem1;
                        break;
                    case 1:
                        connectionFlag |= ConnectionFlag.DecreaseItem2;
                        break;
                    case 2:
                        connectionFlag |= ConnectionFlag.DecreaseItem3;
                        break;
                }
            }
            else
            {

            }

            connectionCount.Value++;

        }).AddTo(this);
    }

    void DecreaseCharacter()
    {
        ServerInterface.Instance.DecreaseItemCount(totalDecreaseChara, UserData.Instance.userItems.OwnedCharacter[characterIndex].ID)
        .Subscribe(resultData =>
        {
            if (resultData)
            {
                connectionFlag |= ConnectionFlag.DecreaseChara;
            }
            else
            {

            }

            connectionCount.Value++;

        }).AddTo(this);
    }

    void DecreaseCoin()
    {
        ServerInterface.Instance.ConsumeCoinObs(totalDecreaseCoin).Subscribe(ResultData =>
        {
            if (ResultData)
            {
                connectionFlag |= ConnectionFlag.DecreaseCoin;
            }
            else
            {

            }

            connectionCount.Value++;

        }).AddTo(this);
    }

    void RarityUpdate()
    {
        UserData.Instance.charaStatusList.charaStatus[characterIndex].rarity = nextRarity;
        ServerInterface.Instance.UpdateCharacterStatus().Subscribe(ResultData =>
        {
            if (ResultData)
            {
                connectionFlag |= ConnectionFlag.UpdateRarity;
            }
            else
            {

            }

            connectionCount.Value++;
        }).AddTo(this);
    }

    void RetryFailedProcess()
    {
        LoadingController.Instance.StartLoading();
        if (isCached(ConnectionFlag.DecreaseChara))
            DecreaseCharacter();
        if (isCached(ConnectionFlag.DecreaseCoin))
            DecreaseCoin();
        if (isCached(ConnectionFlag.UpdateRarity))
            RarityUpdate();
        if (isCached(ConnectionFlag.DecreaseItem1))
            EvolutionDecreaseItem(0);
        if (isCached(ConnectionFlag.DecreaseItem2))
            EvolutionDecreaseItem(1);
        if (isCached(ConnectionFlag.DecreaseItem3))
            EvolutionDecreaseItem(2);

        cachedFailed = ConnectionFlag.None;
    }


    bool isCached(ConnectionFlag flag)
    {
        return (cachedFailed & flag) == flag;
    }

    bool Contains(ConnectionFlag flag)
    {
        return (connectionFlag & flag) == flag;
    }

    bool ContainsAll()
    {
        ConnectionFlag mask = ConnectionFlag.None;
        int count = 0;
        foreach (ConnectionFlag f in Enum.GetValues(typeof(ConnectionFlag)))
        {
            if (count >= items.Length + 3) break;
            mask |= f;
            count++;
        }
        return (connectionFlag & mask) == mask;
    }
}
