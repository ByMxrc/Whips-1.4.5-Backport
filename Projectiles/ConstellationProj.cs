using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Buffs;
using Whips145.Common;

namespace Whips145.Projectiles
{
    public class ConstellationProj : ModProjectile
    {
        private bool spawnedStars = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 20; // 20 segmentos = ~17 estrellas (igual que la original de stardust con armaduras que aumentan rango)
            Projectile.WhipSettings.RangeMultiplier = 1.18f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Evitar muñecos de pruebas y animales
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * (1f - 0.1f));
            
            // Tag de Constellation (4 segundos = 240 ticks) - forzando ignorar inmunidad
            target.buffImmune[ModContent.BuffType<ConstellationTag>()] = false;
            target.AddBuff(ModContent.BuffType<ConstellationTag>(), 240);

            Player player = Main.player[Projectile.owner];

            // Efecto: Liberar 6 estrellas fugaces (solo en el primer golpe de cada balanceo)
            if (!spawnedStars && Projectile.owner == Main.myPlayer)
            {
                spawnedStars = true;
                int starDamage = (int)(Projectile.originalDamage * 0.297f);

                List<Vector2> list = new List<Vector2>();
                Projectile.FillWhipControlPoints(Projectile, list);

                int maxStars = 6;
                int spawned = 0;

                for (int i = 1; i < list.Count - 1 && spawned < maxStars; i++)
                {
                    int j = i - 1;
                    if (j % 3 == 0) // Exactamente la misma lógica del patrón visual (1 sí, 2 no)
                    {
                        Vector2 spawnPos = list[i];
                        
                        // Velocidad inicial hacia afuera
                        Vector2 initialVelocity = Main.rand.NextVector2CircularEdge(5.4f, 5.4f) * Main.rand.NextFloat(0.8f, 1.2f);
                        
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPos,
                            initialVelocity,
                            ModContent.ProjectileType<ConstellationProj1>(),
                            starDamage,
                            Projectile.knockBack,
                            Projectile.owner,
                            target.whoAmI // ai[0] guarda el NPC inicial al que debe perseguir
                        );
                        
                        spawned++;
                    }
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

            int totalFrames = 6;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / totalFrames;

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, frameWidth, frameHeight);
                Vector2 origin = new Vector2(frameWidth / 2f, 4f); 
                float scale = 1f;

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
                
                // Ignorar la luz del mapa SOLO para los segmentos (estrellas). 
                // El mango (i == 0) y la punta (i == list.Count - 2) usan luz normal.
                Color color;
                if (i == 0 || i == list.Count - 2)
                    color = Lighting.GetColor(element.ToTileCoordinates());
                else
                    color = Color.White;

                bool isBig = false;
                bool isX = false;
                if (i > 0 && i < list.Count - 2)
                {
                    int j = i - 1;
                    // Patrón de 3 segmentos: 1 brilla, 2 no brillan
                    isBig = (j % 3 == 0); 
                    isX = (j % 3 == 1);
                    
                    // Todos tamaño similar (0.45), menos la cruz grande que aumenta un ~5% extra (0.58)
                    scale = isBig ? 0.58f : 0.45f;
                    
                    if (isX)
                    {
                        // Si es una "x", la rotamos 45 grados respecto al látigo
                        rotation += MathHelper.PiOver4;
                    }
                }

                // Draw the connecting line (solo si no es el último tramo después de la punta)
                if (i < list.Count - 2)
                {
                    Texture2D magicPixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                    Vector2 lineScale = new Vector2(diff.Length(), 1.5f); // Grosor cambiado de 2f a 1f
                    Color lineColor = new Color(37, 116, 176);
                    // Multiplicar por 0.9f le aplica un 10% de transparencia (90% visible)
                    Color litLineColor = lineColor * 0.75f; 
                    Main.EntitySpriteDraw(magicPixel, element - Main.screenPosition, new Rectangle(0, 0, 1, 1), litLineColor, diff.ToRotation(), new Vector2(0, 0.5f), lineScale, SpriteEffects.None, 0);
                }

                // Luz ambiental
                Lighting.AddLight(element, 0.05f, 0.3f, 0.5f);

                // Ignoramos los primeros segmentos pegados al jugador (i <= 2)
                // Y hacemos que la punta suelte MÁS polvo que la base, para compensar que la punta viaja más rápido
                if (i > 2 && i < list.Count - 1)
                {
                    // i bajo (cerca del jugador) = menos probabilidad
                    // i alto (cerca de la punta) = más probabilidad
                    int chance = 60 - (i * 3);
                    if (chance < 10) chance = 10;

                    if (Main.rand.NextBool(chance))
                    {
                        int d = Dust.NewDust(element - new Vector2(4, 4), 8, 8, 229, 0f, 0f, 0, Color.White, 0.4f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 0.5f;
                        Main.dust[d].fadeIn = 0.8f;
                    }
                }

                // Draw the segment bloom (Mango y todas las estrellas)
                if (i >= 0 && i < list.Count - 2)
                {
                    bool isRedCircle = (i == 0 || isBig);
                    
                    float progress = (float)i / list.Count;
                    float intensity = 0.6f + (0.4f * progress);
                    
                    Color bloomColor = new Color(37, 116, 176, 0) * (0.3f * intensity); 
                    
                    Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_89").Value;
                    Vector2 glowOrigin = glowTex.Size() / 2f;
                    
                    // LÓGICA DE TAMAÑOS POR ZONAS (Amarilla, Verde, Roja)
                    float baseGlowScale;
                    if (progress <= 0.33f) {
                        // Zona Amarilla (Base): Tamaño actual de las verdes (~0.66)
                        baseGlowScale = 0.66f;
                    } else if (progress <= 0.66f) {
                        // Zona Verde (Medio): Aumento del 10% (0.66 * 1.10 = 0.726)
                        // Interpolamos suavemente para que no haya un salto brusco
                        float t = (progress - 0.33f) / 0.33f;
                        baseGlowScale = MathHelper.Lerp(0.66f, 0.726f, t);
                    } else {
                        // Zona Roja (Punta): Aumento del 25% (0.70 * 1.25 = 0.875)
                        float t = (progress - 0.66f) / 0.34f;
                        baseGlowScale = MathHelper.Lerp(0.726f, 0.875f, t);
                    }
                    
                    // Aumento del 15% pedido para las cruces
                    baseGlowScale *= 1.15f;
                    
                    // Mantenemos las estrellas pequeñas a un 20% del tamaño, y damos +5% extra a las grandes
                    float glowScale = baseGlowScale * (isRedCircle ? 1.4f : 0.35f); // Aumentado el 0.90f a 1.4f para que brillen MUCHO más

                    // CÁLCULO DE ALINEACIÓN
                    Vector2 visualCenter = (pos - Main.screenPosition) + diff.SafeNormalize(Vector2.Zero) * ((frameHeight / 2f) - 4f) * scale;

                    // 1. Núcleo central suave
                    Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor * (isRedCircle ? 0.4f : 0.25f), 0f, glowOrigin, glowScale * 0.7f, SpriteEffects.None, 0);
                    
                    // Añadir un núcleo blanco extra para las estrellas que más brillan
                    if (isRedCircle) {
                        Main.EntitySpriteDraw(glowTex, visualCenter, null, Color.White * 0.6f, 0f, glowOrigin, glowScale * 0.2f, SpriteEffects.None, 0);
                    }

                    // ALINEACIÓN EN PANTALLA (0 GRADOS, O 45 GRADOS PARA LAS X)
                    float flareRot = isX ? MathHelper.PiOver4 : 0f;

                    // 2. Destello vertical/diagonal
                    Vector2 scaleLong = new Vector2(0.12f, 1.3f) * glowScale;
                    Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor, flareRot, glowOrigin, scaleLong, SpriteEffects.None, 0);

                    // 3. Destello horizontal/diagonal opuesto
                    Vector2 scaleShort = new Vector2(0.8f, 0.12f) * glowScale;
                    Main.EntitySpriteDraw(glowTex, visualCenter, null, bloomColor, flareRot, glowOrigin, scaleShort, SpriteEffects.None, 0);
                }

                // Draw the main segment
                if (i > 0 && i < list.Count - 2)
                {
                    // Para que las estrellas rotadas (X) no se salgan de la línea, las dibujamos rotando sobre su centro visual
                    Vector2 visualCenter = (pos - Main.screenPosition) + diff.SafeNormalize(Vector2.Zero) * ((frameHeight / 2f) - 4f) * scale;
                    Vector2 trueOrigin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                    Main.EntitySpriteDraw(texture, visualCenter, frame, color, rotation, trueOrigin, scale, SpriteEffects.None, 0);
                }
                else
                {
                    // Mango y punta se dibujan de forma normal
                    Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                }
                
                pos += diff;
            }

            return false;
        }
    }
}
