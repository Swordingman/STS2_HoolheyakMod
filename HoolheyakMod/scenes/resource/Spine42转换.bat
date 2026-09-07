@echo off
setlocal

if "%~1"=="" (
    echo 请把 skel 文件拖到这个bat上
    pause
    exit /b
)

set "converter=SpineSkeletonDataConverter.exe"

for %%f in (%*) do (
    echo 正在转换: %%~nxf

    "%converter%" "%%~f" "%%~dpnf42.skel" -v 4.2.43

    echo 输出: %%~dpnf42.skel
)

echo 全部转换完成
pause