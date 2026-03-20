# HƯỚNG DẪN SETUP NHIỆM VỤ: ĐÁNH BẠI TÍ VUA Ô ĂN QUAN

## Tổng quan

Nhiệm vụ này yêu cầu người chơi nói chuyện với NPC "Tí Vua", chấp nhận thử thách chơi Ô Ăn Quan, và **đánh bại AI** trong mini-game. Khi thắng sẽ nhận thưởng. Khi thua có thể chơi lại.

**Luồng nhiệm vụ:**
1. Player đến gần NPC Tí Vua → bấm `F` → hội thoại thử thách
2. Player chọn "Chấp nhận" → mini-game Ô Ăn Quan khởi động
3. Nếu **thắng** → quay lại game lớn → hiện hội thoại chiến thắng + nhận thưởng
4. Nếu **thua** → quay lại game lớn → hiện hội thoại thua + có thể thử lại
5. Sau khi hoàn thành → các lần gặp sau hiện hội thoại after quest

---

## 1/ Chuẩn bị sẵn các thứ sau

- NPC đã có model trong scene: `TiVua`
- Đã có `MoneyManager` trong scene
- Đã có `Inventory` trong scene
- Đã có mini-game Ô Ăn Quan hoạt động (folder `Assets/MiniGame/`)
- Đã có Prefab `OQuanRoot` (toàn bộ mini-game) trong scene, **tắt sẵn** (inactive)

> **Lưu ý:** Đảm bảo `OQuanBridge.cs`, `OQuanLauncher.cs`, `GameState.cs`, `GameManager.cs` trong folder `Assets/MiniGame/Scripts/Client/` đều hoạt động bình thường.

---

## 2/ Tạo conversation intro cho NPC Tí Vua

- Tạo object tên `TiVuaConversation`
- Gắn `NPC Conversation (Script)` vào
- Viết lời thoại, ví dụ:
  - *"Hehehe, ta là Tí Vua — vô địch ô ăn quan cả làng! Ngươi có dám thách đấu với ta không?"*
- Tạo 2 option:
  - **Option 1 (chấp nhận):** *"Được! Ta sẽ đánh bại ngươi!"*
  - **Option 2 (từ chối):** *"Để lúc khác nhé."*

---

## 3/ Tạo trigger để nói chuyện

- Tạo child của `TiVuaConversation`, đặt tên `ConversationTrigger`
- Gắn `ConversationStarter` vào child này
- Gắn `Sphere Collider` → tick `Is Trigger`, chỉnh Radius vừa đủ
- Trong `ConversationStarter`, kéo `TiVuaConversation` vào ô `My Conversation`

---

## 4/ Tạo các conversation còn lại

Tạo thêm các conversation sau (**không cần** `ConversationTrigger` ở bản copy):

| Object | Mục đích | Nội dung gợi ý |
|---|---|---|
| `TiVuaReminderConversation` | Khi player đã nhận thử thách nhưng chưa chơi | *"Sao? Sợ hả? Đánh đi!"* |
| `TiVuaWinConversation` | Khi player THẮNG mini-game | *"Ngươi... ngươi giỏi thật! Ta thua tâm phục khẩu phục!"* |
| `TiVuaLoseConversation` | Khi player THUA mini-game | *"Hahaha! Ngươi còn non lắm! Muốn thử lại không?"* |
| `TiVuaAfterQuestConversation` | Sau khi nhiệm vụ hoàn thành hẳn | *"Ngươi là cao thủ thật sự! Ta nể ngươi."* |

### 4.1 Với `TiVuaLoseConversation`
- Nên có 2 option:
  - **"Thử lại!"** → gắn event `Mission4OQuanController.MarkChallengeAccepted()` (script sẽ tự reset board + bật lại mini-game khi hội thoại kết thúc)
  - **"Thôi, để sau."** → thoát hội thoại bình thường (lần sau gặp Tí sẽ hiện lại LoseConversation)

---

## 5/ Script `Mission4OQuanController.cs` (đã tạo sẵn)

File đã có tại `Assets/Scripts/Mission/Mission4OQuanController.cs`.

**Cấu trúc giống hệt Mission 1, 2, 3:**
- Gắn cùng object với `ConversationStarter` (ConversationTrigger)
- Implement `IConversationOverrideProvider` → `ConversationStarter` tự gọi để biết hiện conversation nào
- Player vẫn bấm `F` để nói chuyện (do `ConversationStarter` xử lý)
- Script chỉ thêm logic: launch mini-game khi hội thoại kết thúc + nhận kết quả qua `OQuanBridge`
- **KHÔNG cần** `OQuanLauncher` riêng (tránh chồng logic)

---

## 6/ Setup trong Unity Editor

### 6.1 Cấu trúc object (giống NPC khác)

```
TiVuaConversation              ← NPC Conversation (Script) — hội thoại intro
  └── ConversationTrigger      ← ConversationStarter
                               ← Sphere Collider (Is Trigger ✓)
                               ← Mission4OQuanController
```

- Tạo child `ConversationTrigger` giống hệt các NPC khác
- Gắn `ConversationStarter` → kéo `TiVuaConversation` vào ô `My Conversation`
- Gắn `Sphere Collider` → tick `Is Trigger`, chỉnh Radius vừa đủ
- Gắn `Mission4OQuanController` vào **cùng object** `ConversationTrigger`

### 6.2 Gắn reference cho `Mission4OQuanController`

| Field | Kéo vào |
|---|---|
| `Conversation Starter` | component `ConversationStarter` ở `ConversationTrigger` |
| `Money Manager` | object đang giữ script `MoneyManager` |
| `Oquan Root` | GameObject `OQuanRoot` (mini-game, đang tắt sẵn) |
| `Main Game UI` | Canvas UI game lớn (thường là `Canvas_Inventory` hoặc canvas chính) |
| `Main Camera` | Camera chính của game lớn |
| `Player Controller` | Script điều khiển nhân vật (VD: `FirstPersonController`) |
| `Intro Conversation` | `TiVuaConversation` |
| `Reminder Conversation` | `TiVuaReminderConversation` |
| `Win Conversation` | `TiVuaWinConversation` |
| `Lose Conversation` | `TiVuaLoseConversation` |
| `After Quest Conversation` | `TiVuaAfterQuestConversation` |
| `Reward Money` | `30` (hoặc số tuỳ ý) |

### 6.3 Gắn event cho option chấp nhận thử thách

- Mở `TiVuaConversation` trong DialogueEditor
- Tìm option *"Được! Ta sẽ đánh bại ngươi!"*
- Ở phần `Event` → thêm event mới
- Target: object `ConversationTrigger`
- Hàm: `Mission4OQuanController → MarkChallengeAccepted()`
- Mode: `Runtime`

### 6.4 Gắn event cho option "Thử lại" trong LoseConversation

- Mở `TiVuaLoseConversation` trong DialogueEditor
- Tìm option *"Thử lại!"*
- Ở phần `Event` → thêm event mới
- Target: object `ConversationTrigger`
- Hàm: `Mission4OQuanController → MarkChallengeAccepted()`
- Mode: `Runtime`

---

## 7/ Cách hệ này hoạt động

```
Player đến gần Tí Vua → bấm F
        │
        ▼
  Hội thoại Intro
  "Dám thách đấu không?"
        │
    ┌───┴───┐
    │       │
 Chấp nhận  Từ chối
    │       │
    ▼       ▼
MarkChallengeAccepted()  (thoát)
    │
    ▼
Hội thoại kết thúc → LaunchMiniGame()
    │
    ▼
┌─────────────────────┐
│  MINI-GAME Ô ĂN QUAN │
│  (Player vs AI)      │
└─────────┬───────────┘
          │
    ┌─────┴─────┐
    │           │
  THẮNG       THUA
    │           │
    ▼           ▼
 +30 vàng    LoseConversation
 WinConversation  "Thử lại?"
    │           │
    ▼       ┌───┴───┐
 Completed  Thử lại   Thôi
            → quay    → thoát
            lại chơi
```

---

## 8/ Test nhanh

### 8.1 Test nhận thử thách
- Play game → đến NPC Tí Vua → bấm `F`
- Chọn "Chấp nhận"
- Thoát hội thoại → mini-game phải bật lên
- UI game lớn phải ẩn, con trỏ chuột phải hiện

### 8.2 Test thắng
- Chơi và thắng AI
- Mini-game phải tắt, game lớn phải bật lại
- Hội thoại Win phải hiện
- Tiền phải cộng thêm (kiểm tra MoneyManager)

### 8.3 Test thua
- Chơi và thua AI
- Mini-game phải tắt, game lớn phải bật lại
- Hội thoại Lose phải hiện
- Chọn "Thử lại" → mini-game phải bật lại

### 8.4 Test after quest
- Sau khi đã thắng, quay lại Tí Vua → bấm `F`
- Phải hiện `AfterQuestConversation`

---

## 9/ Nếu bị lỗi kiểm tra mấy chỗ này trước

- `OQuanRoot` đã kéo đúng vào `Mission4OQuanController` chưa (phải là GameObject chứa toàn bộ mini-game)
- `OQuanRoot` có đang **tắt sẵn** (inactive) trong scene không
- Event option chấp nhận đã gắn đúng `MarkChallengeAccepted()` chưa
- `Mission4OQuanController` và `ConversationStarter` có đang cùng 1 object không
- Con trỏ chuột có bị khoá sau khi thoát mini-game không (nếu game FPS dùng `Cursor.lockState`)
- Có bị trùng `EventSystem` hoặc `AudioListener` giữa game lớn và mini-game không → xóa bớt ở mini-game
- Nếu mini-game không hiện UI: kiểm tra Canvas của mini-game có đúng sort order cao hơn game lớn không
- `OQuanBridge.OnMiniGameEnd` có bị null không → đảm bảo đăng ký callback trước khi bật `oquanRoot`
