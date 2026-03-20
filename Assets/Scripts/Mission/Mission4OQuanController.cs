using System.Collections;
using DialogueEditor;
using UnityEngine;

/// <summary>
/// Nhiệm vụ: Đánh bại Tí Vua Ô Ăn Quan
/// Gắn vào ConversationTrigger của NPC Tí (cùng object với ConversationStarter).
/// 
/// Luồng: Intro → chấp nhận → hội thoại đóng → mini-game bật → 
///        thắng (thưởng + WinConversation) / thua (LoseConversation + thử lại)
/// </summary>
public class Mission4OQuanController : MonoBehaviour, IConversationOverrideProvider
{
    private enum MissionState
    {
        NotStarted,
        ChallengeAccepted,
        Playing,
        Won,
        Lost,
        Completed
    }

    [Header("References")]
    [SerializeField] private ConversationStarter conversationStarter;
    [SerializeField] private MoneyManager moneyManager;

    [Header("Mini-Game")]
    [SerializeField] private GameObject oquanRoot;            // GameObject chứa toàn bộ mini-game (tắt sẵn)
    [SerializeField] private GameObject mainGameUI;           // Canvas UI game lớn
    [SerializeField] private Camera mainCamera;               // Camera chính
    [SerializeField] private MonoBehaviour playerController;  // Script điều khiển nhân vật (VD: FirstPersonController)

    [Header("Conversations")]
    [SerializeField] private NPCConversation introConversation;
    [SerializeField] private NPCConversation reminderConversation;
    [SerializeField] private NPCConversation winConversation;
    [SerializeField] private NPCConversation loseConversation;
    [SerializeField] private NPCConversation afterQuestConversation;

    [Header("Reward")]
    [SerializeField] private int rewardMoney = 30;

    private MissionState state = MissionState.NotStarted;
    private bool challengeAccepted;

    private void Awake()
    {
        if (conversationStarter == null)
            conversationStarter = GetComponent<ConversationStarter>();

        if (introConversation == null && conversationStarter != null)
            introConversation = conversationStarter.DefaultConversation;
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += HandleConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= HandleConversationEnded;
    }

    // ===================================================================
    // PUBLIC: Gắn vào Event của option "Chấp nhận" trong Intro / Lose
    // ===================================================================

    /// <summary>
    /// Gắn hàm này vào option "Chấp nhận" (IntroConversation)
    /// và option "Thử lại!" (LoseConversation).
    /// </summary>
    public void MarkChallengeAccepted()
    {
        if (state == MissionState.NotStarted || state == MissionState.Lost)
        {
            challengeAccepted = true;
            Debug.Log("Mission4: Thử thách đã được chấp nhận!");
        }
    }

    // ===================================================================
    // IConversationOverrideProvider — ConversationStarter sẽ gọi hàm này
    // ===================================================================

    public bool TryGetConversationOverride(NPCConversation defaultConversation, out NPCConversation conversation)
    {
        conversation = defaultConversation;

        switch (state)
        {
            case MissionState.Completed:
                conversation = afterQuestConversation != null ? afterQuestConversation : defaultConversation;
                return afterQuestConversation != null;

            case MissionState.ChallengeAccepted:
            case MissionState.Playing:
                // Đang giữa chừng: nhắc nhở
                conversation = reminderConversation != null ? reminderConversation : defaultConversation;
                return reminderConversation != null;

            case MissionState.Lost:
                // Đã thua: hiện loseConversation (có option thử lại)
                conversation = loseConversation != null ? loseConversation : defaultConversation;
                return loseConversation != null;

            case MissionState.Won:
                // Đã thắng nhưng chưa mark completed: hiện after quest
                state = MissionState.Completed;
                conversation = afterQuestConversation != null ? afterQuestConversation : defaultConversation;
                return afterQuestConversation != null;

            default: // NotStarted
                return false; // Dùng introConversation mặc định từ ConversationStarter
        }
    }

    // ===================================================================
    // PRIVATE: Xử lý khi hội thoại kết thúc
    // ===================================================================

    private void HandleConversationEnded()
    {
        if (!challengeAccepted) return;

        // Chỉ launch khi đang ở trạng thái hợp lệ
        if (state == MissionState.NotStarted || state == MissionState.Lost)
        {
            challengeAccepted = false;
            state = MissionState.ChallengeAccepted;

            // Delay 1 frame để hội thoại đóng hoàn toàn trước khi bật mini-game
            StartCoroutine(LaunchMiniGameDelayed());
        }
    }

    private IEnumerator LaunchMiniGameDelayed()
    {
        yield return null; // Đợi 1 frame
        LaunchMiniGame();
    }

    // ===================================================================
    // MINI-GAME: Launch / Callback
    // ===================================================================

    private void LaunchMiniGame()
    {
        if (oquanRoot == null)
        {
            Debug.LogError("Mission4: oquanRoot chưa được gán trong Inspector!");
            return;
        }

        state = MissionState.Playing;

        // Tạm ẩn game lớn
        if (mainGameUI != null) mainGameUI.SetActive(false);
        if (playerController != null) playerController.enabled = false;

        // Mở khoá con trỏ chuột (nếu game FPS dùng Cursor.lockState)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Đăng ký callback nhận kết quả
        OQuanBridge.OnMiniGameEnd = OnMiniGameFinished;

        // Bật mini-game
        oquanRoot.SetActive(true);

        // Reset mini-game về trạng thái ban đầu (quan trọng khi chơi lại!)
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetGame();
            Debug.Log("Mission4: Mini-game đã reset thành công.");
        }

        Debug.Log("Mission4: Mini-game Ô Ăn Quan đã khởi động!");
    }

    /// <summary>
    /// Callback từ OQuanBridge khi mini-game kết thúc.
    /// </summary>
    private void OnMiniGameFinished(bool playerWon, int playerScore, int aiScore)
    {
        // Tắt mini-game
        oquanRoot.SetActive(false);

        // Bật lại game lớn
        if (mainGameUI != null) mainGameUI.SetActive(true);
        if (playerController != null) playerController.enabled = true;

        // Khoá lại con trỏ (nếu game FPS)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"Mission4: Kết quả — Thắng: {playerWon}, Điểm: {playerScore} vs {aiScore}");

        if (playerWon)
        {
            state = MissionState.Won;

            // Cộng tiền thưởng
            if (moneyManager != null)
                moneyManager.AddMoney(rewardMoney);

            // Mở hội thoại chiến thắng
            if (winConversation != null)
                ConversationManager.Instance.StartConversation(winConversation);

            Debug.Log($"Mission4: THẮNG! Nhận {rewardMoney} vàng.");
        }
        else
        {
            state = MissionState.Lost;

            // Mở hội thoại thua (có option thử lại)
            if (loseConversation != null)
                ConversationManager.Instance.StartConversation(loseConversation);

            Debug.Log("Mission4: THUA! Có thể thử lại.");
        }
    }
}
