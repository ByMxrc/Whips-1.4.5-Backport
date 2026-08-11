using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    public class SlimeWhipTag : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_TagAllWhips";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Efectos: Slimed + On Fire! se aplican en el proyectil
        }
    }
}
