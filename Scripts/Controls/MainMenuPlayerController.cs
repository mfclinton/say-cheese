using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuPlayerController : MonoBehaviour
{
    public Vector2 PointerScreenPosition { get; private set; }
    private GameplayActions gameplayActions;
    private Camera mainCamera;
    
    // References
    MainMenuManager mainMenuManager;

    private void Awake()
    {
        Debug.Log("MainMenuPlayerController Awake");
        gameplayActions = new GameplayActions();

        gameplayActions.PlayerActions.Position.performed += OnPositionPerformed;
        gameplayActions.PlayerActions.Select.performed += OnSelectStartedPerformed;

        gameplayActions.PlayerActions.Move.performed += OnMovePerformed;
        gameplayActions.PlayerActions.Move.canceled += OnMoveCanceled;
        
        // UIManager uiManager = FindObjectOfType<UIManager>();
        // gameplayActions.PlayerActions.OpenSettings.performed += (InputAction.CallbackContext obj) => uiManager.ToggleSettingsMenu();

        mainCamera = Camera.main;
        
        mainMenuManager = FindObjectOfType<MainMenuManager>();
    }
    
    private void OnEnable()
    {
        gameplayActions.Enable();
    }
    
    private void OnDisable()
    {
        gameplayActions.Disable();
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 movement = ctx.ReadValue<Vector2>();
        HandleMovement(movement);
    }
    
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        Vector2 movement = ctx.ReadValue<Vector2>();
    }
    
    void HandleMovement(Vector2 movement)
    {
        if (movement.x == 0f)
            return;
        
        if (movement.x > 0f)
            mainMenuManager.OnRightArrowClicked();
        else
            mainMenuManager.OnLeftArrowClicked();
    }
    
    private void OnPositionPerformed(InputAction.CallbackContext ctx)
    {
        PointerScreenPosition = ctx.ReadValue<Vector2>();
    }

    private void OnSelectStartedPerformed(InputAction.CallbackContext ctx)
    {
        RaycastFromScreenToPoint(PointerScreenPosition);
    }

    private void RaycastFromScreenToPoint(Vector2 screenPosition)
    {
        int layerMask = LayerMask.GetMask("Clickable");

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Clickable clickable = hit.collider.gameObject.GetComponent<Clickable>();
            if (clickable != null)
            {
                clickable.OnClick();
            }
        }
    }
}