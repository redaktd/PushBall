using UnityEngine;

public class KillSurface : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var stateController = collision.gameObject.GetComponent<StateController>();
        if (stateController != null && stateController.canDie)
        {
            GameManager.Instance.RegisterDeath();
        }
    }
}