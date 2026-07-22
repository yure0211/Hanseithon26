# DualPlaySampleScene 네트워크 예제 작업 기록

## 작업 목적

Unity MCP 서버를 사용해 두 실행 인스턴스가 Host와 Client로 접속하고 각자의 플레이어를 조작할 수 있는 2인 네트워크 샘플 씬을 구현한다.

## 변경한 파일

- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/DefaultNetworkPrefabs.asset`
- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Assets/Scenes/DualPlaySampleScene.unity`
- `Assets/Scripts/DualPlaySample/DualPlayDemoEnvironment.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkLauncher.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- 위 에셋에 대응하는 Unity `.meta` 파일
- `Codex/DualPlaySampleScene_GUIDE.md`
- `Codex/WorkLogs/2026-07-22_1819_Kimdawon_dual-play-network-sample.md`

## 핵심 변경 내용

- Unity MCP의 패키지 관리 기능으로 Netcode for GameObjects `2.13.0`을 추가했다.
- `DualPlaySampleScene`과 Owner 권한 방식의 네트워크 플레이어 프리팹을 생성했다.
- Host 시작, Client 접속, 종료와 주소 입력이 가능한 런타임 UI를 구현했다.
- 연결 승인 콜백으로 최대 접속 인원을 2명으로 제한했다.
- 각 클라이언트가 소유한 플레이어만 `WASD` 또는 방향키로 조작하도록 구현했다.
- `NetworkTransform`을 Owner 권한으로 설정해 플레이어 위치를 동기화했다.
- 샘플 씬을 기존 `SampleScene`을 유지한 채 Build Settings에 추가했다.
- 같은 PC와 LAN에서 실행하는 방법을 별도 가이드로 정리했다.

## 확인 및 테스트 결과

- Unity MCP 씬 검증: 누락 스크립트와 깨진 프리팹 없이 문제 0건.
- C# 스크립트 검증: 오류 0건, 경고 0건.
- Play Mode에서 Host 시작 성공: `IsHost=True`, 연결 인원 1명.
- Host 플레이어 자동 생성 확인: `IsSpawned=True`, `IsOwner=True`.
- `NetworkTransform` 권한이 `Owner`로 설정된 것을 확인했다.
- 게임 화면에서 네트워크 UI, 테스트 공간과 Host 플레이어가 정상 렌더링되는 것을 확인했다.
- 최초 검증에서 발견한 네트워크 프리팹 중복 등록을 제거한 뒤 콘솔 오류와 경고가 0건임을 다시 확인했다.

## 남은 작업 및 주의 사항

- 실제 Standalone Client를 연결하는 2프로세스 수동 테스트는 아직 실행하지 않았다.
- 현재 방식은 Unity Relay가 아닌 직접 UDP 연결이므로 외부 인터넷 접속에는 별도 NAT 대응이 필요하다.
- 기존에 작업자가 수정한 Unity MCP 설치 및 ProjectSettings 변경은 되돌리거나 분리하지 않았다.
- 이번 작업은 사용자의 별도 커밋 요청이 없어 커밋하지 않았다.
