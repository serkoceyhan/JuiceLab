using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Tuğla yok olurken yerine kısa ömürlü bir görsel ceset bırakır.
///
/// Cesetler havuzlanır (Unity'nin yerleşik ObjectPool'u): saniyede onlarca
/// Instantiate/Destroy yerine aynı nesneler tekrar kullanılır.
/// WebGL'de GC duraklamaları doğrudan kare atlamaya dönüştüğü için
/// bu tip efektlerde havuzlama tercih değil zorunluluk.
/// </summary>
public class BrickDeathModule : JuiceModule
{
    public override string DisplayName => "Brick Death";

    [SerializeField] private BrickCorpse corpsePrefab;
    [SerializeField] private int prewarm = 12;

    private ObjectPool<BrickCorpse> pool;
    private Action<BrickCorpse> releaseAction;

    private void Awake()
    {
        // Lambda'yı her çarpmada yeniden oluşturmamak için bir kez önbelleğe al.
        releaseAction = corpse => pool.Release(corpse);

        pool = new ObjectPool<BrickCorpse>(
            createFunc: () =>
            {
                BrickCorpse c = Instantiate(corpsePrefab, transform);
                c.gameObject.SetActive(false);
                return c;
            },
            actionOnGet: null,
            actionOnRelease: c => c.gameObject.SetActive(false),
            actionOnDestroy: c => Destroy(c.gameObject),
            collectionCheck: false,
            defaultCapacity: prewarm,
            maxSize: 64);
    }

    private void OnEnable()  => GameEvents.BrickDestroyed += OnBrickDestroyed;
    private void OnDisable() => GameEvents.BrickDestroyed -= OnBrickDestroyed;

    private void OnBrickDestroyed(ImpactInfo info)
    {
        if (!IsActive || corpsePrefab == null) return;

        BrickCorpse corpse = pool.Get();
        corpse.Play(info.Center, info.Size, info.Color, Amount, releaseAction);
    }
}