@echo off

:: ----------------------
:: KUDU Deployment Script
:: ----------------------

:: Setup
:: -----

setlocal enabledelayedexpansion

SET ARTIFACTS=%~dp0%artifacts

IF NOT DEFINED DEPLOYMENT_SOURCE (
  SET DEPLOYMENT_SOURCE=%~dp0%.
)

IF NOT DEFINED DEPLOYMENT_TARGET (
  SET DEPLOYMENT_TARGET=%ARTIFACTS%\wwwroot
)

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Deployment
:: ----------

:: 3. Build Snow Site
echo -----
echo Start - Building the Snow Site
echo Running Sandra.Snow.PreCompiler.exe config=%DEPLOYMENT_SOURCE%
pushd %DEPLOYMENT_SOURCE%
call  %DEPLOYMENT_SOURCE%\Sandra.Snow.Processor\Sandra.Snow.PreCompiler.exe config=%DEPLOYMENT_SOURCE%\
IF !ERRORLEVEL! NEQ 0 goto error
echo Finish - Building the Snow Site
echo -----


IF NOT DEFINED NEXT_MANIFEST_PATH (
  SET NEXT_MANIFEST_PATH=%ARTIFACTS%\manifest

  IF NOT DEFINED PREVIOUS_MANIFEST_PATH (
    SET PREVIOUS_MANIFEST_PATH=%ARTIFACTS%\manifest
  )
)

IF NOT DEFINED KUDU_SYNC_COMMAND (
  :: Install kudu sync
  echo Installing Kudu Sync
  call npm install kudusync -g --silent
  IF !ERRORLEVEL! NEQ 0 goto error

  :: Locally just running "kuduSync" would also work
  SET KUDU_SYNC_COMMAND=node "%appdata%\npm\node_modules\kuduSync\bin\kuduSync"
)


echo Kudu Sync from "%DEPLOYMENT_SOURCE%\Website" to "%DEPLOYMENT_TARGET%"
call %KUDU_SYNC_COMMAND% -q -f "%DEPLOYMENT_SOURCE%\Website" -t "%DEPLOYMENT_TARGET%" -n "%NEXT_MANIFEST_PATH%" -p "%PREVIOUS_MANIFEST_PATH%" -i ".git;.deployment;deploy.cmd" 2>nul
IF !ERRORLEVEL! NEQ 0 goto error

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

goto end

:error
echo An error has occured during web site deployment.
exit /b 1

:end
echo Finished successfully.
