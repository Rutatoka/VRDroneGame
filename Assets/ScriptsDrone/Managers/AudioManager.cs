using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music Sources")]
    [SerializeField] private AudioSource _musicSource;      // Основной канал для фона
    [SerializeField] private AudioSource _sfxSource;        // Канал для коротких звуков (не перебивает музыку)

    [Header("Audio Clips")]
    [SerializeField] private AudioClip _menuTheme;
    [SerializeField] private AudioClip _droneLoop;          // Жужжание дрона в основном уровне
    [SerializeField] private AudioClip _victoryFanfare;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _buttonClick;
    [SerializeField] private AudioClip _coinPickup;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 0.7f;

    private Coroutine _volumeFadeRoutine;
    private AudioClip _currentMusicClip;
    private DynamicDroneAudio _dynamicDrone;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать между сценами
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _musicSource.loop = true;
        _sfxSource.loop = false;
        _musicSource.volume = _musicVolume;
        _sfxSource.volume = _sfxVolume;
    }

    // === Фоновая музыка ===
    public void PlayMenuMusic()
    {
        PlayBackgroundMusic(_menuTheme);
    }

    public void PlayDroneMusic(GameObject droneObject)
    {
        _dynamicDrone = droneObject.GetComponent<DynamicDroneAudio>();
        if (_dynamicDrone == null)
            _dynamicDrone = droneObject.AddComponent<DynamicDroneAudio>();

        // Настраиваем AudioSource из менеджера
        _dynamicDrone.enabled = true;
    }
    public void SetDroneDamaged(bool damaged)
    {
        if (_dynamicDrone != null)
            _dynamicDrone.SetDroneDamaged(damaged);
    }

    public void DroneBoost(float duration)
    {
        if (_dynamicDrone != null)
            _dynamicDrone.BoostEffect(duration);
    }
    private void PlayBackgroundMusic(AudioClip clip)
    {
        if (_currentMusicClip == clip && _musicSource.isPlaying) return;

        _currentMusicClip = clip;
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    // === Плавное изменение громкости ===
    public void FadeOutMusic(float duration, bool stopAfterFade = true)
    {
        if (_volumeFadeRoutine != null) StopCoroutine(_volumeFadeRoutine);
        _volumeFadeRoutine = StartCoroutine(FadeVolume(_musicSource, _musicSource.volume, 0f, duration, stopAfterFade));
    }

    public void FadeInMusic(float duration)
    {
        if (_volumeFadeRoutine != null) StopCoroutine(_volumeFadeRoutine);
        _musicSource.volume = 0f;
        _musicSource.Play();
        _volumeFadeRoutine = StartCoroutine(FadeVolume(_musicSource, 0f, _musicVolume, duration, false));
    }

    public void FadeDroneToVolume(float targetVolume, float duration)
    {
        if (_musicSource.clip == _droneLoop && _volumeFadeRoutine != null)
            StopCoroutine(_volumeFadeRoutine);

        _volumeFadeRoutine = StartCoroutine(FadeVolume(_musicSource, _musicSource.volume, targetVolume, duration, false));
    }

    private IEnumerator FadeVolume(AudioSource source, float startVol, float endVol, float duration, bool stopAfterFade)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
            yield return null;
        }
        source.volume = endVol;

        if (stopAfterFade && endVol <= 0f)
            source.Stop();

        _volumeFadeRoutine = null;
    }

    // === SFX (короткие звуки) ===
    public void PlayButtonClick()
    {
        PlaySFX(_buttonClick);
    }

    public void PlayCoinPickup()
    {
        PlaySFX(_coinPickup);
    }

    public void PlayVictory()
    {
        PlayBackgroundMusic(_victoryFanfare);
        FadeDroneToVolume(0.2f, 1.5f); // Приглушаем дрон до 20% громкости
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, _sfxVolume);
    }

    // Для любых других UI звуков
    public void PlayCustomSFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            _sfxSource.PlayOneShot(clip, volume * _sfxVolume);
    }
}