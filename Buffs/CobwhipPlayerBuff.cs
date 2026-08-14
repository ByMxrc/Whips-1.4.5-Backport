using Terraria;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    /// <summary>
    /// Buff visible en la barra del JUGADOR cuando tiene la araÃ±a del Cobwhip activa.
    /// La araÃ±a actÃºa como una invocaciÃ³n sin ocupar espacio de invocaciÃ³n,
    /// por eso este buff le aparece al jugador y no como debuff en los enemigos.
    /// </summary>
    public class CobwhipPlayerBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_365";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            // No es debuff: aparece en la barra del jugador como buff positivo
        }
    }
}
