using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    public class ElectricEelDebuff : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_ElectricWhip";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
