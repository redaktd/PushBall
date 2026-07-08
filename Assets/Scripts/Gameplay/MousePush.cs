using UnityEngine;

public class MousePush : MonoBehaviour
{
    public float forceMultiplier = 1f;
    public float maxForce = 10f;
    public float minDragDistance = 0.2f;

    public LineRenderer dragLine;
    public LineRenderer trajectoryLine;

    public int trajectorySteps = 20;
    public float trajectoryTimeStep = 0.1f;

    public float trajectoryDuration = 2f;   // total time to simulate
    public LayerMask collisionMask;         // what the trajectory can hit
    public float bounceDamping = 0.6f;      // energy kept after bounce (0�1)

    private Rigidbody2D rb;
    private Camera cam;
    private bool dragging = false;
    private Vector2 mousePosStart;
    private Vector2 mousePosEnd;
    //private Vector3 offset;
    //private Vector3 targetPos;

    private StateController stateController;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        stateController = GetComponentInParent<StateController>();
        cam = Camera.main;

        dragLine.enabled = false;
        dragLine.positionCount = 2;
    }

    private void OnMouseDown()
    {
        mousePosStart = cam.ScreenToWorldPoint(Input.mousePosition);
        dragLine.enabled = true;
        trajectoryLine.enabled = true;
        dragging = true;
    }

    private void Update()
    {
        if (!dragging) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = (Vector2)transform.position - mousePos;

        DrawDragLine(mousePos);
        DrawTrajectory(dragVector);
    }

    private void OnMouseUp()
    {
        if (!dragging) return;

        if(stateController != null)
        {
            if(stateController.CurrentState != ObjectState.Free)
                stateController.SetState(ObjectState.Free);
        }

        mousePosEnd = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVector = (Vector2)transform.position - mousePosEnd;

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
        // Match OnMouseUp exactly: clamp to maxForce, then convert the impulse
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

    void SimulateBounce(Vector2 startPos, Vector2 velocity, Vector2 gravity, int startIndex)
    {
        float timeStep = trajectoryDuration / trajectorySteps;

        for (int i = startIndex + 1; i < trajectorySteps; i++)
        {
            float t = (i - startIndex) * timeStep;

            Vector2 pos = startPos + velocity * t + 0.5f * gravity * t * t;

            trajectoryLine.SetPosition(i, pos);
        }
    }

    void StopVisuals()
    {
        dragLine.enabled = false;
        trajectoryLine.enabled = false;
    }
}
