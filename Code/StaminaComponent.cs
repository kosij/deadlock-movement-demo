using Sandbox;
using System;

// standalone stamina resource that gates movement actions (dash, double jump, wall jump).
// regen is time-based only
// exposes TryConsume() so states can gate actions without touching regen logic.
public class StaminaComponent : Component
{
    [Property] public float MaxCharges { get; set; } = 3f;    // charges   | total charge pool
    [Property] public float RegenRate { get; set; } = 0.25f;  // charges/s | 1 charge per 4s default

    public float Charges { get; private set; }

    // normalized 0-1 for HUD and UI display
    public float Normalized => Charges / MaxCharges;

    protected override void OnStart()
    {
        Charges = MaxCharges;
    }

    protected override void OnUpdate()
    {
        // time-based regen - no events, no landing reset
        Charges = Math.Clamp( Charges + RegenRate * Time.Delta, 0f, MaxCharges );
    }

    // stamped when TryConsume fails - read by the HUD to trigger the flash feedback
    public TimeSince TimeSinceLastFailedConsume;

    // checks and deducts cost. returns false without deducting if insufficient.
    public bool TryConsume( float cost )
    {
        if ( Charges < cost )
        {
            TimeSinceLastFailedConsume = 0;
            return false;
        }
        Charges -= cost;
        return true;
    }
}
