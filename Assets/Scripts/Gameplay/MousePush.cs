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
    public float bounceDamping = 0.6f;      // energy kept after bounce (0–1)

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

        dragLine.SetPosition(0, transform.position);
        dragLine.SetPosition(1, mousePosition);
    }

    void DrawTrajectory(Vector2 dragVector)
    {
        Vector2 velocity = rb.velocity + (dragVector) * forceMultiplier;
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;

        Vector2 position = rb.position;

        float timeStep = trajectoryDuration / trajectorySteps;

        trajectoryLine.positionCount = trajectorySteps;

        for (int i = 0; i < trajectorySteps; i++)
        {
            float t = i * timeStep;

            Vector2 nextPos = position + velocity * t + 0.5f * gravity * t * t;

            trajectoryLine.SetPosition(i, nextPos);
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
