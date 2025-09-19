# Input System Test Setup Guide

This guide explains how to set up and test the newly implemented Generic Input Management System.

## Overview

The Generic Input Management System consists of:
- **InputEventChannels**: ScriptableObject containing all input events
- **InputService**: Bridges PlayerInput and GameEvent system
- **InputDebugger**: Test component for debugging input flow

## Prerequisites

1. Unity Input System package installed
2. PlayerInput component configured with Input Actions
3. ServiceLocator system available in the project

## Setup Instructions

### Step 1: Create InputEventChannels Asset

1. Right-click in Project window
2. Navigate to `Create > asterivo.Unity60 > Input > Input Event Channels`
3. Name it `InputEventChannels`
4. Save it in `Assets/_Project/Core/ScriptableObjects/Input/`

### Step 2: Create Test Scene

1. Create a new scene or use an existing test scene
2. Create an empty GameObject named "InputManager"

### Step 3: Setup InputManager GameObject

1. Add the following components to InputManager:
   - `PlayerInput` component
   - `InputService` component  
   - `InputDebugger` component (for testing)

### Step 4: Configure PlayerInput

1. Create or assign Input Actions asset to PlayerInput
2. Ensure the following actions exist in your Input Actions:
   - Move (Vector2)
   - Look (Vector2) 
   - Jump (Button)
   - Interact (Button)
   - Attack (Button)
   - Run (Button)
   - Crouch (Button)
   - Menu (Button)
   - Inventory (Button)

### Step 5: Configure InputService

1. Assign the InputEventChannels asset to InputService
2. The PlayerInput component should be automatically detected

### Step 6: Configure InputDebugger

1. Assign the same InputEventChannels asset to InputDebugger
2. Enable debug options as needed:
   - Log Movement Events
   - Log Action Events  
   - Log UI Events
   - Show On Screen Debug

## Testing

### Visual Testing

1. Enter Play Mode
2. Look for the on-screen debug panel
3. Test various inputs and verify:
   - Event counters increase
   - Input values update in real-time
   - Service status shows "✓ Registered"

### Console Testing

1. Open the Console window
2. Test inputs while watching for log messages
3. Use InputDebugger context menu "Print Debug Report" for summary

### Expected Behavior

- **Movement**: WASD keys should trigger OnMoveInput events
- **Camera**: Mouse movement should trigger OnLookInput events  
- **Actions**: Space (Jump), E (Interact), Mouse Click (Attack) should log
- **Modifiers**: Shift (Run), Ctrl (Crouch) should toggle states
- **UI**: ESC (Menu), I (Inventory) should trigger UI events

## Verification Checklist

- [ ] InputService registers with ServiceLocator successfully
- [ ] All input events are fired correctly
- [ ] Event counters increment properly
- [ ] Console shows appropriate log messages
- [ ] On-screen debug shows current input states
- [ ] No errors or warnings in console

## Troubleshooting

### Common Issues

**"InputEventChannels not assigned!"**
- Ensure InputEventChannels asset is created and assigned in both InputService and InputDebugger

**"PlayerInput component not found!"**
- Add PlayerInput component to the same GameObject as InputService

**"Input action 'X' not found!"**
- Verify all required actions exist in your Input Actions asset

**"ServiceLocator instance not found!"**
- Ensure ServiceLocator is initialized before InputService starts

**No events firing**
- Check PlayerInput is enabled and has proper Input Actions assigned
- Verify Input Actions are enabled in the Input Actions asset
- Ensure correct action names match between Input Actions and InputService

### Debug Options

1. Enable "Enable Debug Logging" in InputService inspector
2. Enable "Enable Detailed Logging" in InputDebugger inspector  
3. Use "Print Debug Report" context menu for comprehensive status
4. Check "Show Input Debug Info" in InputService for runtime display

## Integration with Features Layer

Once testing is complete, Feature layer components can listen to these events:

```csharp
// Example: Player movement controller listening to input events
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private InputEventChannels inputEvents;
    
    private void Start()
    {
        inputEvents.OnMoveInput.AddListener(OnMoveReceived);
        inputEvents.OnJumpInputPressed.AddListener(OnJumpReceived);
    }
    
    private void OnMoveReceived(Vector2 moveInput)
    {
        // Handle movement
    }
    
    private void OnJumpReceived()
    {
        // Handle jump
    }
}
```

## Next Steps

After successful testing:
1. Implement Step 4: Impact assessment on existing Features layer
2. Migrate existing input handling to use this new system
3. Remove old input handling code
4. Create Feature-specific input handlers that listen to these events

This completes the Core Architecture Refactoring Step 1 implementation and testing setup.