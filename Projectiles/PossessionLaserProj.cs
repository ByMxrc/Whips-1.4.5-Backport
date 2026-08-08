using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Whips145.Projectiles
{
    public class PossessionLaserProj : ModProjectile
    {
        public override string Texture => "Whips145/Projectiles/PossessionLaser";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                
                // Ángulo aleatorio leve para simular el cuerpo ondulante de una serpiente
                float randomAngle = Main.rand.NextFloat(-0.4f, 0.4f);
                Projectile.localAI[1] = randomAngle;
                
                // Variación de posición ligera arriba/abajo para que la línea suba y baje
                Vector2 dir = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                if (dir == Vector2.Zero) dir = Vector2.UnitX;
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                
                float offset = Main.rand.NextFloat(-14f, 14f);
                Projectile.Center += perp * offset;
                
                // El delay basado en la distancia viene en localAI[2]
                Projectile.timeLeft += (int)Projectile.localAI[2]; // Sumamos el delay a la vida total
            }

            Projectile.velocity = Vector2.Zero;
            
            // Si estamos en periodo de delay, no mostramos nada
            if (Projectile.localAI[2] > 0)
            {
                Projectile.localAI[2]--;
                Projectile.Opacity = 0f;
                return; // Pausamos todo hasta que pase el delay
            }

            // Curva de opacidad calibrada para 20 ticks:
            //   ticks 0.0 - 3.0: fade-IN (aparece rápido y suave)
            //   ticks 3.0 - 8.0: se mantiene brillante
            //   ticks 8.0 - 20.0: fade-OUT (desaparición muy extendida y suave)
            float maxVisibleTime = 20f;
            float elapsed = maxVisibleTime - Projectile.timeLeft;
            if (Projectile.timeLeft <= maxVisibleTime && elapsed >= 0f)
            {
                const float fadeInEnd = 3.0f;
                const float holdEnd = 8.0f;

                if (elapsed <= fadeInEnd)
                {
                    // Smooth fade-in
                    float t = elapsed / fadeInEnd;
                    Projectile.Opacity = MathHelper.SmoothStep(0f, 1f, t);
                }
                else if (elapsed <= holdEnd)
                {
                    Projectile.Opacity = 1f;
                }
                else
                {
                    // Smooth fade-out
                    float t = (elapsed - holdEnd) / (maxVisibleTime - holdEnd);
                    Projectile.Opacity = MathHelper.SmoothStep(1f, 0f, t);
                }

                Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity, 0f, 1f);
            }
            else
            {
                Projectile.Opacity = 0f;
            }
                
            // Cuando desaparece, suelta un "estallido" del polvo del Phantasmal Eye (Vortex)
            // Estas son las mismas partículas que deja el látigo en el aire.
            if (Projectile.timeLeft <= 2)
            {
                for (int i = 0; i < 4; i++) // Generar varias partículas
                {
                    if (Main.rand.Next(100) < 60) // Misma probabilidad del 60%
                    {
                        int dustType = Main.rand.NextBool(2) ? 229 : 228;
                        // Esparcirlas un poco alrededor del centro del láser
                        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                        
                        Dust dust = Dust.NewDustDirect(dustPos - new Vector2(5, 5), 10, 10, dustType, 0f, 0f, 100, default, 1.4f);
                        dust.noGravity = true;
                        dust.velocity *= 0.8f; // Misma estela suave
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 dir = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            if (dir == Vector2.Zero) dir = Vector2.UnitX;

            // Ajustamos la rotación asumiendo que el sprite original es vertical.
            // Le sumamos Pi/2 (90 grados) para que se alinee horizontalmente si atacamos a los lados.
            // Y por último le sumamos el ángulo aleatorio de localAI[1] para el zig-zag.
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2 + Projectile.localAI[1];

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            // Calcular cuánto se ha estirado (de 0 a 1) durante el fade-in
            float maxVisibleTime = 20f;
            float elapsed = maxVisibleTime - Projectile.timeLeft;
            
            // Fade-in direccional (crece desde la base)
            float stretchProgress = MathHelper.Clamp(elapsed / 3.0f, 0f, 1f);
            float stretchY = MathHelper.SmoothStep(0f, 1f, stretchProgress); // Suaviza el estiramiento
            
            // Fade-out direccional (se encoge desde la base hacia la punta)
            float fadeOutProgress = 0f;
            if (elapsed > 8.0f)
            {
                fadeOutProgress = MathHelper.Clamp((elapsed - 8.0f) / 12.0f, 0f, 1f);
            }
            float shrinkProgress = MathHelper.SmoothStep(0f, 1f, fadeOutProgress);

            // La textura del láser originalmente apunta hacia arriba, así que su largo es el eje Y.
            // Anclamos el origen en la parte inferior (Y = Height) para que crezca desde la base hacia la punta.
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height);
            
            // Dirección original hacia la que fue disparado (del jugador al enemigo)
            Vector2 rawDir = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            if (rawDir == Vector2.Zero) rawDir = Vector2.UnitX;
            rawDir.Normalize();

            // Opacidad general
            Color color = new Color(130, 255, 230, 0) * (Projectile.Opacity * 1.05f); 

            // Escalas base (X es el grosor achatado, Y es el largo)
            float baseBloomX = Projectile.scale * 2.0f;
            float baseBloomY = Projectile.scale * 1.0f;
            float baseCoreX = Projectile.scale * 1.2f;
            float baseCoreY = Projectile.scale * 0.8f;

            // Escalas actuales (lo que creció en el fade-in, menos lo que se encoge en el fade-out)
            float currentStretchY = stretchY * (1f - shrinkProgress);
            Vector2 bloomScale = new Vector2(baseBloomX, baseBloomY * currentStretchY);
            Vector2 coreScale = new Vector2(baseCoreX, baseCoreY * currentStretchY);

            // Ajustamos la posición de dibujo base
            Vector2 baseBloomStart = Projectile.Center - rawDir * (texture.Height * 0.5f * baseBloomY);
            Vector2 baseCoreStart = Projectile.Center - rawDir * (texture.Height * 0.5f * baseCoreY);

            // Si se está encogiendo, movemos el punto de inicio hacia adelante para que la punta se quede en su lugar
            Vector2 bloomDrawPos = baseBloomStart + rawDir * (texture.Height * baseBloomY * shrinkProgress) - Main.screenPosition;
            Vector2 coreDrawPos = baseCoreStart + rawDir * (texture.Height * baseCoreY * shrinkProgress) - Main.screenPosition;

            // Capa de bloom suavizada y redonda (halo de 4 capas)
            Color haloColor = color * 0.5f;
            for (int i = 0; i < 4; i++) 
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 3f;
                Main.EntitySpriteDraw(texture, bloomDrawPos + offset, null, haloColor, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0);
            }

            // Capa central (blanco puro)
            Main.EntitySpriteDraw(texture, coreDrawPos, null, Color.White * Projectile.Opacity, Projectile.rotation, origin, coreScale, SpriteEffects.None, 0);

            return false;
        }
    }
}