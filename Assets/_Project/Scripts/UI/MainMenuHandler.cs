using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterViewItem
{
    public CharacterButtonView characterView;
    public CharacterEnum character;
}

public class MainMenuHandler : MonoBehaviour
{
    public List<CharacterViewItem> characterButtonItems = new List<CharacterViewItem>();

    public Button startGameButton;

    public Color selectedColor = Color.green;

    public Color normalColor = Color.white;

    private void Start()
    {
        foreach (var item in characterButtonItems)
        {
            if (item.characterView != null)
            {
                item.characterView.button.onClick.AddListener(
                    () => OnCharacterButtonClicked(item, item.character)
                );
            }
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(RedirectToGame);
        }

        UpdateButtonHighlights();
    }

    private void OnCharacterButtonClicked(CharacterViewItem clickedItem, CharacterEnum character)
    {
        GameSettings.Instance.currentCharacter = character;
        UpdateButtonHighlights();
    }

    private void UpdateButtonHighlights()
    {
        foreach (var item in characterButtonItems)
        {
            bool isSelected = GameSettings.Instance.currentCharacter == item.character;
            if (isSelected)
            {
                item.characterView.Highlight();
            }
            else
            {
                item.characterView.RemoveHighlight();
            }
        }
    }

    public void RedirectToGame()
    {
        _ = TransitionManager.Instance.TransitionToSceneAsync("MainScene");
    }
}
