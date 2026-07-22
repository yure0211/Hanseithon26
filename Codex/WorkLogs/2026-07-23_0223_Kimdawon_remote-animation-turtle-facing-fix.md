# 원격 애니메이션 및 거북이 방향 수정

## 작업 목적

멀티플레이에서 상대 캐릭터의 애니메이션 전환이 불안정한 문제와 거북이의 방향키 조합에 따라 바라보는 각도가 달라지는 문제를 수정한다.

## 변경한 파일

- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- `Assets/Scripts/TurtleController.cs`
- `Codex/DualPlaySampleScene_GUIDE.md`
- `Codex/WorkLogs/2026-07-23_0223_Kimdawon_remote-animation-turtle-facing-fix.md`

## 원인

- 원격 Animator에는 현재 상태 해시만 전달되고 전환 조건인 `IsRun`, `IsGround`, `YVelocity`는 전달되지 않았다.
- 원격 Animator가 전달받은 상태를 로컬 기본 파라미터로 다시 판정하면서 즉시 다른 상태로 전환할 수 있었다.
- 거북이는 입력 벡터 전체의 각도로 회전해 두 방향키를 함께 누르면 45도 대각선으로 기울었다.

## 핵심 변경 내용

- 소유자 Animator의 `IsRun`, `IsGround`, `YVelocity` 값을 Owner 쓰기 NetworkVariable로 동기화한다.
- 원격 Animator에 파라미터를 먼저 적용한 다음 상태 전환을 적용하도록 순서를 정리했다.
- Animator 평가가 끝난 뒤 값을 읽도록 동기화 수집 시점을 `LateUpdate`로 변경했다.
- 거북이 방향을 위 `0도`, 오른쪽 `-90도`, 아래 `180도`, 왼쪽 `90도`의 4방향으로 고정했다.
- 대각 입력에서는 마지막으로 누른 방향키를 바라보고, 한 축만 남으면 해당 이동 방향으로 자동 보정한다.

## 확인 또는 테스트 결과

- `dotnet build Assembly-CSharp.csproj --no-restore` 결과 오류 0개를 확인했다.
- Turtle Animator의 `IsRun`, Bunny Animator의 `IsRun`, `IsGround`, `YVelocity` 파라미터 이름과 타입이 코드와 일치하는지 확인했다.
- 기존 Unity 참조 어셈블리의 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개는 그대로 남아 있다.
- 현재 세션에는 호출 가능한 Unity MCP 도구가 없어 실제 2프로세스 애니메이션 재생은 자동 확인하지 못했다.

## 남은 작업 및 주의 사항

- Host와 Client를 모두 완전히 종료한 뒤 다시 실행해 상대 캐릭터의 대기·이동·점프·낙하 전환을 확인해야 한다.
- 거북이는 W/위쪽 `0도`, D/오른쪽 `-90도`, S/아래쪽 `180도`, A/왼쪽 `90도`로 보이는지 양쪽 화면에서 확인해야 한다.
