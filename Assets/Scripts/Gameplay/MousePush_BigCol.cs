using UnityEngine;

public class MousePush_BigCol : MonoBehaviour
{
    public float forceMultiplier = 1f;
    public float maxForce = 10f;
    public float minDragDistance = 0.2f;
    public float inputRadius = 1.5f;

    public LineRenderer dragLine;
    public LineRenderer trajectoryLine;

    public int trajectorySteps = 20;
    public float trajectoryDuration = 2f;
    public float bounceDamping = 0.6f;

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
        dragLine.SetPosition(0, transform.position);
        dragLine.SetPosition(1, mousePosition);
    }

    void DrawTrajectory(Vector2 dragVector)
    {
        Vector2 velocity = rb.velocity + dragVector * forceMultiplier;
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

    void StopVisuals()
    {
        dragLine.enabled = false;
        trajectoryLine.enabled = false;
    }
}