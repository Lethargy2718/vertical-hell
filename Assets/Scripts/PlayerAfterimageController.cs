using UnityEngine;

public class PlayerAfterimageController : MonoBehaviour
{
    private PlayerController _playerController;
    private Afterimage _afterimage;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
        _afterimage = GetComponent<Afterimage>();
    }

    private void OnEnable()
    {
        _playerController.Dashed += OnDashed;
        _playerController.DashEnded += OnDashEnded;
    }

    private void OnDisable()
    {
        _playerController.Dashed -= OnDashed;
        _playerController.DashEnded -= OnDashEnded;
    }

    private void OnDashed() => _afterimage.StartAfterimages();
    private void OnDashEnded() => _afterimage.WaitThenStopAfterImages();
}