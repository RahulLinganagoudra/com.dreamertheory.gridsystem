# Changelog

All notable changes to this package will be documented in this file.

## [1.2.0] - 2025-06-29

### Added
- **ObjectBrush**  
  - New lightweight painting tool for placing a single prefab on grid positions.
  - Returns the assigned prefab without rotation, ideal for simple grid decoration workflows.

- **SnapControllerEditor**
  - New Editor utility for grid-based snapping.
  - Adds `DT/Toggle Grid Snapping` menu option (`Ctrl+Alt+S`) to toggle snapping globally.
  - Automatically snaps selected GameObjects to the nearest grid position when moved in Scene view.
  - Uses reflection to detect and support diverse grid implementations:
    - Supports both field and property access to `snap`.
    - Compatible with both `GetWorldPosition(int, int, bool)` and `GetGridPosition(Vector3)` or `GetGridPosition(Vector3, ref int, ref int)` signatures.
  - Integrates with Unity’s Undo system and uses position handles for visual feedback.

- `public bool snap` field added to enable/disable snapping per grid system instance.

### Changed
- **RuleTile Rotation Logic Standardized**
  - All edge, corner, and inverted-corner prefabs now apply correct and consistent `Quaternion` rotations based on Unity’s axis conventions.
  - Fixed left/right confusion for edge and L-corner placements.

- **Improved Edge Diagonal Handling**
  - Corrected assignment of diagonal edge prefabs:
    - `edgePrefabWith1DiagonalL`
    - `edgePrefabWith1DiagonalR`
    - `edgePrefabWith2Diagonal`
  - "L" and "R" now correctly refer to diagonals relative to the tile's edge direction.

- **L-Corner Handling Finalized**
  - All four L-shaped corner cases (with and without diagonals) use the correct prefab and rotation.
  - Improved naming and added clear documentation for each case.

- **Single Edge Rotation Fixed**
  - Single neighbor edge tiles now rotate correctly in all cardinal directions.

- **Codebase Cleanup**
  - Removed unused variables and redundant logic.
  - Ensured all serialized prefab fields are utilized.
  - Refactored for better clarity and maintainability.

### Summary
This release standardizes tile placement and rotation logic, introduces a flexible object brush tool, and adds advanced editor snapping support across grid implementations.

---

