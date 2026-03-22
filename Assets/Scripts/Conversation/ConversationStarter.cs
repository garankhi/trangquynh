using System.Collections.Generic;
using DialogueEditor;
using UnityEngine;

public class ConversationStarter : MonoBehaviour
{
    [SerializeField] private NPCConversation myConversation;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private GameObject talkHintPopup;
    [SerializeField] private bool playOnceWhenPlayerInsideTrigger;
    [SerializeField] private GameObject autoPlayConversationCrosshair;

    public NPCConversation DefaultConversation => myConversation;

    private IConversationOverrideProvider[] conversationOverrideProviders;
    private bool hasPlayedInsideTrigger;
    private bool shouldRestoreCrosshairAfterConversation;
    private bool crosshairWasActiveBeforeAutoPlayConversation;

    private void Awake()
    {
        List<IConversationOverrideProvider> providers = new List<IConversationOverrideProvider>();
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IConversationOverrideProvider provider)
            {
                providers.Add(provider);
            }
        }

        conversationOverrideProviders = providers.ToArray();
    }

    private void Start()
    {
        SetHintVisible(false);
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += HandleConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= HandleConversationEnded;
        SetHintVisible(false);
        RestoreCrosshairIfNeeded();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (playOnceWhenPlayerInsideTrigger)
        {
            TryPlayConversationInsideTriggerOnce();
            return;
        }

        SetHintVisible(CanStartConversation());
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (playOnceWhenPlayerInsideTrigger)
        {
            TryPlayConversationInsideTriggerOnce();
            return;
        }

        if (!CanStartConversation())
        {
            SetHintVisible(false);
            return;
        }

        SetHintVisible(true);

        if (Input.GetKeyDown(interactKey))
        {
            TryStartConversation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        SetHintVisible(false);
    }

    private bool CanStartConversation()
    {
        return ConversationManager.Instance != null && !ConversationManager.Instance.IsConversationActive;
    }

    private void TryPlayConversationInsideTriggerOnce()
    {
        if (hasPlayedInsideTrigger)
        {
            SetHintVisible(false);
            return;
        }

        SetHintVisible(false);

        if (TryStartConversation())
        {
            hasPlayedInsideTrigger = true;
        }
    }

    private void SetHintVisible(bool visible)
    {
        if (talkHintPopup == null)
        {
            return;
        }

        if (talkHintPopup.activeSelf != visible)
        {
            talkHintPopup.SetActive(visible);
        }
    }

    private bool TryStartConversation()
    {
        if (!CanStartConversation())
        {
            return false;
        }

        NPCConversation conversationToStart = GetConversationToStart();
        if (conversationToStart == null)
        {
            return false;
        }

        ConversationManager.Instance.StartConversation(conversationToStart);
        SetHintVisible(false);
        HideCrosshairForConversation();
        return true;
    }

    private void HideCrosshairForConversation()
    {
        if (autoPlayConversationCrosshair == null)
        {
            shouldRestoreCrosshairAfterConversation = false;
            return;
        }

        crosshairWasActiveBeforeAutoPlayConversation = autoPlayConversationCrosshair.activeSelf;

        if (crosshairWasActiveBeforeAutoPlayConversation)
        {
            autoPlayConversationCrosshair.SetActive(false);
        }

        shouldRestoreCrosshairAfterConversation = crosshairWasActiveBeforeAutoPlayConversation;
    }

    private void HandleConversationEnded()
    {
        RestoreCrosshairIfNeeded();
    }

    private void RestoreCrosshairIfNeeded()
    {
        if (!shouldRestoreCrosshairAfterConversation)
        {
            return;
        }

        shouldRestoreCrosshairAfterConversation = false;

        if (autoPlayConversationCrosshair != null)
        {
            autoPlayConversationCrosshair.SetActive(true);
        }
    }

    private NPCConversation GetConversationToStart()
    {
        foreach (IConversationOverrideProvider provider in conversationOverrideProviders)
        {
            if (provider != null && provider.TryGetConversationOverride(myConversation, out NPCConversation conversation) && conversation != null)
            {
                return conversation;
            }
        }

        return myConversation;
    }
}
