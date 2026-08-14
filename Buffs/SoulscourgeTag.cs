using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    public class SoulscourgeTag : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_358";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
