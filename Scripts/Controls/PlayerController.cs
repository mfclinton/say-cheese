using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public delegate void PointerAction(Vector2 position);
    public event PointerAction OnSelectStarted;
    public event PointerAction OnSelectEnded;
    
    // Event for when the player moves
    public delegate void MoveAction(Vector2 movement);
    public event MoveAction OnMove;
    
    public delegate void ZoomAction(float zoom);
    public event ZoomAction OnZoom;

    public Camera UsedCamera;
    public Vector2 PointerScreenPosition { get; private set; }
    public Vector2 PointerWorldPosition => UsedCamera.ScreenToWorldPoint(PointerScreenPosition); // TODO: make better
    
    private GameplayActions gameplayActions;
    
    #region Unity Callbacks

    private void Awake()
    {
        gameplayActions = new GameplayActions();
        
        gameplayActions.PlayerActions.Position.performed += OnPositionPerformed;

        gameplayActions.PlayerActions.Select.performed += OnSelectStartedPerformed;
        gameplayActions.PlayerActions.Select.canceled += OnSelectEndedPerformed;
        
        gameplayActions.PlayerActions.Move.performed += OnMovePerformed;
        gameplayActions.PlayerActions.Move.canceled += OnMoveCanceled;
        
        gameplayActions.PlayerActions.Zoom.performed += OnZoomPerformed;
        UIManager uiManager = FindObjectOfType<UIManager>();
        gameplayActions.PlayerActions.OpenSettings.performed += (InputAction.CallbackContext obj) => uiManager.ToggleSettingsMenu();
        
        UsedCamera = Camera.main;
    }

    private void OnEnable()
    {
        gameplayActions.Enable();
    }

    private void OnDisable()
    {
        gameplayActions.Disable();
    }

    #endregion

    #region Input Readers
    
    void OnPositionPerformed(InputAction.CallbackContext ctx)
    {
        PointerScreenPosition = ctx.ReadValue<Vector2>();
    }
    
    void OnSelectStartedPerformed(InputAction.CallbackContext ctx)
    {
        OnSelectStarted?.Invoke(PointerScreenPosition);
        CheckForClickable();
    }

    void OnSelectEndedPerformed(InputAction.CallbackContext ctx)
    {
        OnSelectEnded?.Invoke(PointerScreenPosition);
    }
    
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.IsGameOver)
            return;
        
        Vector2 movement = ctx.ReadValue<Vector2>();
        OnMove?.Invoke(movement);
    }
    
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(Vector2.zero);
    }
    
    void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.IsGameOver)
            return;
        
        float zoom = ctx.ReadValue<float>();
        OnZoom?.Invoke(zoom);
    }

    #endregion

    public Clickable GetClickable()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return null; // Pointer is over a UI element, so ignore in-game clicks
        }
        
        int layerMask = LayerMask.GetMask("Clickable");

        RaycastHit2D hit = Physics2D.Raycast(PointerWorldPosition, Vector2.zero, Mathf.Infinity, layerMask);
        if (hit.collider != null)
        {
            Clickable clickable = hit.collider.GetComponent<Clickable>();
            if (clickable == null)
                clickable = hit.collider.GetComponentInParent<Clickable>();

            return clickable;
        }

        return null;
    }
    
    private void CheckForClickable()
    {
        Clickable clickable = GetClickable();
        if (clickable != null && clickable.IsClickable)
            clickable.OnClick();
    }

}
