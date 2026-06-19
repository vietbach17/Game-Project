# Input Mapping & Customization

The game supports fully customizable controls. Keybindings can be adjusted through the **Settings Menu** (accessible via the gear button on the bottom-right HUD or the **`Esc`** key) or the Main Menu settings tab. 

Settings are stored persistently across play sessions using `PlayerPrefs`.

## Default Keybindings

* **Move Up**: `W` (Customizable)
* **Move Down**: `S` (Customizable)
* **Move Left**: `A` (Customizable)
* **Move Right**: `D` (Customizable)
* **Interact / Action**: `E` (Customizable)
* **Run**: `LeftShift` (Customizable)
* **Rotate Camera**: Hold `Right Mouse Button` and drag
* **Toggle Inventory**: `I` or `Tab`
* **Toggle Weather details**: `M`
* **Toggle Community/Affection panel**: `C`
* **Toggle Settings Menu**: `Esc` or click the HUD Gear Button
* **Toggle Developer Debug Menu**: `F1`

## Input Backend System
The system is built to support both **Unity Legacy Input Manager** and the **New Input System** automatically. Action prompts and interaction hover labels dynamically update to display the customized key name.
