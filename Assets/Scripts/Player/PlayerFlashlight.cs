using UnityEngine;

namespace ZombieLand.Player
{
    /// <summary>
    /// Toggleable flashlight (F key). Drains a battery while on,
    /// auto-turns off at 0. Battery is exposed for the HUD.
    /// </summary>
    public class PlayerFlashlight : MonoBehaviour
    {
        public Light flashlight;
        public float maxBattery = 100f;
        public float drainRate = 6f;

        public float Battery { get; private set; }
        public bool On { get; private set; }

        void Awake()
        {
            Battery = maxBattery;
            On = true;
            ApplyToLight();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && Battery > 0f)
            {
                On = !On;
                ApplyToLight();
            }

            if (On)
            {
                Battery = Mathf.Max(0f, Battery - drainRate * Time.deltaTime);
                if (Battery <= 0f)
                {
                    On = false;
                    ApplyToLight();
                }
            }
        }

        public void Recharge(float amount)
        {
            Battery = Mathf.Min(maxBattery, Battery + amount);
        }

        void ApplyToLight()
        {
            if (flashlight) flashlight.enabled = On;
        }
    }
}
