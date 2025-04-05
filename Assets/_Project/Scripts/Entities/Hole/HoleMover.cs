using System.Collections.Generic;
using Player;
using UnityEngine;

public class HoleMover : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private ScreenJoystickMovement ScreenJoystickMovement;

    [SerializeField]
    private HoleController HoleController;

    [SerializeField]
    private List<Collider> worldColliders = new();

    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (ScreenJoystickMovement.IsMoving)
        {
            HoleController.Move(ScreenJoystickMovement.MoveInput);
        }
        else
        {
            HoleController.StopMoving();
        }
    }
}
