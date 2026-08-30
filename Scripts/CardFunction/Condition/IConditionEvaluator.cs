using UnityEngine;

public interface IConditionEvaluator
{
    // 조건 데이터(JSON 오브젝트 등)를 받아 조건 충족 여부 판단
    bool Evaluate(CardActionDTO cardAction, Player player , GameObject enemy);
}
