using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Borrowed from RobotVenture
public class GhostTrail : MonoBehaviour {
    [SerializeField] private float timeBetweenGhosts = 0.1f;
    [SerializeField] private float fadeTime = 0.6f;
    [SerializeField] private Color ghostColor = Color.cyan;
    
    private Color clearGhostColor;

    void Awake() {
        clearGhostColor = new Color(ghostColor.r, ghostColor.g, ghostColor.b, 0);
    }

    public void DrawGhost(int numGhosts) {
        StartCoroutine(DrawGhostTrail(numGhosts));
    }

    IEnumerator DrawGhostTrail(int numGhosts) {
        int count = 0;
        while (count < numGhosts) {
            // Create dummy game object with sprite renderer
            GameObject trailPart = new GameObject();
            SpriteRenderer trailPartRenderer = trailPart.AddComponent<SpriteRenderer>();

            // Capture current sprite, and overwrite shader/color for ghost appearance
            SpriteRenderer currSr = GetComponent<SpriteRenderer>();
            trailPartRenderer.sprite = currSr.sprite;
            trailPartRenderer.color = ghostColor;
            trailPartRenderer.flipX = currSr.flipX;

            // Set position/size to current snapshot of player position, and begin fading ghost
            trailPart.transform.position = transform.position;
            trailPart.transform.localScale = transform.localScale;
            StartCoroutine(FadeGhost(trailPartRenderer));

            // Wait before making another ghost
            yield return new WaitForSeconds(timeBetweenGhosts);
            count++;
        }
    }

    IEnumerator FadeGhost(SpriteRenderer spriteRenderer) {
        // Track fade progress
        float t = 0;
        while (t < fadeTime) {
            spriteRenderer.color = Color.Lerp(ghostColor, clearGhostColor, t / fadeTime);
            yield return null;
            t += Time.deltaTime;
        }
        spriteRenderer.color = clearGhostColor;

        // When fade finishes, destroy game object
        Destroy(spriteRenderer.gameObject);
    }
}