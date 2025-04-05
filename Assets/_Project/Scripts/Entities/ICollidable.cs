using System;
using UnityEngine;

public interface ICollidable
{
    event Action OnDisableMove;
    Collider GetCollider();
    void DisableMovement();
}
