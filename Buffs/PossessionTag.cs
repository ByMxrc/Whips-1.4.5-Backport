using Terraria;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    /// <summary>
    /// Tag de marca del Possession. Cuando las invocaciones golpean al enemigo marcado
    /// reciben +25 de daÃ±o extra (mecÃ¡nica de tag damage de lÃ¡tigos).
    /// Las mecÃ¡nicas adicionales del Possession se implementarÃ¡n mÃ¡s adelante.
    /// </summary>
    public class PossessionTag : ModBuff
    {
        public override string Texture => "TerrariaWhipsBackport/Buffs/Buff_TagAllWhips";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Terraria.ID.BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
