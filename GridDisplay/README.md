# Grid Display

A 2D grid visualization application built with C# and XNA Framework (MonoGame). Each cell in the grid has three properties: blocked status, height, and alignment.

## Features

- **Interactive 2D Grid**: 20x15 grid with 40px cell size
- **Cell Properties**:
  - **Blocked** (bool): Whether the cell is blocked or passable
  - **Height** (integer 0-9): Elevation level of the cell
  - **Alignment** (integer -5 to 5): Alignment value of the cell
- **Start Cell**: Designated starting position with line-of-sight visualization
- **Real-time Property Display**: Live updates of cell information under mouse cursor

## Visual Elements

### Colors
- **Green Cells**: Normal passable cells (darker green = lower height, brighter green = higher height)
- **Dark Gray Cells**: Blocked cells (impassable)
- **Green Highlight**: Start cell indicator
- **Red Highlight**: Currently selected/hovered cell
- **Cyan Line**: Line of sight from start cell to mouse position
- **Black Grid Lines**: Cell boundaries

### Numbers on Cells
- **White Number (top)**: Height value (0-9)
- **Yellow Number (bottom)**: Alignment value (-5 to 5)

### UI Elements
- **Cell Info Window**: Displays properties of the cell under the mouse cursor
- **Line of Sight**: 4px thick cyan line connecting start cell center to mouse cell center

## Controls

### Mouse Controls
- **Left Click**: Toggle blocked status of the cell
- **Right Click**: Cycle height value (0-9)
- **Middle Click**: Cycle alignment value (-5 to 5)

### Keyboard Controls
- **Arrow Keys**: Move camera view
- **S Key**: Set the currently hovered cell as the new start cell
- **R Key**: Reset grid with random values
- **ESC Key**: Exit application

## Technical Details

- **Framework**: MonoGame (XNA Framework successor)
- **Target**: .NET 8.0
- **Platform**: Desktop (Cross-platform)
- **Resolution**: 1400x800 pixels
- **Grid Size**: 20x15 cells, 40px per cell
- **Custom Rendering**: All text and UI elements use custom pixel-based rendering (no external fonts required)

## Building and Running

```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

## Requirements

- .NET 8.0 or later
- MonoGame Framework (automatically installed via NuGet)

## Project Structure

- `GridCell.cs`: Cell data model with blocked, height, and alignment properties
- `Grid.cs`: Grid management and rendering logic
- `GridGame.cs`: Main game loop, input handling, and UI rendering
- `Program.cs`: Application entry point

## Default Behavior

- Grid initializes with random values (20% chance for blocked cells)
- Start cell begins at position (0,0)
- Camera starts with 50px offset
- Cell properties are randomly generated on startup and grid reset