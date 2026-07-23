# Kimdawon 작업 내용 - 타일맵 충돌 및 네트워크 상자 긴급 수정

## 문제 원인

- 게임 씬의 타일맵에는 `TilemapCollider2D`만 존재하고 `CompositeCollider2D`와 Static `Rigidbody2D` 구성이 빠져 있었다.
- 네트워크 상자 식별자가 루트 오브젝트의 sibling index를 사용했다. 런타임 네트워크 오브젝트가 추가되는 순서는 호스트와 클라이언트에서 달라질 수 있어 서로 다른 상자를 선택할 수 있었다.

## 수정 내용

- 모든 로드된 씬의 일반 타일맵 콜라이더에 Static `Rigidbody2D`와 Polygon `CompositeCollider2D`를 런타임에 자동 구성한다.
- `TilemapCollider2D.compositeOperation`을 `Merge`로 설정하여 타일별 경계를 하나의 충돌체로 합친다.
- 씬 파일은 직접 수정하지 않아 진행 중인 레벨 작업을 보존한다.
- 상자 네트워크 ID를 sibling index가 아닌 씬 경로와 계층 이름 경로로 생성한다.
- 호스트와 클라이언트가 동일한 상자를 찾아 이동하도록 보정한다.

## 확인

- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity quiet`
- 컴파일 오류 0개. 기존 Unity 참조 어셈블리 버전 경고 2개만 존재한다.

## 빠른 테스트

1. 호스트와 클라이언트가 InGame 씬에 입장한다.
2. 토끼가 바닥과 벽을 통과하지 않는지 확인한다.
3. 거북이가 여러 상자 중 하나를 E로 든다.
4. 양쪽 화면에서 같은 상자 하나만 머리 위에 표시되는지 확인한다.
5. 이동 후 E로 내려놓고 양쪽 위치가 같은지 확인한다.
