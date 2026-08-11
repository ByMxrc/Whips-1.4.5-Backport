using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Whips145.Projectiles
{
    public class SlimeWhipProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 10;
            Projectile.WhipSettings.RangeMultiplier = 0.39f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.WhipSettings.Segments = (int)(10 * player.whipRangeMultiplier);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muñecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.6f));
            // Tag del Slime Whip (3 segundos) - forzando ignorar inmunidad
            target.buffImmune[ModContent.BuffType<SlimeWhipTag>()] = false;
            target.AddBuff(ModContent.BuffType<SlimeWhipTag>(), 180);

            // (Removido: El debuff de slime ahora solo se aplica con la punta)

            // Partículas azules de slime (4 es el tipo clásico de slime)
            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(target.position, target.width, target.height, 4, 0f, 0f, 150, new Color(0, 80, 255, 100), 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }

            // La punta del látigo también puede infligir On Fire!
            // Para detectar la punta, obtenemos los puntos del látigo y vemos si el último punto golpea
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            if (list.Count > 0)
            {
                Vector2 tipPos = list[list.Count - 1];
                // Usamos un hitbox imaginario en la punta del látigo para ver si el enemigo lo tocó
                Rectangle tipHitbox = new Rectangle((int)tipPos.X - 20, (int)tipPos.Y - 20, 40, 40);
                if (target.Hitbox.Intersects(tipHitbox))
                {
                    // Partículas de fuego en la punta
                    for (int i = 0; i < 4; i++)
                    {
                        int dustF = Dust.NewDust(target.position, target.width, target.height, DustID.Torch, 0f, 0f, 100, default, 1.3f);
                        Main.dust[dustF].noGravity = true;
                        Main.dust[dustF].velocity *= 0.8f;
                    }

                    // Evitar aplicar On Fire! si ya tiene un debuff de fuego más fuerte
                    // (para no gastar slots de buffs del enemigo y empujar el de Frostburn)
                    bool hasFire = target.HasBuff(BuffID.OnFire) || 
                                   target.HasBuff(BuffID.OnFire3) || 
                                   target.HasBuff(BuffID.CursedInferno) || 
                                   target.HasBuff(BuffID.Frostburn) || 
                                   target.HasBuff(BuffID.Frostburn2) || 
                                   target.HasBuff(BuffID.ShadowFlame);

                    if (!hasFire)
                    {
                        target.AddBuff(BuffID.OnFire, 180);
                    }
                    
                    // Aplica el debuff visual del slime (solo con la punta)
                    target.buffImmune[ModContent.BuffType<SlimeWhipDebuff>()] = false;
                    target.AddBuff(ModContent.BuffType<SlimeWhipDebuff>(), 180);
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

            int totalFrames = 5;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / totalFrames;

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, 4f); 

                if (i == 0)
                {
                    frame.Y = 0;
                }
                else if (i == list.Count - 2)
                {
                    frame.Y = frameHeight * (totalFrames - 1);
                    // Acercamos el punto de anclaje de la punta (baja en el sprite, sube visualmente hacia el mango)
                    // para cerrar el hueco vacío transparente que tenga la punta en su frame.
                    origin.Y = 16f; 
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

                // Escala Y dinámica proporcional a la distancia. 
                // Usamos 14f (aprox. la altura real del slime en el sprite sin transparencias)
                // para que se estire y cubra exactamente todo el hueco entre los puntos del látigo.
                float divisor = 14f;
                float scaleY = diff.Length() / divisor;
                
                // La punta (último segmento) no debe deformarse como un fideo. Limitamos cuánto se estira.
                if (i == list.Count - 2 && scaleY > 1.2f)
                {
                    scaleY = 1.2f;
                }

                Vector2 scale = new Vector2(1f, scaleY);

                // Draw the segment
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                // Partículas al usar el látigo
                if (Main.rand.NextBool(20)) 
                {
                    int dust = Dust.NewDust(pos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, 4, 0f, 0f, 150, new Color(0, 80, 255, 100), 1.2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.3f;
                }

                // Fuego solo en la punta
                if (i == list.Count - 2 && Main.rand.NextBool(3))
                {
                    int dustF = Dust.NewDust(pos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, DustID.Torch, 0f, 0f, 100, default, 1.3f);
                    Main.dust[dustF].noGravity = true;
                    Main.dust[dustF].velocity *= 0.8f;
                }

                pos += diff;
            }

            return false;
        }
    }
}







