# 접속 후 캐릭터 선택 흐름 작업

## 작업 목적

Host와 Client가 접속한 뒤 레벨 선택 화면으로 바로 이동하지 않고, 각자 토끼 또는 거북이를 선택한 다음 `Level`로 이동하도록 네트워크 흐름을 확장한다.

## 변경한 파일

- `Assets/Scenes/CharacterSelectScene.unity`
- `Assets/Scenes/CharacterSelectScene.unity.meta`
- `Assets/Scripts/DualPlaySample/DualPlayConnectionSettings.cs`
- `Assets/Scripts/DualPlaySample/DualPlayMainMenu.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkLauncher.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Codex/GAME_DESIGN.md`
- `Codex/DualPlaySampleScene_GUIDE.md`
- `Codex/WorkLogs/2026-07-23_0141_Kimdawon_character-selection-flow.md`

## 핵심 변경 내용

- 네트워크 연결 완료 후 `CharacterSelectScene`으로 함께 이동하도록 씬 흐름을 변경했다.
- Host/Client에 따라 캐릭터를 강제 배정하던 로직을 제거했다.
- 각 플레이어가 토끼 또는 거북이를 선택하는 UI를 추가했다.
- 캐릭터 선택 요청을 ServerRpc로 전달하고, 서버가 이미 다른 플레이어가 선택한 역할인지 검사한다.
- 선택 결과와 선택 완료 상태를 NetworkVariable로 동기화한다.
- 두 플레이어가 서로 다른 캐릭터를 모두 선택했을 때만 Host가 `Level`을 네트워크 씬으로 불러온다.
- 캐릭터 선택 씬 이름을 공용 연결 설정에 추가하고 Build Settings에 씬을 등록했다.
- 메인 메뉴, 실행 가이드와 기준 기획서에서 고정 역할 안내를 선택식 흐름으로 갱신했다.

## 확인 또는 테스트 결과

- `dotnet build Assembly-CSharp.csproj --no-restore` 결과 오류 0개를 확인했다.
- 기존 Unity 참조 어셈블리의 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개는 그대로 남아 있다.
- `git diff --check`에서 공백 오류가 없음을 확인했다.
- 새 씬 GUID와 Build Settings의 GUID가 일치하는지 파일 기준으로 확인했다.
- 현재 세션에는 호출 가능한 Unity MCP 도구가 없어 실제 Host/Client 2프로세스 선택 동기화는 자동 실행하지 못했다.

## 남은 작업 및 주의 사항

- Unity 에디터와 Standalone 빌드를 함께 실행해 캐릭터 중복 선택 방지와 `CharacterSelectScene` → `Level` 전환을 수동 확인해야 한다.
- `Level`에서 실제 게임 씬을 선택한 뒤 각 실행 인스턴스가 자신이 고른 캐릭터의 컨트롤러만 활성화하는지 확인해야 한다.
