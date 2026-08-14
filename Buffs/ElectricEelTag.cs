using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    // Tag del Electric Eel: genera un aura elÃ©ctrica que daÃ±a enemigos cercanos.
    public class ElectricEelTag : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_362";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Terraria.ID.BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
