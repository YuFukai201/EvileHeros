using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CharacterSelect : MonoBehaviour {

    enum MenuState
    {
        Description,
        Selection
    };

    [SerializeField] CharacterBase[] m_characters;
    [SerializeField] GameObject parents;
    [SerializeField] Button characterBtn;
    [SerializeField] Animator selectAnim;
    [SerializeField] Button[] btnCharacters;
    [SerializeField] GameObject selectionFrame;

    private int selectedIndex;
    private Image[] characterImgs;

    bool selectLeader = false;

    MenuState currentState = MenuState.Description;
   
    void Awake()
    {
        selectedIndex = 0;
        
        var pos = btnCharacters[0].transform.localPosition;
        selectionFrame.transform.localPosition = pos;
        pos.y += 30;
        btnCharacters[0].transform.localPosition = pos;

        characterImgs = new Image[btnCharacters.Length];
        for(int i = 0; i < btnCharacters.Length; i++)
        {
            characterImgs[i] = btnCharacters[i].transform.GetChild(0).GetComponent<Image>();
        }
    }

    void Start () {
        UpdateSelectedCharImg();

        for (int bi = 0; bi < m_characters.Length; bi++)
        {
            var button =  Instantiate(characterBtn, parents.transform);

            button.gameObject.SetActive(true);
            var tex = m_characters[bi].Texture;
            var btnImg = button.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            int index = bi;
            button.onClick.AddListener(() => {
                CharacterSet(index);
            });
        }
    }

    void UpdateSelectedCharImg()
    {
        var tex = GameState.Instance.LeaderCharacter.Texture;
        characterImgs[0].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        if(GameState.Instance.SubCharacter1 != null)
        {
            tex = GameState.Instance.SubCharacter1.Texture;
            characterImgs[1].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        if(GameState.Instance.SubCharacter2 != null)
        {
            tex = GameState.Instance.SubCharacter2.Texture;
            characterImgs[2].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }

    public void SelectParty(int i)
    {
        if (selectedIndex == i || currentState == MenuState.Selection)
            return;

        var pos = btnCharacters[i].transform.localPosition;
        selectionFrame.transform.localPosition = pos;
        pos.y += 30;
        btnCharacters[i].transform.localPosition = pos;

        var oldPos = btnCharacters[selectedIndex].transform.localPosition;
        oldPos.y = 0;
        btnCharacters[selectedIndex].transform.localPosition = oldPos;

        selectedIndex = i;
    }

    public void ChangeToSelectChar()
    {
        if (currentState == MenuState.Selection)
            return;

        selectAnim.SetBool("selectMenu", true);
        currentState = MenuState.Selection;

        foreach(var charImg in characterImgs)
        {
            charImg.color = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        }

        characterImgs[selectedIndex].color = new Color(1, 1, 1, 1);
    }

    public void ChangeToDesc()
    {
        if (currentState == MenuState.Description)
            return;

        selectAnim.SetBool("selectMenu", false);
        currentState = MenuState.Description;

        foreach (var charImg in characterImgs)
        {
            charImg.color = new Color(1f, 1f, 1f, 1.0f);
        }
    }

    public void CharacterSet(int index)
    {
        if(selectedIndex == 0) {
            GameState.Instance.LeaderCharacter = m_characters[index];
        } else if(selectedIndex == 1) {
            GameState.Instance.SubCharacter1 = m_characters[index];
        } else if (selectedIndex == 2) {
            GameState.Instance.SubCharacter2 = m_characters[index];
        }

        UpdateSelectedCharImg();
    }
}
