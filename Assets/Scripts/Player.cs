using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private float superJumpVelocity;
    [SerializeField] private float gravityScale;

    [SerializeField] private float dashVelocity;
    [SerializeField] private float dashDrag;
    [SerializeField] private float dashTime;

    [SerializeField] private float changeSizeTime;

    [SerializeField] private float glideDescendVelocity;

    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;

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
    private bool shouldSuperJump;

    private bool canDash;
    private bool dashing;
    private bool shrunk;
    private bool changingSize;
    private bool gliding;

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
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        normalScale = transform.localScale.x;

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

        // TODO: Replace GetKeyDown with GetButtonDown
        // Also finalize super jump key

        if (grounded && !shouldJump && !shouldSuperJump) {
            if (Input.GetButtonDown("Jump")) {
                shouldJump = true;
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                // TODO: Not sure if this should charge up?
                shouldSuperJump = true;
            }
        }

        // Allow dash once grounded
        canDash = canDash || grounded;
        if (canDash && !dashing && Input.GetKeyDown(KeyCode.Z)) {
            StartCoroutine(Dash());
        }

        if (!changingSize) {
            if (vertical < 0 && !shrunk) {
                StartCoroutine(ChangeSize(true));
            }

            if (vertical > 0 && shrunk) {
                StartCoroutine(ChangeSize(false));
            }
        }

        gliding = Input.GetKey(KeyCode.LeftShift);
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

        if (grounded) {
            // Actions for grounded (jump, super jump)
            if (shouldJump) {
                currVelocity.y = jumpVelocity;
                shouldJump = false;
            }

            if (shouldSuperJump) {
                currVelocity.y = superJumpVelocity;
                shouldSuperJump = false;
            }
        } else {
            // Actions for ungrounded (glide)
            if (gliding) {
                currVelocity.y = Mathf.Max(currVelocity.y, glideDescendVelocity);
            }
        }

        rb.velocity = currVelocity;
    }

    IEnumerator Dash() {
        dashing = true;
        canDash = false;
        rb.drag = dashDrag;
        rb.gravityScale = 0;

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

    public void EnablePlayer() {
        canAct = true;
    }

    public void UnlockRune(Rune r) {
        abilityChecks[r.AbilityId] = true;
    }
}
