# 연결 완료 후 레벨 씬 전환 작업

## 작업 목적

Host와 Client가 로비에서 모두 연결된 직후 `InGame`으로 바로 이동하지 않고, 연결을 유지한 채 `Level` 씬으로 함께 이동하도록 네트워크 씬 흐름을 변경한다.

## 변경한 파일

- `Assets/Scripts/DualPlaySample/DualPlayConnectionSettings.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkLauncher.cs`
- `Assets/Scripts/DualPlaySample/DualPlayMainMenu.cs`
- `Assets/Scripts/LevelSceneLoader.cs`
- `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Codex/GAME_DESIGN.md`
- `Codex/WorkLogs/2026-07-23_0058_Kimdawon_connection-to-level-flow.md`

## 핵심 변경 내용

- 연결 직후 이동할 씬을 위한 `LevelSceneName` 설정을 추가하고 기본값을 `Level`로 지정했다.
- 실제 캐릭터 조작 씬 설정인 `GameplaySceneName`은 `InGame`으로 유지해 레벨 선택 화면과 플레이 화면의 역할을 분리했다.
- 두 명이 연결되면 Host의 NGO SceneManager가 `Level` 씬을 로드하도록 변경했다.
- 연결이 유지된 `Level`과 `InGame` 양쪽에서 세션 HUD와 연결 해제 처리가 작동하도록 범위를 확장했다.
- `Level` 씬을 Build Settings에서 `DualPlayConnectionTestScene` 다음 순서로 추가했다.
- 네트워크 접속 중에는 Client의 레벨 선택 버튼을 비활성화하고 Host만 선택할 수 있게 했다.
- Host가 레벨을 선택하면 NGO SceneManager로 선택한 씬을 로드해 Client도 함께 이동하도록 했다.
- 네트워크 연결 없이 `Level` 씬을 직접 실행한 경우에는 기존 로컬 비동기 로드를 유지했다.
- 메인 메뉴 안내 문구와 기준 기획서의 씬 흐름을 새 구조에 맞췄다.

## 확인 또는 테스트 결과

- 설정 에셋의 `levelSceneName` 값이 `Level`이고 `gameplaySceneName` 값은 `InGame`으로 유지되는 것을 파일 기준으로 확인했다.
- `Level.unity`의 GUID와 Build Settings 항목이 일치하는지 확인했다.
- `Assembly-CSharp.csproj` 빌드 결과 오류 0개를 확인했다. Unity 참조 어셈블리의 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개는 남아 있다.
- 기존 사용자 작업 중인 `Level.unity`, 캐릭터 컨트롤러, Animator Controller는 수정하지 않았다.
- 현재 세션에는 Unity MCP 연결 도구가 없어 실제 Host/Client 2프로세스 전환은 자동 확인하지 못했다.

## 남은 작업 및 주의 사항

- Host와 Client를 연결해 두 화면이 모두 `Level`로 이동하는지 수동 확인해야 한다.
- `Level` 씬 버튼 중 `InGame_2`부터 `InGame_5`는 현재 프로젝트에 해당 씬 파일이 없어 선택 시 로드되지 않는다.
