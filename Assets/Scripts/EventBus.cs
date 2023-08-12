using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour {
    public static EventBus instance = null;

    public event Action OnGameStart = delegate {};
    public event Action<Rune> OnRuneCollected = delegate {};

    // TODO: Not sure if this is best place for this
    private bool gameStarted;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void Update() {
        if (!gameStarted && Input.anyKeyDown) {
            gameStarted = true;
            TriggerOnGameStart();
        }
    }

    public void TriggerOnGameStart() {
        OnGameStart?.Invoke();
    }

    public void TriggerOnRuneCollected(Rune r) {
        OnRuneCollected?.Invoke(r);
    }
}
