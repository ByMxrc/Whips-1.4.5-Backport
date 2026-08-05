using Terraria;
using Terraria.ModLoader;

namespace Whips145.Common
{
    public class WhipPlayer : ModPlayer
    {
        public int starcrashCooldown = 0;
        public int vulgarCooldown = 0;
        public int constellationCooldown = 0;
        public int possessionCooldown = 0;

        public override void PostUpdate()
        {
            if (starcrashCooldown > 0) starcrashCooldown--;
            if (vulgarCooldown > 0) vulgarCooldown--;
            if (constellationCooldown > 0) constellationCooldown--;
            if (possessionCooldown > 0) possessionCooldown--;
        }
    }
}
