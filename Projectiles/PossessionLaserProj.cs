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
            Projectile.timeLeft = 9;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                // Ángulo aleatorio leve (como en el dibujo, para que no sea totalmente plano)
                float randomAngle = Main.rand.NextFloat(-0.4f, 0.4f);
                Projectile.localAI[1] = randomAngle;
                
                // Variación de posición ligera arriba/abajo para que la línea no sea perfecta
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

            // Curva de opacidad calibrada contra la referencia (~9 ticks totales visibles):
            //   ticks 0.0 - 4.5: fade-IN (aparece gradualmente, no de golpe)
            //   ticks 4.5 - 7.0: se mantiene casi al máximo ("se movió un poco")
            //   ticks 7.0 - 9.0: fade-OUT rápido hasta desaparecer
            float maxVisibleTime = 9f;
            float elapsed = maxVisibleTime - Projectile.timeLeft;
            if (Projectile.timeLeft <= maxVisibleTime && elapsed >= 0f)
            {
                const float fadeInEnd = 4.5f;
                const float holdEnd = 7f;

                if (elapsed <= fadeInEnd)
                {
                    Projectile.Opacity = elapsed / fadeInEnd;
                }
                else if (elapsed <= holdEnd)
                {
                    Projectile.Opacity = 1f;
                }
                else
                {
                    Projectile.Opacity = 1f - (elapsed - holdEnd) / (maxVisibleTime - holdEnd);
                }

                Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity, 0f, 1f);
            }
            else
            {
                Projectile.Opacity = 0f;
            }
                
            // Cuando desaparece, suelta el polvo del Phantasmal Eye (Vortex)
            if (Projectile.timeLeft <= 2)
            {
                if (Main.rand.NextBool(2))
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 229, 0f, 0f, 100, default, 1.2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.5f;
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
            Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            // Al poner el canal Alpha en 0, Terraria lo interpreta como Additive Blending.
            // Opacidad general
            Color color = new Color(130, 255, 230, 0) * (Projectile.Opacity * 0.7f); 

            // Escalas
            // Dejamos el grosor en 1.9f para la máxima intensidad, y el largo en 1.5f para que siga brillando intensamente sin hacerse infinito.
            Vector2 bloomScale = new Vector2(Projectile.scale * 1.5f, Projectile.scale * 1.9f);
            Vector2 coreScale = new Vector2(Projectile.scale, Projectile.scale);

            // Capa de bloom puramente horizontal de la forma del láser (el canal Alpha en 0 ya hace el efecto aditivo de Terraria)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0);

            // Capa central (blanco puro)
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, origin, coreScale, SpriteEffects.None, 0);

            return false;
        }
    }
}