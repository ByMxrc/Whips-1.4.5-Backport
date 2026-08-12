using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Projectiles
{
    public class PossessionEyeProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100; // Suficiente tiempo para el delay y los 49 ticks de vida
        }

        public override void AI()
        {
            int targetIndex = (int)Projectile.ai[0];
            float delay = Projectile.ai[1];

            // Fase de Delay (aÃºn no ha aparecido)
            if (Projectile.localAI[0] < delay)
            {
                Projectile.localAI[0]++;
                Projectile.Opacity = 0f;
                return;
            }

            // Una vez pasado el delay, localAI[1] lleva el tiempo activo
            if (Projectile.localAI[1] == 0f)
            {
                // Aparecemos
                Projectile.timeLeft = 50; // Ajustamos la vida restante a 50 ticks
            }
            
            if (Projectile.localAI[1] == 0f)
            {
                // Disparar el lÃ¡ser curvo vinculado a este ojo y apuntando al mismo enemigo con 2 ticks de retraso
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PossessionEyeLaserProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner, targetIndex);
                }
            }

            Projectile.localAI[1]++;
            float timer = Projectile.localAI[1];

            // Apuntar hacia el enemigo si existe
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active)
                {
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    // Ajustar sprite si mira a la izquierda (flip)
                    if (target.Center.X < Projectile.Center.X)
                    {
                        Projectile.spriteDirection = -1;
                        Projectile.rotation += MathHelper.Pi; // Corregir rotaciÃ³n por el flip
                    }
                    else
                    {
                        Projectile.spriteDirection = 1;
                    }
                }
            }

            // Mismo ciclo de vida visual que el lÃ¡ser (aprox 50 ticks)
            // 0 a 4 ticks: Fade IN (desde 50% hasta 100%)
            if (timer <= 4f)
            {
                Projectile.Opacity = MathHelper.Lerp(0.5f, 1f, timer / 4f);
            }
            // 4 a 18 ticks: Hold (100%)
            else if (timer <= 18f)
            {
                Projectile.Opacity = 1f;
            }
            // 18 a 49 ticks: Fade OUT
            else
            {
                float fadeOut = 1f - ((timer - 18f) / 31f);
                Projectile.Opacity = MathHelper.Clamp(fadeOut, 0f, 1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity <= 0f) return false;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color drawColor = Color.White * Projectile.Opacity; // Brilla en la oscuridad y aplica la opacidad

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}
