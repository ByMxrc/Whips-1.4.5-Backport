using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Constellation - LÃ¡tigo de Stardust Fragments.
    /// Al golpear lanza 6 estrellas fugaces hacia el enemigo con un pequeÃ±o delay.
    /// DaÃ±o: 130 | Tag: +15 | Multi-hit penalty: 10%
    /// Receta: 18 Stardust Fragments en Ancient Manipulator
    /// </summary>
    public class Constellation : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<ConstellationProj>(), 130, 4f, 8f, 30);
            Item.width = 36;
            Item.height = 36;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ItemRarityID.Purple; // Rareza morada (Lunar tier)
            Item.UseSound = SoundID.Item152;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentStardust, 18)
                .AddTile(TileID.LunarCraftingStation) // Ancient Manipulator
                .Register();
        }
    }
}
