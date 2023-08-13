using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] private GameObject intro;
    [SerializeField] private Image transitionImage;
    [SerializeField] private float fadeTime;
    [SerializeField] private float fadeWaitTime;

    // TODO: Should also handle panel to show available buttons

    void Start() {
        EventBus.instance.OnGameStart += HideIntroText;
        EventBus.instance.OnDoorEntrance += ShowTransition;
    }

    void OnDestroy() {
        EventBus.instance.OnGameStart -= HideIntroText;
        EventBus.instance.OnDoorEntrance -= ShowTransition;
    }

    void Update() {
        
    }

    void HideIntroText() {
        StartCoroutine(FadeIntro());
    }

    void ShowTransition(Door d) {
        StartCoroutine(FadeTransition());
    }

    IEnumerator FadeIntro() {
        Text[] introText = intro.GetComponentsInChildren<Text>();
        Color startColor = Color.white;
        Color endColor = Color.white;
        endColor.a = 0;

        float t = 0;
        while (t < 1) {
            foreach (Text text in introText) {
                text.color = Color.Lerp(startColor, endColor, t);
            }
            yield return null;
            t += Time.deltaTime;
        }

        foreach (Text text in introText) {
            text.color = endColor;
        }

        intro.SetActive(false);
    }

    IEnumerator FadeTransition() {
        Color show = Color.clear;
        Color hide = Color.black;

        // First, fade to black
        float t = 0;
        while (t < fadeTime) {
            transitionImage.color = Color.Lerp(show, hide, t / fadeTime);
            yield return null;
            t += Time.deltaTime;
        }
        transitionImage.color = hide;

        yield return new WaitForSeconds(fadeWaitTime);

        // Then reveal
        t = 0;
        while (t < fadeTime) {
            transitionImage.color = Color.Lerp(hide, show, t / fadeTime);
            yield return null;
            t += Time.deltaTime;
        }
        transitionImage.color = show;
    }
}
