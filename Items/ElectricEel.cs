using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Electric Eel - Drop de Duke Fishron (25%).
    /// Crea un aura elÃ©ctrica alrededor del enemigo marcado que daÃ±a a los cercanos.
    /// DaÃ±o: 150 | Tag: +12 | Multi-hit penalty: 5%
    /// </summary>
    public class ElectricEel : ModItem
    {
        public override string Texture => "Terraria/Images/Item_5478";

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<ElectricEelProj>(), 150, 3f, 8f, 30);
            Item.width = 36;
            Item.height = 36;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Cyan; // Rareza cyan (Duke Fishron tier)
            Item.UseSound = SoundID.Item152;
        }

        // Se obtiene como drop de Duke Fishron (ver WhipDropGlobalNPC)
    }
}
