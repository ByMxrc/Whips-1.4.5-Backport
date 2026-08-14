using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Buffs
{
    public class SlimeWhipTag : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_357";
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
