using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Buffs
{
    public class CobwhipTag : ModBuff
    {
        public override string Texture => "Whips145/Buffs/Buff_TagAllWhips";

        public override void SetStaticDefaults()
        {
            // Tag que dejan las arañas del Cobwhip sobre los enemigos
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            BuffID.Sets.IsATagBuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // WhipTagGlobalNPC maneja el daño extra
        }
    }
}
