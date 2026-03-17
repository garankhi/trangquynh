# HƯỚNG DẪN SETUP NHIỆM VỤ 2: ĐƯA THƯ

## 1/ Chuẩn bị sẵn các thứ sau

- 2 NPC đã có model trong scene: `OngDuaThu` và `OngQuanTong`
- Đã có `MoneyManager` trong scene
- Đã có `Inventory` trong scene
- Đã có item asset: `Assets/Item/Thu/Thu.asset`

---

## 2/ Tạo item asset Tờ Thư

- Tạo folder `Assets/Item/Thu/`
- Chuột phải trong folder → Create → **New Item** → đặt tên `Thu`
- Thêm icon cho tờ thư (tách nền, đặt Texture type: Sprite → Apply → kéo vào ô Icon của item)
- **Lưu ý:** Tờ thư **không cần đặt lên scene**. Script sẽ tự thêm vào Inventory khi player nhận nhiệm vụ.

---

## 3/ Setup NPC 1: Ông Đưa Thư (NPC giao nhiệm vụ)

### 3.1 Tạo conversation intro

- Tạo object tên `OngDuaThuConversation`
- Gắn `NPC Conversation (Script)` vào
- Viết lời thoại: *"Cháu ơi, ta có bức thư cần gửi đến Ông Quan Tổng trong làng. Cháu có thể giúp ta mang đến không?"*
- Tạo option nhận nhiệm vụ: *"Vâng! Cháu sẽ mang ngay."*

### 3.2 Tạo trigger

- Tạo child của `OngDuaThuConversation`, đặt tên `ConversationTrigger`
- Gắn `ConversationStarter` → kéo `OngDuaThuConversation` vào ô `My Conversation`
- Gắn `Sphere Collider` → tick `Is Trigger`, chỉnh Radius vừa đủ
- Gắn script `Mission2Controller` vào **cùng object** `ConversationTrigger`

### 3.3 Tạo conversation after quest

Tạo thêm 2 conversation (không cần `ConversationTrigger` ở bản copy):

| Object | Nội dung gợi ý |
|---|---|
| `OngDuaThuReminderConversation` | *"Cháu chưa giao thư à? Nhớ mang đến Ông Quan Tổng nhé!"* |
| `OngDuaThuAfterQuestConversation` | *"Cảm ơn cháu! Thư đã được giao rồi nhé."* |

### 3.4 Gắn reference cho `Mission2Controller`

| Field | Kéo vào |
|---|---|
| `Conversation Starter` | component `ConversationStarter` ở `ConversationTrigger` |
| `Inventory` | object đang giữ script `Inventory` |
| `Thu Item` | `Assets/Item/Thu/Thu.asset` |
| `Checklist Panel` | Panel UI checklist (xem bước 5) |
| `Intro Conversation` | `OngDuaThuConversation` |
| `Reminder Conversation` | `OngDuaThuReminderConversation` |
| `After Quest Conversation` | `OngDuaThuAfterQuestConversation` |

### 3.5 Gắn event nhận nhiệm vụ

- Mở `OngDuaThuConversation`
- Tìm đúng option nhận nhiệm vụ
- Ở phần `Event` → thêm event mới
- Target: object `ConversationTrigger`
- Hàm: `Mission2Controller → MarkIntroAccepted()`
- Mode: `Runtime`

---

## 4/ Setup NPC 2: Ông Quan Tổng (NPC nhận thư)

### 4.1 Tạo conversation

- Tạo object tên `OngQuanTongConversation`
- Gắn `NPC Conversation (Script)` vào
- Viết lời thoại bình thường (khi chưa có nhiệm vụ): *"Cháu cần gì không?"*

### 4.2 Tạo trigger

- Tạo child `ConversationTrigger`
- Gắn `ConversationStarter` → kéo `OngQuanTongConversation` vào `My Conversation`
- Gắn `Sphere Collider` → tick `Is Trigger`, chỉnh Radius
- Gắn script `Mission2TurnInController` vào **cùng object** `ConversationTrigger`

### 4.3 Tạo các conversation còn lại

| Object | Nội dung gợi ý |
|---|---|
| `OngQuanTongReminderConversation` | *"Cháu có thư của ta không? Mang sang đây nhanh!"* |
| `OngQuanTongCompleteConversation` | *"À thư rồi! Cảm ơn cháu, đây là thưởng cho cháu."* |
| `OngQuanTongAfterQuestConversation` | *"Chào cháu Quỳnh!"* |

### 4.4 Gắn reference cho `Mission2TurnInController`

| Field | Kéo vào |
|---|---|
| `Conversation Starter` | component `ConversationStarter` ở `ConversationTrigger` |
| `Inventory` | object đang giữ script `Inventory` |
| `Money Manager` | object đang giữ script `MoneyManager` |
| `Thu Item` | `Assets/Item/Thu/Thu.asset` |
| `Default Idle Conversation` | `OngQuanTongConversation` |
| `Reminder Conversation` | `OngQuanTongReminderConversation` |
| `Complete Conversation` | `OngQuanTongCompleteConversation` |
| `After Quest Conversation` | `OngQuanTongAfterQuestConversation` |
| `Reward Money` | `10` |

---

## 5/ Tạo UI Checklist (1 dòng)

- Vào `Canvas_Inventory` → tìm object `Inventory`
- Tạo 1 UI Panel con, đặt tên `MissionChecklistPanel_Quest2`
- Bên trong tạo:
  - `Title` text: `"Nhiệm vụ"`
  - `ItemRow_Letter`:
    - Text label: `"Tờ thư"`
    - Tick image (ẩn ban đầu)
- **Để panel này tắt ban đầu** (script sẽ bật khi nhận nhiệm vụ)
- Kéo panel này vào ô `Checklist Panel` của `Mission2Controller` (bước 3.4)

---

## 6/ Cách hệ này hoạt động

1. Player đến gần **Ông Đưa Thư** → bấm `F`
2. `ConversationStarter` mở hội thoại intro
3. Player chọn option nhận nhiệm vụ → event gọi `Mission2Controller.MarkIntroAccepted()`
4. Hội thoại kết thúc → `Mission2Controller` thêm `Tờ thư` vào Inventory + bật Checklist Panel
5. Player đến **Ông Quan Tổng** → bấm `F`
   - Nếu chưa có thư: hiện ReminderConversation
   - Nếu có thư: trừ thư, cộng 10 vàng, hoàn thành mission, hiện CompleteConversation
6. Các lần sau gặp cả 2 NPC đều hiện AfterQuestConversation

---

## 7/ Test nhanh

### 7.1 Test nhận nhiệm vụ
- Play game → đến Ông Đưa Thư → `F` → chọn option nhận
- Thoát hội thoại → kiểm tra Inventory có `Tờ thư` chưa
- Kiểm tra Checklist Panel có hiện chưa

### 7.2 Test khi chưa có thư
- Đến Ông Quan Tổng trước khi nhận nhiệm vụ → `F` → phải hiện conversation bình thường (idle)
- Đến Ông Quan Tổng sau khi đã nhận nhiệm vụ nhưng thư bị mất → phải hiện ReminderConversation

### 7.3 Test giao thư
- Đến Ông Quan Tổng khi đã có thư → `F`
- Kiểm tra: Inventory không còn thư, tiền cộng thêm 10, panel ẩn, hiện CompleteConversation

### 7.4 Test after quest
- Quay lại cả 2 NPC → `F` → phải hiện AfterQuestConversation

---

## 8/ Nếu bị lỗi kiểm tra mấy chỗ này trước

- Option nhận nhiệm vụ đã gắn event `MarkIntroAccepted()` chưa
- `Mission2Controller` và `ConversationStarter` có đang cùng 1 object không
- `Mission2TurnInController` có kéo đúng `Thu.asset` chưa (phải cùng asset với `Mission2Controller`)
- Có bị 2 `ConversationTrigger` active cùng lúc không
- `Mission2Controller.Instance` có null không (đảm bảo object Ông Đưa Thư được active trước Ông Quan Tổng)
