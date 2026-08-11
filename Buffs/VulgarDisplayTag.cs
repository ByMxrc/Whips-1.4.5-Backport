using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    // Tag de Vulgar Display of Flower: cuando los minions golpean al enemigo marcado,
    // florecen pétalos que hacen daño adicional.
    public class VulgarDisplayTag : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_TagAllWhips";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
