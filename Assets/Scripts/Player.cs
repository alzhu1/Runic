using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public const int MOVE_LEFT_ID = 0;
    public const int MOVE_RIGHT_ID = 1;
    public const int JUMP_ID = 2;
    public const int DASH_ID = 3;
    public const int CHANGE_SIZE_ID = 4;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float gravityScale;

    [SerializeField] private float dashVelocity;
    [SerializeField] private float dashDrag;
    [SerializeField] private float dashTime;

    [SerializeField] private float changeSizeTime;

    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float transitionTime;

    // TODO: REMOVE
    [SerializeField] private bool debugMode;

    private Animator animator;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool canAct;

    private float normalScale;

    private float horizontal;
    private float vertical;
    private bool facingLeft;

    private bool grounded;

    private bool shouldJump;

    private bool dashRecharged;
    private bool dashing;
    private bool shrunk;
    private bool changingSize;

    // Rune unlocks and properties
    private bool[] abilityChecks;

    private bool CanMoveLeft { get { return abilityChecks[MOVE_LEFT_ID]; }}
    private bool CanMoveRight { get { return abilityChecks[MOVE_RIGHT_ID]; }}
    private bool CanJump { get { return abilityChecks[JUMP_ID]; }}
    private bool CanDash { get { return abilityChecks[DASH_ID]; }}
    private bool CanChangeSize { get { return abilityChecks[CHANGE_SIZE_ID]; }}

    void Awake() {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        normalScale = transform.localScale.x;

        abilityChecks = new bool[5];
        if (debugMode) {
            for (int i = 0; i < abilityChecks.Length; i++) {
                abilityChecks[i] = true;
            }
        }
    }

    void Start() {
        // Attach events
        EventBus.instance.OnGameStart += EnablePlayer;
        EventBus.instance.OnRuneCollected += UnlockRune;
        EventBus.instance.OnDoorEntrance += ReceiveDoorEvent;
        EventBus.instance.OnTimerElapsed += ReceiveTimerElapsedEvent;
    }

    void OnDestroy() {
        // Detach events
        EventBus.instance.OnGameStart -= EnablePlayer;
        EventBus.instance.OnRuneCollected -= UnlockRune;
        EventBus.instance.OnDoorEntrance -= ReceiveDoorEvent;
        EventBus.instance.OnTimerElapsed -= ReceiveTimerElapsedEvent;
    }

    void Update() {
        if (!canAct) {
            return;
        }

        animator.SetFloat("xSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Dashing", dashing);

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        bool leftCheck = !CanMoveLeft && horizontal < 0;
        bool rightCheck = !CanMoveRight && horizontal > 0;
        if (leftCheck || rightCheck) {
            horizontal = 0;
        }

        if (horizontal < 0) {
            facingLeft = true;
        } else if (horizontal > 0) {
            facingLeft = false;
        }
        sr.flipX = facingLeft;

        // TODO: fix this mess of boolean expressions...

        if (CanJump && grounded && !dashing && !shouldJump && Input.GetButtonDown("Jump")) {
            shouldJump = true;

        }

        // Recharge dash once grounded
        dashRecharged = dashRecharged || grounded;
        if (CanDash && dashRecharged && !dashing && Input.GetButtonDown("Dash")) {
            StartCoroutine(Dash());
        }

        if (!changingSize && CanChangeSize) {
            if (vertical < 0 && !shrunk) {
                StartCoroutine(ChangeSize(true));
            }

            if (vertical > 0 && shrunk) {
                StartCoroutine(ChangeSize(false));
            }
        }
    }

    void FixedUpdate() {
        if (!canAct) {
            rb.gravityScale = 0;
            return;
        }

        if (dashing) {
            return;
        }

        if (rb.gravityScale < gravityScale) {
            rb.gravityScale = gravityScale;
        }

        Vector2 currVelocity = rb.velocity;
        currVelocity.x = horizontal * moveSpeed * Time.fixedDeltaTime;

        grounded = Physics2D.OverlapCircle(groundCheckTransform.position, 0.05f, groundLayer);

        // Handle Jump
        if (grounded) {
            // Actions for grounded (jump, super jump)
            if (shouldJump) {
                currVelocity.y = jumpVelocity;
                shouldJump = false;
                TriggerActionEvent(JUMP_ID, true);
            }
        }

        rb.velocity = currVelocity;
    }

    IEnumerator Dash() {
        dashing = true;
        dashRecharged = false;
        rb.drag = dashDrag;
        rb.gravityScale = 0;

        TriggerActionEvent(DASH_ID, true);

        float factor = facingLeft ? -1 : 1;
        rb.velocity = new Vector2(dashVelocity * factor, 0);
        yield return new WaitForSeconds(dashTime);

        dashing = false;
        rb.drag = 0f;
        rb.gravityScale = gravityScale;
    }

    IEnumerator ChangeSize(bool shouldShrink) {
        changingSize = true;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(normalScale, normalScale, 1);
        if (shouldShrink) {
            endScale.x *= 0.5f;
            endScale.y *= 0.5f;
        }
        float currEdgeRadius = boxCollider.edgeRadius;
        float targetEdgeRadius = shouldShrink ? currEdgeRadius / 2 : currEdgeRadius * 2;

        TriggerActionEvent(CHANGE_SIZE_ID, shouldShrink);

        float t = 0;
        while (t < changeSizeTime) {
            transform.localScale = Vector3.Lerp(startScale, endScale, t / changeSizeTime);
            boxCollider.edgeRadius = Mathf.Lerp(currEdgeRadius, targetEdgeRadius, t / changeSizeTime);
            yield return null;
            t += Time.deltaTime;
        }
        transform.localScale = endScale;
        boxCollider.edgeRadius = targetEdgeRadius;

        shrunk = shouldShrink;
        changingSize = false;
    }

    void EnablePlayer() {
        canAct = true;
    }

    void UnlockRune(Rune r) {
        abilityChecks[r.AbilityId] = true;
    }

    void ReceiveDoorEvent(Door d) {
        StartCoroutine(MovePlayerToLocation(d.GetTargetLocation()));
    }

    void ReceiveTimerElapsedEvent(Timer t) {
        StartCoroutine(MovePlayerToLocation(t.GetRespawnLocation()));
    }

    IEnumerator MovePlayerToLocation(Vector3 location) {
        yield return new WaitForSeconds(transitionTime);
        transform.position = location;
    }

    void TriggerActionEvent(int abilityId, bool state) {
        EventBus.instance.TriggerOnPlayerAction(abilityId, state);
    }
}
