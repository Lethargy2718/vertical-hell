using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    private void Update()
    {
        if (transform.position.y < -transform.localScale.y)
        {
            Destroy(gameObject);
        }
    }
}
