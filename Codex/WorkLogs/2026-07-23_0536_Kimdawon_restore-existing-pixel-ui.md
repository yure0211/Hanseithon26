# 기존 도트 UI 에셋 복구 및 재적용

## 작업 목적

- 임의로 생성했던 네이비·크림 런타임 테마를 제거한다.
- `bak_Start`에 보존된 시작 화면과 기존 도트 UI 에셋을 기준으로 UI를 다시 구성한다.
- 완성된 타이틀 에셋은 수정하거나 교체하지 않는다.

## 기준으로 사용한 기존 에셋

- 타이틀: `Assets/Animations/Sprite-00011.aseprite`
- 9분할 버튼 프레임: `Assets/Animations/Sprite-00012.aseprite`
- 글꼴: `Assets/Font/NeoDunggeunmoPro-Regular.ttf` 및 SDF 에셋
- 캐릭터 선택 이미지: `Assets/Animations/Turtle.aseprite`, `Assets/Animations/Bunny.aseprite`
- 시작 화면 원본: `Assets/Scenes/bak_Start.unity`

## 핵심 변경 내용

- `Start.unity`를 `bak_Start.unity`의 저장 내용으로 복구했다.
- `Square` 오브젝트의 타이틀 스프라이트 참조와 배치 값은 수정하지 않았다.
- 기존 `Button_Sample`을 실제 `연결 대기실` 버튼으로 사용하고 같은 버튼을 복제해 `게임 종료` 버튼을 만든다.
- 시작 화면 위를 덮던 별도 IMGUI 메뉴와 임의 색상·그라데이션 테마를 제거했다.
- 연결 대기실과 캐릭터 선택 UI는 `Sprite-00012`의 실제 픽셀 프레임을 잘라 9분할 배경으로 사용한다.
- 캐릭터 선택 화면에 기존 거북이·토끼 스프라이트를 표시한다.
- 레벨 씬에서는 버튼 이미지와 버튼 글꼴만 바꾸며 기존 패널, 배경, 슬라이더 이미지는 변경하지 않는다.
- 연결 대기실은 주소, 포트, Host/Client, 연결 끊기와 상태만 표시한다.
- Build Settings의 첫 씬은 `Start`로 유지하고 사용하지 않는 `MainMenuScene`은 빌드 목록에서 제외했다.

## 보존한 사용자 변경

- `Sprite-00011` 타이틀 에셋 파일과 임포트 설정은 수정하지 않았다.
- `Sprite-00012.aseprite`와 해당 `.meta`의 9분할 Border 설정은 수정하지 않았다.
- `bak_Start.unity`와 `.meta`는 백업으로 그대로 유지했다.
- 기존 게임플레이 에셋과 씬 배경 이미지는 수정하지 않았다.

## 확인 결과

- `Start.unity`의 타이틀 GUID가 기존 `Sprite-00011` GUID와 동일함을 확인했다.
- `Start.unity`의 `Button_Sample`이 기존 `Sprite-00012` GUID를 참조함을 확인했다.
- 네트워크 부트스트랩이 기존 버튼, 글꼴, 토끼, 거북이 스프라이트를 직접 참조함을 확인했다.
- `dotnet build Assembly-CSharp.csproj --no-restore` 결과 C# 오류 0개로 통과했다.
- Unity 참조의 `System.IO.Compression`, `System.Net.Http` 버전 경고 2개만 남았으며 이번 UI 코드 오류는 아니다.
- 플레이 모드와 2인 연결 확인은 팀에서 직접 진행하기로 한 요청에 따라 실행하지 않았다.

## 수동 확인 절차

1. Unity에서 스크립트 컴파일이 끝난 뒤 `Start` 씬을 실행한다.
2. 기존 타이틀이 그대로 보이고 그 아래에 도트 버튼 2개가 표시되는지 확인한다.
3. `연결 대기실`을 눌러 접속 화면의 버튼과 입력창이 같은 도트 프레임인지 확인한다.
4. 두 플레이어 연결 후 캐릭터 선택 화면에 거북이와 토끼 이미지가 표시되는지 확인한다.
5. 레벨 화면에서 기존 배경·패널은 유지되고 버튼만 도트 프레임으로 표시되는지 확인한다.
