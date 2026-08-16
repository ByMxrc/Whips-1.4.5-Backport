using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Projectiles;

namespace TerrariaWhipsBackport.Items
{
    /// <summary>
    /// Possession - LÃ¡tigo Drop de Moon Lord (25%).
    /// LÃ¡tigo simple sin efectos adicionales, debuffs ni marcas.
    /// DaÃ±o: 110 (Invocador) | Knockback: 4 (DÃ©bil) | Use Time: 30 (Promedio) | Rango: 34 tiles | Velocidad: 8
    /// </summary>
    public class Possession : ModItem
    {
        public override string Texture => "Terraria/Images/Item_5480";

        public override void SetDefaults()
        {
            // DefaultToWhip configura useTime, useAnimation, shoot, shootSpeed, damage, knockback, etc.
            Item.DefaultToWhip(ModContent.ProjectileType<PossessionProj>(), 110, 4f, 8f, 30); // 30 ticks = 0.50 segundos
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red; // Rarity Red (Raridad 10 - Moon Lord tier)
            Item.UseSound = SoundID.Item152;
            
            // Convertimos el arma en una canalizada (como el Zenith o Solar Eruption)
            // Esto evade por completo las mecÃ¡nicas estrictas de swing de Terraria que causan pausas
            //
            // IMPORTANTE: NO usar autoReuse junto con channel. channel ya cubre el uso
            // continuo mientras se mantiene el click; autoReuse hace que Terraria reintente
            // "reusar" el item cada vez que itemAnimation toca un valor bajo, sin depender
            // bien de si el click sigue apretado. Como forzamos itemAnimation manualmente
            // en el manager (Math.Max(2, ...)), ese patrÃ³n fluctuante terminaba disparando
            // Shoot() solo, cada ~50 ticks, para siempre, aunque hubieras soltado el click
            // hace rato â€” eso era el "golpe extra" repetido.
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<PossessionManager>();
        }

        public override bool MeleePrefix() => true;

        public override bool CanUseItem(Player player)
        {
            // Prevenir spam de clicks que cree mÃºltiples managers simultÃ¡neos
            return player.ownedProjectileCounts[ModContent.ProjectileType<PossessionManager>()] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Segunda barrera, independiente de CanUseItem: si por lo que sea Terraria
            // vuelve a llamar a Shoot() con un manager todavÃ­a vivo, no dejamos que nazca uno nuevo.
            if (player.ownedProjectileCounts[ModContent.ProjectileType<PossessionManager>()] > 0)
            {
                return false;
            }

            // Dejamos que el motor dispare el PossessionManager de forma natural
            return true;
        }
    }
}
