using UnityEngine;
using System.Collections.Generic;

namespace SownInStone.Audio
{
    /// <summary>
    /// Bộ quản lý âm thanh trung tâm của game (Music, Ambient, SFX).
    /// Hỗ trợ tự động tải tệp từ thư mục Assets/Resources/Audio/ bằng cơ chế Resources.Load.
    /// Tự động bỏ qua và ghi nhật ký cảnh báo nếu thiếu tệp âm thanh (không gây crash/lỗi).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        private AudioSource musicSource;
        private AudioSource ambientSource;
        private List<AudioSource> sfxSources = new List<AudioSource>();

        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private HashSet<string> missingClips = new HashSet<string>();

        private float musicVolume = 0.5f;
        private float sfxVolume = 1f;

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                PlayerPrefs.Save();
                if (musicSource != null) musicSource.volume = musicVolume;
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Load volumes
                musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

                InitializeSources();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSources()
        {
            // Thiết lập kênh nhạc nền (BGM)
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            // Thiết lập kênh âm thanh môi trường dông bão (Ambient)
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        /// <summary>
        /// Phát nhạc nền (BGM). Tự động loop.
        /// </summary>
        public void PlayMusic(string clipName)
        {
            AudioClip clip = GetAudioClip(clipName);
            
            // Fallback nếu thiếu bgm_menu thì lấy tạm bgm_main chơi đỡ im lặng
            if (clip == null && clipName == "bgm_menu")
            {
                Debug.LogWarning("[AUDIO] Thiếu bgm_menu! Tự động sử dụng bgm_main làm fallback.");
                clip = GetAudioClip("bgm_main");
            }

            if (clip == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
            Debug.Log($"[AUDIO] Đang phát Nhạc nền BGM: {clipName}");
        }

        /// <summary>
        /// Dừng phát nhạc nền.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Phát âm thanh môi trường dông bão, gió rít (Ambient). Tự động loop.
        /// </summary>
        public void PlayAmbient(string clipName, float volume = 1f)
        {
            AudioClip clip = GetAudioClip(clipName);
            if (clip == null) return;

            if (ambientSource.clip == clip && ambientSource.isPlaying)
            {
                ambientSource.volume = volume * musicVolume;
                return;
            }

            ambientSource.clip = clip;
            ambientSource.volume = volume * musicVolume;
            ambientSource.Play();
            Debug.Log($"[AUDIO] Đang phát Âm thanh môi trường Ambient: {clipName} (Âm lượng: {volume * musicVolume})");
        }

        /// <summary>
        /// Dừng âm thanh môi trường.
        /// </summary>
        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        /// <summary>
        /// Phát hiệu ứng âm thanh ngắn (SFX) không lặp.
        /// Sử dụng Object Pooling để tái sử dụng AudioSource tránh rác bộ nhớ.
        /// </summary>
        public void PlaySFX(string clipName, float volume = 1f)
        {
            AudioClip clip = GetAudioClip(clipName);
            
            // Fallback nếu thiếu tiếng sfx_dig thì phát sfx_clear_rocks đỡ im lặng
            if (clip == null && clipName == "sfx_dig")
            {
                clip = GetAudioClip("sfx_clear_rocks");
            }

            // Fallback hỗ trợ linh hoạt giữa sfx_coins và sfx_coin
            if (clip == null && clipName == "sfx_coins")
            {
                clip = GetAudioClip("sfx_coin");
            }
            else if (clip == null && clipName == "sfx_coin")
            {
                clip = GetAudioClip("sfx_coins");
            }

            if (clip == null) return;

            // Tìm AudioSource đang rảnh trong pool
            AudioSource source = null;
            foreach (var src in sfxSources)
            {
                if (src != null && !src.isPlaying)
                {
                    source = src;
                    break;
                }
            }

            // Nếu không tìm thấy, tạo thêm AudioSource mới
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
                sfxSources.Add(source);
            }

            source.clip = clip;
            source.volume = volume * sfxVolume;
            source.loop = false;
            source.Play();
            Debug.Log($"[AUDIO] Đang phát SFX: {clipName} (Âm lượng: {volume * sfxVolume})");
        }

        /// <summary>
        /// Lấy tài nguyên AudioClip động từ Resources/Audio. Caching để tối ưu hóa.
        /// </summary>
        private AudioClip GetAudioClip(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return null;

            if (audioClips.ContainsKey(clipName))
            {
                return audioClips[clipName];
            }

            AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipName}");
            if (clip != null)
            {
                audioClips[clipName] = clip;
                Debug.Log($"[AUDIO] Tải thành công tệp âm thanh: '{clipName}' từ Resources/Audio/");
            }
            else
            {
                if (!missingClips.Contains(clipName))
                {
                    missingClips.Add(clipName);
                    Debug.LogWarning($"[AUDIO] Cảnh báo: Không tìm thấy tệp âm thanh ở Assets/Resources/Audio/{clipName}. Âm thanh này sẽ bị bỏ qua.");
                }
            }
            return clip;
        }
    }
}
