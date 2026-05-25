using UnityEngine;

[DisallowMultipleComponent]
public class DollAnimationEventRelay : MonoBehaviour
{
    private UnitCombat combat;
    private DollHorrorBehaviour horror;

    void Awake()
    {
        combat = GetComponent<UnitCombat>();
        horror = GetComponent<DollHorrorBehaviour>();
    }

    // Evento para poner al inicio o en medio de la animación Attack.
    public void DollAttackSound()
    {
        if(horror != null)
            horror.PlayAttackSound();
    }

    // Evento obligatorio al final de la animación Attack.
    // Sin este evento, tu sistema actual no sabe que terminó el golpe.
    public void DollAttackFinished()
    {
        if(combat != null)
            combat.AttackAnimationFinished();
    }

    // Solo úsalo si algún día haces a la muñeca de rango.
    public void DollSpawnProjectile()
    {
        if(combat != null)
            combat.SpawnProjectile();
    }
}
