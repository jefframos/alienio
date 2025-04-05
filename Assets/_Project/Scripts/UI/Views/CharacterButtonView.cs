using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonView : MonoBehaviour
{
    [SerializeField]
    private GameObject highligher;
    public Button button;

    public void Highlight()
    {
        highligher.SetActive(true);
    }

    public void RemoveHighlight()
    {
        highligher.SetActive(false);
    }
}
