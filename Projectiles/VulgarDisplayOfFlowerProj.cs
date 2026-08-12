using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Buffs;

namespace TerrariaWhipsBackport.Projectiles
{
    public class VulgarDisplayOfFlowerProj : ModProjectile
    {
        private bool appliedEnergy = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 11;
            Projectile.WhipSettings.RangeMultiplier = 1f;
        }

        public override void PostAI()
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            // DirecciÃ³n general del lÃ¡tigo para la inercia de las partÃ­culas
            Vector2 whipDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX * Projectile.spriteDirection);
            Player player = Main.player[Projectile.owner];

            // PartÃ­culas durante la animaciÃ³n de ataque
            for (int i = 0; i < list.Count; i++)
            {
                // Evitamos generar polvo si los segmentos estÃ¡n retraÃ­dos o muy cerca del jugador
                if (Vector2.Distance(list[i], player.MountedCenter) < 50f)
                    continue;

                if (Main.rand.NextBool(13)) // Densidad ajustada (un 10% adicional mÃ¡s)
                {
                    int type = Main.rand.NextBool() ? 166 : 167;
                    
                    // Inercia en la direcciÃ³n del lÃ¡tigo con una ligera dispersiÃ³n
                    Vector2 dustVelocity = whipDirection * Main.rand.NextFloat(3f, 6f) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                    
                    int dust = Dust.NewDust(list[i], 0, 0, type, dustVelocity.X, dustVelocity.Y, 100, default, 1.4f);
                    
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].velocity *= 1.2f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muÃ±ecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.15f));
            
            // Tag damage (9 tag damage, dura 4 segundos = 240 ticks) - forzando ignorar inmunidad
            target.buffImmune[ModContent.BuffType<VulgarDisplayTag>()] = false;
            target.AddBuff(ModContent.BuffType<VulgarDisplayTag>(), 240);

            // La energÃ­a volÃ¡til solo se aplica al primer enemigo golpeado por cada balanceo
            if (!appliedEnergy)
            {
                appliedEnergy = true;
                target.buffImmune[ModContent.BuffType<VulgarDisplayEnergy>()] = false;
                target.AddBuff(ModContent.BuffType<VulgarDisplayEnergy>(), 240);
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
                Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f); 

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

                // Draw the connecting line (solo si no es el Ãºltimo tramo despuÃ©s de la punta)
                if (i < list.Count - 2)
                {
                    Texture2D magicPixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                    Vector2 lineScale = new Vector2(diff.Length(), 2f);
                    Color lineColor = new Color(126, 176, 54);
                    Color litLineColor = Lighting.GetColor(element.ToTileCoordinates(), lineColor);
                    Main.EntitySpriteDraw(magicPixel, element - Main.screenPosition, new Rectangle(0, 0, 1, 1), litLineColor, diff.ToRotation(), new Vector2(0, 0.5f), lineScale, SpriteEffects.None, 0);
                }

                // Las flores van creciendo en tamaÃ±o desde el mango hasta la punta para igualar el diseÃ±o original
                float segmentScale = MathHelper.Lerp(0.5f, 1.2f, (float)i / list.Count);
                if (i == list.Count - 2) segmentScale = 1f; // La punta mantiene su tamaÃ±o original

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, segmentScale, flip, 0);
                pos += diff;
            }

            return false;
        }
    }
}





