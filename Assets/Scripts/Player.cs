using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float gravityScale;

    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    private bool canAct;

    private float horizontal;
    private bool shouldJump;
    private bool grounded;

    // Rune unlocks and properties
    private bool[] abilityChecks;
    private bool CanMoveLeft { get { return abilityChecks[0]; }}
    private bool CanMoveRight { get { return abilityChecks[1]; }}
    private bool CanJump { get { return abilityChecks[2]; }}
    private bool CanDash { get { return abilityChecks[3]; }}
    private bool CanChangeSize { get { return abilityChecks[4]; }}
    private bool CanGlide { get { return abilityChecks[5]; }}
    private bool CanSuperJump { get { return abilityChecks[6]; }}

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        abilityChecks = new bool[7];
    }

    void Start() {
        // Attach events
        EventBus.instance.OnGameStart += EnablePlayer;
        EventBus.instance.OnRuneCollected += UnlockRune;
    }

    void OnDestroy() {
        // Detach events
        EventBus.instance.OnGameStart -= EnablePlayer;
        EventBus.instance.OnRuneCollected -= UnlockRune;
    }

    void Update() {
        if (!canAct) {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        bool leftCheck = !CanMoveLeft && horizontal < 0;
        bool rightCheck = !CanMoveRight && horizontal > 0;
        if (leftCheck || rightCheck) {
            horizontal = 0;
        }

        if (grounded && !shouldJump && Input.GetButtonDown("Jump")) {
            shouldJump = true;
        }
    }

    void FixedUpdate() {
        if (!canAct) {
            rb.gravityScale = 0;
            return;
        }

        if (rb.gravityScale < gravityScale) {
            rb.gravityScale = gravityScale;
        }

        Vector2 currVelocity = rb.velocity;
        currVelocity.x = horizontal * moveSpeed * Time.fixedDeltaTime;

        grounded = Physics2D.OverlapCircle(groundCheckTransform.position, 0.05f, groundLayer);

        if (grounded &&shouldJump) {
            currVelocity.y = jumpVelocity;
            shouldJump = false;
        }

        rb.velocity = currVelocity;
    }

    public void EnablePlayer() {
        canAct = true;
    }

    public void UnlockRune(Rune r) {
        abilityChecks[r.AbilityId] = true;
    }
}
