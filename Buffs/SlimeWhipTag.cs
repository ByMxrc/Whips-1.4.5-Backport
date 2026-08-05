using Terraria;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    public class SlimeWhipTag : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Efectos: Slimed + On Fire! se aplican en el proyectil
        }
    }
}
