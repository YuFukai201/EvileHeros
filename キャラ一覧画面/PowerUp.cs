using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    public struct VariableValue
    {
        public int hp;
        public int atk;
        public int def;
    }

    public VariableValue CalcCharacterStatus(CharacterBase chara)
    {
        if (chara == null) Debug.LogError("Character is null");
        VariableValue resultStatus = new VariableValue();

        resultStatus.hp = chara.Hp + CalcStatusUp(chara.HpGrowValue, UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].level);
        resultStatus.atk = chara.Atk + CalcStatusUp(chara.AtkGrowValue, UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].level);
        resultStatus.def = chara.Def + CalcStatusUp(chara.DefGrowValue, UserData.Instance.charaStatusList.charaStatus[Int32.Parse(chara.ID.Split('c')[1])].level);

        return resultStatus;
    }

    public int CalcStatusUp(float growValue, int level)
    {
        return (int)(growValue * level + (int)(level / 10 * growValue) * 5);
    }
}
