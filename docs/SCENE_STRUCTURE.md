# Scene Structure: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**. Tài liệu này mô tả cấu trúc scene **mong muốn/được docs cũ ghi nhận**, kèm ghi chú những điểm cần mở Unity để xác minh. Source C# compile sạch không đảm bảo scene references đã đúng.

---

## 1. Main Scene Target

- Scene demo mục tiêu: `Assets/Scenes/Village_Demo.unity`.
- `SampleScene.unity` nếu còn tồn tại thì chỉ nên dùng làm sandbox.
- Trước khi sửa scene, mở Unity và xác nhận Build Settings / active scene.

---

## 2. Expected Root Layout

```text
Village_Demo.unity
├── _Managers
├── _UI
├── Player
├── Main Camera
├── Lighting
├── Environment
├── FarmingArea
├── NPCs
├── InteractionZones
├── DisasterObjects
└── Audio
```

Nếu hierarchy thực tế khác, ưu tiên scene trong Unity hơn tài liệu này.

---

## 3. `_Managers`

Expected manager components:

| Component | Script | Current source status |
|---|---|---|
| GameManager | `SownInStone.Core.GameManager` | Required. Day/time/phase events. |
| PlayerStats | `SownInStone.Core.PlayerStats` | Required. Health/stamina/morale/coins. |
| WeatherManager | `SownInStone.Weather.WeatherManager` | Required. Weather/flood stats. Needs `waterPlanePrefab`/`disasterContainer` if using water plane. |
| StorageManager | `SownInStone.Storage.StorageManager` | Required. Runtime inventory. |
| CommunityManager | `SownInStone.Community.CommunityManager` | Required for Nghĩa Tình/NPC events. |
| AudioManager | `SownInStone.Audio.AudioManager` | Optional but many scripts call it with null-safe `?.`. |
| TutorialManager | `SownInStone.UI.TutorialManager` | Required if tutorial gate is used. |
| EndingManager | `SownInStone.UI.EndingManager` | Required if ending panel is used. |

---

## 4. `_UI` / Canvas

Expected `SurvivalUIManager` references:

- `mainUICanvasGroup`
- survival sliders: health, stamina, morale
- text fields: day/phase/weather/time as applicable
- `dialoguePanel`, `speakerNameText`, `dialogueContentText`
- `inventoryPanel`, `itemSlotContainer`, `itemSlotPrefab`
- optional `villageSpeakerBanner`
- item data fields for shop/crafting if not auto-loaded in Editor

Runtime-generated/managed UI in source:

- `ResourcePanel`
- `TimeSeasonPanel`
- `NghiaTinhPanel` positioning if `NghiaTinhUI` exists
- `InteractionPrompt`
- `HUDToastNotification`
- `ControlsLegendHUD`
- `Panel_Community`
- `Panel_WeatherDetails`
- `Panel_Shop`
- `ChoiceContainer`
- phase announcement panel

Important partials:

- `StartEvacuationCountdown()` is currently toast-only.
- `ShowDrownGameOverPanel()` is currently toast-only.
- Tutorial quest panel text is routed through `SetInteractionPrompt()` in current compatibility implementation.

---

## 5. Player

Expected:

```text
Player
└── visual/model child
```

Components expected:

- `Rigidbody`
- collider
- `PlayerController`

Critical verification:

- Current `PlayerController.cs` source does **not** contain a full movement loop, so Unity Play Mode must verify whether another component or an older serialized script handles movement. If not, player will not move until movement is restored.

---

## 6. Environment / Farming

Expected groups:

```text
Environment
├── Ground_Main
├── _Environment/Houses
├── _Environment/NPCs or NPCs root
├── Village_Well
├── Village_Speaker
└── props/fences/rocks/plants

FarmingArea
├── SoilCell parent/helper
└── SoilCell_Grid1 ... SoilCell_Grid12
```

Source assumptions:

- `SoilCell.Awake()` auto-links parent to grid children by names containing `Grid` and XZ distance `< 6.0f`.
- Each gameplay soil cell should have `SoilCell` and collider trigger.
- 3D visuals are optional references on `SoilCell`: rocky/clean/tilled/wet.

Verify in Unity:

- There are actually 12 active grid cells.
- Colliders are triggers and reachable by player interaction.
- `CropData` assets are assigned wherever planting is initiated.

---

## 7. NPCs

Expected core NPCs:

- `NPC_OTham`
- `NPC_BacNam`
- `NPC_CuBay`
- `NPC_BeTi`

Each should have:

- `NPCCharacter`
- collider trigger or collider detectable by proximity UI
- correct `characterType`

Source supports all four names in `NPCCharacter.StoryCharacterType`, but scene placement and collider references must be verified in Unity.

---

## 8. InteractionZones / Props

Expected interactables:

- `Shrine` / altar with `AncestralAltar`
- `bep_gas` with `KitchenHearth`
- `Coracle` with `Coracle`
- `MudPuddle` prefabs/instances with `MudPuddle`
- flood protection prefabs with `FloodBarrier`

Current code notes:

- `AncestralAltar` triggers storm if pre-storm and incense exists.
- `KitchenHearth` implements singular namespace `SownInStone.Interaction.IInteractable`.
- `FloodBarrier` is a radius marker; full node/placement systems are not complete beyond inventory prefab spawning.

---

## 9. DisasterObjects

Expected:

- Empty parent transform for water plane.
- `WeatherManager` can instantiate `waterPlanePrefab` at current `floodLevel` if references are set.

Verify:

- `waterPlanePrefab` is assigned.
- water plane material/scale is correct.
- no collider blocks player after spawn; source disables `MeshCollider` if present.

---

## 10. Scene Validation Checklist

After opening Unity:

- [ ] Console has no compile errors.
- [ ] Main scene opens without missing scripts.
- [ ] `_Managers` has required singletons.
- [ ] Player can move.
- [ ] Camera follows player.
- [ ] Inventory opens with `I/Tab`.
- [ ] NPC proximity UI appears near NPC.
- [ ] Altar can trigger `MuaBao`.
- [ ] Weather/flood values update after phase transition.
- [ ] Soil cells can be targeted and changed by gameplay.
