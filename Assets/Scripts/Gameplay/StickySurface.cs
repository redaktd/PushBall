using UnityEngine;

public class StickySurface : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            Debug.Log("collided: " + collision.gameObject.name);

            if (collision.gameObject.layer != 6 || collision.gameObject.layer != 7)
            {
                StateController state = collision.gameObject.GetComponent<StateController>();
                if (state != null)
                {
                    state.SetState(ObjectState.Stuck);
                }
            }
        }
    }
}