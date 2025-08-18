using UnityEngine;

/// <summary>
/// Old Input system that can be used for other components to read abstracted input data.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool Attacked { get; private set; }
    
    private void Update()
    {
        GetInputValues();
    }

    private void GetInputValues()
    {
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        JumpPressed = Input.GetKeyDown(KeyCode.Space);
        JumpReleased = Input.GetKeyUp(KeyCode.Space);
        Attacked = Input.GetKeyDown(KeyCode.J);
    }

    /// <summary>
    /// Optional helper to "consume" one-time inputs (so other scripts don't re-trigger them)
    /// </summary>
    public void ConsumeJumpInputs()
    {
        JumpPressed = false;
        JumpReleased = false;
    }
}