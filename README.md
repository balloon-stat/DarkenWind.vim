# DarkenWind.vim
Add visual and sound effects in your vim life on Windows.  

This plugin is inspired by JoelBesada/activate-power-mode.  
JoelBesada/activate-power-mode and mattn/vim-particle are helpful to create this plugin.  
Thanks to JoelBesada and mattn.  


## Usage
```vim
:DarkenWindStart
```

## Requirements
- Windows
- PowerShell
- vimproc

## Commands
```vim
:DarkenWindStart
```

```vim
:DarkenWindStop
```

## Functions
```vim
DarkenWind#start()
DarkenWind#stop()
DarkenWind#background(manual, color)
DarkenWind#fitin()
```

## Variables
```vim
let g:darken_wind_show_frame  = get(g:, 'darken_wind_show_frame',  0)
let g:darken_wind_line_height = get(g:, 'darken_wind_line_height', 20)
let g:darken_wind_font_width  = get(g:, 'darken_wind_font_width',  9)
let g:darken_wind_padding_top = get(g:, 'darken_wind_padding_top', 70)
```
