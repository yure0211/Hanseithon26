# 버니 바닥 상태 Animator 갱신 수정

## 작업 목적

`BunnyController`에서 계산한 바닥 접촉 상태가 Animator의 `isGround` Bool 파라미터에 전달되지 않는 문제를 최소 범위로 수정한다.

## 변경한 파일

- `Assets/Scripts/BunnyController.cs`
- `Codex/WorkLogs/2026-07-23_0016_Kimdawon_bunny-grounded-animator.md`

## 핵심 변경 내용

- `FixedUpdate()` 마지막에서 기존 `state()` 메서드를 호출하도록 연결했다.
- Animator가 존재할 때 반환하던 반대 방향의 null 검사를 `animator == null` 검사로 수정했다.
- 기존 Animator 파라미터 이름과 애니메이션 에셋은 변경하지 않았다.

## 확인 또는 테스트 결과

- 코드상 `isGrounded` 계산 이후 매 물리 프레임에 `animator.SetBool("isGround", isGrounded)`이 호출되는 흐름을 확인했다.
- 현재 세션에는 Unity MCP 연결 도구가 없어 Unity 컴파일과 Play Mode 동작은 자동 확인하지 못했다.

## 남은 작업 및 주의 사항

- 연결된 Animator Controller에 대소문자가 정확히 일치하는 `isGround` Bool 파라미터가 있어야 한다.
- Unity에서 스크립트 컴파일 후 착지와 점프 시 `isGround` 값이 전환되는지 확인해야 한다.
