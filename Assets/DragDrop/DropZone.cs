using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public MiniGameManager miniGameManager;
    public UnityEvent OnWinGame;
    public enum ZoneType { Table, Tray }
    public ZoneType type; // Đánh dấu đây là Bàn hay Mâm
    
    public int maxItems = 9; // Mâm thì chứa 5, Bàn chứa 8

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        DraggableFruit fruit = droppedObj.GetComponent<DraggableFruit>();

        if (fruit != null)
        {
            // 1. Tìm xem có Slot nào còn trống không
            Transform emptySlot = null;
            foreach (Transform child in transform)
            {
                // Nếu Slot này chưa có "trái cây" nào bên trong
                if (child.childCount == 0) 
                {
                    emptySlot = child;
                    break;
                }
            }

            // 2. Nếu tìm thấy Slot trống
            if (emptySlot != null)
            {
                // Nhận quả này làm con của SLOT (thay vì làm con của Tray)
                fruit.parentAfterDrag = emptySlot; 
                
                if (type == ZoneType.Tray)
                {
                    CheckTrayCondition();
                }
            }
            else
            {
                Debug.Log("Mâm đã đầy 5 chỗ!");
            }
        }
    }

    private void CheckTrayCondition()
    {
        // Đợi đến cuối frame để Unity cập nhật xong danh sách con (childCount)
        Invoke(nameof(EvaluateWin), 0.1f); 
    }

    private void EvaluateWin()
    {
        // Tìm tất cả Script DraggableFruit nằm trong Tray (bao gồm cả trong các Slot)
        DraggableFruit[] fruitsInTray = GetComponentsInChildren<DraggableFruit>();

        if (fruitsInTray.Length < 5) return;

        int correctCount = 0;
        foreach (DraggableFruit f in fruitsInTray)
        {
            if (f.isCorrectFruit) correctCount++;
        }

        if (correctCount == 5)
        {
            Debug.Log("Chúc mừng!");
            MiniGameManager.Instance.CloseMiniGame();
            OnWinGame?.Invoke(); 
        }
        else
        {
            // Có thể thêm hiệu ứng rung mâm hoặc hiện text thông báo "Sai quả rồi"
            Debug.Log("Mâm đủ 5 quả nhưng có quả sai!");
        }
    }
}