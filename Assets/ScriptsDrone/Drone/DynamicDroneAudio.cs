using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DynamicDroneAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _droneSource;
    [SerializeField] private AudioClip _droneBaseLoop;    // Основной гул

    [Header("Speed-Based Pitch")]
    [SerializeField] private float _minSpeed = 0f;        // Мин. скорость (м/с)
    [SerializeField] private float _maxSpeed = 20f;       // Макс. скорость
    [SerializeField] private float _minPitch = 0.8f;      // Мин. тон (медленно)
    [SerializeField] private float _maxPitch = 1.5f;      // Макс. тон (быстро)

    [Header("Altitude-Based Volume")]
    [SerializeField] private float _minAltitude = 0f;     // На земле
    [SerializeField] private float _maxAltitude = 10f;    // Высоко
    [SerializeField] private float _minVolume = 0.3f;     // Тише высоко
    [SerializeField] private float _maxVolume = 1f;       // Громко у земли

    [Header("Turn Effects")]
    [SerializeField] private float _turnPitchMultiplier = 0.3f;  // Доп. повышение тона при повороте
    [SerializeField] private float _turnSmoothing = 5f;          // Плавность эффекта

    [Header("Acceleration Effects")]
    [SerializeField] private float _accelPitchMultiplier = 0.5f; // Эффект ускорения
    [SerializeField] private float _accelSmoothing = 3f;

    [Header("Random Variations (Realism)")]
    [SerializeField] private float _randomPitchVariation = 0.03f;  // Случайные колебания
    [SerializeField] private float _randomUpdateSpeed = 0.1f;

    [Header("Filter Effects (Optional)")]
    [SerializeField] private AudioLowPassFilter _lowPassFilter;     // Приглушение на расстоянии
    [SerializeField] private float _minCutoffFreq = 500f;           // Приглушённый звук
    [SerializeField] private float _maxCutoffFreq = 22000f;         // Чёткий звук

    // Приватные переменные
    private Rigidbody _rb;
    private Vector3 _lastPosition;
    private float _currentSpeed;
    private float _currentPitch;
    private float _targetPitch;
    private float _lastSpeed;
    private float _currentAcceleration;
    private float _turnIntensity;
    private float _randomPitchOffset;
    private float _randomTimer;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogWarning("Drone needs Rigidbody for realistic audio!");
            enabled = false;
            return;
        }

        // Настройка AudioSource
        if (_droneSource == null) _droneSource = GetComponent<AudioSource>();
        _droneSource.clip = _droneBaseLoop;
        _droneSource.loop = true;
        _droneSource.Play();

        _lastPosition = transform.position;
        _randomPitchOffset = Random.Range(0f, 1f);

        // Добавляем фильтр если его нет
        if (_lowPassFilter == null)
            _lowPassFilter = GetComponent<AudioLowPassFilter>();

        StartCoroutine(RandomPitchVariations());
    }

    void Update()
    {
        CalculateSpeed();
        CalculateAcceleration();
        CalculateTurnIntensity();
        UpdateDroneAudio();
    }

    void CalculateSpeed()
    {
        // Текущая скорость в м/с
        _currentSpeed = _rb.velocity.magnitude;

        // Сглаживаем скорость для более реалистичного звука
        _currentSpeed = Mathf.Lerp(_currentSpeed, _rb.velocity.magnitude, Time.deltaTime * 5f);
    }

    void CalculateAcceleration()
    {
        float speedDelta = (_currentSpeed - _lastSpeed) / Time.deltaTime;
        _currentAcceleration = Mathf.Lerp(_currentAcceleration, speedDelta, Time.deltaTime * _accelSmoothing);
        _lastSpeed = _currentSpeed;
    }

    void CalculateTurnIntensity()
    {
        // Получаем угловую скорость
        Vector3 angularVelocity = _rb.angularVelocity;
        _turnIntensity = Mathf.Lerp(_turnIntensity, angularVelocity.magnitude, Time.deltaTime * _turnSmoothing);
    }

    void UpdateDroneAudio()
    {
        // 1. Базовый тон от скорости
        float speedPitch = Mathf.Lerp(_minPitch, _maxPitch,
            (_currentSpeed - _minSpeed) / (_maxSpeed - _minSpeed));

        // 2. Эффект поворота (дрон напрягается при манёврах)
        float turnPitch = _turnIntensity * _turnPitchMultiplier;

        // 3. Эффект ускорения/торможения
        float accelPitch = Mathf.Abs(_currentAcceleration) * _accelPitchMultiplier / 10f;

        // Итоговый целевой тон
        _targetPitch = speedPitch + turnPitch + accelPitch;
        _targetPitch = Mathf.Clamp(_targetPitch, 0.6f, 1.8f);

        // Плавное изменение тона
        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, Time.deltaTime * 3f);
        _droneSource.pitch = _currentPitch + _randomPitchOffset;

        // 4. Громкость от высоты (эффект удаления)
        float altitude = transform.position.y;
        float volumeTarget = Mathf.Lerp(_maxVolume, _minVolume,
            (altitude - _minAltitude) / (_maxAltitude - _minAltitude));
        volumeTarget = Mathf.Clamp(volumeTarget, _minVolume, _maxVolume);
        _droneSource.volume = Mathf.Lerp(_droneSource.volume, volumeTarget, Time.deltaTime * 2f);

        // 5. Фильтр низких частот (звук приглушается на расстоянии)
        if (_lowPassFilter != null)
        {
            float cutoff = Mathf.Lerp(_maxCutoffFreq, _minCutoffFreq,
                (altitude - _minAltitude) / (_maxAltitude - _minAltitude));
            cutoff = Mathf.Clamp(cutoff, _minCutoffFreq, _maxCutoffFreq);
            _lowPassFilter.cutoffFrequency = Mathf.Lerp(
                _lowPassFilter.cutoffFrequency, cutoff, Time.deltaTime * 3f);
        }
    }

    IEnumerator RandomPitchVariations()
    {
        while (true)
        {
            yield return new WaitForSeconds(_randomUpdateSpeed);
            // Реалистичные колебания тона (имитация вибрации двигателя)
            _randomPitchOffset = Random.Range(-_randomPitchVariation, _randomPitchVariation);

            // Иногда добавляем небольшой "чих" мотора (эффект неисправности)
            if (Random.Range(0f, 1f) < 0.02f) // 2% шанс
            {
                StartCoroutine(MotorGlitch());
            }
        }
    }

    IEnumerator MotorGlitch()
    {
        float originalPitch = _droneSource.pitch;
        _droneSource.pitch = originalPitch * 0.7f;
        yield return new WaitForSeconds(0.05f);
        _droneSource.pitch = originalPitch * 1.3f;
        yield return new WaitForSeconds(0.05f);
        _droneSource.pitch = originalPitch;
    }

    // Публичные методы для внешних эффектов
    public void SetDroneDamaged(bool isDamaged)
    {
        if (isDamaged)
        {
            _minPitch = 0.5f;
            _maxPitch = 1.2f;
            _randomPitchVariation = 0.1f; // Больше хаоса
        }
        else
        {
            _minPitch = 0.8f;
            _maxPitch = 1.5f;
            _randomPitchVariation = 0.03f;
        }
    }

    public void BoostEffect(float duration)
    {
        StartCoroutine(BoostCoroutine(duration));
    }

    IEnumerator BoostCoroutine(float duration)
    {
        float originalMaxSpeed = _maxSpeed;
        _maxSpeed *= 2f;
        yield return new WaitForSeconds(duration);
        _maxSpeed = originalMaxSpeed;
    }
}