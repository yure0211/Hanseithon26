# 거북이 스프라이트 정면 축 교정

## 작업 목적

- 거북이 스프라이트의 원본 정면이 오른쪽인 점을 반영해 방향키별 표시 방향을 바로잡는다.

## 변경한 파일

- `Assets/Scripts/TurtleController.cs`
- `Codex/WorkLogs/2026-07-23_0228_Kimdawon_turtle-sprite-forward-axis-fix.md`

## 핵심 변경 내용

- 거북이의 최초 바라보는 방향을 오른쪽으로 변경했다.
- 원본 스프라이트의 오른쪽을 0도로 삼아 방향별 회전을 다음과 같이 교정했다.
  - 오른쪽: 0도
  - 위쪽: 90도
  - 왼쪽: 180도
  - 아래쪽: -90도
- 이동, 애니메이션, 네트워크 동기화 관련 기존 로직은 변경하지 않았다.

## 확인 또는 테스트 결과

- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:quiet` 결과 오류 0개로 컴파일을 통과했다.
- Unity 패키지 참조에서 기존 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개가 출력됐으며 이번 방향 교정과는 무관하다.
- 현재 Unity MCP Editor 조작 도구가 연결되어 있지 않아 실제 화면 방향은 Unity에서 수동 확인이 필요하다.

## 남은 작업이나 주의 사항

- 실행 중이던 Host와 Client를 모두 종료하고 다시 실행한 뒤, 각 방향키 입력에서 거북이가 오른쪽/위쪽/왼쪽/아래쪽을 올바르게 바라보는지 확인한다.
- 빌드된 Client를 사용한다면 코드 변경 후 다시 빌드해야 한다.
