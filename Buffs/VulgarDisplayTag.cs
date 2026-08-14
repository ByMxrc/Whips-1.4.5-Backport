using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    // Tag de Vulgar Display of Flower: cuando los minions golpean al enemigo marcado,
    // florecen pÃ©talos que hacen daÃ±o adicional.
    public class VulgarDisplayTag : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_361";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
