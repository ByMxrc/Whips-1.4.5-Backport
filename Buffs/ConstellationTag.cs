using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    // Tag de Constellation: al golpear, se lanzan 6 estrellas fugaces con un delay.
    public class ConstellationTag : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
