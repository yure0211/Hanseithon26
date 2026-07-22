# 비동기 씬 매니저 구현

## 작업 목적

사용자가 제공한 참고 이미지와 동일한 흐름으로 최소 로딩 시간과 진행률 표시를 지원하는 비동기 씬 전환 기능을 완성한다.

## 변경한 파일

- `Assets/Scripts/SceneManager.cs`
- `Assets/Scripts/SceneManager.cs.meta`
- `Codex/WorkLogs/2026-07-22_2059_Leewonwoo_scene-manager.md`

## 핵심 변경 내용

- 기존 `LevelPanel`과 `LoadPanel` 필드를 유지하면서 로딩 시작 시 패널을 전환하도록 구현했다.
- uGUI `Slider`를 로딩 바로 받아 비동기 씬 로딩 진행률을 표시하도록 구현했다.
- 실제 씬 로딩 진행률과 최소 로딩 시간 0.5초의 진행률 중 작은 값을 화면에 반영하도록 구현했다.
- 실제 로딩과 최소 시간이 모두 완료된 뒤 씬 활성화를 허용해 무한 대기하지 않도록 구현했다.
- 중복 씬 로딩 요청을 막고, 프로젝트의 `SceneManager` 클래스와 Unity API의 이름 충돌을 별칭으로 해결했다.

## 확인 또는 테스트 결과

- Unity MCP 스크립트 표준 검증: 오류 0건, 경고 0건.
- Unity 컴파일 완료 후 콘솔 확인: 오류 0건, 경고 0건.
- Unity 6000.3.15f1의 `LoadSceneAsync`, `AsyncOperation.allowSceneActivation`, `Slider.value` API 존재를 리플렉션으로 확인했다.

## 남은 작업이나 주의 사항

- `Level` 씬의 `SceneManager` 컴포넌트에서 `levelPanel`, `loadPanel`, `loadingBar` 참조가 아직 연결되지 않았다.
- `LoadScene`에 전달할 대상 씬은 실행 전에 Build Settings에 추가해야 한다.
- 기존 `Level` 씬과 ProjectSettings를 포함한 사용자 변경은 이번 작업에서 수정하거나 커밋하지 않는다.
