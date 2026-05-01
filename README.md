# OllamaWpfClient

오프라인 환경에서 로컬 PC에 설치한 **Ollama**와 REST API로 통신하는 WPF 채팅 클라이언트입니다.
인터넷 연결 없이 `127.0.0.1:11434` 로컬호스트에서 Llama 3, Qwen 등 LLM 모델을 구동하고 사용하는 것을 목표로 합니다.

---

## 주요 기능

- 설치된 Ollama 모델 자동 조회 (`GET /api/tags`)
- 모델 선택 (ComboBox) — 여러 모델이 있을 때 전환 가능
- 채팅 (`POST /api/chat`) — 대화 문맥(history) 유지하며 전송
- **Enter** → 전송, **Shift+Enter** → 줄바꿈
- 진행 중 요청 취소 (Cancel)
- 대화 초기화

---

## 기술 스택

| 영역 | 사용 기술 |
|---|---|
| Framework | .NET 10 (`net10.0-windows`), WPF |
| 언어 | C# (nullable enabled) |
| MVVM | 직접 구현 (`Common/BaseViewModel`, `RelayCommand`, `AsyncRelayCommand`) |
| HTTP | `HttpClient` (BCL) |
| JSON | `System.Text.Json` (BCL) |
| 외부 NuGet | 없음 |

외부 NuGet 의존성 없이 BCL만으로 동작하도록 설계했습니다.

---

## 아키텍처

```
OllamaWpfClient/
├── Behaviors/
│   └── EnterKey.cs              # 첨부 속성: Enter→Command, Shift+Enter→줄바꿈
├── Common/
│   ├── BaseViewModel.cs         # INotifyPropertyChanged 베이스
│   ├── RelayCommand.cs          # 동기 ICommand
│   └── AsyncRelayCommand.cs     # 비동기 ICommand (실행 중 재진입 방지)
├── Models/
│   ├── ChatMessage.cs           # 앱 내부용 메시지 모델 (Role, Content)
│   └── OllamaApiModels.cs       # Ollama REST API DTO 묶음
├── Services/
│   ├── IOllamaClient.cs         # 추상화된 Ollama 통신 인터페이스
│   └── OllamaClient.cs          # HttpClient 기반 구현체
├── ViewModels/
│   └── MainViewModel.cs         # 메인 화면 상태/명령
├── App.xaml(.cs)
├── MainWindow.xaml              # UI (모델 선택, 메시지 목록, 입력창)
└── MainWindow.xaml.cs           # 의존성 와이어링 (DataContext 설정)
```

### 계층 분리 원칙

- **View(XAML/code-behind)** — UI 표시와 입력 디바이스 처리만 담당
- **ViewModel** — 화면 상태와 명령을 노출, View에 대한 직접 참조 없음
- **Service** — Ollama API 통신을 캡슐화. 내부에서만 DTO를 사용하고, 외부에는 앱 도메인 모델(`ChatMessage`)만 노출
- **Behavior(첨부 속성)** — View 전용 로직(키 이벤트 등)을 코드비하인드 없이 XAML 선언만으로 결합

---

## 빌드 / 실행

```powershell
# 빌드
dotnet build OllamaWpfClient/OllamaWpfClient.csproj

# 실행
dotnet run --project OllamaWpfClient/OllamaWpfClient.csproj
```

또는 Visual Studio에서 `OllamaWpfClient.slnx` 열고 F5.

> **사전 조건:** 아래 "Ollama 세팅 가이드" 절을 먼저 수행해 모델이 설치돼 있어야 합니다.

---

## Ollama 세팅 가이드 (Windows)

### 1. Ollama 설치

1. 공식 사이트에서 Windows 설치 파일을 다운로드합니다.
   - 다운로드 페이지: https://ollama.com/download/windows
   - 파일명: `OllamaSetup.exe`
2. 설치 파일을 실행하고 안내에 따라 설치합니다.
3. 설치가 완료되면 Ollama가 백그라운드 서비스로 자동 실행되며,
   시스템 트레이에 라마 아이콘이 표시됩니다.

설치 후 PowerShell에서 버전 확인:

```powershell
ollama --version
```

> winget으로 설치도 가능:
> ```powershell
> winget install --id Ollama.Ollama -e
> ```

### 2. 서버 실행 확인

Ollama는 설치 직후 자동으로 로컬 서버를 띄웁니다. 기본 주소는 다음과 같습니다.

```
http://127.0.0.1:11434
```

PowerShell에서 동작 여부 확인:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:11434" -UseBasicParsing
```

정상이면 `Ollama is running` 응답이 옵니다.

> 서버가 떠있지 않다면 PowerShell에서 수동 실행:
> ```powershell
> ollama serve
> ```

### 3. 모델 다운로드 (Pull)

원하는 모델을 받습니다. 모델은 한 번 받으면 로컬에 저장되어 **인터넷 없이도 사용 가능**합니다.

#### Llama 3 계열

```powershell
ollama pull llama3.2:3b
ollama pull llama3.1:8b
```

#### Qwen 계열 (한국어 성능 양호)

```powershell
ollama pull qwen2.5:7b
ollama pull qwen2.5:3b
```

> **TIP:** 모델 뒤의 `:3b`, `:7b`는 파라미터 수입니다.
> - 3B: 8GB RAM/VRAM 환경에서도 동작
> - 7B: 16GB 이상 권장
> - 14B 이상: 24GB 이상 VRAM 권장

설치된 모델 목록 확인:

```powershell
ollama list
```

### 4. 오프라인 환경 사용 시 주의사항

- 모델은 **반드시 인터넷이 연결된 상태에서 미리 `ollama pull`** 로 받아둘 것
- 모델 저장 경로 (기본값):
  ```
  C:\Users\<사용자명>\.ollama\models
  ```
- 인터넷을 끊은 뒤에도 위 경로의 모델은 계속 사용 가능
- 새 모델이 필요하면 잠시 인터넷을 연결해 `ollama pull` 후 다시 끊으면 됨

### 5. 자주 쓰는 환경 변수 (선택)

| 변수 | 설명 | 예시 |
|------|------|------|
| `OLLAMA_HOST` | 서버 바인딩 주소/포트 | `127.0.0.1:11434` |
| `OLLAMA_MODELS` | 모델 저장 경로 변경 | `D:\ollama-models` |
| `OLLAMA_KEEP_ALIVE` | 모델 메모리 유지 시간 | `30m`, `-1`(무한) |

---

## 사용된 Ollama REST API 엔드포인트

| 메서드 | 경로 | 용도 |
|---|---|---|
| `GET` | `/api/tags` | 설치된 모델 목록 조회 |
| `POST` | `/api/chat` | 채팅 (대화 history 포함) |

응답은 모두 비스트리밍(`stream: false`)으로 처리합니다.

---

## 향후 계획 (Roadmap)

- [ ] 스트리밍 응답 (`stream: true`) — 토큰 단위 출력
- [ ] 대화 내역 저장/불러오기 (JSON 파일)
- [ ] 시스템 프롬프트 설정 UI
- [ ] 모델별 옵션 조정 (temperature, top_p 등)
- [ ] 마크다운 렌더링 (응답 표시)
- [ ] 다중 대화 세션 (탭)

---

## 참고 링크

- Ollama 공식 사이트: https://ollama.com
- Ollama GitHub: https://github.com/ollama/ollama
- REST API 문서: https://github.com/ollama/ollama/blob/main/docs/api.md
- 모델 라이브러리: https://ollama.com/library
