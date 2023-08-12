using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune : MonoBehaviour {
    [SerializeField] private int abilityId;
    [SerializeField] private float amplitude;
    [SerializeField] private float speed;

    public int AbilityId { get { return abilityId; } }

    private float startY;

    void Awake() {
        startY = transform.position.y;
    }

    void Update() {
        Vector3 p = transform.position;
        p.y = amplitude * Mathf.Sin(Time.time * speed) + startY;
        transform.position = p;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        // Player layer
        if (collider.gameObject.layer == 6) {
            EventBus.instance.TriggerOnRuneCollected(this);

            // TODO: Play some kind of short cutscene before this is destroyed?
            // Place in either the Rune class or a CutsceneManager?

            Destroy(gameObject);
        }
    }
}
