using Terraria;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    public class ElectricEelDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_144";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
