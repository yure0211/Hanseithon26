# Kimdawon 작업 내용 - 소유자 물리 권한 및 충돌 로그

## 목적

- 네트워크 접속 시 토끼가 바닥을 통과하는 원인이 로컬 충돌 미발생인지, 충돌 이후 네트워크 위치 보정인지 구분함.
- 토끼가 호스트인지 클라이언트인지와 무관하게 토끼를 소유한 사용자 컴퓨터에서 물리를 계산하도록 고정함.

## 변경

- `DualPlayNetworkPlayer.OnCollisionEnter2D`에 상세 로그를 추가함.
- 출력 항목: 역할, 소유권, 서버 여부, 소유자/로컬 클라이언트 ID, 충돌 대상과 레이어, 접점 수, Rigidbody 타입, simulated 상태, Collider 활성 상태, 위치, 속도.
- `NetworkTransform`의 Authority Mode를 런타임에도 `Owner`로 강제함.
- 로컬 소유자의 Rigidbody2D만 Dynamic 및 Collider 활성 상태로 유지함.
- 소유하지 않은 복제 플레이어는 Kinematic 및 Collider 비활성 상태로 유지하고, 소유자가 전송한 위치만 표시함.

## 확인 방법

- 호스트와 클라이언트 연결 후 토끼가 바닥에 닿을 때 Console에서 `[NetworkPlayer OnCollisionEnter2D]` 로그를 확인함.
- 로그가 없으면 토끼 소유자 측 Collider/물리 충돌 문제이고, 로그가 출력된 뒤 추락하면 위치 동기화가 충돌 결과를 덮어쓰는 문제로 판단할 수 있음.
