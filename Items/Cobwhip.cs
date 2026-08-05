using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Projectiles;

namespace Whips145.Items
{
    /// <summary>
    /// Cobwhip - Látigo inicial de telarañas.
    /// Daño: 9 | Tag: +3 | Receta: 30 Cobweb + 10 Wood en Loom
    /// Efecto: Genera una pequeña araña que hace 2 de daño por 3 segundos al golpear.
    /// </summary>
    public class Cobwhip : ModItem
    {
        public override void SetDefaults()
        {
            // DefaultToWhip(proyectil, daño, golpe crítico %, velocidad de swing)
            Item.DefaultToWhip(ModContent.ProjectileType<CobwhipProj>(), 9, 0.7f, 6f, 35);
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(silver: 54);
            Item.rare = ItemRarityID.White; // Sin rareza (gris)
            Item.UseSound = SoundID.Item152; // Sonido de látigo vanilla
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Cobweb, 30)
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.Loom)
                .Register();
        }
    }
}
