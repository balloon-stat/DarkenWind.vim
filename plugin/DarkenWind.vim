if exists('g:loaded_darken_wind')
  finish
endif

let s:save_cpo = &cpo
set cpo&vim

command! -nargs=0 DarkenWindStart call DarkenWind#start()
command! -nargs=0 DarkenWindStop  call DarkenWind#stop()
let g:loaded_darken_wind = 1

let g:darken_wind_show_frame  = get(g:, 'darken_wind_show_frame',  0)
let g:darken_wind_line_height = get(g:, 'darken_wind_line_height', 20)
let g:darken_wind_font_width  = get(g:, 'darken_wind_font_width',  9)
let g:darken_wind_padding_top = get(g:, 'darken_wind_padding_top', 70)

let &cpo = s:save_cpo
unlet s:save_cpo
