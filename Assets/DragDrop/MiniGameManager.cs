using UnityEngine;
using UnityEngine.Events;
using GinjaGaming.FinalCharacterController; 

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;
    public GameObject dragDropCanvas;
    public GameObject playerObject;

    public DropZone trayZone; 
    public UnityEvent onWinGameEvent;

    [Header("Result UI")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject winTextObject;
    [SerializeField] private GameObject failTextObject;
    [SerializeField] private float resultDisplaySeconds = 5f;

    private Inventory inventoryScript; 
    private bool isResolvingResult;
    private Coroutine resultFlowRoutine;

    public bool IsResolvingResult => isResolvingResult;

    private void Awake() => Instance = this;

    private void Start()
    {
        inventoryScript = FindAnyObjectByType<Inventory>();
        TryAutoAssignResultUI();
        ResetResultUI();
    }

    public void OpenMiniGame()
    {
        if (resultFlowRoutine != null)
        {
            StopCoroutine(resultFlowRoutine);
            resultFlowRoutine = null;
        }

        isResolvingResult = false;
        ResetResultUI();
        dragDropCanvas.SetActive(true);

      
        if (inventoryScript != null)
        {
            inventoryScript.enabled = false;
        }

        PlayerController controller = playerObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetCameraControlEnabled(false);
            controller.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseMiniGame()
    {
        dragDropCanvas.SetActive(false);

        if (inventoryScript != null)
        {
            inventoryScript.enabled = true;
        }

        PlayerController controller = playerObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = true;
            controller.SetCameraControlEnabled(true);
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResolveTrayResult(bool isWin, UnityEvent onWinGame = null)
    {
        if (isResolvingResult)
        {
            return;
        }

        if (resultFlowRoutine != null)
        {
            StopCoroutine(resultFlowRoutine);
        }

        resultFlowRoutine = StartCoroutine(isWin ? WinFlow(onWinGame) : LoseFlow());
    }

    private System.Collections.IEnumerator WinFlow(UnityEvent onWinGame)
    {
        isResolvingResult = true;
        SetGameplayPanelVisible(false);
        SetResultTextVisibility(win: true, fail: false, unable: false);

        yield return new WaitForSeconds(resultDisplaySeconds);

        SetResultTextVisibility(win: false, fail: false, unable: false);
        CloseMiniGame(); // Return to main gameplay scene state
        onWinGame?.Invoke();

        isResolvingResult = false;
        resultFlowRoutine = null;
    }

    private System.Collections.IEnumerator LoseFlow()
    {
        isResolvingResult = true;
        SetGameplayPanelVisible(false);
        SetResultTextVisibility(win: false, fail: true, unable: false);

        yield return new WaitForSeconds(resultDisplaySeconds);

        SetResultTextVisibility(win: false, fail: false, unable: false);
        SetGameplayPanelVisible(true);

        isResolvingResult = false;
        resultFlowRoutine = null;
    }

    private void ResetResultUI()
    {
        SetGameplayPanelVisible(true);
        SetResultTextVisibility(win: false, fail: false, unable: false);
    }

    private void SetGameplayPanelVisible(bool visible)
    {
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(visible);
        }
    }

    private void SetResultTextVisibility(bool win, bool fail, bool unable)
    {
        if (winTextObject != null) winTextObject.SetActive(win);
        if (failTextObject != null) failTextObject.SetActive(fail);
    }

    private void TryAutoAssignResultUI()
    {
        if (dragDropCanvas == null)
        {
            return;
        }

        if (gameplayPanel == null)
        {
            gameplayPanel = FindChildRecursive(dragDropCanvas.transform, "Panel");
        }

        if (winTextObject == null)
        {
            winTextObject = FindChildRecursive(dragDropCanvas.transform, "Win");
        }

        if (failTextObject == null)
        {
            failTextObject = FindChildRecursive(dragDropCanvas.transform, "Fail");
        }
    }

    private GameObject FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == childName)
            {
                return child.gameObject;
            }

            GameObject found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
    public void ConfirmResult()
    {
        if (isResolvingResult || trayZone == null) return;

        bool isWin = trayZone.CheckResult();
        
        // Gọi Coroutine Win/Lose flow như bạn đã viết
        ResolveTrayResult(isWin, onWinGameEvent);
    }
}