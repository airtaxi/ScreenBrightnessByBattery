# Screen Brightness By Battery

한국어 | **[English](README.md)**

디바이스의 전원 상태(배터리 또는 AC 전원)에 따라 화면 밝기를 자동으로 조절하고, 외부 모니터가 연결되어 있을 때 절전 모드를 방지하는 경량 Windows 애플리케이션입니다.

![image](https://github.com/user-attachments/assets/9a6db803-8424-4cba-a9e3-1d2b48b3c14b)

## 기능

- **자동 밝기 조절**: 배터리와 AC 전원 간 전환 시 화면 밝기를 자동으로 조절합니다.
- **스마트 절전 방지**: 외부 모니터가 연결되어 있을 때 디바이스가 절전 모드에 진입하는 것을 선택적으로 방지합니다.
- **시스템 트레이 통합**: 시스템 트레이에서 조용히 실행되어 설정에 쉽게 접근할 수 있습니다.
- **자동 밝기 지원**: Windows 자동 밝기 기능을 지원합니다.
- **자동 시작 옵션**: Windows 시작 시 자동으로 실행되도록 설정할 수 있습니다.
- **간단한 설정**: 편집이 쉬운 INI 파일을 사용하여 설정합니다.

## 작동 방식

### 밝기 조절

디바이스가 배터리 전원과 AC 전원 간 전환될 때 자동으로 감지하여, 사용자 설정에 따라 화면 밝기를 조절합니다:

- **배터리 전원**으로 실행 시: 에너지 절약을 위해 밝기를 낮은 값으로 설정
- **AC 전원**에 연결 시: 더 나은 가시성을 위해 밝기를 높은 값으로 설정
- **자동 밝기** 설정 ("auto" 모드) 지원

### 절전 방지

활성화 시 애플리케이션은:

- 외부 모니터가 연결되어 있는지 감지
- 외부 모니터가 연결되어 있는 동안 시스템이 절전 모드에 진입하는 것을 방지
- 외부 모니터가 분리되면 정상적인 절전 동작을 허용

## 설정

애플리케이션은 다음 섹션이 포함된 `settings.ini` 파일에 설정을 저장합니다:

### [Brightness] 섹션

- `Enabled`: 밝기 조절 기능 활성화 여부 ("on" 또는 "off")
- `Battery`: 배터리 전원 시 밝기 수준 (0-100 또는 "auto")
- `AC`: AC 전원 시 밝기 수준 (0-100 또는 "auto")

### [Sleep] 섹션

- `Enabled`: 외부 모니터 연결 시 절전 모드 방지 여부 ("on" 또는 "off")

## 시스템 요구 사항

- Windows 10/11 (10.0.17763.0 이상)
- x64 및 ARM64 아키텍처 호환

## 설치

1. Releases 페이지에서 최신 릴리즈의 설치 프로그램을 다운로드하여 실행
2. 시작 메뉴에서 `Screen Brightness By Battery` 실행
3. 시스템 트레이에 애플리케이션이 표시됩니다

## 사용법

- 시스템 트레이 아이콘을 **우클릭**하여 설정에 접근
- **"Screen Brightness by Battery: Enabled/Disabled"** 를 선택하여 밝기 조절 기능 토글
- **"Prevent Sleep (External Monitor): Enabled/Disabled"** 를 선택하여 절전 방지 기능 토글
- **"Open Settings File"** 을 선택하여 설정 파일을 수동으로 편집
- **"Add to Startup Process"** 를 선택하여 Windows 시작 시 자동 실행 설정

## 소스에서 빌드

이 프로젝트는 다음을 사용합니다:

- .NET 9.0
- Windows App SDK
- WinUI 3
- 대상 Windows 버전 10.0.22621.0 (최소 10.0.17763.0)

### 요구 사항

- Visual Studio 2022 이상
- Windows App SDK 개발 도구
- .NET 9.0 SDK

### 빌드 방법

1. 리포지토리를 클론합니다
2. Visual Studio에서 `ScreenBrightnessByBattery.sln`을 엽니다
3. 대상 아키텍처(x64 또는 ARM64)에 맞게 솔루션을 빌드합니다

## 라이선스

이 프로젝트는 MIT 라이선스에 따라 라이선스가 부여됩니다. 자세한 내용은 [LICENSE.txt](LICENSE.txt) 파일을 참조하세요.

## 개발자

`이호원 (Howon Lee) a.k.a hoyo321 or kck4156, airtaxi`
