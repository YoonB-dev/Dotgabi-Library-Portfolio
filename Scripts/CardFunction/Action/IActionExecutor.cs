using UnityEngine;

// 단일 책임 원칙을 지키는 것이 중요하지만, 단순한 구조이기 때문에 오버라이딩 사용.
public interface IActionExecutor
{
    // 행동 실행 (여기선 damage를 레퍼런스로 받아서 수정 가능)
    void Execute(CardActionDTO cardAction, ref int damage, CharacterBase enemy);
}