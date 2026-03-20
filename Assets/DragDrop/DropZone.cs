using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public MiniGameManager miniGameManager;
    public enum ZoneType { Table, Tray }
    public ZoneType type;
    
    public int maxItems = 9;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        DraggableFruit fruit = droppedObj.GetComponent<DraggableFruit>();

        if (fruit != null)
        {
            Transform closestSlot = FindClosestEmptySlot(droppedObj.transform.position);

            if (closestSlot != null)
            {
                // Cập nhật thông tin vị trí
                fruit.parentAfterDrag = closestSlot;
                fruit.originalContainer = closestSlot; 
                fruit.currentZone = this;

                if (type == ZoneType.Table)
                {
                    fruit.lastTableContainer = closestSlot;
                }

                fruit.wasDroppedSuccessfully = true; 

                droppedObj.transform.SetParent(closestSlot);
                droppedObj.transform.localPosition = Vector3.zero;
                droppedObj.transform.localScale = Vector3.one;

                // XÓA HOẶC COMMENT DÒNG NÀY: Không tự động check nữa
                // if (type == ZoneType.Tray) { CheckTrayCondition(); }
            }
        }
    }

    // Hàm phụ để tìm slot trống gần nhất (tách ra cho sạch code)
    private Transform FindClosestEmptySlot(Vector3 position)
    {
        Transform closestSlot = null;
        float minDistance = float.MaxValue;
        foreach (Transform slot in transform)
        {
            if (slot.childCount > 0) continue;
            float distance = Vector3.Distance(slot.position, position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSlot = slot;
            }
        }
        return closestSlot;
    }

    // Hàm này sẽ được MiniGameManager gọi khi bấm nút Confirm
    public bool CheckResult()
    {
        DraggableFruit[] fruitsInTray = GetComponentsInChildren<DraggableFruit>();
        
        // Nếu chưa đủ số lượng quả yêu cầu thì coi như chưa xong (hoặc tùy bạn)
        if (fruitsInTray.Length < maxItems) return false;

        foreach (DraggableFruit f in fruitsInTray)
        {
            if (!f.isCorrectFruit) return false; // Chỉ cần 1 quả sai là fail
        }
        return true; 
    }
}