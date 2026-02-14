using UnityEngine;

public class MoveUp : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector2.up);
    }
}
