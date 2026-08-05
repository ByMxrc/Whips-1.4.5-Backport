using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    public class SoulscourgeTag : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
