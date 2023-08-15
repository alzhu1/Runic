using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] private GameObject intro;
    [SerializeField] private Image transitionImage;
    [SerializeField] private float fadeTime;
    [SerializeField] private float fadeWaitTime;
    [SerializeField] private GameObject timerUI;
    [SerializeField] private RawImage panelUI;

    private Text timerText;
    private Image[] panelImages;

    void Awake() {
        timerText = timerUI.GetComponentInChildren<Text>();
        panelImages = panelUI.GetComponentsInChildren<Image>();
    }

    void Start() {
        EventBus.instance.OnGameStart += HideIntroText;
        EventBus.instance.OnRuneCollected += ReceiveRuneCollectedEvent;
        EventBus.instance.OnDoorEntrance += ReceiveDoorEvent;
        EventBus.instance.OnTimerStateChange += ReceiveTimerEvent;
        EventBus.instance.OnTimerElapsed += ReceiveTimerElapsedEvent;
    }

    void OnDestroy() {
        EventBus.instance.OnGameStart -= HideIntroText;
        EventBus.instance.OnRuneCollected -= ReceiveRuneCollectedEvent;
        EventBus.instance.OnDoorEntrance -= ReceiveDoorEvent;
        EventBus.instance.OnTimerStateChange -= ReceiveTimerEvent;
        EventBus.instance.OnTimerElapsed -= ReceiveTimerElapsedEvent;
    }

    void Update() {
        
    }

    void HideIntroText() {
        StartCoroutine(FadeIntro());
    }

    void ReceiveRuneCollectedEvent(Rune r) {
        StartCoroutine(ShowRune(r.AbilityId));
    }

    void ReceiveDoorEvent(Door d) {
        StartCoroutine(FadeTransition());
    }

    void ReceiveTimerEvent(Timer t, bool timerStarted) {
        timerUI.SetActive(timerStarted);
        if (timerStarted) {
            StartCoroutine(UpdateTimer(t));
        }
    }

    void ReceiveTimerElapsedEvent(Timer t) {
        StartCoroutine(FadeTransition());
    }

    IEnumerator FadeIntro() {
        Text[] introText = intro.GetComponentsInChildren<Text>();
        Text panelText = panelUI.GetComponentInChildren<Text>();

        Color white = Color.white;
        Color clearWhite = Color.white;
        clearWhite.a = 0;

        Color panelColor = panelUI.color;
        Color endPanelColor = panelUI.color;
        endPanelColor.a = 0.9f;

        float t = 0;
        while (t < 1) {
            // Fade out text
            foreach (Text text in introText) {
                text.color = Color.Lerp(white, clearWhite, t);
            }

            // Fade in panel
            panelUI.color = Color.Lerp(panelColor, endPanelColor, t);
            panelText.color = Color.Lerp(clearWhite, white, t);
            yield return null;
            t += Time.deltaTime;
        }

        // Clear out intro
        foreach (Text text in introText) {
            text.color = clearWhite;
        }

        // Focus panel
        panelUI.color = endPanelColor;
        panelText.color = white;

        intro.SetActive(false);
    }

    IEnumerator ShowRune(int abilityId) {
        Image runeImage = panelImages[abilityId];
        Image otherImage = abilityId == Player.CHANGE_SIZE_ID ? panelImages[abilityId + 1] : null;
        Color startColor = runeImage.color;
        Color endColor = runeImage.color;
        endColor.a = 1;

        float t = 0;
        while (t < 1) {
            runeImage.color = Color.Lerp(startColor, endColor, t);
            if (otherImage != null) {
                otherImage.color = Color.Lerp(startColor, endColor, t);
            }
            yield return null;
            t += Time.deltaTime;
        }
        runeImage.color = endColor;
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

    IEnumerator UpdateTimer(Timer timer) {
        while (timerUI.activeInHierarchy && timer.TimeLeft > 0) {
            timerText.text = String.Format("{0:0.00}", timer.TimeLeft);
            yield return null;
        }

        timerUI.SetActive(false);
    }
}
