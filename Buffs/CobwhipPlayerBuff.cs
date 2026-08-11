using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    /// <summary>
    /// Buff visible en la barra del JUGADOR cuando tiene la araña del Cobwhip activa.
    /// La araña actúa como una invocación sin ocupar espacio de invocación,
    /// por eso este buff le aparece al jugador y no como debuff en los enemigos.
    /// </summary>
    public class CobwhipPlayerBuff : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_SpiderWhip";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            // No es debuff: aparece en la barra del jugador como buff positivo
        }
    }
}
