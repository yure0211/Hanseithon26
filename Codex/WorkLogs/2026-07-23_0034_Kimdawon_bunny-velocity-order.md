# 버니 속도 갱신 순서 수정

## 작업 목적

`BunnyController`의 Animator에 이전 물리 프레임 속도가 전달되고 달리기 상태가 항상 false가 되는 문제를 최소 범위로 수정한다.

## 변경한 파일

- `Assets/Scripts/BunnyController.cs`
- `Codex/WorkLogs/2026-07-23_0034_Kimdawon_bunny-velocity-order.md`

## 핵심 변경 내용

- `velocity`를 이동과 점프 처리 전에 저장하던 코드를 제거했다.
- 이동과 점프 처리가 끝난 후 `body.linearVelocity`를 `velocity`에 저장하도록 순서를 변경했다.
- `IsRun` 판정을 항상 false였던 `Mathf.Abs(velocity.x) < 0`에서 `Mathf.Abs(velocity.x) > 0.01f`로 수정했다.
- Animator가 없는 오브젝트에서 점프할 때 NullReferenceException이 발생하지 않도록 Jump Trigger 호출에 null 검사를 추가했다.
- `state()` 메서드 이름을 역할이 드러나는 `UpdateAnimatorState()`로 변경했다.

## 확인 또는 테스트 결과

- 코드상 좌우 이동속도와 점프 힘 적용 후의 Rigidbody2D 속도가 Animator에 전달되는 순서를 확인했다.
- 사용자 작업 중인 `Bunny.controller`, `InGame.unity`, 가이드 문서는 수정하지 않았다.
- 현재 세션에는 Unity MCP 연결 도구가 없어 Unity 컴파일과 Play Mode 동작은 자동 확인하지 못했다.

## 남은 작업 및 주의 사항

- Unity Play Mode에서 오른쪽 이동 시 `velocity.x`가 약 `5`, 왼쪽 이동 시 약 `-5`, 정지 시 약 `0`인지 확인한다.
- 점프 시 `YVelocity`가 양수에서 0 근처를 거쳐 음수로 변하는지 Animator 창에서 확인한다.
