using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    public class VulgarDisplayEnergy : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            // No se muestra a menos que el usuario tenga un icono, pero como es interno a menudo está bien.
        }
    }
}
