@echo off
REM ============================================================================
REM SceneSetup.bat - Unity CLI로 씬 자동 설정
REM ============================================================================

echo [SceneSetup] Unity 씬 설정 시작...

REM Unity 경로 (필요시 수정)
SET UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.XX\Editor\Unity.exe"

REM 프로젝트 경로
SET PROJECT_PATH=%~dp0GameofDestiny

REM 실행
%UNITY_PATH% -batchmode -projectPath "%PROJECT_PATH%" -executeMethod SceneSetup.SetupScene -quit

echo [SceneSetup] 완료! GameofDestiny/Assets/Scenes/GameScene.unity 확인
pause
