# DT GridSystem RuleTile (3D)

[![Unity](https://img.shields.io/badge/Unity-2021%2B-black?logo=unity)](https://unity.com/)  
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)  
[![Editor-Tools](https://img.shields.io/badge/Editor%20Tools-Yes-success)](#)  
[![Platform](https://img.shields.io/badge/Platform-All-lightgrey)](#)  

> **A lightweight, editor-friendly 3D RuleTile system for Unity**  
> Procedural tile authoring based on neighbor rules, with SceneView editing, shortcuts, and full Undo/Redo.

---

## 📖 Overview

This package allows you to **automatically place and rotate tiles** in a 3D grid based on their neighbors — similar to Unity’s RuleTile, but in 3D.

**Features:**
- 🖱️ **SceneView tools**: click/drag select, box select  
- ⌨ **Menu shortcuts** for quick actions  
- ♻ **Undo/Redo support**  
- ⚡ **Optimized generation** – only updates affected cells  

---

## ⚙️ Setup

### 1. Import Scripts
``` text : 
RuleTile.cs // ScriptableObject`  
RuleTileManager.cs // MonoBehaviour; requires GridSystem3D base 

(Editor-only)
RuleTileManagerEditor.cs // Custom Inspector & menu items 
``` 

---

### 2. Create a RuleTile Asset
1. **Project Window** → `Create → DT → GridSystem → RuleTile`
2. Assign all prefab slots in the Inspector *(see [Prefab Authoring Guide](#prefab-authoring-guide))*.

---

### 3. Configure RuleTileManager
1. Add **RuleTileManager** to a GameObject in your scene.
2. Assign the RuleTile asset to `ruleTile`.    
3. _(Optional)_ Set a Container Transform — defaults to manager’s transform.   
---

### 4. Enter Edit Mode

- Toggle via **Inspector button** or `DT → Toggle Editing (Alt+T)`
    
- Editor tools are disabled at runtime (`UNITY_EDITOR` guarded).
    

---

## 🛠 Editor Workflow

| Action                  | Shortcut             | Effect |
|-------------------------|----------------------|--------|
| **Toggle Editing**      | Alt+T                | Enable/disable editing mode in Inspector |
| **Select Cell**         | Click                | Select a single cell (clears previous selection) |
| **Toggle Cell Selection** | Ctrl/Cmd + Click    | Add/remove a single cell from the selection |
| **Box Select**          | Shift + Drag         | Select multiple cells (small drags ignored) |
| **Generate Tiles**      | Alt+G                | Generate/update tiles for selected cells and their 8-neighbor ring |
| **Clear Selection**     | Alt+S                | Deselect all cells |
| **Delete Selected**     | Alt+D                | Remove tiles for selected cells and update neighbors |
| **Destroy All**         | Ctrl/Cmd + Alt + D   | Remove every tile in the grid |

---

## Prefab Authoring Guide

**Grid Plane:** XZ _(Y is up)_

**Orientation Rule:**

- "Forward (Z+) should face up in grid"

- No rotation → Forward aligns with grid +Z
    
- Y-axis rotations applied to match neighbors
    

---

### Prefab Slots
- `singlePrefab`
- `centerPrefab` 
- `edgePrefab` 
- `edgeWith2SidePrefab`
- `edgePrefabWith1DiagonalL/R` 
- `edgePrefabWith2Diagonal`
- `cornerPrefab` 
- `cornerPrefabWithCorner` 
- `invertedCornerPrefab` 
- `invertedTopPrefab`
- `invertedTopThreePrefab`
- `crossPrefab`
- `slashPrefab`
- `singleEdgePrefab`

---

### Testing Tips

- Start with **small grid shapes** _(isolated tile, straight edge, corner)_
    
- Verify **rotations and pivot alignment**
    
- Missing prefabs are skipped _(no tile placed)_
    

---

## 💡 Tips & Gotchas
- 📦 **Container Transform**: Use a child object for cleanliness
    
- 🔄 **Undo/Redo**: Use provided buttons/menu items to keep data synced
    
- ▶ **Play Mode**: Editor tools disabled, but tile creation works
    
- 🖱 **Box-Select Threshold**: Small drags (<5px) are ignored
    
---

## 📜 License

MIT License — feel free to use in commercial and non-commercial projects.