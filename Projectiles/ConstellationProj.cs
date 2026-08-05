using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;
using Whips145.Common;

namespace Whips145.Projectiles
{
    public class ConstellationProj : ModProjectile
    {
        private bool spawnedStars = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 14; // Ajustado a 14 segmentos a petición del usuario
            Projectile.WhipSettings.RangeMultiplier = 1.18f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar muñecos de pruebas y animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.1f));
            
            // Tag de Constellation (4 segundos = 240 ticks)
            target.AddBuff(ModContent.BuffType<ConstellationTag>(), 240);

            Player player = Main.player[Projectile.owner];

            // Efecto: Liberar 6 estrellas fugaces (solo en el primer golpe de cada balanceo)
            if (!spawnedStars && Projectile.owner == Main.myPlayer)
            {
                if (player.TryGetModPlayer<WhipPlayer>(out var modPlayer))
                {
                    // No aparecer si ya hay estrellas activas o si el cooldown está activo
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<ConstellationProj1>()] > 0 || modPlayer.constellationCooldown > 0)
                    {
                        return;
                    }
                }

                spawnedStars = true;

                int starDamage = (int)(Projectile.originalDamage * 0.297f);

                for (int i = 0; i < 6; i++)
                {
                    // El usuario pidió que aparezcan a la mitad de camino entre el jugador y la entidad
                    Vector2 spawnPos = Vector2.Lerp(player.Center, target.Center, 0.5f);
                    // Un pequeño offset aleatorio para que no estén apiladas en el mismo píxel exacto
                    spawnPos += Main.rand.NextVector2Circular(16f, 16f);
                    
                    // Evitar que aparezcan dentro de bloques (mover hacia el jugador hasta encontrar espacio libre)
                    for (int j = 0; j < 20; j++)
                    {
                        if (!Collision.SolidCollision(spawnPos - new Vector2(12, 12), 24, 24))
                            break; // Encontró espacio libre

                        Vector2 toPlayer = player.Center - spawnPos;
                        if (toPlayer.LengthSquared() < 400f) break; // Si está muy cerca del jugador, parar
                        
                        toPlayer.Normalize();
                        spawnPos += toPlayer * 16f; // Retroceder un bloque (16px)
                    }
                    
                    // Velocidad inicial rápida hacia afuera (scatter)
                    Vector2 initialVelocity = Main.rand.NextVector2CircularEdge(8f, 8f) * Main.rand.NextFloat(0.8f, 1.2f);
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        initialVelocity,
                        ModContent.ProjectileType<ConstellationProj1>(),
                        starDamage,
                        Projectile.knockBack,
                        Projectile.owner,
                        target.whoAmI // ai[0] guarda el NPC inicial al que debe perseguir
                    );
                }
            }
        }
    
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            Terraria.Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int totalFrames = 6;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / totalFrames;

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, 4f); 
                float scale = 1f;

                if (i == 0)
                {
                    frame.Y = 0;
                }
                else if (i == list.Count - 2)
                {
                    frame.Y = frameHeight * (totalFrames - 1);
                }
                else
                {
                    int midFrame = 1 + (i % (totalFrames - 2));
                    frame.Y = frameHeight * midFrame;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                if (i > 0 && i < list.Count - 2)
                {
                    float dist = diff.Length();
                    scale = dist / (frameHeight - 6.5f);
                    if (scale < 1f) scale = 1f;
                }

                // Draw the connecting line (solo si no es el último tramo después de la punta)
                if (i < list.Count - 2)
                {
                    Texture2D magicPixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                    Vector2 lineScale = new Vector2(diff.Length(), 2f);
                    Color lineColor = new Color(11, 175, 237);
                    Color litLineColor = Lighting.GetColor(element.ToTileCoordinates(), lineColor);
                    Main.EntitySpriteDraw(magicPixel, element - Main.screenPosition, new Rectangle(0, 0, 1, 1), litLineColor, diff.ToRotation(), new Vector2(0, 0.5f), lineScale, SpriteEffects.None, 0);
                }

                // Draw the segment
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                pos += diff;
            }

            return false;
        }
    }
}
