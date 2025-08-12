# DT GridSystem RuleTile (3D) — Core Classes & Structures

[![Unity](https://img.shields.io/badge/Unity-2021%2B-black?logo=unity)](https://unity.com/)  
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)  
[![Platform](https://img.shields.io/badge/Platform-All-lightgrey)](#)  

> **Technical reference** for the main classes, data structures, and editor integrations powering the DT GridSystem 3D RuleTile workflow.

---

## 📦 Core ScriptableObjects

### **`BaseRuleTile`**
``` cs :
public abstract class BaseRuleTile : ScriptableObject
{
    public abstract RuleTilePrefabResult GetPrefabForPosition(
        int x, int y,
        Dictionary<Vector2Int, GameObject> placedTiles,
        HashSet<Vector2Int> selectedCells);
}
```
- Purpose: Defines the interface for all rule tile assets.

- GetPrefabForPosition → Returns the prefab and rotation based on neighbor occupancy.

RuleTilePrefabResult

``` cs :

public struct RuleTilePrefabResult
{
    public GameObject prefab;
    public Quaternion rotation;

    public RuleTilePrefabResult(GameObject prefab, Quaternion rotation) { … }
}
```
- Purpose: Encapsulates the prefab and rotation for placement in the grid.
--- 
## RuleTile (inherits BaseRuleTile)
Exposes multiple GameObject fields for each neighborhood pattern.

Implements complex neighbor-check logic (8 directions + diagonals).

Selects the appropriate prefab and rotation for placement.

---

## 🖥 Public API Methods

| Method                       | Returns | Description                                |
| ---------------------------- | ------- | ------------------------------------------ |
| `bool IsEditing()`           | `bool`  | Returns whether edit mode is active        |
| `void ToggleEditing()`       | —       | Toggles edit mode                          |
| `void ClearSelection()`      | —       | Clears the current cell selection          |
| `void SelectAllCells()`      | —       | Selects every cell in the grid             |
| `void GenerateGrid()`        | —       | Generates tiles for the selected cells     |
| `void DeleteSelectedTiles()` | —       | Deletes tiles in the selection             |
| `void DeleteAllChildren()`   | —       | Clears all placed tiles from the container |
