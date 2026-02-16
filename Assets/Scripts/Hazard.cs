using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log("Hit Player");
        }
    }
}
