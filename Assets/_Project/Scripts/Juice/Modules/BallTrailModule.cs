using UnityEngine;

/// <summary>
/// Topun arkasında iz bırakması.
/// Diğerlerinden farkı: olay tabanlı değil, sürekli. Her karede global
/// yoğunluğa göre ölçeklendiği için slider'la kademeli olarak belirir.
/// </summary>
public class BallTrailModule : JuiceModule
{
    public override string DisplayName => "Ball Trail";

    [SerializeField] private TrailRenderer trail;
    [SerializeField] private float maxTime = 0.16f;
    [SerializeField] private float maxWidth = 0.3f;

    private void Update()
    {
        if (trail == null) return;

        trail.emitting = IsActive;
        trail.time = maxTime * Amount;
        trail.widthMultiplier = maxWidth * Amount;
    }
}