for /r %%I in (*.mgfxo) do del %%I
for /r %%i in (*.fx) do c:\"Program Files (x86)"\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe %%i %%~di%%~pi%%~ni.mgfxo