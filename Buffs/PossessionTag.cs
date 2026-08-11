using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    /// <summary>
    /// Tag de marca del Possession. Cuando las invocaciones golpean al enemigo marcado
    /// reciben +25 de daño extra (mecánica de tag damage de látigos).
    /// Las mecánicas adicionales del Possession se implementarán más adelante.
    /// </summary>
    public class PossessionTag : ModBuff
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
