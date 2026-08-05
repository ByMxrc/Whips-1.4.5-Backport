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
            Projectile.WhipSettings.Segments = 20; // 20 segmentos para un látigo largo y fluido
            Projectile.WhipSettings.RangeMultiplier = 1.3f; // Ajustado para un alcance de aproximadamente 34 tiles
        }

        private int originalItemAnimation;
        private int originalItemAnimationMax;
        private bool hasSavedOriginalAnimation = false;
        private bool isManualDamage = false;

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

            // Forzamos el daño manualmente AHORA, mientras el brazo del jugador sigue falsificado
            isManualDamage = true;
            Projectile.Damage();
            isManualDamage = false;

            // Restauramos los valores originales inmediatamente para no afectar a otros proyectiles ni al jugador
            if (hasSavedOriginalAnimation)
            {
                Player player = Main.player[Projectile.owner];
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

            // Reflejar también los puntos de colisión solo mientras se estira
            if (Projectile.ai[1] == 4f && isExtending)
            {
                MirrorWhipPoints(Projectile.WhipPointsForCollision);
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // El usuario solicitó que TODOS los látigos tengan exactamente el mismo alcance (34 casillas).
            // Ajustamos el RangeMultiplier a 1.16f, ya que 1.3f equivale a ~38 casillas cuando está totalmente estirado.
            Projectile.WhipSettings.RangeMultiplier = 1.16f;

            // Actualiza los segmentos dinámicamente
            Projectile.WhipSettings.Segments = (int)(20 * player.whipRangeMultiplier);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.friendly || target.type == NPCID.TargetDummy || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            Projectile.damage = (int)(Projectile.damage * 0.95f);
        }

        private static readonly int[] frameContentHeight = { 18, 14, 14, 14, 30, 30, 28 };
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

            player.itemAnimation = currentFakeAnimation;
            player.itemAnimationMax = duration;

            Projectile.FillWhipControlPoints(Projectile, list);

            // Si es la tercera ola (dinámico), aplicamos la reflexión solo al estirarse
            if (Projectile.ai[1] == 4f && isExtending)
            {
                MirrorWhipPoints(list);
            }

            // Restaurar valores reales
            player.itemAnimation = prevAnim;
            player.itemAnimationMax = prevAnimMax;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            // Si invertimos la curva, también volteamos el sprite verticalmente para que la iluminación cuadre
            if (Projectile.ai[1] == 4f && isExtending)
            {
                flip |= SpriteEffects.FlipVertically;
            }

            int totalFrames = 7;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height / totalFrames;

            // "pos" es la posición de dibujo acumulada. A propósito NO se
            // recalcula directamente desde list[i] en cada vuelta: se va
            // sumando el vector con el que REALMENTE se dibujó el segmento
            // anterior. Así, aunque limitemos el ángulo del último segmento
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
                    // Punta final: frames 4, 5 y 6 NO son piezas encadenables:
                    // cada uno trae su propio "cuello" dibujado y es un gancho
                    // completo e independiente. Usamos solo UNO como punta.
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

                Rectangle frame = new Rectangle(0, frameHeight * frameIndex, frameWidth, frameHeight);
                // origin apunta al contenido real, no al borde del frame
                Vector2 origin = new Vector2(frameWidth / 2f, frameContentTop[frameIndex]);

                Vector2 element = list[i];
                Vector2 rawDiff = list[i + 1] - element;
                float length = rawDiff.Length();

                float rotation;
                if (length * length < minSegmentLengthSq)
                {
                    // Segmento casi nulo: no confiamos en su dirección,
                    // mantenemos la orientación del segmento anterior.
                    rotation = hasRotation ? lastRotation : 0f;
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
                // ocultamos el segmento. De lo contrario, forzar un mínimo de 0.5f crea
                // una tira fea de texturas apiladas en los pies del jugador.
                if (length < 2f)
                {
                    scaleY = 0f;
                }
                // Piso mínimo de escala para evitar huecos en la curva cuando se estira.
                else if (scaleY < 0.5f)
                {
                    scaleY = 0.5f;
                }

                // El frame de la punta (6) ya trae una curva dibujada dentro
                // del propio sprite. Si el último segmento del whip se alarga
                // mucho (el "chasquido" natural del whip al extenderse), estirar
                // ese sprite con scaleY distorsiona su curva ya dibujada y hace
                // que se vea como si "volara" lejos del resto de la cuerda.
                // Por eso, solo para ese frame, topamos cuánto se puede estirar.
                if (frameIndex == 6 && scaleY > 1.3f)
                    scaleY = 1.3f;

                Vector2 scale = new Vector2(1f, scaleY);
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
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