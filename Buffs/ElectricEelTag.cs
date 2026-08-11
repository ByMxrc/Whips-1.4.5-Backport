using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    // Tag del Electric Eel: genera un aura eléctrica que daña enemigos cercanos.
    public class ElectricEelTag : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_TagAllWhips";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Terraria.ID.BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
