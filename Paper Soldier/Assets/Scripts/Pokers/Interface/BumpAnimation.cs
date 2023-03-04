using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class BumpAnimation : MonoBehaviour
{

    CanvasGroup graphics;
    IEnumerator routine;

    public void Appear(float delay, int index, int row)
    {
        if (graphics == null) graphics = GetComponent<CanvasGroup>();
        if (routine != null) StopCoroutine(routine);
        routine = Routine();
        StartCoroutine(routine);

        IEnumerator Routine()
        {
            int y = index / row;
            int x = index - y * row;
            float distance = Mathf.Sqrt(y * y + x * x);
            yield return new WaitForSeconds(delay + distance * 0.05f);
            float progress = 0;
            float anim = 0;
            while (progress < 1) {
                progress += Time.unscaledDeltaTime / UISettings.current.bumpDuration / 3;
                anim = UISettings.current.bumpCurve.Evaluate(progress);
                graphics.alpha = anim;
                graphics.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1, anim);
                yield return null;
            }
            graphics.alpha = 1;
            graphics.transform.localScale = Vector3.one;
        }
    }

    public void Bump()
    {
        if (graphics == null) graphics = GetComponent<CanvasGroup>();
        if (routine != null) StopCoroutine(routine);
        routine = Routine();
        StartCoroutine(routine);
        IEnumerator Routine()
        {
            float progress = 0;
            float anim = 0;
            while (progress < 1) {
                progress += Time.unscaledDeltaTime / UISettings.current.bumpDuration * 5;
                anim = UISettings.current.bumpCurve.Evaluate(progress);
                graphics.alpha = anim;
                graphics.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1, anim);
                yield return null;
            }
            graphics.alpha = 1;
            graphics.transform.localScale = Vector3.one;
        }
    }
}