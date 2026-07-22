# 네트워크 캐릭터 중복 표시 및 원격 표시 수정

## 작업 목적

네트워크 플레이 중 사각형 플레이어와 씬에 배치된 애니메이션 캐릭터가 동시에 나타나 함께 움직이는 문제를 제거하고, 실제 캐릭터 아트와 애니메이션 상태가 상대 화면에서도 유지되도록 한다.

## 변경한 파일

- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- `Assets/Scripts/BunnyController.cs`
- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Codex/WorkLogs/2026-07-23_0119_Kimdawon_network-character-visual-fix.md`

## 핵심 변경 내용

- 네트워크 플레이어 프리팹에 토끼·거북이 스프라이트와 Animator Controller 참조를 연결했다.
- 네트워크 역할이 정해지면 사각형 대신 해당 역할의 실제 스프라이트와 Animator Controller를 사용하도록 변경했다.
- 네트워크로 `InGame`에 진입했을 때 씬에 배치된 로컬 `Bunny`, `Turtle`만 런타임에서 비활성화해 중복 입력과 중복 표시를 제거했다.
- Owner의 현재 Animator 상태 해시와 토끼 좌우 방향을 NetworkVariable로 동기화해 원격 화면에서도 같은 상태를 재생하도록 했다.
- 거북이 입력 방향 회전이 원격 화면에도 전달되도록 NetworkTransform의 Z 회전 동기화를 활성화했다.
- 토끼 좌우 반전이 루트 Transform 스케일을 바꾸지 않고 SpriteRenderer의 `flipX`를 사용하도록 수정했다.
- 네트워크 플레이어는 연결·레벨 선택 화면에서 숨기고 실제 `InGame`에서만 표시한다.

## 확인 또는 테스트 결과

- 파일 기준으로 로컬 캐릭터와 네트워크 플레이어가 동시에 입력을 받던 원인을 확인했다.
- 프리팹의 역할별 스프라이트·Animator Controller GUID가 현재 `InGame` 씬에서 사용 중인 에셋과 일치하는지 확인했다.
- `Assembly-CSharp.csproj` 빌드 결과 오류 0개를 확인했다. Unity 참조 어셈블리의 기존 버전 충돌 경고 2개는 남아 있다.
- Unity MCP 연결 도구가 없어 실제 Host/Client 2프로세스 장시간 동작은 아직 확인하지 못했다.

## 남은 작업 및 주의 사항

- Host와 Client를 연결해 각 화면에 네트워크 토끼와 거북이만 하나씩 표시되는지 확인해야 한다.
- 두 캐릭터를 1분 이상 움직이며 상대 캐릭터의 위치와 애니메이션이 계속 표시되는지 확인해야 한다.
- 카메라는 현재 고정형이므로 캐릭터가 화면 범위를 벗어나면 정상적으로 보이지 않을 수 있다.
