using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSplashScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image instructionsSplashScreen;
    [SerializeField] private Button prevButton, nextButton;
    [SerializeField] private Sprite[] instructionSprites;
    
    // Next Button Variables
    private TextMeshProUGUI nextButtonText;
    private string nextButtonDefaultText;
    private string nextButtonCloseText = "Done!";
    
    // Internal Variables
    private int currentInstructionIndex = 0;

    private void Awake()
    {
        // Initialize Next Button Variables
        nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        nextButtonDefaultText = nextButtonText.text;
        
        // Subscribe to Button Events
        prevButton.onClick.AddListener(OnPrevButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        
        // Update Instructions
        UpdateInstructions();
    }

    public void OnNextButtonClicked()
    {
        UpdateCurrentInstructionIndex(1);
    }
    
    public void OnPrevButtonClicked()
    {
        UpdateCurrentInstructionIndex(-1);
    }
    
    #region Helpers
    
    private void UpdateInstructions()
    {
        if (instructionSprites.Length <= currentInstructionIndex)
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            uiManager.SetInstructionsSplashScreen(false);
            return;
        }
        
        instructionsSplashScreen.sprite = instructionSprites[currentInstructionIndex];
        
        bool isFirstInstruction = currentInstructionIndex == 0;
        bool isLastInstruction = currentInstructionIndex == instructionSprites.Length - 1;
        
        prevButton.interactable = !isFirstInstruction;
        nextButtonText.text = isLastInstruction ? nextButtonCloseText : nextButtonDefaultText;
    }
    
    private void UpdateCurrentInstructionIndex(int value)
    {
        currentInstructionIndex += value;
        UpdateInstructions();
    }
    
    #endregion
}
