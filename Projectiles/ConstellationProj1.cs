using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Buffs;
using TerrariaWhipsBackport.Common;

namespace TerrariaWhipsBackport.Projectiles
{
    public class ConstellationProj1 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1039";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20; // Rastro mucho mÃ¡s largo para una hermosa estela mÃ¡gica
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // Rastro con rotaciÃ³n y posiciÃ³n
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed; // DaÃ±o summon del lÃ¡tigo
            Projectile.penetrate = 1; // Non-piercing
            Projectile.timeLeft = 240; // 2 segundos, pero como extraUpdates es 1, usamos 240 ticks
            Projectile.ignoreWater = true; // No es frenado por lÃ­quidos
            Projectile.tileCollide = true; // Rebotan en bloques
            Projectile.extraUpdates = 1; // Hace que el juego calcule su fÃ­sica 2 veces por frame (suaviza el rastro formando un trazo)
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Un golpe por estrella
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Rebotar en bloques sin perder velocidad
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false; // Evita que se destruya al chocar
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.type == Terraria.ID.NPCID.TargetDummy)
            {
                return;
            }
            // Aplica el tag del Constellation al enemigo - forzando ignorar inmunidad
            target.buffImmune[ModContent.BuffType<ConstellationTag>()] = false;
            target.AddBuff(ModContent.BuffType<ConstellationTag>(), 240);
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Evita golpear durante la fase de dispersiÃ³n (29 frames visuales a 60fps = 58 ticks con extraUpdates)
            if (Projectile.localAI[0] <= 58)
            {
                return false;
            }
            return null;
        }

        public override void AI()
        {
            // Las estrellas mÃ¡gicas siempre deben mantenerse derechas (sin rotar hacia donde vuelan), 
            // asÃ­ sus puntas y destellos se ven perfectos y no se ven 'chuecos' o inclinados.
            Projectile.rotation = 0f;

            // Rastro de polvo de estrella suave (reducido)
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, default, 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Projectile.velocity * 0.1f;
            }

            // localAI[0] maneja el contador del retraso
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] <= 58) // 70 frames @ 144fps = ~29 frames @ 60fps = 58 ticks (extraUpdates)
            {
                // Primera fase: salen volando y frenan un poco
                Projectile.velocity *= 0.95f;
            }
            else
            {
                // Segunda fase: PersecuciÃ³n
                NPC initialTarget = null;
                int initialTargetIndex = (int)Projectile.ai[0]; // El NPC original
                
                if (initialTargetIndex >= 0 && initialTargetIndex < Main.maxNPCs)
                {
                    NPC npc = Main.npc[initialTargetIndex];
                    if (npc.active && npc.CanBeChasedBy())
                    {
                        initialTarget = npc;
                    }
                }

                NPC targetToTrack = initialTarget;

                // Si el objetivo inicial muriÃ³ o desapareciÃ³, buscar otro a 50 bloques (800 pÃ­xeles) con visiÃ³n
                if (targetToTrack == null)
                {
                    float maxDist = 800f;
                    float sqrMaxDist = maxDist * maxDist;
                    NPC closest = null;
                    
                    foreach (NPC target in Main.ActiveNPCs)
                    {
                        if (target.CanBeChasedBy() && Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1))
                        {
                            float sqrDist = Vector2.DistanceSquared(Projectile.Center, target.Center);
                            if (sqrDist < sqrMaxDist)
                            {
                                sqrMaxDist = sqrDist;
                                closest = target;
                            }
                        }
                    }
                    targetToTrack = closest;
                }

                // Si no hay objetivo, se frena poco a poco y se desvanece
                if (targetToTrack == null)
                {
                    Projectile.velocity *= 0.95f;
                    Projectile.alpha += 10;
                    if (Projectile.alpha >= 255)
                    {
                        Projectile.Kill();
                    }
                }
                else
                {
                    // Velocidad reducida un 10% (antes 45f, ahora ~40f). 
                    // Como tiene extraUpdates = 1, viaja a 20f dos veces por frame (40f).
                    float speed = 20f; 
                    float inertia = 15f; 
                    Vector2 direction = targetToTrack.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;

                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Usar la textura original del lÃ¡tigo (ConstellationProj) para que el color sea exactamente idÃ©ntico
            Texture2D texture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_1034").Value;
            
            // El sprite del lÃ¡tigo tiene 6 frames. Debemos recortar solo el frame de la estrella grande (Frame 3)
            int frameHeight = texture.Height / 6;
            Rectangle frame = new Rectangle(0, frameHeight * 3, texture.Width, frameHeight);
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);

            Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_89").Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            float alphaFactor = (255 - Projectile.alpha) / 255f;
            Color baseColor = new Color(37, 116, 176, 0); 
            
            // Reducido un 5% (0.5 * 0.95 = 0.475) para que el bloom no sea tan invasivo
            float glowScale = Projectile.scale * 0.475f; 

            // 1. Dibujar el rastro (lÃ­nea continua y difuminada mediante interpolaciÃ³n para rellenar los huecos)
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i - 1] == Vector2.Zero) continue;

                Vector2 p1 = Projectile.oldPos[i - 1] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Vector2 p2 = Projectile.oldPos[i] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                
                float fade1 = (Projectile.oldPos.Length - (i - 1)) / (float)Projectile.oldPos.Length;
                float fade2 = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                
                // Rellenamos el espacio entre p1 y p2 dibujando mÃºltiples cÃ­rculos difuminados
                int steps = 4;
                for (int j = 0; j < steps; j++)
                {
                    float t = j / (float)steps;
                    Vector2 drawPos = Vector2.Lerp(p1, p2, t);
                    float fade = MathHelper.Lerp(fade1, fade2, t);
                    
                    // Color azul brillante, con opacidad reducida al 50%
                    Color trailColor = baseColor * fade * 0.75f * alphaFactor;
                    
                    // Escala pequeÃ±a que decrece para formar la punta del cometa
                    float trailScale = Projectile.scale * 0.25f * fade;
                    
                    Main.EntitySpriteDraw(glowTex, drawPos, null, trailColor, 0f, glowOrigin, trailScale, SpriteEffects.None, 0);
                }
            }

            // 2. BLOOM PRINCIPAL
            Color bloomColor = baseColor * alphaFactor; 
            Vector2 visualCenter = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // NÃºcleo central (escalado correctamente)
            Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor * 0.7f, 0f, glowOrigin, glowScale * 1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glowTex, visualCenter, null, Color.White * 0.3f * alphaFactor, 0f, glowOrigin, glowScale * 0.5f, SpriteEffects.None, 0);

            // Destello de 4 puntas (Cruz recta +), con las proporciones exactas que usa el lÃ¡tigo (0.12f, 1.3f)
            Vector2 scaleLong = new Vector2(0.12f, 1.3f) * glowScale; 
            Vector2 scaleShort = new Vector2(1.3f, 0.12f) * glowScale;

            Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor, Projectile.rotation, glowOrigin, scaleLong, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor, Projectile.rotation, glowOrigin, scaleShort, SpriteEffects.None, 0);

            // 3. Dibujar la estrella base recortada (Usamos Color.White tal como el lÃ¡tigo lo hace para las estrellas)
            Main.EntitySpriteDraw(texture, visualCenter, frame, Color.White * alphaFactor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Puf con un rastro de humo y chispas al desaparecer
            for (int i = 0; i < 6; i++)
            {
                // Humo (Smoke)
                int smoke = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.2f);
                Main.dust[smoke].noGravity = true;
                Main.dust[smoke].velocity *= 0.2f;

                // Chispa cyan remanente
                int spark = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, default, 1.0f);
                Main.dust[spark].noGravity = true;
                Main.dust[spark].velocity *= 0.3f;
            }

            // Aplicar 1 segundo de cooldown al jugador. 
            // Como esto corre para cada estrella al morir, el cooldown efectivo de 1 seg cuenta desde que desaparece la ÃšLTIMA estrella.
            if (Main.player[Projectile.owner].TryGetModPlayer<WhipPlayer>(out var modPlayer))
            {
                modPlayer.constellationCooldown = 45; // 60 ticks = 1 segundo
            }
        }
    }
}
