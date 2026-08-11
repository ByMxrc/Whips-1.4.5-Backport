using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Whips145.Buffs
{
    public class PossessionEyeTag : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_TagAllWhips";

        public override void SetStaticDefaults()
        {
            // Ocultar el buff si no queremos llenar la barra de buffs del enemigo, 
            // pero normalmente se deja visible para que el jugador sepa que la marca está activa.
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Solo es un marcador, la lógica de consumo está en WhipTagGlobalNPC
        }
    }
}
