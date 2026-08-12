using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaWhipsBackport.Projectiles
{
    public class CobwhipProj1 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4; // 4 frames de animaciÃ³n de la araÃ±a (24x14 cada uno)
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1; // No desaparece al penetrar
            Projectile.timeLeft = 180; // 3 segundos
            
            // Ahora choca con las paredes y el suelo
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            
            // 30 armor penetration y sin knockback como la wiki indica
            Projectile.ArmorPenetration = 30;

            // Invincibility frames locales (20 ticks = hasta 3 golpes por segundo)
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            
            // Empujar el sprite hacia abajo para que no flote
            DrawOriginOffsetY = 8;
        }

        public override void AI()
        {
            // AnimaciÃ³n
            if (System.Math.Abs(Projectile.velocity.X) > 0.5f) 
            {
                if (++Projectile.frameCounter >= 5)
                {
                    Projectile.frameCounter = 0;
                    if (++Projectile.frame >= Main.projFrames[Type])
                        Projectile.frame = 0;
                }
            } 
            else 
            {
                Projectile.frame = 0; // Se queda quieta si no camina rÃ¡pido
            }

            // 2. BUSCAR OBJETIVO
            NPC target = null;
            float maxDist = 450f; // 28.125 casillas

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < maxDist)
                    {
                        maxDist = dist;
                        target = npc;
                    }
                }
            }

            // 3. MOVIMIENTO HORIZONTAL
            Player player = Main.player[Projectile.owner];
            Vector2 targetPos = target != null ? target.Center : player.Center;
            
            float diffX = targetPos.X - Projectile.Center.X;
            float distToTarget = Vector2.Distance(Projectile.Center, targetPos);

            // 1. GRAVEDAD BÃSICA
            Projectile.velocity.Y += 0.4f;
                if (Projectile.velocity.Y > 10f)
                {
                    Projectile.velocity.Y = 10f;
                }

                // MOVIMIENTO HORIZONTAL
                if (System.Math.Abs(diffX) > 20f)
                {
                    Projectile.velocity.X += System.Math.Sign(diffX) * 0.5f;
                    float maxSpeed = target != null ? 7f : 5f; // Corre mÃ¡s rÃ¡pido si persigue
                    
                    if (System.Math.Abs(Projectile.velocity.X) > maxSpeed)
                    {
                        Projectile.velocity.X = maxSpeed * System.Math.Sign(Projectile.velocity.X);
                    }
                }
                else
                {
                    Projectile.velocity.X *= 0.8f; // FricciÃ³n al llegar
                }

                // Reducir cooldown de salto
                if (Projectile.ai[0] > 0)
                {
                    Projectile.ai[0]--;
                }

                // Verificamos si estamos tocando el suelo
                bool onGround = Projectile.velocity.Y == 0 || Projectile.oldVelocity.Y == 0;
                
                if (onGround)
                {
                    // Si se está moviendo hacia un objetivo y no está en cooldown
                    if (System.Math.Abs(diffX) > 20f && Projectile.ai[0] == 0)
                    {
                        if (Main.rand.NextBool(6)) // 1/6 de probabilidad de salto grande
                        {
                            Projectile.velocity.Y = -9.8f; // Salto de 7.5 bloques (120px)
                        }
                        else
                        {
                            Projectile.velocity.Y = -5.65f; // Salto de 2.5 bloques (40px)
                        }
                        
                        // Cooldown de 10 frames para que no rebote instantáneamente cada milisegundo
                        Projectile.ai[0] = 10; 
                    }
                    // Si topamos con una pared, saltamos el obstáculo
                    else if (System.Math.Abs(Projectile.velocity.X) > 0.5f)
                    {
                        Vector2 checkPos = Projectile.position + new Vector2(System.Math.Sign(Projectile.velocity.X) * 16, 0);
                        if (Collision.SolidCollision(checkPos, Projectile.width, Projectile.height))
                        {
                            Projectile.velocity.Y = -5.65f;
                        }
                    }
                }

            // Girar sprite hacia donde se mueve
            if (Projectile.velocity.X != 0f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
            }

            // Desvanecimiento suave antes de desaparecer
            if (Projectile.timeLeft <= 30)
            {
                Projectile.alpha += (int)(255f / 30f);
            }
        }

        // Evitar que el proyectil desaparezca al chocar contra bloques
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = 0f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = 0f;
            }
            return false; 
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.type == Terraria.ID.NPCID.TargetDummy)
            {
                return;
            }
            // Aplica el buff (el tag de 3 de daÃ±o) como indica la wiki - forzando inmunidad
            target.buffImmune[ModContent.BuffType<Buffs.CobwhipTag>()] = false;
            target.AddBuff(ModContent.BuffType<Buffs.CobwhipTag>(), 180);
        }
    }
}
