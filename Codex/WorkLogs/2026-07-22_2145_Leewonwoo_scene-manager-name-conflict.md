# 씬 매니저 이름 충돌 수정

## 작업 목적

Leewonwoo가 작성한 전역 `SceneManager` 클래스가 Unity 기본 `UnityEngine.SceneManagement.SceneManager`를 가려 발생한 네트워크 코드 컴파일 오류를 해결한다.

## 변경한 파일

- `Assets/Scripts/SceneManager.cs` → `Assets/Scripts/LevelSceneLoader.cs`
- `Assets/Scripts/SceneManager.cs.meta` → `Assets/Scripts/LevelSceneLoader.cs.meta`
- `Codex/WorkLogs/2026-07-22_2145_Leewonwoo_scene-manager-name-conflict.md`

## 핵심 변경 내용

- 커스텀 스크립트 파일과 클래스 이름을 `SceneManager`에서 `LevelSceneLoader`로 변경했다.
- `.meta` 파일을 함께 이동하고 기존 GUID `28224cf292eecda47a7abef700f07647`을 보존해 `Level` 씬의 컴포넌트 참조가 유지되도록 했다.
- Yure가 작성한 네트워크 스크립트는 수정하지 않았다.
- `main`에 직접 커밋하지 않고 `codex/fix-scene-manager-name-conflict` 브랜치에서 수정했다.

## 확인 또는 테스트 결과

- Unity 재컴파일 후 기존 `SceneManager` 이름 충돌 오류 9건이 모두 제거됐다.
- Unity 콘솔 오류 및 경고 0건을 확인했다.
- `LevelSceneLoader.cs` 표준 검증 결과 오류 및 경고 0건이었다.
- 활성 `MainMenuScene` 검증 결과 누락 스크립트와 깨진 프리팹이 모두 0건이었다.
- `Level.unity`가 변경 전과 동일한 스크립트 GUID를 참조하는 것을 확인했다.

## 남은 작업이나 주의 사항

- 실제 버튼을 눌러 `LevelSceneLoader.LoadScene`이 동작하는 플레이 모드 확인은 별도로 필요하다.
- 수정 브랜치는 아직 원격에 푸시하거나 `main`에 병합하지 않았다.
