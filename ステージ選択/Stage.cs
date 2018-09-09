using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Stage", menuName = "ScriptableObjects/Stage")]
public class Stage : ScriptableObject {

    [SerializeField]
    private int m_stageNumber;

    public enum RewardType
    {
        EasyReward,
        NormalReward,
        HardReward,
        EasyCoinReward,
        NormalCoinReward,
        HardCoinReward,
    };

    [System.SerializableAttribute]
    public class FloorMonster
    {
        public MonsterBase Monster;
        public int level;
    }

    [System.SerializableAttribute]
    public class StageFloor
    {
        public FloorMonster[] Monsters;

        public StageFloor(FloorMonster[] list)
        {
            Monsters = list;
        }
    }

    [SerializeField]
    private List<StageFloor> m_stage;
    [SerializeField]
    private Texture2D m_background;

    [SerializeField]
    private int m_staminaCost;
    [SerializeField]
    private RewardType m_rewardType;
    [SerializeField]
    private bool endless = false;
    [SerializeField]
    private MonsterHolder m_holder;

    public  List<StageFloor> Floor
    {
        get { return m_stage; }
    }

    public Texture2D Background
    {
        get { return m_background; }
    }

    public int StaminaCost
    {
        get { return m_staminaCost; }
    }

    public RewardType StageRewardType
    {
        get { return m_rewardType; }
    }
    public int StageNumber
    {
        get { return m_stageNumber; }
    }
    public bool isEndless
    {
        get { return endless; }
    }

}
