using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Slime Whip - Látigo de slime early game.
    /// Daño: 12 | Tag: +3 | Receta: 50 Gel en Solidifier
    /// Efecto: Aplica Slimed + On Fire! (la punta) al golpear.
    ///
    /// Item ID vanilla 1.4.5: 5688
    /// Texture apunta a "Terraria/Images/Item_5688" — sin .png en el mod.
    /// tModLoader resuelve la textura directamente desde la instalación local.
    /// </summary>
    public class SlimeWhip : ModItem
    {
        // Redirige a la textura vanilla — sin necesidad de un .png propio en el mod.
        // tModLoader busca "Terraria/Images/Item_5688" en la instalación local de Terraria.
        public override string Texture => "Terraria/Images/Item_5688";

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<SlimeWhipProj>(), 12, 1f, 2.7f, 30);
            Item.width = 28;
            Item.height = 28;
            Item.value = Item.sellPrice(silver: 72);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item152;
        }

        public override bool MeleePrefix() => true;

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Gel, 50)
                .AddTile(TileID.Solidifier)
                .Register();
        }
    }
}
