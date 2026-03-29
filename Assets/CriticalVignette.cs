using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CriticalVignette : MonoBehaviour
{
    private Vignette _vignette;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float holdDuration = 0.5f;
    private CancellationTokenSource cancellationTokenSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Volume volume = GetComponent<Volume>();
        VolumeProfile volumeProfile = volume.profile;
        volumeProfile.TryGet<Vignette>(out _vignette);
    }
    void OnEnable()
    {
        cancellationTokenSource = new CancellationTokenSource();
        _ = PulseLoopAsync(cancellationTokenSource.Token);

    }
    void OnDisable()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }

    private async Awaitable PulseLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await FadeToAsync(1, cancellationToken);
                await Awaitable.WaitForSecondsAsync(holdDuration, cancellationToken);
                await FadeToAsync(0, cancellationToken);
                await Awaitable.WaitForSecondsAsync(holdDuration, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("catch at pulse loop async");
        }
    }
    private async Awaitable FadeToAsync(float target, CancellationToken cancellationToken)
    {
        float initialValue = _vignette.smoothness.value;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _vignette.smoothness.value = Mathf.Lerp(initialValue, target, elapsed / fadeDuration);
            await Awaitable.NextFrameAsync(cancellationToken);
        }
    }
}
