using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Thêm thư viện này để chuyển cảnh

public class VideoManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private VideoPlayer _videoPlayer;
    
    [Header("Settings")]
    [SerializeField] private string _nextSceneName;
    void Start()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }
        else
        {
            Debug.LogWarning("Bạn chưa nhập tên Scene tiếp theo trong Inspector!");
        }
    }

    void OnDestroy()
    {
        if (_videoPlayer != null)
            _videoPlayer.loopPointReached -= OnVideoEnd;
    }
}