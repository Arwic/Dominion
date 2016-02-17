REM arg1 = $(OutDir)
SET "outdir=%1Content"
SET "pipelinedir=Content\bin\Windows"
SET "otherdir=Data"
ECHO "Copying other data"
ROBOCOPY %otherdir% "%outdir%" /S /R:0
SET "%ERRORLEVEL%=0"

GOTO:eof
ECHO "Removing output directory"
RMDIR %outdir% /s /q
ECHO "Creating output directory"
MKDIR %outdir%
ECHO "Copying pipeline assets"
ROBOCOPY %pipelinedir% %outdir% /S /R:0
ECHO "Copying other data"
ROBOCOPY %otherdir% "%outdir%" /S /R:0
SET "%ERRORLEVEL%=0"


