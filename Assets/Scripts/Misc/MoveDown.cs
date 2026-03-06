using UnityEngine;

public class MoveDown : MonoBehaviour
{
    [SerializeField] private float _speed = 1.0f;
    public float Speed
    {
        get => _speed;

        set
        {
            _speed = Mathf.Max(0, value);
        }
    }

    private void Update()
    {
        transform.position += Speed * Time.deltaTime * Vector3.down;
    }
}
