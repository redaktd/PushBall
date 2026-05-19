using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class StateController : MonoBehaviour
{
    public ObjectState CurrentState { get; private set; } = ObjectState.Free;

    [SerializeField] private bool canWin = false;
    [SerializeField] public bool canDie = false;
    [SerializeField] private float stateCooldown = 0.001f;
    [SerializeField] private float winTimer = 3f;
    private Rigidbody2D rb;
    private Coroutine _winCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision != null)
        {
            StateController state = collision.gameObject.GetComponent<StateController>();
            if (state != null)
            {
                state.SetState(ObjectState.Free);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.tag == "WinZone")
            {
                if (canWin)
                {
                    _winCoroutine = StartCoroutine(WinTimer(winTimer));
                }
            }

            // Out of bounds logic
            if (collision.gameObject.tag == "OutOfBounds")
            {
                if (canDie)
                {
                    GameManager.Instance.RegisterDeath();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.tag == "WinZone")
            {
                if (canWin && _winCoroutine != null)
                {
                    StopCoroutine(_winCoroutine);
                    _winCoroutine = null;
                }
            }
        }
    }

    public void SetState(ObjectState state)
    {
        if(CurrentState == state) return;

        StartCoroutine(StateCooldown(state));
    }

    private void EnterState(ObjectState state)
    {
        switch (state)
        {
            case ObjectState.Free:
                rb.constraints = RigidbodyConstraints2D.None;
                break;
            case ObjectState.Stuck:
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                break;
            case ObjectState.Disabled:
                rb.simulated = false;
                break;
        }
    }

    private void ExitState(ObjectState state)
    {
        switch (state)
        {
            case ObjectState.Stuck:
                rb.constraints = RigidbodyConstraints2D.None;
                rb.simulated = true;
                break;
            case ObjectState.Disabled:
                rb.constraints = RigidbodyConstraints2D.None;
                rb.simulated = true;
                break;
        }
    }

    private IEnumerator StateCooldown(ObjectState state)
    {
        ExitState(CurrentState);
        yield return new WaitForSeconds(stateCooldown);
        CurrentState = state;
        EnterState(state);
    }

    private IEnumerator WinTimer(float t)
    {
        yield return new WaitForSeconds(t);
        GameManager.Instance.LevelComplete();
    }
}

public enum ObjectState
{
    Free,       //normal physics
    Stuck,      //attached to sticky surface
    Disabled    //cannot be interacted with
}
