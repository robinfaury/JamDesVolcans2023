using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (RectTransform))]
public class MoveOnEnable : MonoBehaviour
{

    // =========================================== VARIABLES (variables utilisées par le scripts)

    // ********** Inspecteur
    [Header("SETTINGS")]
    [SerializeField] float delay;
    [SerializeField] Vector2 startDeltaPosition;
    [SerializeField] Vector2 endDeltaPosition;

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

    void OnEnable()
    {
        Move();
    }

    // =========================================== CORPS (fonction propres au script)

    public void Move ()
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
                percent += Time.unscaledDeltaTime / UISettings.current.fadeDuration;
                Vector2 delta = Vector2.Lerp(startDeltaPosition, endDeltaPosition, UISettings.current.fadeCurve.Evaluate(percent));
                rect.anchoredPosition = startAnchoredPosition + delta;
                yield return null;
            }
            rect.anchoredPosition = startAnchoredPosition + endDeltaPosition;
        }
    }
}