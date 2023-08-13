using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour {
    [SerializeField] private GameObject goal;
    [SerializeField] private Transform respawnLocation;

    private Collider2D startCollider;
    private Collider2D goalCollider;

    private bool timerStarted;
    private float timeLeft;
    public float TimeLeft { get { return timeLeft; } }

    void Start() {
        startCollider = GetComponent<Collider2D>();
        goalCollider = goal.GetComponent<Collider2D>();

        goalCollider.enabled = false;
    }

    void Update() {
        startCollider.enabled = !timerStarted;
        goalCollider.enabled = timerStarted;

        if (timeLeft > 0) {
            timeLeft = Mathf.Max(timeLeft - Time.deltaTime, 0);

            // Only reset position if timer was started
            if (timeLeft == 0 && timerStarted) {
                timerStarted = false;
                EventBus.instance.TriggerOnTimerElapsed(this);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.layer == 6) {
            timerStarted = !timerStarted;
            if (timerStarted) {
                timeLeft = 60f;
            }

            // Hit a trigger, either activate or stop timer
            EventBus.instance.TriggerOnTimerStateChange(this, timerStarted);
        }
    }

    public Vector2 GetRespawnLocation() {
        return respawnLocation.position;
    }
}
