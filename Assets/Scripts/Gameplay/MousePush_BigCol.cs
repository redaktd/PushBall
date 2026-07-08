using UnityEngine;

public class MousePush_BigCol : MonoBehaviour
{
    public float forceMultiplier = 1f;
    public float maxForce = 10f;
    public float minDragDistance = 0.2f;
    public float inputRadius = 1.5f;

    public LineRenderer dragLine;
    public LineRenderer trajectoryLine;

    public float trajectoryDuration = 2f;

    private Rigidbody2D rb;
    private Camera cam;
    private bool dragging = false;
    private Vector2 mousePosEnd;
    private StateController stateController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stateController = GetComponent<StateController>();
        cam = Camera.main;

        dragLine.enabled = false;
        dragLine.positionCount = 2;
    }

    private void Update()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // Start drag
        if (Input.GetMouseButtonDown(0) && !dragging)
        {
            float distance = Vector2.Distance(mousePos, rb.position);
            if (distance <= inputRadius)
            {
                dragLine.enabled = true;
                trajectoryLine.enabled = true;
                dragging = true;
            }
        }

        // While dragging
        if (dragging)
        {
            Vector2 dragVector = rb.position - mousePos;
            DrawDragLine(mousePos);
            DrawTrajectory(dragVector);
        }

        // Release
        if (Input.GetMouseButtonUp(0) && dragging)
        {
            if (stateController != null)
            {
                if (stateController.CurrentState != ObjectState.Free)
                    stateController.SetState(ObjectState.Free);
            }

            mousePosEnd = mousePos;
            Vector2 dragVector = rb.position - mousePosEnd;

            if (dragVector.magnitude < minDragDistance)
            {
                StopVisuals();
                dragging = false;
                return;
            }

            dragVector = Vector2.ClampMagnitude(dragVector, maxForce);
            rb.AddForce(dragVector * forceMultiplier, ForceMode2D.Impulse);

            StopVisuals();
            dragging = false;
        }
    }

    void DrawDragLine(Vector2 mousePosition)
    {
        dragLine.startWidth = 0.08f;
        dragLine.endWidth = 0.02f;

        // Clamp the visible aim line to maxForce so it reflects the real power cap.
        Vector2 ballPos = transform.position;
        Vector2 pull = Vector2.ClampMagnitude(mousePosition - ballPos, maxForce);

        dragLine.SetPosition(0, ballPos);
        dragLine.SetPosition(1, ballPos + pull);
    }

    void DrawTrajectory(Vector2 dragVector)
    {
        // Match the launch exactly: clamp to maxForce, then convert the impulse
        // to a velocity change by dividing by mass.
        Vector2 clampedDrag = Vector2.ClampMagnitude(dragVector, maxForce);
        Vector2 impulse = clampedDrag * forceMultiplier;
        Vector2 velocity = rb.velocity + impulse / rb.mass;
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;
        Vector2 position = rb.position;

        // Preview length scales with pull strength: a small pull shows a short
        // line, growing to the full trajectoryDuration at maxForce.
        float forceRatio = maxForce > 0f ? clampedDrag.magnitude / maxForce : 0f;

        // Step at the physics tick rate so the preview matches the real ball's
        // gravity AND linear drag. Unity applies linear drag as v *= 1/(1 + drag*dt).
        float dt = Time.fixedDeltaTime;
        int maxSteps = Mathf.Max(2, Mathf.CeilToInt(trajectoryDuration / dt));
        int steps = Mathf.Max(2, Mathf.CeilToInt(maxSteps * forceRatio));
        float dragFactor = 1f / (1f + rb.drag * dt);

        trajectoryLine.positionCount = steps;
        trajectoryLine.SetPosition(0, position);

        for (int i = 1; i < steps; i++)
        {
            velocity += gravity * dt;   // gravity
            velocity *= dragFactor;     // linear drag (no-op when Linear Drag = 0)
            position += velocity * dt;  // integrate
            trajectoryLine.SetPosition(i, position);
        }
    }

    void StopVisuals()
    {
        dragLine.enabled = false;
        trajectoryLine.enabled = false;
    }
}