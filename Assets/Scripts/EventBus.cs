using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour {
    public static EventBus instance = null;

    public event Action OnGameStart = delegate {};
    public event Action<int, bool> OnPlayerAction = delegate {};
    public event Action<Rune> OnRuneCollected = delegate {};
    public event Action<Door> OnDoorEntrance = delegate {};
    public event Action<Timer, bool> OnTimerStateChange = delegate {};
    public event Action<Timer> OnTimerElapsed = delegate {};

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

    public void TriggerOnPlayerAction(int abilityId, bool state) {
        OnPlayerAction?.Invoke(abilityId, state);
    }

    public void TriggerOnRuneCollected(Rune r) {
        OnRuneCollected?.Invoke(r);
    }

    public void TriggerOnDoorEntrance(Door d) {
        OnDoorEntrance?.Invoke(d);
    }

    public void TriggerOnTimerStateChange(Timer t, bool timerStarted) {
        OnTimerStateChange?.Invoke(t, timerStarted);
    }

    public void TriggerOnTimerElapsed(Timer t) {
        OnTimerElapsed?.Invoke(t);
    }
}
