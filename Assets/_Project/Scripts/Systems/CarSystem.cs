using System;
using System.Collections.Generic;
using UnityEngine;

public class CarSystem : MonoBehaviour
{
    public BoxCollider worldBounds;

    private readonly List<Car> cars = new();

    private Dictionary<Car, Action> disableCallbacks = new Dictionary<Car, Action>();

    public GlobalCollidablesManager globalCollidables;

    private void Start()
    {
        // Find all cars in the scene.
        Car[] foundCars = GameObject.FindObjectsOfType<Car>();
        foreach (Car car in foundCars)
        {
            AddCar(car);
        }
    }

    private void Update()
    {
        cars.RemoveAll(car => car == null);

        List<ICollidable> collidables = globalCollidables.collidables;

        foreach (Car car in cars)
        {
            OutBoundsTeleporter teleporter = car.GetComponent<OutBoundsTeleporter>();
            if (teleporter == null)
            {
                teleporter = car.gameObject.AddComponent<OutBoundsTeleporter>();
                teleporter.worldBounds = worldBounds;
            }
            else if (teleporter.worldBounds != worldBounds)
            {
                teleporter.worldBounds = worldBounds;
            }

            car.Move(collidables);
        }
    }

    public void AddCar(Car car)
    {
        if (car == null)
            return;

        void callback() => OnCarDisableMove(car);
        car.OnDisableMove += callback;
        disableCallbacks[car] = callback;
        cars.Add(car);
    }

    private void OnCarDisableMove(Car car)
    {
        if (disableCallbacks.ContainsKey(car))
        {
            car.OnDisableMove -= disableCallbacks[car];
            disableCallbacks.Remove(car);
        }

        if (cars.Contains(car))
        {
            cars.Remove(car);
        }
    }
}
