using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;

namespace SownInStone.UI
{
    /// <summary>
    /// Phát video giới thiệu (videomoman.mp4) toàn màn hình trước khi bắt đầu game.
    /// Video được load từ Resources/UI/videomoman.mp4.
    /// Khi video kết thúc hoặc người chơi nhấn phím bất kỳ/click chuột, callback sẽ được gọi.
    /// </summary>
    public class IntroVideoPlayer : MonoBehaviour
    {
        private VideoPlayer videoPlayer;
        private AudioSource videoAudioSource;
        private RenderTexture renderTexture;
        private System.Action onVideoFinished;
        private bool isPlaying = false;
        private bool hasFinished = false;
        private float skipCooldown = 1f; // Cho phép skip sau 1 giây

        /// <summary>
        /// Phát video intro. Gọi callback khi video kết thúc hoặc bị skip.
        /// </summary>
        public void PlayIntroVideo(System.Action onFinished)
        {
            onVideoFinished = onFinished;
            isPlaying = true;
            hasFinished = false;
            skipCooldown = 1f;

            // Tạo RenderTexture để hiển thị video qua OnGUI
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            renderTexture.Create();

            // Tạo AudioSource cho video
            videoAudioSource = gameObject.AddComponent<AudioSource>();
            videoAudioSource.playOnAwake = false;
            videoAudioSource.volume = 1f;

            // Setup VideoPlayer
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, videoAudioSource);
            videoPlayer.isLooping = false;

            // Load video từ Resources
            VideoClip clip = Resources.Load<VideoClip>("UI/videomoman");
            if (clip != null)
            {
                videoPlayer.clip = clip;
                videoPlayer.loopPointReached += OnVideoEnd;
                videoPlayer.prepareCompleted += OnVideoPrepared;
                videoPlayer.Prepare();
            }
            else
            {
                Debug.LogWarning("[IntroVideoPlayer] Không tìm thấy video Resources/UI/videomoman! Bỏ qua video.");
                FinishVideo();
            }
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.Play();
        }

        private void OnVideoEnd(VideoPlayer source)
        {
            FinishVideo();
        }

        private void Update()
        {
            if (!isPlaying || hasFinished) return;

            // Đếm thời gian cooldown trước khi cho phép skip
            if (skipCooldown > 0f)
            {
                skipCooldown -= Time.unscaledDeltaTime;
                return;
            }

            // Cho phép skip video bằng click chuột hoặc nhấn phím bất kỳ
            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame))
            {
                FinishVideo();
            }
        }

        private void OnGUI()
        {
            if (!isPlaying || hasFinished) return;

            // Vẽ nền đen toàn màn hình
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Vẽ video
            if (renderTexture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTexture, ScaleMode.ScaleAndCrop);
            }

            // Hiển thị hướng dẫn skip (sau khi hết cooldown)
            if (skipCooldown <= 0f)
            {
                GUIStyle skipStyle = new GUIStyle();
                skipStyle.normal.textColor = new Color(1f, 1f, 1f, 0.6f);
                skipStyle.fontSize = 14;
                skipStyle.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(Screen.width - 250f, Screen.height - 40f, 230f, 25f), "Nhấn phím bất kỳ để bỏ qua...", skipStyle);
            }
        }

        private void FinishVideo()
        {
            if (hasFinished) return;
            hasFinished = true;
            isPlaying = false;

            // Dọn dẹp VideoPlayer
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.loopPointReached -= OnVideoEnd;
                videoPlayer.prepareCompleted -= OnVideoPrepared;
                Destroy(videoPlayer);
            }

            // Dọn dẹp AudioSource
            if (videoAudioSource != null)
            {
                Destroy(videoAudioSource);
            }

            // Dọn dẹp RenderTexture
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }

            // Gọi callback
            onVideoFinished?.Invoke();

            // Tự hủy component
            Destroy(this);
        }

        private void OnDestroy()
        {
            // Đảm bảo dọn dẹp tài nguyên khi bị hủy bất ngờ
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
