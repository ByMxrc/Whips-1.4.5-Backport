using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    // El tag del Starcrash "carga" al enemigo con energÃ­a meteÃ³rica.
    // Cuando un minion golpea al enemigo marcado, se invoca un meteorito.
    public class StarcrashTag : ModBuff
    {
        public override string Texture => "TerrariaWhipsBackport/Buffs/Buff_TagAllWhips";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;

            // Marca de lÃ¡tigo: bypasea la inmunidad a debuffs de todos los enemigos
            // (igual que los tags de lÃ¡tigos vanilla como el Firecracker)
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
