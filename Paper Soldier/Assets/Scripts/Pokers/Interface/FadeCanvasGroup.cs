using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(CanvasGroup))]
public class FadeCanvasGroup : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public IEnumerator routine;

    // ----------------------------- PARTIE CONTROLE D'UI

    public void Fade(float startAlpha, float targetAlpha, float duration, AnimationCurve curve, System.Action onEnd = null)
    {
        // On va chercher le composant si il n'est pas déjà assigné
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // On lance la routine
        if (routine != null) StopCoroutine(routine);
        routine = Routine();
        StartCoroutine(routine);

        // Routine d'animation
        IEnumerator Routine()
        {
            float percent = 0;
            while (percent < 1) {
                percent += Time.unscaledDeltaTime / duration;
                canvasGroup.alpha = Mathf.Lerp (startAlpha, targetAlpha, curve.Evaluate(percent));
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
            onEnd?.Invoke();
        }
    }
}