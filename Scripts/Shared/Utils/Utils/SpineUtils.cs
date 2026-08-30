using System.Collections;
using Spine.Unity;
using UnityEngine;

public class SpineUtils : Singleton<SpineUtils>
{
    public IEnumerator FadeOutInChangeSpine(SkeletonAnimation anim, SkeletonDataAsset newData, float duration)
    {
        float elapsed = 0f;

        // 투명도 1 -> 0 (페이드 아웃)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            anim.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 스켈레톤 데이터 교체
        anim.skeletonDataAsset = newData;
        anim.Initialize(true);
        anim.AnimationName = "idle";

        elapsed = 0f;
        // 투명도 0 -> 1 (페이드 인)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            anim.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }
}
