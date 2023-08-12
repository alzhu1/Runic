using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] private GameObject intro;

    // TODO: Should also handle panel to show available buttons

    void Start() {
        EventBus.instance.OnGameStart += StartGame;
    }

    void OnDestroy() {
        EventBus.instance.OnGameStart -= StartGame;
    }

    void Update() {
        
    }

    void StartGame() {
        StartCoroutine(FadeIntro());
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
}
