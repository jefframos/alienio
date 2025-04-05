using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalCollidablesManager : MonoBehaviour
{
    [Tooltip("Automatically updated list of all ICollidable objects in the scene.")]
    public List<ICollidable> collidables = new List<ICollidable>();

    private void Update()
    {
        // Each frame, update the list by finding all MonoBehaviours that implement ICollidable.
        collidables = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ICollidable>().ToList();
    }
}
