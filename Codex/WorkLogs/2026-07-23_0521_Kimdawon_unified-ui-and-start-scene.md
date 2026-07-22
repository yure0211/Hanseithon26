# 시작 씬 및 공통 UI 정비 작업

> 후속 수정: 이 작업의 임의 생성 UI 테마는 사용자 피드백에 따라 제거되었으며,
> `2026-07-23_0536_Kimdawon_restore-existing-pixel-ui.md`의 기존 에셋 기반 구현으로 대체되었습니다.

## 작업 목적

- 프로젝트의 최초 진입 씬을 `Start`로 변경한다.
- `Button_Sample`의 둥근 픽셀 테두리 인상을 기준으로 화면별 UI를 하나의 스타일로 통일한다.
- 연결 대기실에는 네트워크 연결에 필요한 정보와 조작만 표시한다.

## 변경한 파일

- `Assets/Scripts/DualPlaySample/DualPlayUiTheme.cs`
- `Assets/Scripts/DualPlaySample/DualPlayUiTheme.cs.meta`
- `Assets/Scripts/DualPlaySample/DualPlayMainMenu.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkLauncher.cs`
- `Assets/Scripts/TurtleCarrySkill.cs`
- `Assets/Prefabs/DualPlayNetworkBootstrap.prefab`
- `Assets/Scenes/MainMenuScene.unity`
- `Assets/Scenes/Level.unity`
- `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Codex/GAME_DESIGN.md`

## 핵심 변경 내용

- Build Settings의 첫 씬을 `Start`로 변경하고 연결 해제 후 돌아갈 메인 씬 설정도 `Start`로 맞췄다.
- `Start` 씬에 메뉴 컴포넌트가 저장되어 있지 않아도 런타임에 자동으로 시작 메뉴가 만들어지도록 구성했다.
- 네이비·크림·민트·골드 색상과 픽셀 느낌의 둥근 테두리를 사용하는 공통 UI 테마를 추가했다.
- 공통 테마에서 IMGUI 메뉴와 uGUI 버튼·패널·텍스트·슬라이더를 함께 스타일링하도록 했다.
- 시작 화면, 연결 대기실, 캐릭터 선택 화면, 레벨 선택 화면, 인게임 HUD와 거북이 상자 들기 안내를 같은 스타일로 통일했다.
- 연결 대기실에서는 주소, 포트, 호스트/클라이언트 시작, 연결 끊기, 현재 연결 상태만 표시하도록 문구를 정리했다.
- 레벨 선택 화면의 임시 영문 문구와 `Button` 이름을 한국어 스테이지 문구로 교체했다.
- `Start`에 저장되지 않은 `Button_Sample`과 새 `Sprite-00012.aseprite`는 덮어쓰거나 수정하지 않았다.

## 확인 결과

- 모든 Build Settings 씬 경로와 `.meta` GUID가 일치함을 확인했다.
- `Start`가 Build Settings의 0번 씬이고 네트워크 설정의 메인 메뉴 씬도 `Start`임을 확인했다.
- 새 공통 UI 테마 스크립트를 포함해 `dotnet build Assembly-CSharp.csproj --no-restore`를 실행했고 C# 오류 0개로 통과했다.
- 표시된 2개 경고는 Unity 참조의 `System.IO.Compression` 및 `System.Net.Http` 버전 충돌 경고이며 이번 UI 코드 오류는 아니다.
- `git diff --check`에서 공백 오류가 없음을 확인했다.
- 플레이 모드 및 2인 연결 검증은 팀에서 직접 진행하기로 한 기존 요청에 따라 실행하지 않았다.

## 수동 확인 절차

1. Unity가 새 스크립트를 리컴파일한 뒤 `Start` 씬을 실행한다.
2. 시작 화면의 `연결 대기실로 이동` 버튼으로 연결 씬에 진입한다.
3. 연결 대기실에 연결 관련 UI만 표시되는지 확인한다.
4. Host와 Client를 연결해 캐릭터 선택, 레벨 선택, `InGame_1` 순으로 이동하며 UI 스타일과 한글 표시를 확인한다.
5. 해상도를 바꿔도 UI가 중앙 정렬되고 화면 밖으로 벗어나지 않는지 확인한다.

## 주의 사항

- `Button_Sample`은 작업 시작 시 `Start` 씬에 저장되어 있지 않아 원본 오브젝트를 직접 복제하지 않았다. 대신 같은 인상의 둥근 픽셀 버튼을 코드로 생성해 모든 화면에서 재사용한다.
- 기존 `MainMenuScene`은 호환성을 위해 Build Settings 마지막에 유지했지만 실제 시작 및 복귀 흐름은 `Start`를 사용한다.
