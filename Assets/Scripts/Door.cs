using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Transform targetLocation;
    [SerializeField] private bool isCloseable;

    private SpriteRenderer sr;
    private bool open;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = openSprite;
        open = true;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (open && collider.gameObject.layer == 6) {
            EventBus.instance.TriggerOnDoorEntrance(this);
            open = false;
            if (isCloseable) {
                StartCoroutine(CloseDoor());
            }
        }
    }

    public Vector3 GetTargetLocation() {
        return targetLocation.position;
    }

    IEnumerator CloseDoor() {
        // Can close door after a long time
        yield return new WaitForSeconds(2f);
        sr.sprite = closedSprite;
    }
}
