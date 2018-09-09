using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDetailWindow : MonoBehaviour {

    [SerializeField]
    GameObject detailWindow;

    [SerializeField]
    TextMeshProUGUI charaName;
    [SerializeField]
    Image jobIcon;
    [SerializeField]
    TextMeshProUGUI hpText;
    [SerializeField]
    TextMeshProUGUI atkText;
    [SerializeField]
    TextMeshProUGUI defText;
    [SerializeField]
    TextMeshProUGUI skillName;
    [SerializeField]
    TextMeshProUGUI skillDetail;
    [SerializeField]
    TextMeshProUGUI levelText;

    [SerializeField]
    Image charaImage;
    [SerializeField]
    Image[] rarityImage;
    [SerializeField]
    Sprite[] jobSprites; 


    [SerializeField]
    GameObject powerUpWindow;
    [SerializeField]
    TextMeshProUGUI charaCount;
    [SerializeField]
    Button evolution;
    [SerializeField]
    Button powerUpButton;
    [SerializeField]
    LevelUpWindow levelUpWindow;
    [SerializeField]
    EvolutionWindow evolutionWindow;
    [SerializeField]
    PowerUp calcStatus;

    private CharacterBase baseCharacter;

    public void setCharacterInfomation(CharacterBase chara, bool isBattle)
    {
        baseCharacter = chara;
        var calcResult = calcStatus.CalcCharacterStatus(chara);

        charaName.text = chara.name;
        levelText.text = UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].level + " / " + UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].rarity * 10;
        hpText.text = calcResult.hp.ToString();
        atkText.text = calcResult.atk.ToString();
        defText.text = calcResult.def.ToString();
        skillName.text = chara.ActiveSkill.name.ToString();
        //skillDetail.text = chara.ActiveSkill.skillName.ToString();
        charaCount.text = (UserData.Instance.userItems.OwnedCharacter[Int32.Parse(chara.ID.Split('c')[1])].Value - 1).ToString();

        jobIcon.sprite = jobSprites[(int)chara.CharacterJob];
        jobIcon.preserveAspect = true;
        var tex = chara.Texture;
        charaImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        charaImage.preserveAspect = true;

        for (int i = 0; i < UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].rarity; i++)
        {
            rarityImage[i].gameObject.SetActive(true);
        }

        detailWindow.SetActive(true);
    }

    public void UpdateInfomation()
    {
        levelText.text = UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].level + " / " + UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].rarity * 10;
        var calcResult = calcStatus.CalcCharacterStatus(baseCharacter);
        hpText.text = calcResult.hp.ToString();
        atkText.text = calcResult.atk.ToString();
        defText.text = calcResult.def.ToString();
        for (int i = 0; i < UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].rarity; i++)
        {
            rarityImage[i].gameObject.SetActive(true);
        }
        charaCount.text = (UserData.Instance.userItems.OwnedCharacter[Int32.Parse(baseCharacter.ID.Split('c')[1])].Value - 1).ToString();
    }

    public void openLevelUpWindow()
    {
        levelUpWindow.setStatusInfo(baseCharacter);
    }

    public void openEvolutionWindow()
    {
        if (UserData.Instance.charaStatusList.charaStatus[Int32.Parse(baseCharacter.ID.Split('c')[1])].rarity == 5)
        {
            Debug.LogError("これ以上進化できません");
            return;
        }

        evolutionWindow.setEvolutionInfo(baseCharacter);
    }

    public void closeWindow(GameObject window)
    {
        foreach (var img in rarityImage)
        {
            img.gameObject.SetActive(false);
        }
        window.SetActive(false);
    }

}
