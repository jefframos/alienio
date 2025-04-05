@echo off
setlocal enabledelayedexpansion

REM Check if .git folder exists, if not initialize a new Git repo.
if not exist ".git" (
    echo Initializing new Git repository...
    git init
)

REM Install Git LFS.
echo Installing Git LFS...
git lfs install

REM Download Unity .gitignore if it doesn't exist.
if not exist ".gitignore" (
    echo Downloading Unity .gitignore...
    curl -o .gitignore https://raw.githubusercontent.com/github/gitignore/main/Unity.gitignore
)

REM Stage all files.
echo Staging files...
git add .

REM Check if any commits exist by counting the commits.
for /f "tokens=*" %%i in ('git rev-list --count HEAD 2^>nul') do set commitCount=%%i
if "%commitCount%"=="0" (
    echo Creating initial commit...
    git commit -m "Initial commit"
)

REM Check if remote "origin" exists.
git remote | findstr /i "origin" >nul
if errorlevel 1 (
    echo Creating remote repository 'hole-unity' on GitHub...
    gh repo create hole-unity --source=. --public --push
)

echo Repository setup complete.
pause
