# Dominion

4X (eXplore, eXpand, eXploit, and eXterminate) grand stratergy game based on Civilization 5

##Requirements:
- C# 6.0
- .Net framework 4.6
- MonoGame 3.4 (DX11)
- Windows 7, 8.1, 10

##ArwicEngine
The games's engine, built ontop of MonoGame 3.4.
Features:
- Full winforms-like event based GUI for MonoGame
  - Canvas
  - Form
  - Button
  - Check box
  - Images
  - Label
  - Progress bar
  - Scroll box
  - Spin button
  - Text box (Full windows-like functionality, text selection, advanced cursor control, key repeat, etc.)
  - Text log
  - Tool tip
- Rich text (Supports multiple fonts, colours, sizes)
- Audio wrappper and juke box (MonoGame's is broken)
- Custom content pack system. Useful for individual levels, mods, etc.
- 2D Camera
- Win32 cursor API wrapper
- 2D Sprites
- 2D Sprite animations
- Sprite Atlases
- Application level packet based TCP client/server wrapper
- DNS resolver
- Ping utility
- Console variables, config script run on startup can set user settings

##ArwicInterfaceDesigner
Simple interface designer for ArwicEngine.Forms
- GUI editor
- Most properties editable

##ArwicXmlEditor
Edits xml files (must specify types at compile time).  
Used for unit/tile/empire editing, useful for modding.

##Dominion
The game itself.
- Client/server based
- Single player games simply start local server
- Full GUI config
- Full GUI lobby
- Procedural map generation
- A* unit pathing
- Unit combat
- Unit control (Full GUI)
- Cities
  - Population growth based on food and current population
  - Income based on buidlings and improvments
  - Border expansion based on culture and current border size
  - Building production
  - Unit production
- Tech tree (Full GUI)
- Social policy (Full GUI)
- Fog of war (currently insecure as client stills knows everything)
- Turn timer
- Minimap (Full GUI)
- Tile improvments
- Tile data texts
