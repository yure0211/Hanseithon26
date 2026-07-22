# 네트워크 캐릭터 중복 이동 및 사각형 표시 수정

## 작업 목적

`InGame`에서 한 캐릭터를 조작할 때 다른 캐릭터도 같은 입력으로 움직이고, 실제 캐릭터와 임시 사각형이 동시에 표시되는 문제를 해결한다.

## 변경한 파일

- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Assets/Scripts/BunnyController.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- `Codex/DualPlaySampleScene_GUIDE.md`
- `Codex/WorkLogs/2026-07-23_0154_Kimdawon_network-player-duplicate-fix.md`

## 원인

- `InGame` 씬에 로컬 테스트용 `Bunny`와 `Turtle`이 활성화되어 있는데 네트워크 플레이어도 별도로 생성되어 캐릭터가 중복됐다.
- 로컬 테스트용 두 컨트롤러가 같은 키보드 입력을 직접 읽어서 선택하지 않은 캐릭터까지 같이 움직였다.
- 네트워크 플레이어 프리팹에 실제 캐릭터 스프라이트와 Animator Controller 참조가 없어 런타임 임시 사각형이 표시됐다.

## 핵심 변경 내용

- 네트워크 플레이가 `InGame`에 들어오면 로컬 테스트용 `Bunny`와 `Turtle`을 런타임에 비활성화한다.
- 네트워크 플레이어 프리팹에 토끼·거북이 스프라이트와 Animator Controller를 연결했다.
- 실제 캐릭터 에셋이 연결된 경우 임시 사각형을 사용하지 않고 선택한 캐릭터 아트를 표시한다.
- 선택한 로컬 소유자의 컨트롤러와 물리만 활성화하는 기존 소유권 조건을 유지했다.
- Owner 권한 NetworkTransform의 X/Y 위치와 거북이 Z 회전을 동기화하고, Animator 상태와 토끼 좌우 방향도 별도로 동기화한다.
- 토끼 방향 전환이 루트 Transform 스케일을 바꾸지 않고 SpriteRenderer의 `flipX`만 사용하도록 변경했다.

## 확인 또는 테스트 결과

- `dotnet build Assembly-CSharp.csproj --no-restore` 결과 오류 0개를 확인했다.
- 스프라이트와 Animator Controller GUID가 실제 에셋 `.meta`의 GUID와 일치하는지 확인했다.
- NetworkTransform이 Owner 권한이며 X/Y 위치와 Z 회전 동기화가 활성화된 것을 프리팹 파일 기준으로 확인했다.
- 기존 Unity 참조 어셈블리의 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개는 그대로 남아 있다.
- 현재 세션에는 호출 가능한 Unity MCP 도구가 없어 실제 2프로세스 플레이 검증은 자동 실행하지 못했다.

## 남은 작업 및 주의 사항

- Play 모드를 완전히 종료한 뒤 Host와 Client를 다시 접속해 두 화면에 토끼와 거북이가 각각 하나씩만 보이는지 확인해야 한다.
- 한 플레이어가 움직일 때 상대 캐릭터는 입력을 받지 않고, 이동한 캐릭터의 위치만 양쪽 화면에서 동일하게 갱신되는지 확인해야 한다.
