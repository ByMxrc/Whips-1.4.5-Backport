using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Buffs;

namespace TerrariaWhipsBackport.Projectiles
{
    // El Starcrash marca al enemigo con energÃ­a "cargada".
    // Cuando un minion golpea al enemigo marcado, se invoca un meteorito
    // que hace 1.33x el daÃ±o del minion en Ã¡rea pequeÃ±a.
    public class StarcrashProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1031";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 10;
            Projectile.WhipSettings.RangeMultiplier = 1f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar efectos en muÃ±ecos de pruebas o animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.3f));
            
            // La marca de meteorito solo se aplica al primer enemigo golpeado
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                // Aplicar tag - forzando ignorar inmunidad a debuffs
                target.buffImmune[ModContent.BuffType<StarcrashTag>()] = false;
                target.AddBuff(ModContent.BuffType<StarcrashTag>(), 300);
                target.buffImmune[ModContent.BuffType<StarcrashEnergy>()] = false;
                target.AddBuff(ModContent.BuffType<StarcrashEnergy>(), 300);

                // Resetear el flag para que pueda caer un nuevo meteorito con esta marca
                target.GetGlobalNPC<Common.WhipTagGlobalNPC>().starcrashMeteorDropped = false;
            }
        }
    
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            if (list.Count < 2) return false;

            Terraria.Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int totalFrames = 6;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / totalFrames;

            // --- 1. DIBUJAR LA LÃNEA MORADA ORIGINAL ---
            // Dibujamos la lÃ­nea usando TODOS los puntos fÃ­sicos para que la curva siempre sea 100% suave
            for (int i = 0; i < list.Count - 2; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                
                Texture2D magicPixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                Vector2 lineScale = new Vector2(diff.Length() + 4f, 2f);
                Color lineColor = new Color(95, 78, 171);
                Color litLineColor = Lighting.GetColor(element.ToTileCoordinates(), lineColor);
                
                Main.EntitySpriteDraw(magicPixel, element - Main.screenPosition, new Rectangle(0, 0, 1, 1), litLineColor, diff.ToRotation(), new Vector2(0, 0.5f), lineScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(magicPixel, element - Main.screenPosition, new Rectangle(0, 0, 1, 1), litLineColor, 0f, new Vector2(0.5f, 0.5f), 2f, SpriteEffects.None, 0);
            }

            // --- 2. CALCULAR Y DIBUJAR EL MANGO ---
            Vector2 handlePos = list[0];
            float handleRot = (list[1] - list[0]).ToRotation() - MathHelper.PiOver2;
            Rectangle handleFrame = new Rectangle(0, 0, frameWidth, frameHeight);
            Main.EntitySpriteDraw(texture, handlePos - Main.screenPosition, handleFrame, Lighting.GetColor(handlePos.ToTileCoordinates()), handleRot, new Vector2(frameWidth / 2f, 4f), 1f, flip, 0);

            // --- 3. CALCULAR Y DIBUJAR LAS ESTRELLAS INTERPOLADAS ---
            float totalLength = 0f;
            for (int i = 0; i < list.Count - 2; i++)
            {
                totalLength += Vector2.Distance(list[i], list[i + 1]);
            }
            
            // Mantengo tu +2f extra
            float segmentLength = (totalLength / 7f) + 2f; 

            for (int s = 1; s <= 6; s++)
            {
                float targetDist = s * segmentLength;
                
                // Prevenir que una estrella desaparezca si la separaciÃ³n extra de +2f sobrepasa el largo total de la cuerda
                if (targetDist >= totalLength) targetDist = totalLength - 0.01f;

                float accumulated = 0f;
                for (int i = 0; i < list.Count - 2; i++)
                {
                    float dist = Vector2.Distance(list[i], list[i + 1]);
                    if (accumulated + dist >= targetDist)
                    {
                        float amount = (targetDist - accumulated) / dist;
                        Vector2 starPos = Vector2.Lerp(list[i], list[i + 1], amount);
                        float starRot = (list[i + 1] - list[i]).ToRotation() - MathHelper.PiOver2;
                        
                        int midFrame = 1 + ((s - 1) % (totalFrames - 2));
                        Rectangle starFrame = new Rectangle(0, frameHeight * midFrame, frameWidth, frameHeight);
                        
                        Main.EntitySpriteDraw(texture, starPos - Main.screenPosition, starFrame, Color.White, starRot, new Vector2(frameWidth / 2f, frameHeight / 2f), 1f, flip, 0);
                        break;
                    }
                    accumulated += dist;
                }
            }

            // --- 4. CALCULAR Y DIBUJAR LA PUNTA ---
            Vector2 tipPos = list[list.Count - 2];
            float tipRot = (list[list.Count - 1] - list[list.Count - 2]).ToRotation() - MathHelper.PiOver2;
            Rectangle tipFrame = new Rectangle(0, frameHeight * (totalFrames - 1), frameWidth, frameHeight);
            Main.EntitySpriteDraw(texture, tipPos - Main.screenPosition, tipFrame, Color.White, tipRot, new Vector2(frameWidth / 2f, frameHeight / 2f), 1f, flip, 0);
            
            if (Vector2.Distance(tipPos, Main.player[Projectile.owner].Center) > 40f && Main.rand.NextBool(1)) 
            {
                int tipDust = Dust.NewDust(tipPos - new Vector2(frameWidth / 2f, frameHeight / 2f), frameWidth, frameHeight, DustID.Torch, 0f, 0f, 100, default, 1.4f);
                Main.dust[tipDust].noGravity = true;
                Main.dust[tipDust].fadeIn = 1.2f; // +0.5s antes de empezar a desvanecerse
                Vector2 outward = tipPos - Main.player[Projectile.owner].Center;
                if (outward != Vector2.Zero)
                    outward.Normalize();
                Main.dust[tipDust].velocity = outward * Main.rand.NextFloat(3f, 5.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            return false;
        }
    }
}
