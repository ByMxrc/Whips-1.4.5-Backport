using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Whips145.Projectiles
{
    /// <summary>
    /// PossessionProj - Proyectil para la animación del látigo Possession.
    /// Látigo simple de 20 segmentos y largo alcance sin marcas, buffs ni efectos adicionales.
    /// </summary>
    public class PossessionProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.Segments = 20; // 20 segmentos (añadir más solo hace que el látigo se enrosque y pierda alcance)
            Projectile.WhipSettings.RangeMultiplier = 1.09f; // Multiplicador exacto para llegar a 34 casillas
        }

        private int originalItemAnimation;
        private int originalItemAnimationMax;
        private bool hasSavedOriginalAnimation = false;
        private bool isManualDamage = false;

        // ─── COMBO ELIMINADO DEL PROYECTIL ───────────────────
        // Ahora la lógica de combo se maneja en Possession.cs con PossessionPlayer.

        private int ChainRoleCode => (int)Projectile.ai[1];
        private bool IsChainWhip => ChainRoleCode >= 10; // los roles legacy son 0,1,2,4
        private int ChainIndex => IsChainWhip ? ChainRoleCode / 10 : 0;
        private int ChainMember => IsChainWhip ? ChainRoleCode % 10 : -1; // 0=solo,1=pairA,2=pairB
        private bool IsChainPairA => IsChainWhip && ChainMember == 1;
        private bool IsChainPairB => IsChainWhip && ChainMember == 2;
        private bool IsChainSolo => IsChainWhip && ChainMember == 0;


        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = player.itemAnimationMax > 0 ? player.itemAnimationMax : 30;
                Projectile.localAI[2] = Projectile.ai[2];
            }

            float delay = Projectile.localAI[2];
            int realFramesPassed = (int)Projectile.localAI[0] / (Projectile.extraUpdates + 1);

            if (realFramesPassed < delay)
            {
                Projectile.timeLeft = (int)Projectile.localAI[1] * (Projectile.extraUpdates + 1);
                Projectile.localAI[0]++;
                return false;
            }

            if (!hasSavedOriginalAnimation)
            {
                originalItemAnimation = player.itemAnimation;
                originalItemAnimationMax = player.itemAnimationMax;
                hasSavedOriginalAnimation = true;
            }

            int duration = (int)Projectile.localAI[1];
            int framesSinceStart = realFramesPassed - (int)delay;
            int currentFakeAnimation = duration - framesSinceStart;
            if (currentFakeAnimation < 0) currentFakeAnimation = 0;

            player.itemAnimation = currentFakeAnimation;
            player.itemAnimationMax = duration;

            if (currentFakeAnimation <= 0)
            {
                Projectile.Kill();
                return false;
            }

            Projectile.localAI[0]++;
            return true;
        }

        public override void PostAI()
        {
            float delay = Projectile.localAI[2];
            int realFramesPassed = (int)Projectile.localAI[0] / (Projectile.extraUpdates + 1);

            if (realFramesPassed < delay)
            {
                return;
            }

            Player player = Main.player[Projectile.owner];

            // Forzamos el daño manualmente AHORA, mientras el brazo del jugador sigue falsificado
            isManualDamage = true;
            Projectile.Damage();
            isManualDamage = false;

            // Restauramos los valores originales inmediatamente para no afectar a otros proyectiles ni al jugador
            if (hasSavedOriginalAnimation)
            {
                player.itemAnimation = originalItemAnimation;
                player.itemAnimationMax = originalItemAnimationMax;
                hasSavedOriginalAnimation = false;
            }

            // Calcular si está estirándose o recogiéndose
            int duration = (int)Projectile.localAI[1];
            int framesSinceStart = realFramesPassed - (int)delay;
            int currentFakeAnimation = duration - framesSinceStart;
            if (currentFakeAnimation < 0) currentFakeAnimation = 0;

            bool isExtending = currentFakeAnimation > duration / 2;

            // progreso de vida del látigo, de 0 (recién sale) a 1 (totalmente recogido)
            float progressT = duration > 0 ? MathHelper.Clamp(framesSinceStart / (float)duration, 0f, 1f) : 0f;

            // Partículas a lo largo del cuerpo del látigo (como Snapthorn)
            // Solo lo aplicamos al látigo principal (roles 0 y 2) para no saturar la pantalla con los 4 látigos.
            if (Projectile.ai[1] == 0f || Projectile.ai[1] == 2f)
            {
                if (Main.rand.Next(100) < 90) // 90% de probabilidad cada frame (+15% extra respecto al 75% anterior)
                {
                    // Falsificamos la animación para obtener los puntos del látigo en este frame exacto
                    int originalMax = player.itemAnimationMax;
                    int originalAnim = player.itemAnimation;
                    player.itemAnimationMax = duration;
                    player.itemAnimation = currentFakeAnimation;

                    List<Vector2> list = new List<Vector2>();
                    Projectile.FillWhipControlPoints(Projectile, list);

                    player.itemAnimationMax = originalMax;
                    player.itemAnimation = originalAnim;

                    if (list.Count > 0)
                    {
                        // Elegimos un punto aleatorio en la mitad exterior del látigo (para alejarlo del jugador)
                        int index = Main.rand.Next(list.Count / 2, list.Count);
                        Vector2 dustPos = list[index];

                        // Asegurarnos de que no aparezcan pegadas al jugador ni cuando el látigo está enrollado
                        if (Vector2.Distance(dustPos, player.Center) > 80f)
                        {
                            // 228 y 229 son los IDs exactos que usa el Phantasmal Eye de Moon Lord en el código original de Terraria
                            int dustType = Main.rand.NextBool(2) ? 229 : 228;

                            Dust dust = Dust.NewDustDirect(dustPos - new Vector2(5, 5), 10, 10, dustType, 0f, 0f, 100, default, 1.2f);
                            dust.noGravity = true;
                            dust.velocity *= 0.3f; // Que se queden flotando suavemente como una estela
                        }
                    }
                }
            }

            // Reflejar también los puntos de colisión solo mientras se estira (látigo ancla legacy)
            if (Projectile.ai[1] == 4f && isExtending)
            {
                MirrorWhipPoints(Projectile.WhipPointsForCollision);
            }
            // Pareja de la cadena nueva: uno viene de arriba (normal) y otro de abajo (espejado)
            // Ya no usamos interpolación (progressT) para no "forzar" la curva en el centro.
            else if (IsChainPairA || IsChainPairB)
            {
                float mix = IsChainPairA ? 0f : 1f;
                MirrorWhipPointsLerp(Projectile.WhipPointsForCollision, mix);
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Usamos 1.09f, que es el multiplicador matemáticamente exacto para que el 
            // alcance base (que es 31) llegue clavado a los 34 bloques.
            Projectile.WhipSettings.RangeMultiplier = 1.09f;

            // Actualiza los segmentos dinámicamente
            Projectile.WhipSettings.Segments = (int)(20 * player.whipRangeMultiplier);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * 0.95f);
        }

        private static readonly int[] frameContentHeight = { 18, 14, 14, 14, 24, 26, 28 };
        private static readonly int[] frameContentTop    = { 2, 6, 6, 6, 0, 0, 0 };

        // Función matemática para invertir la curva del látigo ("de abajo hacia arriba")
        private void MirrorWhipPoints(List<Vector2> points)
        {
            if (points == null || points.Count < 2) return;

            Vector2 origin = points[0];
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                Vector2 v = p - origin;
                
                // Proyección sobre la dirección en la que apuntamos
                Vector2 proj = Vector2.Dot(v, direction) * direction;
                // Componente perpendicular (la curva)
                Vector2 perp = v - proj;
                
                // Invertimos la componente perpendicular
                points[i] = origin + proj - perp;
            }
        }

        // Versión continua del mirror: en vez de invertir de golpe, interpola la
        // componente perpendicular entre la curva normal (mix=0, curva pronunciada
        // "hacia arriba") y la espejada (mix=1, "hacia abajo"). La curva hacia abajo
        // se atenúa con downStrength para que no se doble tan pronunciada como la
        // de arriba (queda más recta). Dos látigos con mix opuesto se cruzan cerca
        // de mix=0.5.
        private const float DownCurveStrength = 0.5f; // 1 = igual de pronunciada, 0 = totalmente recta hacia abajo

        private void MirrorWhipPointsLerp(List<Vector2> points, float mixFactor)
        {
            if (points == null || points.Count < 2) return;

            Vector2 origin = points[0];
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float t = MathHelper.Clamp(mixFactor, 0f, 1f);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                Vector2 v = p - origin;

                Vector2 proj = Vector2.Dot(v, direction) * direction;
                Vector2 perp = v - proj;

                Vector2 mirroredPerp = -perp * DownCurveStrength;
                Vector2 mixedPerp = Vector2.Lerp(perp, mirroredPerp, t);
                points[i] = origin + proj + mixedPerp;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float delay = Projectile.localAI[2];
            int realFramesPassed = (int)Projectile.localAI[0] / (Projectile.extraUpdates + 1);

            if (realFramesPassed < delay)
            {
                return false; // No dibujar nada si está en espera
            }

            List<Vector2> list = new List<Vector2>();

            Player player = Main.player[Projectile.owner];
            int prevAnim = player.itemAnimation;
            int prevAnimMax = player.itemAnimationMax;

            // Engañar a FillWhipControlPoints
            int duration = (int)Projectile.localAI[1];
            int framesSinceStart = realFramesPassed - (int)delay;
            int currentFakeAnimation = duration - framesSinceStart;
            if (currentFakeAnimation < 0) currentFakeAnimation = 0;

            bool isExtending = currentFakeAnimation > duration / 2;

            // Mix de cruce para la pareja de la cadena nueva
            // En vez de usar progressT (que deformaba la curva hacia el centro),
            // dejamos uno fijo arriba (0) y otro fijo abajo (1).
            bool isChainPair = IsChainPairA || IsChainPairB;
            float chainMix = 0f;
            if (isChainPair)
            {
                chainMix = IsChainPairA ? 0f : 1f;
                // Modificamos el timer falso del látigo
                // para que esté muy rezagado. Así la Vanilla AI generará los puntos
                // en una posición totalmente diferente de su rotación.
                int timeOffset = duration / 3;
                currentFakeAnimation -= timeOffset;
                
                // Si el offset hace que baje de 0, significa que para la Vanilla AI 
                // ya terminó su golpe. En lugar de dejarlo colgando congelado en el frame 0
                // (lo que crea la "tira" fea colgando hacia abajo), simplemente dejamos de dibujarlo.
                if (currentFakeAnimation <= 0) 
                {
                    return false;
                }
            }

            player.itemAnimation = currentFakeAnimation;
            player.itemAnimationMax = duration;

            Projectile.FillWhipControlPoints(Projectile, list);

            // Si es la tercera ola (dinámico), aplicamos la reflexión solo al estirarse
            if (Projectile.ai[1] == 4f && isExtending)
            {
                MirrorWhipPoints(list);
            }
            // Pareja de la cadena nueva: mirror continuo durante toda su vida sin deformar
            else if (isChainPair)
            {
                MirrorWhipPointsLerp(list, chainMix);
            }

            // Restaurar valores reales
            player.itemAnimation = prevAnim;
            player.itemAnimationMax = prevAnimMax;

            if (list.Count < 2)
                return false;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("Whips145/Projectiles/PossessionProj_1").Value;
            
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            int totalFrames = 7;
            int frameWidth = texture.Width; 
            int frameHeight = texture.Height / totalFrames;

            // (la punta), el punto de partida del siguiente segmento sigue
            // siendo exactamente donde terminó el anterior -> nunca hay hueco.
            Vector2 pos = list[0];

            float lastRotation = 0f;
            bool hasRotation = false;

            // Umbral de longitud (en px^2) por debajo del cual consideramos el
            // segmento "casi nulo" y reutilizamos la rotación anterior en vez
            // de calcular una nueva dirección poco fiable.
            const float minSegmentLengthSq = 4f; // ~2px

            // Máxima diferencia angular (radianes) permitida entre un segmento
            // y el anterior. Evita que la punta "chasquee" lejos del resto de
            // la curva cuando el último punto de control de
            // FillWhipControlPoints se adelanta bruscamente (crack del whip).
            const float maxAngleDeltaFromPrev = MathHelper.PiOver4 * 0.6f; // ~27°

            for (int i = 0; i < list.Count - 1; i++)
            {
                int frameIndex;
                if (i == 0)
                {
                    frameIndex = 0; // Mango
                }
                else if (i == list.Count - 2)
                {
                    // Punta final: forzamos el frame 6 para todos los látigos 
                    // porque su sprite es más delgado y encaja a la perfección sin sobresalir.
                    frameIndex = 6;
                }
                else if (i == list.Count - 3)
                {
                    frameIndex = 3; // Pre-punta (último segmento recto antes del gancho)
                }
                else
                {
                    frameIndex = 1 + (i % 2); // Cicla solo entre frame 1 y 2 para el cuerpo
                }

                int currentFrameHeight = frameHeight;
                Vector2 origin = new Vector2(frameWidth / 2f, frameContentTop[frameIndex]);

                // Solo para las puntas (frames 4, 5 y 6), recortamos la altura del frame
                // para ignorar los píxeles basura accidentales que están en el fondo del sprite.
                if (frameIndex >= 4)
                {
                    currentFrameHeight = frameContentHeight[frameIndex];
                }

                Rectangle frame = new Rectangle(0, frameHeight * frameIndex, frameWidth, currentFrameHeight);

                Vector2 element = list[i];
                Vector2 rawDiff = list[i + 1] - element;
                float length = rawDiff.Length();

                float rotation;
                if (length * length < minSegmentLengthSq)
                {
                    // Segmento casi nulo: no confiamos en su dirección,
                    // mantenemos la orientación del segmento anterior, o si es el primero, 
                    // usamos la velocidad general del látigo para que no apunten hacia abajo.
                    rotation = hasRotation ? lastRotation : Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                }
                else
                {
                    // Si es la punta (frames 4, 5 y 6), forzamos la misma rotación que la pre-punta.
                    // Como el sprite de la punta tiene 6 píxeles de "cuerpo" por encima del punto de origen,
                    // si la punta gira respecto a la pre-punta, esos 6 píxeles sobresalen por los lados.
                    // Al obligarla a seguir la misma línea, esos píxeles se ocultan perfectamente dentro de la pre-punta.
                    if (frameIndex >= 4 && hasRotation)
                    {
                        rotation = lastRotation;
                    }
                    else
                    {
                        rotation = rawDiff.ToRotation() - MathHelper.PiOver2;

                        if (hasRotation)
                        {
                            // Limita cuánto puede girar este segmento respecto al anterior.
                            float delta = MathHelper.WrapAngle(rotation - lastRotation);
                            if (delta > maxAngleDeltaFromPrev)
                                rotation = lastRotation + maxAngleDeltaFromPrev;
                            else if (delta < -maxAngleDeltaFromPrev)
                                rotation = lastRotation - maxAngleDeltaFromPrev;
                        }
                    }
                }

                lastRotation = rotation;
                hasRotation = true;

                // Vector de dibujo REAL: misma longitud que el original, pero
                // en la dirección (posiblemente limitada) con la que
                // efectivamente se va a rotar el sprite. Esto es lo que
                // garantiza que no quede un hueco visual entre segmentos.
                Vector2 drawDirection = (rotation + MathHelper.PiOver2).ToRotationVector2();
                Vector2 diff = drawDirection * length;

                Color color = Lighting.GetColor(element.ToTileCoordinates());

                float divisor = frameContentHeight[frameIndex];
                float scaleY = length / divisor;

                // Si la longitud del segmento es prácticamente nula (el látigo está recogido),
                // ocultamos el segmento.
                if (length < 2f)
                {
                    scaleY = 0f;
                }

                // Los frames de la punta (4, 5 y 6) ya traen una curva dibujada.
                // Si permitimos que se encojan, la curva se distorsiona y se ven "aplastadas".
                // Mientras no estén ocultas (scaleY > 0), forzamos un mínimo de 1f.
                if (frameIndex >= 4 && scaleY > 0f)
                {
                    if (scaleY < 1f)
                        scaleY = 1f; // Evita puntas aplastadas
                    else if (scaleY > 1.3f)
                        scaleY = 1.3f; // Evita que se estiren de más y vuelen
                }

                Vector2 scale = new Vector2(1f, scaleY);
                
                // Capa base (con iluminación del mundo)
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                
                // Capa brillante (Glowmask pura que no es afectada por la oscuridad)
                // SOLO la dibujamos para las puntas (frames 4, 5 y 6) para evitar
                // que el cuerpo se vuelva un manchón blanco al superponerse.
                if (frameIndex >= 4)
                {
                    Main.EntitySpriteDraw(glowTexture, pos - Main.screenPosition, frame, Color.White, rotation, origin, scale, flip, 0);
                }
                
                pos += diff;
            }

            return false;
        }

        public override bool? CanDamage()
        {
            if (isManualDamage) return true;
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            
            // Para la detección de colisiones, Terraria usa FillWhipControlPoints, el cual puede leer itemAnimation e itemAnimationMax.
            // Si el jugador hace un solo click, su itemAnimation real vuelve a 0 mientras nuestros látigos con delay siguen vivos.
            // Esto causa que FillWhipControlPoints colapse la hitbox al centro del jugador, haciendo que no haga daño.
            // Solución: Falsificamos ambos valores para la colisión exactamente igual que en la IA y el dibujo.
            int originalMax = player.itemAnimationMax;
            int originalAnim = player.itemAnimation;

            int duration = (int)Projectile.localAI[1] > 0 ? (int)Projectile.localAI[1] : 30;
            float delay = Projectile.localAI[2];
            int realFramesPassed = (int)Projectile.localAI[0] / (Projectile.extraUpdates + 1);
            int framesSinceStart = realFramesPassed - (int)delay;
            int currentFakeAnimation = duration - framesSinceStart;
            if (currentFakeAnimation < 0) currentFakeAnimation = 0;

            player.itemAnimationMax = duration;
            player.itemAnimation = currentFakeAnimation;

            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            // Restauramos
            player.itemAnimationMax = originalMax;
            player.itemAnimation = originalAnim;

            for (int n = 0; n < list.Count - 1; n++)
            {
                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), list[n], list[n + 1], Projectile.width, ref collisionPoint))
                {
                    return true;
                }
            }
            return false;
        }
    }
}