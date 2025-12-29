using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips;
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Start()
    {
        if (videoClips.Length > 0 && videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            PlayRandomVideo();
        }
    }

    private void PlayRandomVideo()
    {
        if (videoClips.Length > 0)
        {
            int randomIndex = Random.Range(0, videoClips.Length);
            videoPlayer.clip = videoClips[randomIndex];
            videoPlayer.Play();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        PlayRandomVideo();
    }

    public void LoadMainScene()
    {
        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0;
            fadeImage.color = color;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                color.a = Mathf.Lerp(0, 1, t / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = 1;
            fadeImage.color = color;
        }

        SceneManager.LoadScene("escena  main");
    }
}
