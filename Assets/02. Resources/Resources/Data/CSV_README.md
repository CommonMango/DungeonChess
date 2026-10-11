# CSV 매니저 사용법

## 파일 형식
- 1행: 열 이름
- 2행: 자료형
- 3행: 사람이 읽는 설명. 데이터로 저장하거나 형 변환하지 않고 건너뜁니다. 빈 행도 가능합니다.
- 4행부터: 실제 데이터

```csv
id,hp,isAlly
string,float,bool
고유 ID,최대 체력,아군 여부
knight,100,true
```

로드 시 모든 값을 실제 C# 자료형으로 변환합니다. 잘못된 값은 파일명·행·열과 함께 오류를 반환합니다.
기본 자료형: string, int, long, float, double, bool, vector2.
자료형 이름은 대소문자를 구분하지 않습니다. 열 이름·테이블 이름·문자열 ID는 구분합니다.
vector2는 `1;2`, bool은 true/false 또는 1/0입니다.
숫자 소수점은 `.`이고 빈 숫자는 오류입니다. 빈 string은 허용됩니다.

## 여러 테이블 사용
CSVManager의 CSV Files 배열에 여러 TextAsset을 넣으면 시작할 때 파일명별로 로드합니다.
같은 파일명의 항목은 중복 오류입니다. 추가 파일은 Load(file, "다른이름")으로 로드할 수 있습니다.
한 표를 다시 읽어도 다른 표는 유지되며 실패하면 이전 데이터를 보존합니다.
Reload CSV는 Inspector 배열의 표들을 다시 읽습니다. Load로 추가했던 다른 표도 유지합니다.

```csharp
var units = CSVManager.Instance.GetTable("Units");
var knight = units.GetById("knight");
float hp = knight.Get<float>("unitMaxHp"); // 이미 float로 변환된 값 조회
object typedValue = knight["unitMaxHp"]; // 실제 내용은 float

// id 열은 선택사항. ID 없는 설정/스테이지 CSV도 사용 가능
var firstRow = CSVManager.Instance.GetTable("Settings").Rows[0];
// 씬 없이 사용
var table = CSVManager.Parse(csvText, "Settings");
```

id 열이 있으면 빈 ID·중복 ID를 거부합니다. 정수 ID라면 GetById(1)처럼 같은 자료형으로 조회하세요.

## 사용자 정의 자료형
CSV 로드 전에 등록하세요. 자동 로드는 CSVManager.Awake에서 실행되므로
BeforeSceneLoad 초기화나 먼저 실행되는 Awake에서 등록해야 합니다.

```csharp
CSVManager.RegisterType<PhaseState>("phase", text =>
    (PhaseState)System.Enum.Parse(typeof(PhaseState), text.Trim(), true));
// 이후 CSV 2행에 phase를 선언하면 해당 열이 PhaseState로 자동 변환됩니다.
```

변환 함수는 값을 반환하거나 유효하지 않으면 예외를 던지면 됩니다. 자료형 이름 중복 등록은 오류입니다.
Unity 프로젝트에서 Domain Reload를 끄면 정적 등록이 유지되므로 같은 이름을 반복 등록하지 마세요.

템플릿 예제는 Knight.asset / eyeMonster.asset의 현재 값을 사용합니다.
SP는 없으며 스킬 Unity 오브젝트 참조는 포함하지 않습니다.
targetLayer는 레이어 번호가 아닌 비트 마스크입니다.
CSV의 SO 적용은 아래 에디터 메뉴에서 실행합니다. 런타임 매니저는 CSV 조회를 담당하며 SO를 자동 수정하지 않습니다.
Excel에서는 CSV UTF-8(쉼표로 분리)로 저장하세요. 쉼표·줄바꿈이 있는 셀은 따옴표로 감쌉니다.

## Unity에서 SO에 적용하기
1. Units.csv를 수정하고 CSV UTF-8로 저장합니다.
2. 상단 메뉴 **Dungeon Chess > CSV 가져오기**를 엽니다.
3. CSV 파일을 선택하고 **1. 파싱**을 누릅니다.
4. 적용 대상과 필드별 이전 값 → 새 값을 확인합니다.
5. **2. SO에 적용**을 누르면 SO가 수정되고 저장됩니다. Ctrl+Z로 되돌릴 수 있습니다.

- `assetPath`는 적용할 기존 SO의 프로젝트 경로입니다. 템플릿에 Knight/eyeMonster 경로가 연결되어 있습니다.
- `id`, `assetPath`는 SO 필드에 쓰지 않는 관리용 열입니다.
- 다른 열 이름은 SO의 직렬화된 필드 이름과 정확히 일치해야 합니다. 3행 설명은 적용하지 않습니다.
- 다른 종류의 SO도 같은 규칙으로 적용할 수 있습니다.
- 지원 필드: string, bool, int/long, float/double, Vector2, LayerMask(int), enum(string 이름).
- 스킬 참조·배열 등 지원하지 않는 필드는 오류로 표시합니다. CSV에 없는 SO 필드는 유지합니다.
- 새 SO 생성은 하지 않습니다. assetPath의 SO를 먼저 만들어야 합니다.
- 잘못된 값·필드·경로·중복 대상이 하나라도 있으면 적용하지 않습니다.
- 파싱 후 CSV나 대상 SO가 바뀌면 다시 파싱해야 합니다. 플레이 중에는 적용할 수 없습니다.