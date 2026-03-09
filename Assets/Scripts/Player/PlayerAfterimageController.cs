using UnityEngine;

public class PlayerAfterimageController : MonoBehaviour
{
    private PlayerStateDriver _playerController;
    private Afterimage _afterimage;

    private int afterImageUsers;
    private int AfterImageUsers
    {
        get => afterImageUsers;
        set
        {
            afterImageUsers = Mathf.Max(0, value);
        }
    }

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerStateDriver>();
        _afterimage = GetComponent<Afterimage>();
    }

    private void OnEnable()
    {
        _playerController.ctx.Dashed += AddAfterimageUser;
        _playerController.ctx.DashEnded += RemoveAfterimageUser;
        _playerController.ctx.FlyStarted += AddAfterimageUser;
        _playerController.ctx.FlyEnded += RemoveAfterimageUser;
        _playerController.ctx.GroundSlamStarted += AddAfterimageUser;
        _playerController.ctx.GroundSlamEnded += RemoveAfterimageUser;
    }

    private void OnDisable()
    {
        _playerController.ctx.Dashed -= AddAfterimageUser;
        _playerController.ctx.DashEnded -= RemoveAfterimageUser;
        _playerController.ctx.FlyStarted -= AddAfterimageUser;
        _playerController.ctx.FlyEnded -= RemoveAfterimageUser;
        _playerController.ctx.GroundSlamStarted -= AddAfterimageUser;
        _playerController.ctx.GroundSlamEnded -= RemoveAfterimageUser;
    }

    private void AddAfterimageUser()
    {
        AfterImageUsers++;
        if (AfterImageUsers == 1)
        {
            _afterimage.StartAfterimages();
        }
    }

    private void RemoveAfterimageUser()
    {
        AfterImageUsers--;
        if (AfterImageUsers == 0)
        {
            _afterimage.WaitThenStopAfterimages();
        }
    }
}