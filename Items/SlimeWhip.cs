using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Slime Whip - LÃ¡tigo de slime early game.
    /// DaÃ±o: 12 | Tag: +3 | Receta: 50 Gel en Solidifier
    /// Efecto: Aplica Slimed + On Fire! (la punta) al golpear.
    /// </summary>
    public class SlimeWhip : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<SlimeWhipProj>(), 12, 1f, 2.7f, 30);
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(silver: 72);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item152;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Gel, 50)
                .AddTile(TileID.Solidifier)
                .Register();
        }
    }
}
