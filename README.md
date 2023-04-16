# Scribble
A Beat Saber mod that allows you to draw in the scene

![alt text](https://github.com/ToniMacaroni/Scribble/blob/master/ScribblePreview.png?raw=true)

[Download](https://github.com/ToniMacaroni/Scribble/releases)

**Preview**
https://www.youtube.com/watch?v=e_fAMT1CF_U

## Controls
__**Make sure you don't point at a UI element**__ and start drawing by holding the trigger button.  
Disabling drawing when over a UI element is by design, so you don't accidentally click on the UI
while drawing and still being able to interact with the UI when in drawing mode.  
  
Most of the buttons on the left side are tools that you can toggle.  
Click ones to switch them on, click again to swith them off.

## Configuration
The path to the configuration file is `Beat Saber/UserData/Scribble.json`
```c
bool       VisibleDuringPlay              //Is the drawing visible during gameplay
bool       LoadDrawingsAnimated           //Should the drawings be loaded in with animation
int        ThumbnailSize                  //Resolution of the thumbnails
string     AutoLoadDrawing                //Here you can specify the path to a drawing that will be loaded on startup
float      MoveToolMultiplier             //How much the drawing will move when using the move tool
float      ScaleToolMultiplier            //How much the drawing will scale when using the rotate tool
float      ScaleLineWidthToolMultiplier   //How much the drawing lines will scale when using the scale line width tool
bool       AnimationWaitForStableFPS      //Waits for a stable FPS before rendering the drawing
```

## Brushes
The path to the brushes is `Beat Saber/UserData/Scribble/CustomBrushes.ini`.  
Brushes are in the ini format and seperated by a new line.
A brush can look like this:
```ini
; name of the brush
[LitBrush]
; Color of the brush
Color = #303030
; The texture of the brush
Texture = brush
; The shader (also called effect) of the brush
Effect = Lit
; The size of the brush
Size = 100
; The glow (if the shader supports if) of the brush
Glow = 0.8
; The texture mode of the brush (Stretch, Tile)
TextureMode = Stretch
; The tiling of the brush
Tiling = (1.0, 1.0)
; Additional shader properties of the shader you want to set
_Smoothness = 1
_FogScale = 0.1
```
