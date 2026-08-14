using Terraria;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    public class SlimeWhipDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_137";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
