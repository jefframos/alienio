using UnityEngine;

public enum CharacterEnum
{
    Character1,
    Character2,
    Character3
}

public class GameSettings : MonoBehaviour
{
    private static GameSettings instance;
    public static GameSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameSettings>();

                if (instance == null)
                {
                    GameObject obj = new("GameSettings");
                    instance = obj.AddComponent<GameSettings>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public CharacterEnum currentCharacter = CharacterEnum.Character2;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    //the ingame hue for the character
    public float GetCharacterHUE()
    {
        switch (currentCharacter)
        {
            case CharacterEnum.Character1:
                return 135f;
            case CharacterEnum.Character2:
                return 0f;
            case CharacterEnum.Character3:
                return 360f;
            default:
                return 0f;
        }
    }
}
