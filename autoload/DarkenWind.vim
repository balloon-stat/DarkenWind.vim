let s:save_cpo = &cpo
set cpo&vim

let s:is_started = 0
let s:path = expand('<sfile>:p:h:h')
let s:has_vimproc = 0
silent! let s:has_vimproc = vimproc#version()

function! DarkenWind#start() abort
  if !s:has_vimproc
    echo 'vimproc is required'
    return
  endif
  if s:is_started
    echo 'DarkenWind is already'
    return
  endif
  let s:is_started = 1

  let boot = s:path . '\darken.ps1'
  let leg  = s:path . '\DarkenWind.cs'
  let cmd  = ['powershell', '-ExecutionPolicy', 'RemoteSigned', '-NoProfile', '-Command']
  let arg  = [boot, leg, v:windowid, g:darken_wind_show_frame]
  let g:darken_wind_proc = vimproc#popen2(cmd + arg)
  call DarkenWind#background(0, 0)

  augroup DarkenWind
    autocmd!
    autocmd TextChanged * call s:shot()
    autocmd TextChangedI * call s:shot()
    autocmd InsertCharPre * let [s:xo, s:yo] = [screencol() - wincol(), screenrow() - winline()]
    autocmd VimLeave * call DarkenWind#stop()
  augroup END
endfunction

function! DarkenWind#stop() abort
  if !s:is_started
    echo 'DarkenWind is down'
    return
  endif
  let s:is_started = 0
  call g:darken_wind_proc.stdin.write("exit\n")
  call g:darken_wind_proc.stdin.close()
  call g:darken_wind_proc.stdout.close()
  augroup DarkenWind
    autocmd!
  augroup END
endfunction

let s:ctb = [
\ '000000', 'aa0000', '00aa00', '0000aa', 'aa5500', 'aa00aa', '00aaaa', 'aaaaaa',
\ '555555', 'ff5555', '55ff55', 'ffff55', '5555ff', 'ff55ff', '55ffff', 'ffffff'
\]
function! s:shot() abort
  let c = synIDattr(synIDtrans(synID(line("."), col(".")-1, 1)), "fg#")
  if c =~ '^#'
    let c = c[1:]
  elseif c =~ '^[0-9]\+$'
    let c = s:ctb[c]
  else
    let c = 'ffffff'
  endif
  let [x, y] = [wincol() + s:xo, winline() + s:yo]
  let [px, py] = [x * g:darken_wind_font_width, y * g:darken_wind_line_height + g:darken_wind_padding_top]
  let target = join(['shot', px, py, str2nr(c, 16), "\n"])
  "echo ['x:', x, s:xo, px, 'y:', y, s:yo, py, c]
  call g:darken_wind_proc.stdin.write(target)
endfunction

function! DarkenWind#background(manual, color) abort
  if !s:is_started
    echo 'DarkenWind is down'
    return
  endif
  if a:manual
    let c = a:color
  else
    let c = synIDattr(hlID('Normal'), "bg#")
  endif

  if c =~ '^#'
    let c = c[1:]
  elseif c =~ '^[0-9]\+$'
    let c = s:ctb[c]
  else
    echo "wrong color code"
    return
  endif
  let cmd = printf("bg %d\n", str2nr(c, 16))
  call g:darken_wind_proc.stdin.write(cmd)
endfunction

function! DarkenWind#fitin() abort
  if !s:is_started
    echo 'DarkenWind is down'
    return
  endif
  let cmd = printf("set %d\n", v:windowid)
  call g:darken_wind_proc.stdin.write(cmd)
endfunction

let &cpo = s:save_cpo
unlet s:save_cpo
