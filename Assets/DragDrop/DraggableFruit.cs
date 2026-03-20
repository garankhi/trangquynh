using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableFruit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
{
    [Header("Thông tin quả")]
    public string fruitName;
    public bool isCorrectFruit;
    
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform originalContainer; // 🔥 Lưu vị trí ban đầu để có thể quay về
    [HideInInspector] public Transform lastTableContainer;
    [HideInInspector] public DropZone currentZone;
    [HideInInspector] public bool wasDroppedSuccessfully = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 🔥 Lưu vị trí ban đầu TRƯỚC khi kéo
        originalContainer = transform.parent;
        DropZone originZone = originalContainer != null ? originalContainer.GetComponentInParent<DropZone>() : null;
        if (originZone != null)
        {
            currentZone = originZone;

            // Trái cây đặt sẵn trên table từ đầu scene cũng cần lưu vị trí để click từ tray quay về.
            if (originZone.type == DropZone.ZoneType.Table)
            {
                lastTableContainer = originalContainer;
            }
        }

        parentAfterDrag = transform.parent; 
        wasDroppedSuccessfully = false;

        transform.SetParent(transform.root); 
        transform.SetAsLastSibling(); 

        canvasGroup.blocksRaycasts = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 🔥 Nếu không drop vào DropZone nào, quay về vị trí BAN ĐẦU (originalContainer)
        // Không phải parentAfterDrag, vì nếu quả ở trong Slot của Tray, 
        // parentAfterDrag = Slot của Tray (vẫn trong Tray)
        if (!wasDroppedSuccessfully)
        {
            transform.SetParent(originalContainer);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Canvas UI thường không bắn OnPointerClick ổn định sau thao tác drag,
        // nên xử lý trả về ngay từ PointerDown để luôn hoạt động.
        if (TryReturnFromTrayToTable())
        {
            eventData.Use();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging) 
        {
            TryReturnFromTrayToTable();
        }
    }

    private bool TryReturnFromTrayToTable()
    {
        DropZone liveZone = transform.parent != null ? transform.parent.GetComponentInParent<DropZone>() : null;
        if (liveZone != null)
        {
            currentZone = liveZone;
        }

        if (currentZone == null || currentZone.type != DropZone.ZoneType.Tray)
        {
            return false;
        }

        if (lastTableContainer == null)
        {
            lastTableContainer = FindAnyTableSlot();
            if (lastTableContainer == null)
            {
                return false;
            }
        }

        if (IsSlotOccupiedByAnotherFruit(lastTableContainer, this))
        {
            Transform fallbackSlot = FindFallbackTableSlot();
            if (fallbackSlot == null)
            {
                return false;
            }

            lastTableContainer = fallbackSlot;
        }

        transform.SetParent(lastTableContainer);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        parentAfterDrag = lastTableContainer;
        originalContainer = lastTableContainer;
        wasDroppedSuccessfully = true;

        DropZone tableZone = lastTableContainer.GetComponentInParent<DropZone>();
        if (tableZone != null && tableZone.type == DropZone.ZoneType.Table)
        {
            currentZone = tableZone;
        }

        return true;
    }

    private Transform FindFallbackTableSlot()
    {
        DropZone tableZone = lastTableContainer.GetComponentInParent<DropZone>();
        if (tableZone == null || tableZone.type != DropZone.ZoneType.Table)
        {
            return null;
        }

        foreach (Transform slot in tableZone.transform)
        {
            if (!IsSlotOccupiedByAnotherFruit(slot, this))
            {
                return slot;
            }
        }

        return null;
    }

    private Transform FindAnyTableSlot()
    {
        DropZone[] zones = FindObjectsByType<DropZone>(FindObjectsSortMode.None);
        foreach (DropZone zone in zones)
        {
            if (zone == null || zone.type != DropZone.ZoneType.Table)
            {
                continue;
            }

            foreach (Transform slot in zone.transform)
            {
                if (!IsSlotOccupiedByAnotherFruit(slot, this))
                {
                    return slot;
                }
            }

            // Nếu table đầy, vẫn lấy slot đầu tiên làm fallback để không bị kẹt logic.
            if (zone.transform.childCount > 0)
            {
                return zone.transform.GetChild(0);
            }
        }

        return null;
    }

    private bool IsSlotOccupiedByAnotherFruit(Transform slot, DraggableFruit self)
    {
        for (int i = 0; i < slot.childCount; i++)
        {
            DraggableFruit childFruit = slot.GetChild(i).GetComponent<DraggableFruit>();
            if (childFruit != null && childFruit != self)
            {
                return true;
            }
        }

        return false;
    }
}
