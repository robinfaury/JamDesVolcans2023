using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRecTransform : MonoBehaviour
{

    // =========================================== VARIABLES (variables utilisées par le scripts)

    // ********** Bookmarker
    Vector2 startAnchoredPosition;
    RectTransform rect;
    IEnumerator routine;

    // =========================================== UNITY (events relatifs à unity)

    void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        startAnchoredPosition = rect.anchoredPosition;
    }

    // =========================================== CORPS (fonction propres au script)

    public void Move(Vector2 startDeltaPosition, Vector2 endDeltaPosition, float duration, float delay, AnimationCurve curve)
    {
        // On lance la routine
        if (routine != null) StopCoroutine(routine);
        routine = Routine();
        StartCoroutine(routine);

        // Routine d'animation
        IEnumerator Routine()
        {
            yield return new WaitForSeconds(delay);

            float percent = 0;
            while (percent < 1) {
                percent += Time.unscaledDeltaTime / duration;
                Vector2 delta = Vector2.Lerp(startDeltaPosition, endDeltaPosition, curve.Evaluate(percent));
                rect.anchoredPosition = startAnchoredPosition + delta;
                yield return null;
            }
            rect.anchoredPosition = startAnchoredPosition + endDeltaPosition;
        }
    }


}
