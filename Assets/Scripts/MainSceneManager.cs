using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainSceneManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Start()
    {
        StartCoroutine(FadeFromBlack());
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1;
            fadeImage.color = color;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                color.a = Mathf.Lerp(1, 0, t / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = 0;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(false);
        }
    }
}
