using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DamageCal : Singleton<DamageCal>
{
    /// <summary>
    /// 공격할 때 데미지 계산
    /// </summary>
    public static int AttackDamageCal(CharacterBase fromCharacter, CharacterBase targetCharacter, int damage, bool isCalculate, Dictionary<string, object> extraData)
    {
        float rAmount = damage;

        //공격자 디버프 - 허약(고정 1피해), 카드 사용시만 적용
        if (fromCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 10) && isCalculate)
        {
            return 1;
        }

        // 대상자 버프 - 의심(고정 1피해), 무적(고정 0피해)
        if (targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 8) || targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 15))
        {
            return 0;
        }

        if (targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 7))
        {
            rAmount = 1;
        }

        //곱 연산
        if(isCalculate)
        {
            // 공격자 곱 연산 (버프 디버프)
            if (fromCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 3)) {rAmount *= 1.5f;}
            if (fromCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 4)) {rAmount *= 0.7f;}
            // 대상자 곱 연산 (버프 디버프)
            if (targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.debuff, 3)) {rAmount *= 1.5f;}
            if (targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 4)) {rAmount *= 0.7f;}

            // 공격자 합 연산 (버프 디버프)
            rAmount += fromCharacter.GetBuffOrDebuffValue(EnumTypes.Status.buff, 1);
            rAmount -= fromCharacter.GetBuffOrDebuffValue(EnumTypes.Status.debuff, 1);
            // 대상자 합 연산 (버프 디버프)
            rAmount += targetCharacter.GetBuffOrDebuffValue(EnumTypes.Status.debuff, 2);
            rAmount -= targetCharacter.GetBuffOrDebuffValue(EnumTypes.Status.buff, 2);
        }
        //리턴
        if (rAmount < 0) rAmount = 0;
        rAmount = Mathf.CeilToInt(rAmount);
        return (int)rAmount;
    }

    /// <summary>
    /// 받을 때 데미지 계산
    /// </summary>
    public static int GetDamageCal(CharacterBase fromCharacter, CharacterBase targetCharacter, int damage)
    {
        float reAmount = damage;
        //회피 - 30%확률로 데미지 무시
        if (reAmount != 0 && targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 9))
        {
            int k = Random.Range(0, 10);
            reAmount = (k > 2) ? damage : 0;

            if (reAmount == 0)
            {
                // 회피 성공
                //string text = GameData.Text_Log[87][logLan].ToString();
                //player.ShowDamageText(0, "Other", text);
                //player.battleLog.AddTxtString(GameData.Text_Log[51][logLan].ToString());
                //SFX
                AudioManager.Instance.MissSound();
                return 0;
            }
        }

        //무적 - 목걸이 - 이거 아마도 버프 디버프 말고 장비로 따로 빼두는게 낫지 않을까 하는 생각이 있음ㅋㅋ
        // if (player.playerState.buff[19] != 0 && reAmount != 0)
        // {
        //     player.playerState.buff[19] = 0;
        //     string text = GameData.Text_Log[89][logLan].ToString();
        //     player.ShowDamageText(0, "Other", text);
        //     player.battleLog.AddTxtString(GameData.Text_Log[52][logLan].ToString());
        //     //SFX
        //     AudioManager.Instance.GetShieldSound();
        //     return 0;
        // }

        // 대상자 버프 - 무적
        if (targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 8))
        {
            //string text = GameData.Text_Log[88][logLan].ToString();
            //player.ShowDamageText(0, "Other", text);
            //player.battleLog.AddTxtString(GameData.Text_Log[51][logLan].ToString());
            //SFX
            AudioManager.Instance.GetShieldSound();
            return 0;
        }
        // 대상자 버프 - 의심(받는 피해량 1로됨)
        if (targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 7))
        {
            reAmount = 1;
        }
        //만약 0 이하면 0으로
        if (reAmount < 0) reAmount = 0;
        // 대상자 버프 - 버티기
        if (targetCharacter.Stats.currHp <= 0 && targetCharacter.CheckHaveBuffOrDebuff(EnumTypes.Status.buff, 10)) targetCharacter.Stats.currHp = 1;

        return (int)reAmount;
    }

    public static int GetUnLimitcard(CardActionDTO cardActionDTO, int cardUpgrade)
    {
        if (cardActionDTO.ExtraData != null && cardActionDTO.ExtraData.ContainsKey("unlimit"))
        {
            int difference = cardActionDTO.Value[1] - cardActionDTO.Value[0];
            return cardActionDTO.Value[0] + difference * cardUpgrade;
        }
        else
        {
            return cardActionDTO.Value[cardUpgrade];
        }
    }
}
