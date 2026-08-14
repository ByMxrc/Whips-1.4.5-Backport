using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Cobwhip - LÃ¡tigo inicial de telaraÃ±as.
    /// DaÃ±o: 9 | Tag: +3 | Receta: 30 Cobweb + 10 Wood en Loom
    /// Efecto: Genera una pequeÃ±a araÃ±a que hace 2 de daÃ±o por 3 segundos al golpear.
    /// </summary>
    public class Cobwhip : ModItem
    {
        public override string Texture => "Terraria/Images/Item_5473";

        public override void SetDefaults()
        {
            // DefaultToWhip(proyectil, daÃ±o, golpe crÃ­tico %, velocidad de swing)
            Item.DefaultToWhip(ModContent.ProjectileType<CobwhipProj>(), 9, 0.7f, 2f, 35);
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(silver: 54);
            Item.rare = ItemRarityID.White; // Sin rareza (gris)
            Item.UseSound = SoundID.Item152; // Sonido de lÃ¡tigo vanilla
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
