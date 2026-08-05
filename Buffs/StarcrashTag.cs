using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    // El tag del Starcrash "carga" al enemigo con energía meteórica.
    // Cuando un minion golpea al enemigo marcado, se invoca un meteorito.
    public class StarcrashTag : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;

            // Marca de látigo: bypasea la inmunidad a debuffs de todos los enemigos
            // (igual que los tags de látigos vanilla como el Firecracker)
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
