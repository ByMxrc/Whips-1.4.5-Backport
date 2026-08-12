using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace TerrariaWhipsBackport.Projectiles
{
    public class VulgarDisplayOfFlowerProj1 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Ajusta este valor al nÃºmero real de frames de tu sprite.
            // Si la imagen es 18x160 y cada frame es cuadrado (18x20), son 8 frames.
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120; // 2 segundos de vida
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ArmorPenetration = 50; // Ignora hasta 50 de defensa
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Evita golpear durante la fase de expansiÃ³n. Depende de si es de los rezagados o no.
            bool isDelayed = Projectile.ai[1] == 2;
            int homingStartTick = isDelayed ? 40 : 30;

            if (Projectile.localAI[0] < homingStartTick)
            {
                return false;
            }
            return null;
        }

        public override void AI()
        {
            // AnimaciÃ³n
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.localAI[0]++;
            int tick = (int)Projectile.localAI[0];
            bool isDelayed = Projectile.ai[1] == 2;
            int homingStartTick = isDelayed ? 40 : 30;

            if (tick < homingStartTick)
            {
                // Fase de expansiÃ³n
                Projectile.velocity *= 0.95f; // Frena suavemente
                Projectile.rotation += Projectile.velocity.X * 0.05f; // Rota un poco visualmente
            }
            else 
            {
                // Fase de persecuciÃ³n
                float maxDetectRadius = 340f; // 21.25 tiles
                float sqrMaxDetectRadius = maxDetectRadius * maxDetectRadius;
                NPC closestNPC = null;
                float sqrMinDistance = sqrMaxDetectRadius;

                foreach (NPC target in Main.ActiveNPCs)
                {
                    if (target.CanBeChasedBy())
                    {
                        float sqrDistance = Vector2.DistanceSquared(target.Center, Projectile.Center);
                        if (sqrDistance < sqrMinDistance)
                        {
                            sqrMinDistance = sqrDistance;
                            closestNPC = target;
                        }
                    }
                }

                if (closestNPC != null)
                {
                    float speed = 14f;
                    float inertia = 15f;
                    Vector2 direction = closestNPC.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;

                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;

                    // Si estÃ¡ persiguiendo a un objetivo, se asegura de ser completamente visible
                    if (Projectile.alpha > 0)
                        Projectile.alpha = Math.Max(0, Projectile.alpha - 15);
                        
                    // Orientar en la direcciÃ³n de movimiento
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    // Si no hay objetivos (o el que perseguÃ­a muriÃ³), se frena poco a poco y se desvanece
                    Projectile.velocity *= 0.98f;
                    Projectile.rotation += Projectile.velocity.X * 0.05f;
                    Projectile.alpha += 10;
                    if (Projectile.alpha >= 255)
                    {
                        Projectile.Kill();
                    }
                }
            }

            // PartÃ­culas opcionales al brillar
            bool isGlowing = (!isDelayed && tick >= 8 && tick <= 19) || (isDelayed && tick >= 22 && tick <= 33);
            if (isGlowing && Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkFairy, 0f, 0f, 100, default, 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle frameRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            bool isDelayed = Projectile.ai[1] == 2;
            int tick = (int)Projectile.localAI[0];
            
            bool isGlowing = false;
            float glowFactor = 0f;

            if (!isDelayed)
            {
                if (tick >= 8 && tick <= 19)
                {
                    isGlowing = true;
                    // ParÃ¡bola matemÃ¡tica (seno) para hacer fade in (0 a 1) y fade out (1 a 0)
                    glowFactor = (float)Math.Sin(Math.PI * (tick - 8) / 11f);
                }
            }
            else
            {
                if (tick >= 22 && tick <= 33)
                {
                    isGlowing = true;
                    glowFactor = (float)Math.Sin(Math.PI * (tick - 22) / 11f);
                }
            }

            if (isGlowing)
            {
                Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_89").Value;
                Vector2 glowOrigin = glowTex.Size() / 2f;
                
                // Bloom rosa
                Color bloomColor = new Color(255, 105, 180, 0) * glowFactor * 0.8f;
                Vector2 visualCenter = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                float glowScale = Projectile.scale * 0.6f * glowFactor;

                // NÃºcleo
                Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor * 0.7f, 0f, glowOrigin, glowScale * 1.5f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(glowTex, visualCenter, null, Color.White * 0.6f * glowFactor, 0f, glowOrigin, glowScale * 0.5f, SpriteEffects.None, 0);

                // Destellos en cruz (+)
                Vector2 scaleLong = new Vector2(0.12f, 1.3f) * glowScale;
                Vector2 scaleShort = new Vector2(1.3f, 0.12f) * glowScale;

                // Usamos una rotaciÃ³n fija o la del proyectil, aquÃ­ usamos la rotaciÃ³n del proyectil
                Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor, Projectile.rotation, glowOrigin, scaleLong, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor, Projectile.rotation, glowOrigin, scaleShort, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), frameRect, lightColor * ((255 - Projectile.alpha) / 255f), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            // PartÃ­culas rosas que se desvanecen en el lugar donde el pÃ©talo desaparece
            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkSlime, 0f, 0f, 150, default, 1.2f);
                Main.dust[dust].noGravity = true; // Flotan en el lugar
                Main.dust[dust].velocity *= 0.3f; // Casi no se mueven para desaparecer ahÃ­ mismo
            }
        }
    }
}
