using System;
using System.Collections.Generic;
using UnityEngine;

public class PeopleSystem : MonoBehaviour
{
    public BoxCollider worldBounds;

    public Collider[] peopleAreas;

    private List<People> peopleList = new List<People>();

    private Dictionary<People, Action> disableCallbacks = new Dictionary<People, Action>();

    public GlobalCollidablesManager globalCollidables;

    private void Start()
    {
        People[] foundPeople = GameObject.FindObjectsOfType<People>();
        //peopleList.AddRange(foundPeople);

        foreach (People people in foundPeople)
        {
            AddPeople(people);
        }
    }

    private void Update()
    {
        // Remove destroyed people.
        peopleList.RemoveAll(p => p == null);

        List<ICollidable> collidables = globalCollidables.collidables;

        foreach (People p in peopleList)
        {
            OutBoundsTeleporter teleporter = p.GetComponent<OutBoundsTeleporter>();
            if (teleporter == null)
            {
                teleporter = p.gameObject.AddComponent<OutBoundsTeleporter>();
                teleporter.worldBounds = worldBounds;
            }
            else if (teleporter.worldBounds != worldBounds)
            {
                teleporter.worldBounds = worldBounds;
            }

            p.Move(collidables, peopleAreas);
        }
    }

    public void AddPeople(People people)
    {
        if (people == null)
            return;

        void callback() => OnCarDisableMove(people);
        people.OnDisableMove += callback;
        disableCallbacks[people] = callback;
        peopleList.Add(people);
    }

    private void OnCarDisableMove(People people)
    {
        if (disableCallbacks.ContainsKey(people))
        {
            people.OnDisableMove -= disableCallbacks[people];
            disableCallbacks.Remove(people);
        }

        if (peopleList.Contains(people))
        {
            Debug.Log("People movement disabled: " + people.gameObject.name);
            peopleList.Remove(people);
        }
    }
}
