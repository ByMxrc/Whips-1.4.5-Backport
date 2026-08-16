using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Buffs;

namespace TerrariaWhipsBackport.Projectiles
{
    public class CobwhipProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1028";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 20;
            Projectile.WhipSettings.RangeMultiplier = 1f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.WhipSettings.Segments = (int)(20 * player.whipRangeMultiplier);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muÃ±ecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.6f));
            // Forzar el tag ignorando inmunidad a debuffs (igual que los tags vanilla de lÃ¡tigos)
            target.buffImmune[ModContent.BuffType<CobwhipTag>()] = false;
            target.AddBuff(ModContent.BuffType<CobwhipTag>(), 180);

            // Buff visible en la barra del JUGADOR (la araÃ±a es una invocaciÃ³n que no ocupa slot)
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<CobwhipPlayerBuff>(), 180);

            // PartÃ­culas blancas de telaraÃ±a
            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(target.position, target.width, target.height, DustID.Web, 0f, 0f, 150, default, 1f);
                Main.dust[dust].noGravity = true; // No caen
                Main.dust[dust].velocity *= 0.5f; // Casi no salen volando
            }

            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            // Solo spawnear nuestra araÃ±ita si el enemigo es vÃ¡lido (no dummies, no animales amistosos)
            if (Main.myPlayer == Projectile.owner && target.CanBeChasedBy())
            {
                int existingSpider = -1;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<CobwhipProj1>())
                    {
                        existingSpider = i;
                        break;
                    }
                }

                if (existingSpider != -1)
                {
                    // Refrescar duraciÃ³n si ya existe
                    Main.projectile[existingSpider].timeLeft = 180;
                    Main.projectile[existingSpider].alpha = 0; // Reiniciar transparencia
                }
                else
                {
                    // Si no existe, crearla encima de su cabeza para evitar spawnear dentro de un bloque
                    Vector2 spawnPos = target.Center - new Vector2(0, target.height / 2 + 16f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        Vector2.Zero,
                        ModContent.ProjectileType<CobwhipProj1>(),
                        2, // 2 de daÃ±o base
                        0f, // 0 knockback
                        Projectile.owner
                    );
                }
            }

            SoundEngine.PlaySound(SoundID.Item17, target.Center); // Sonido de impacto
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer) { }
        public override void ReceiveExtraAI(System.IO.BinaryReader reader) { }
    
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

                // Como la textura original tiene padding transparente y mide 14px pero el arte
                // solo mide unos 6px, al dividir por 14 (frameHeight) el juego la aplasta a la mitad.
                // Usando 6f evitamos que se aplaste, ya que diff.Length() es aprox 7.2px.
                float scaleY = diff.Length() / 6f;
                Vector2 scale = new Vector2(1f, scaleY);

                // Draw the segment
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                // PartÃ­culas al usar el lÃ¡tigo
                if (Vector2.Distance(pos, Main.player[Projectile.owner].Center) > 40f && Main.rand.NextBool(25)) 
                {
                    int dust = Dust.NewDust(pos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, DustID.Web, 0f, 0f, 150, default, 1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.5f;
                }

                pos += diff;
            }

            return false;
        }
    }
}
