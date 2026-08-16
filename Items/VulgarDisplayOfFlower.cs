using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Vulgar Display of Flower - Drop de Plantera (25%).
    /// Las flores brotan de los enemigos marcados cuando los minions los golpean.
    /// DaÃ±o: 75 | Tag: +9 | Multi-hit penalty: 15%
    /// </summary>
    public class VulgarDisplayOfFlower : ModItem
    {
        public override string Texture => "Terraria/Images/Item_5477";

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<VulgarDisplayOfFlowerProj>(), 75, 2f, 7.1f, 30);
            Item.width = 34;
            Item.height = 34;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Lime; // Rareza lima (post-Plantera)
            Item.UseSound = SoundID.Item152;
        }

        public override bool MeleePrefix() => true;

        // Esta arma se obtiene como drop de Plantera, no tiene receta de crafteo.
        // El drop se configura en WhipTagGlobalNPC usando GlobalNPC.NPCLoot
    }
}
