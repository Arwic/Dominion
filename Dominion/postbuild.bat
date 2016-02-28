REM arg1 = $(OutDir)
rem GOTO :EOF
echo %1
SET "outdir=%1Content"
SET "pipelinedir=Content\bin\Windows"
SET "otherdir=Content_NonPipeline"
SET "contentzipname=Core"
SET "contentzippath=%outdir%\%contentzipname%"

ECHO Removing output directory
RMDIR %outdir% /s /q
ECHO Creating output directory
MKDIR %outdir%
ECHO Compressing files
"c:\Program Files\7-Zip\7z.exe" a %contentzippath%.zip -ir!.\Content\bin\Windows\*.xnb
"c:\Program Files\7-Zip\7z.exe" a %contentzippath%.zip ".\%otherdir%\*"
REN "%contentzippath%.zip" "%contentzipname%.acp"
SET "%ERRORLEVEL%=0"