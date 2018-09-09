using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class TeamEdit : MonoBehaviour
{
    private const int MAX_TEAM_MEMBER = 3;
    private const int MAX_TEAM_SLOT = 3;

    enum EditState
    {
        Description,
        Selection
    }

    EditState currentState = EditState.Description;

    [SerializeField]
    Button[] m_teamIcon = new Button[9];
    [SerializeField]
    Button m_characterIcon;
    [SerializeField]
    GameObject m_contentsObject;
    [SerializeField]
    Button m_charaInfoButton;
    [SerializeField]
    Button m_charaUseButton;
    [SerializeField]
    Button m_removeButton;
    [SerializeField]
    CharacterDetailWindow m_detailWindow;
    [SerializeField]
    CharacterHolder m_charaHolder;

    [SerializeField]
    Sprite[] jobIcons;

    private CharacterBase m_selectedCharacter;

    bool m_isEdit = false;

    [SerializeField]
    CharacterBase m_dummyCharacter;

    [SerializeField]
    GameObject m_iconContainer;

    void Awake()
    {
        for (int i = 0; i < MAX_TEAM_SLOT * MAX_TEAM_MEMBER; i++)
        {
            int teamNumberIndex = i;

            m_teamIcon[i].onClick.AddListener(() =>
            {
                if (currentState == EditState.Description)
                {
                    if (GetTeamCharacterFromIndex(teamNumberIndex / MAX_TEAM_SLOT, teamNumberIndex % MAX_TEAM_MEMBER) != m_dummyCharacter)
                    {
                        setInfoButton(m_teamIcon[teamNumberIndex], GetTeamCharacterFromIndex(teamNumberIndex / MAX_TEAM_SLOT, teamNumberIndex % MAX_TEAM_MEMBER), 3);
                        if (checkTeamMemberCount() == false) return;
                        setRemoveButton(teamNumberIndex);
                        m_charaUseButton.gameObject.SetActive(false);
                    }
                    return;
                }
                else
                {
                    m_teamIcon[teamNumberIndex].transform.GetChild(0).GetComponent<Image>().sprite = m_selectedCharacter.CharacterIconprefab.GetComponent<Image>().sprite;
                    if (teamNumberIndex % MAX_TEAM_MEMBER == 0)
                        GameState.Instance.GetTeams()[teamNumberIndex / MAX_TEAM_SLOT].m_leader = m_selectedCharacter;
                    else if (teamNumberIndex % MAX_TEAM_MEMBER == 1)
                        GameState.Instance.GetTeams()[teamNumberIndex / MAX_TEAM_SLOT].m_sub1 = m_selectedCharacter;
                    else if (teamNumberIndex % MAX_TEAM_MEMBER == 2)
                        GameState.Instance.GetTeams()[teamNumberIndex / MAX_TEAM_SLOT].m_sub2 = m_selectedCharacter;

                    m_isEdit = true;
                    currentState = EditState.Description;
                }
            });
        }
        SetTeamIcon();
    }

    // Use this for initialization
    void Start()
    {
        int count = 0;
        Vector2 buttonPos = new Vector2();
        buttonPos.y = 0;
        GameObject parent = null;

        for (int i = 0; i < UserData.Instance.userItems.OwnedCharacter.Length; i++)
        {
            if (UserData.Instance.userItems.OwnedCharacter[i].Value != 0)
            {
                var chara = m_charaHolder.GetCharacterFromHolder((characterEnum)Enum.ToObject(typeof(characterEnum), i));

                if (chara == null)
                {
                    Debug.LogError("Not Set CharaObject to Holder");
                    continue;
                }

                if (count % 3 == 0)
                {
                    var nextParent = Instantiate(m_contentsObject, m_contentsObject.transform.parent.transform);
                    nextParent.SetActive(true);
                    nextParent.name = ((count / 3)).ToString();
                    parent = nextParent;
                }

                var button = Instantiate(m_characterIcon, parent.transform);
                button.gameObject.SetActive(true);
                buttonPos.x = button.GetComponent<RectTransform>().anchoredPosition.x + ((count % 3) * 100);

                button.GetComponent<RectTransform>().anchoredPosition = buttonPos;

                var buttonImg = button.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
                buttonImg.sprite = chara.CharacterIconprefab.GetComponent<Image>().sprite;
                var buttonRect = new Rect(chara.CharacterIconprefab.GetComponent<RectTransform>().anchoredPosition, chara.CharacterIconprefab.GetComponent<RectTransform>().sizeDelta);
                buttonImg.rectTransform.localPosition = buttonRect.position;
                buttonImg.rectTransform.sizeDelta = buttonRect.size;

                var setPos = buttonPos;
                setPos.y -= 30;
                button.onClick.AddListener(() =>
                {
                    m_selectedCharacter = chara;
                    scrollAnim(button.gameObject);
                    setInfoButton(button, m_selectedCharacter, 2);
                    m_removeButton.gameObject.SetActive(false);
                    if (checkAddTeamCaracter(m_selectedCharacter) == false)
                    {
                        m_charaUseButton.gameObject.SetActive(false);
                        return;
                    }
                    else
                    {
                        m_charaUseButton.transform.parent = button.transform.parent;
                        m_charaUseButton.GetComponent<RectTransform>().anchoredPosition = setPos;
                        m_charaUseButton.gameObject.SetActive(true);

                        m_charaUseButton.onClick.RemoveAllListeners();
                        m_charaUseButton.onClick.AddListener(() =>
                        {
                            currentState = EditState.Selection;
                            m_charaUseButton.gameObject.SetActive(false);
                            m_charaInfoButton.gameObject.SetActive(false);
                        });
                    }
                });

                count++;
            }
        }

        UpdateIconInformation();

        var dummyContents = Instantiate(m_contentsObject, m_contentsObject.transform.parent.transform);
        dummyContents.name = "DummyContents";
        dummyContents.SetActive(true);
    }

    public void UpdateIconInformation()
    {
        int count = 0;
        int containerIndex = 0;

        for (int i = 0; i < UserData.Instance.userItems.OwnedCharacter.Length; i++)
        {
            if (UserData.Instance.userItems.OwnedCharacter[i].Value != 0)
            {
                var chara = m_charaHolder.GetCharacterFromHolder((characterEnum)Enum.ToObject(typeof(characterEnum), i));

                if (chara == null)
                {
                    Debug.LogError("Not Set CharaObject to Holder");
                    continue;
                }

                if (count % 3 == 0)
                {
                    containerIndex++;
                    count = 0;
                }
               // Debug.Log(containerIndex.ToString());
                var buttonHolder = m_iconContainer.transform.GetChild(containerIndex);
                var icon = buttonHolder.GetChild(count+2);

                //Update stars-----------------------
                var starHolder = icon.GetChild(1);
                var rarity = UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].rarity;
                if (rarity > 3)
                {
                    starHolder.GetChild(3).gameObject.SetActive(true);
                    if(rarity > 4)
                    {
                        starHolder.GetChild(4).gameObject.SetActive(true);
                    }
                }

                //Update level-----------------------
                icon.GetChild(2).GetComponent<TextMeshProUGUI>().text = UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].level.ToString();
                //Update job icon--------------------
                icon.GetChild(3).GetComponent<Image>().sprite = jobIcons[(int)chara.CharacterJob];

                count++;
            }
        }
    }

    void SetTeamIcon()
    {
        for (int i = 0; i < MAX_TEAM_SLOT; i++)
        {
            Texture2D tex;
            if (GameState.Instance.GetTeams()[i].m_leader.ID != m_dummyCharacter.ID)
            {
                tex = GameState.Instance.GetTeams()[i].m_leader.Texture;
                m_teamIcon[i * MAX_TEAM_MEMBER].transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            if (GameState.Instance.GetTeams()[i].m_sub1.ID != m_dummyCharacter.ID)
            {
                tex = GameState.Instance.GetTeams()[i].m_sub1.Texture;
                m_teamIcon[i * MAX_TEAM_MEMBER + 1].transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            if (GameState.Instance.GetTeams()[i].m_sub2.ID != m_dummyCharacter.ID)
            {
                tex = GameState.Instance.GetTeams()[i].m_sub2.Texture;
                m_teamIcon[i * MAX_TEAM_MEMBER + 2].transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    void setInfoButton(Button icon, CharacterBase chara, int pos)
    {

        m_charaInfoButton.transform.parent = icon.transform.parent;
        m_charaInfoButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(icon.GetComponent<RectTransform>().anchoredPosition.x, icon.GetComponent<RectTransform>().anchoredPosition.y + pos * 8);

        m_charaInfoButton.gameObject.SetActive(true);
        m_charaInfoButton.onClick.RemoveAllListeners();
        m_charaInfoButton.onClick.AddListener(() =>
        {
            m_detailWindow.setCharacterInfomation(chara, false);
            m_charaUseButton.gameObject.SetActive(false);
            m_charaInfoButton.gameObject.SetActive(false);
            m_removeButton.gameObject.SetActive(false);
        });
    }

    void setRemoveButton(int index)
    {

        m_removeButton.transform.parent = m_teamIcon[index].transform.parent;
        m_removeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(m_teamIcon[index].GetComponent<RectTransform>().anchoredPosition.x, m_charaInfoButton.GetComponent<RectTransform>().anchoredPosition.y - 50);

        m_removeButton.gameObject.SetActive(true);
        m_removeButton.onClick.RemoveAllListeners();
        m_removeButton.onClick.AddListener(() =>
        {
            if (index % MAX_TEAM_MEMBER == 0)
                GameState.Instance.GetTeams()[index / MAX_TEAM_SLOT].m_leader = m_dummyCharacter;
            else if (index % MAX_TEAM_MEMBER == 1)
                GameState.Instance.GetTeams()[index / MAX_TEAM_SLOT].m_sub1 = m_dummyCharacter;
            else if (index % MAX_TEAM_MEMBER == 2)
                GameState.Instance.GetTeams()[index / MAX_TEAM_SLOT].m_sub2 = m_dummyCharacter;

            m_teamIcon[index].transform.GetChild(0).GetComponent<Image>().sprite = null;

            m_isEdit = true;
            m_removeButton.gameObject.SetActive(false);
            m_charaInfoButton.gameObject.SetActive(false);
        });
    }

    CharacterBase GetTeamCharacterFromIndex(int sInd, int tInd)
    {
        CharacterBase chara = null;
        switch (tInd)
        {
            case 0:
                chara = GameState.Instance.GetTeams()[sInd].m_leader;
                break;
            case 1:
                chara = GameState.Instance.GetTeams()[sInd].m_sub1;
                break;
            case 2:
                chara = GameState.Instance.GetTeams()[sInd].m_sub2;
                break;
            default:
                chara = m_dummyCharacter;
                break;
        }
        return chara;

    }

    private bool checkTeamMemberCount()
    {
        int count = 0;
        if (GameState.Instance.LeaderCharacter.ID != m_dummyCharacter.ID) count++;
        if (GameState.Instance.SubCharacter1.ID != m_dummyCharacter.ID) count++;
        if (GameState.Instance.SubCharacter2.ID != m_dummyCharacter.ID) count++;
        if (count <= 1) return false;
        return true;
    }

    private bool checkAddTeamCaracter(CharacterBase chara)
    {
        if (GameState.Instance.LeaderCharacter.ID == chara.ID) return false;
        else if (GameState.Instance.SubCharacter1.ID == chara.ID) return false;
        else if (GameState.Instance.SubCharacter2.ID == chara.ID) return false;
        return true;

    }

    public void initTeamEdit()
    {
        currentState = EditState.Description;
        m_selectedCharacter = null;
        m_removeButton.gameObject.SetActive(false);
        m_charaUseButton.gameObject.SetActive(false);
        m_charaInfoButton.gameObject.SetActive(false);
    }

    public void scrollAnim(GameObject button)
    {
        var parents = button.transform.parent;
        var num = Int32.Parse(parents.name);
        parents.transform.parent.GetComponent<RectTransform>().DOAnchorPosY(110 * num, 0.3f);

    }

    private void OnDestroy()
    {
        if (m_isEdit == true)
        {
            ServerInterface.Instance.SaveTeamData();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)&&(m_selectedCharacter != null || m_charaInfoButton.enabled == true))
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                initTeamEdit();
            }
        }
    }
}
