# 거북이 방향 기반 상자 들기

## 작업 목적

- 거북이가 상자를 들었을 때 상자가 월드 위쪽에 고정되지 않고, 현재 바라보는 방향의 머리 앞에 놓이도록 개선한다.

## 변경한 파일

- `Assets/Scripts/TurtleCarrySkill.cs`
- `Codex/WorkLogs/2026-07-23_0311_Kimdawon_turtle-directional-box-carry.md`

## 핵심 변경 내용

- 거북이 스프라이트의 기본 머리 방향인 로컬 오른쪽 축(`transform.right`)을 상자 운반 방향으로 사용한다.
- 상자 위치를 `거북이 Rigidbody2D 위치 + 머리 방향 × carryDistance`로 계산한다.
- 거북이가 오른쪽, 위쪽, 왼쪽, 아래쪽을 바라보면 상자도 각 방향의 머리 앞을 따라간다.
- 기존 상자 확대 효과, 색상, 충돌 무시, 내려놓기 속도 처리는 변경하지 않았다.

## 확인 또는 테스트 결과

- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:quiet` 결과 오류 0개로 컴파일을 통과했다.
- Unity 패키지 참조에서 기존 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개가 출력됐으며 이번 변경과는 무관하다.
- 실제 방향별 위치와 거리 체감은 팀에서 Unity 플레이 테스트를 진행한다.

## 남은 작업이나 주의 사항

- 머리와 상자 사이가 너무 멀거나 겹치면 씬의 `TurtleCarrySkill.carryDistance` 값을 조절한다.
- 네트워크 환경에서 상자 소유권과 위치 동기화가 필요해질 경우 별도의 NGO 동기화 설계가 필요하다.
