using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    public class VulgarDisplayEnergy : ModBuff
    {
        public override string Texture => "TerrariaWhipsBackport/Buffs/Buff_TagAllWhips";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
            // No se muestra a menos que el usuario tenga un icono, pero como es interno a menudo estÃ¡ bien.
        }
    }
}
