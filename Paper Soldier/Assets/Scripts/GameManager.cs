using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public Sound music;
    public List<Level> levels;

    private void Start()
    {
        canvasGroup.alpha = 0;
        music.Play();
        FadeFromTo(1, 0, 4f);
    }

    void LoadLevel ()
    {

    }

    void FadeFromTo(float from, float to, float duration)
    {
        StartCoroutine(Routine()); IEnumerator Routine ()
        {
            float percent = 0; while ((percent += Time.deltaTime / duration) < 1) {
                float value = Mathf.Lerp (from, to, percent);
                canvasGroup.alpha = value;
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}