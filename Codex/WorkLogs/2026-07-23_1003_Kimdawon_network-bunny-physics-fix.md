# Kimdawon 작업 내용 - 네트워크 토끼 바닥 통과 수정

## 증상

- 로컬 Unity 테스트에서는 토끼가 바닥에 정상 착지한다.
- 호스트 또는 클라이언트 통신이 시작된 상태에서만 토끼가 바닥을 통과한다.
- 동일한 네트워크 플레이어 프리팹을 사용하는 거북이는 중력이 없어 문제가 잘 드러나지 않았다.

## 원인

- 네트워크 플레이어에 `NetworkTransform`과 `Rigidbody2D`는 있었지만 NGO 물리 연동 컴포넌트인 `NetworkRigidbody2D`가 없었다.
- 토끼의 중력 기반 물리 위치와 `NetworkTransform`의 위치 동기화가 따로 동작하여 2D 물리 접촉 계산이 무시될 수 있었다.

## 수정 내용

- `DualPlayNetworkPlayer` 프리팹에 `NetworkRigidbody2D`를 추가했다.
- `UseRigidBodyForMotion`, `AutoUpdateKinematicState`, `AutoSetKinematicOnDespawn`을 활성화했다.
- 소유자는 Dynamic Rigidbody2D로 실제 물리와 입력을 처리한다.
- 원격 플레이어는 Kinematic Rigidbody2D로 네트워크 위치를 물리 엔진을 통해 적용한다.
- 원격 Rigidbody2D의 simulation은 유지하되 원격 Collider는 비활성화하여 플레이어 간 충돌은 만들지 않는다.

## 확인

- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity quiet`
- 컴파일 오류 0개. 기존 Unity 참조 어셈블리 버전 경고 2개만 존재한다.

## 빠른 테스트

1. 호스트와 클라이언트를 연결한다.
2. InGame_1로 진입한다.
3. 토끼가 시작 지점 아래 바닥에 착지하는지 양쪽 화면에서 확인한다.
4. 점프 후 같은 바닥과 타일 벽을 통과하지 않는지 확인한다.
