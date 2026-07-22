# 듀얼플레이 메인 화면·역할 고정·인게임 전환 작업 기록

## 작업 목적

메인 화면에서 연결 테스트 로비로 이동하고, Host와 Client가 모두 접속하면 연결 상태를 유지한 채 `InGame`으로 전환되도록 구현한다. Host는 거북이, Client는 토끼를 고정으로 담당한다.

## 변경한 파일

- `Assets/Scenes/MainMenuScene.unity` 및 `.meta`
- `Assets/Scenes/InGame.unity`
- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Assets/Scripts/BunnyController.cs`
- `Assets/Scripts/TurtleController.cs`
- `Assets/Scripts/DualPlaySample/DualPlayMainMenu.cs` 및 `.meta`
- `Assets/Scripts/DualPlaySample/DualPlayConnectionSettings.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkLauncher.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Codex/GAME_DESIGN.md`
- `Codex/DualPlaySampleScene_GUIDE.md`
- `Codex/WorkLogs/2026-07-22_2052_Kimdawon_dual-play-main-flow.md`

## 핵심 변경 내용

- `MainMenuScene`과 메인 메뉴 UI를 추가했다.
- 빌드 시작 씬을 메인 화면으로 설정하고 연결 로비, `InGame`, 네트워크 샘플 순서로 Build Settings를 정리했다.
- 연결 로비에서 2명이 접속하면 Host가 NGO 씬 관리자로 `InGame`을 자동 로드하도록 구현했다.
- 마지막 Host 주소와 Host/Client 선택을 `PlayerPrefs`에 저장하도록 확장했다.
- Host 소유 플레이어는 거북이, Client 소유 플레이어는 토끼 역할을 `NetworkVariable`로 동기화한다.
- `InGame`에서 소유자에게만 역할별 컨트롤러, `Rigidbody2D`, 충돌체를 활성화한다.
- 기존 로컬 전용 토끼·거북이 오브젝트는 네트워크 플레이어와 중복되지 않도록 비활성화했다.
- 토끼와 거북이 컨트롤러를 Input System의 `Keyboard` 입력과 `FixedUpdate` 물리 처리 구조로 정리했다.
- 게임 화면에 현재 역할, 접속 인원, 연결 종료 및 메인 화면 복귀 HUD를 추가했다.
- 자동 2프로세스 검증을 위한 `-dualPlayHost`, `-dualPlayClient`, `-dualPlayAddress=` 실행 인자를 추가했다.

## 확인 및 테스트 결과

- `MainMenuScene`, `DualPlayConnectionTestScene`, `InGame` 씬 검증: 각 문제 0건, 누락 스크립트 0건, 깨진 프리팹 0건.
- Unity 콘솔 오류 및 경고: 0건.
- 변경 스크립트 표준 검증: 오류 0건.
- Windows Standalone Development Build 성공: 오류 0건.
- 에디터 Host와 Standalone Client의 실제 2프로세스 연결 성공.
- 접속 인원 `2/2`가 되면 두 실행 인스턴스가 `InGame`으로 자동 전환되는 것을 확인했다.
- Host 플레이어: `NetworkTurtle`, Owner, 거북이 컨트롤러와 로컬 물리 활성화 확인.
- Client 플레이어: `NetworkBunny`, 토끼 역할 동기화 및 바닥 착지 위치 `y=-2.06` 확인.
- 네트워크 플레이어 충돌체 크기 `1×1`과 Owner 권한 `NetworkTransform` 동기화를 확인했다.

## 남은 작업 및 주의 사항

- 현재 캐릭터 표시는 역할 확인용 녹색·주황색 사각형이며 실제 토끼·거북이 아트 연결은 후속 작업이다.
- 물리는 Owner 권한 방식이므로 경쟁 요소나 치트 방지가 필요해지면 서버 권한 물리 구조를 검토한다.
- 직접 UDP 연결 방식이므로 외부 인터넷 플레이에는 Relay 또는 NAT/포트 포워딩 대응이 필요하다.
- Standalone 빌드에서 발생한 셰이더 처리 경고는 빌드 오류가 아니며 게임 스크립트 콘솔에는 오류·경고가 없었다.
