using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaWhipsBackport.Buffs;

namespace TerrariaWhipsBackport.Projectiles
{
    /// <summary>
    /// PossessionProj - Proyectil para la animaciÃ³n del lÃ¡tigo Possession.
    /// Aplica PossessionTag a los enemigos golpeados (+25 tag damage para invocaciones).
    /// Las mecÃ¡nicas adicionales del Possession se implementarÃ¡n mÃ¡s adelante.
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
            Projectile.WhipSettings.Segments = 20; // 20 segmentos (aÃ±adir mÃ¡s solo hace que el lÃ¡tigo se enrosque y pierda alcance)
            Projectile.WhipSettings.RangeMultiplier = 1.09f; // Multiplicador exacto para llegar a 34 casillas
        }

        private int originalItemAnimation;
        private int originalItemAnimationMax;
        private bool hasSavedOriginalAnimation = false;
        private bool isManualDamage = false;

        // â”€â”€â”€ COMBO ELIMINADO DEL PROYECTIL â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // Ahora la lÃ³gica de combo se maneja en Possession.cs con PossessionPlayer.

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

            // Forzamos el daÃ±o manualmente AHORA, mientras el brazo del jugador sigue falsificado
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

            // Calcular si estÃ¡ estirÃ¡ndose o recogiÃ©ndose
            int duration = (int)Projectile.localAI[1];
            int framesSinceStart = realFramesPassed - (int)delay;
            int currentFakeAnimation = duration - framesSinceStart;
            if (currentFakeAnimation < 0) currentFakeAnimation = 0;

            bool isExtending = currentFakeAnimation > duration / 2;

            // progreso de vida del lÃ¡tigo, de 0 (reciÃ©n sale) a 1 (totalmente recogido)
            float progressT = duration > 0 ? MathHelper.Clamp(framesSinceStart / (float)duration, 0f, 1f) : 0f;

            // PartÃ­culas a lo largo del cuerpo del lÃ¡tigo (alta densidad como lÃ¡tigos mÃ¡gicos/elÃ©ctricos)
            // Ya no lo limitamos a un par de lÃ¡tigos, y ademÃ¡s generamos varias partÃ­culas por frame.
            if (Projectile.ai[1] == 0f || Projectile.ai[1] == 2f || IsChainPairA || IsChainPairB)
            {
                // Para alta densidad, iteramos varias veces por frame
                int dustsPerFrame = 3; 
                for (int i = 0; i < dustsPerFrame; i++)
                {
                    if (Main.rand.Next(100) < 45) // Reducido un 15% adicional (de 72 a 60)
                    {
                        // Falsificamos la animaciÃ³n para obtener los puntos del lÃ¡tigo en este frame exacto
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
                            // Elegimos un punto aleatorio a lo largo del lÃ¡tigo
                            // (ahora permitimos desde el 20% del lÃ¡tigo en adelante para mÃ¡s densidad visual)
                            int index = Main.rand.Next(list.Count / 5, list.Count);
                            Vector2 dustPos = list[index];

                            // Asegurarnos de que no aparezcan directamente dentro del cuerpo del jugador
                            if (Vector2.Distance(dustPos, player.Center) > 40f)
                            {
                                // 228 y 229 son los IDs exactos que usa el Phantasmal Eye de Moon Lord
                                int dustType = Main.rand.NextBool(2) ? 229 : 228;

                                Dust dust = Dust.NewDustDirect(dustPos - new Vector2(5, 5), 10, 10, dustType, 0f, 0f, 100, default, 1.4f);
                                dust.noGravity = true;
                                dust.velocity *= 0.8f; // Estela mÃ¡s pronunciada al moverse
                            }
                        }
                    }
                }
            }

            // Reflejar tambiÃ©n los puntos de colisiÃ³n solo mientras se estira (lÃ¡tigo ancla legacy)
            if (Projectile.ai[1] == 4f && isExtending)
            {
                MirrorWhipPoints(Projectile.WhipPointsForCollision);
            }
            // Pareja de la cadena nueva: uno viene de arriba (normal) y otro de abajo (espejado)
            // Ya no usamos interpolaciÃ³n (progressT) para no "forzar" la curva en el centro.
            else if (IsChainPairA || IsChainPairB)
            {
                float mix = IsChainPairA ? 0f : 1f;
                MirrorWhipPointsLerp(Projectile.WhipPointsForCollision, mix);
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Usamos 1.09f, que es el multiplicador matemÃ¡ticamente exacto para que el 
            // alcance base (que es 31) llegue clavado a los 34 bloques.
            Projectile.WhipSettings.RangeMultiplier = 1.09f;

            // Actualiza los segmentos dinÃ¡micamente
            Projectile.WhipSettings.Segments = (int)(20 * player.whipRangeMultiplier);
        }

        private int hitCount = 0;
        private HashSet<int> hitEnemiesForLasers = new HashSet<int>();

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Calculamos el multiplicador acumulativo del 5% basado en cuÃ¡ntos enemigos ya hemos golpeado.
            // System.Math.Pow(0.95, hitCount) hace que el 1er golpe sea 100%, el 2do 95%, el 3ro 90.25%, etc.
            float mult = (float)System.Math.Pow(0.95, hitCount);
            modifiers.SourceDamage *= mult;
            hitCount++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.friendly || target.lifeMax <= 5 || NPCID.Sets.CountsAsCritter[target.type])
                return;

            // Aplicar la marca de daÃ±o del Possession - forzando ignorar inmunidad, EXCEPTO a dummys
            if (target.type != Terraria.ID.NPCID.TargetDummy)
            {
                target.buffImmune[ModContent.BuffType<PossessionTag>()] = false;
                target.AddBuff(ModContent.BuffType<PossessionTag>(), 300); // 5 segundos
                
                target.buffImmune[ModContent.BuffType<PossessionEyeTag>()] = false;
                target.AddBuff(ModContent.BuffType<PossessionEyeTag>(), 300); // 5 segundos
                
                target.GetGlobalNPC<Common.WhipTagGlobalNPC>().possessionEyeDropped = false;
            }

            // Asegurarnos de que este lÃ¡tigo especÃ­fico solo genere 1 lÃ¡ser por cada enemigo que toque.
            if (hitEnemiesForLasers.Contains(target.whoAmI))
                return;
            
            hitEnemiesForLasers.Add(target.whoAmI);

            Player player = Main.player[Projectile.owner];
            Vector2 dir = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);
            
            if (Main.rand.Next(1) == 0) // chance = 1 (100%)
            {
                int activeCount = 0;
                int laserType = ModContent.ProjectileType<PossessionLaserProj>();
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].type == laserType)
                        activeCount++;
                }

                // LÃ­mite seguro para evitar llenar los 1000 slots de Terraria si se golpea a cientos de dummys
                if (activeCount < 100)
                {
                    // Delay chico y escalonado por distancia: como el lÃ¡tigo golpea a casi
                    // toda la fila casi en el mismo instante (estÃ¡n muy juntos), sin ALGO de
                    // escalonado la fila entera se ilumina de golpe. Pero dist/24 (el valor
                    // original) era demasiado fuerte y hacÃ­a que los lÃ¡seres lejanos
                    // aparecieran reciÃ©n despuÃ©s de terminado el combo entero. dist/60 es un
                    // punto medio: para el rango mÃ¡ximo del lÃ¡tigo (~34 tiles â‰ˆ 544px) da
                    // como mucho ~9 ticks de retraso, dentro de la vida visible de un lÃ¡ser.
                    float dist = Vector2.Distance(player.Center, target.Center);
                    // Delay casi instantÃ¡neo. Solo aplicamos un mini escalonamiento
                    // por orden de golpe y un pequeÃ±o ruido aleatorio para que no aparezcan 
                    // 100% en el mismo frame si golpea a varios a la vez.
                    int hitOrder = hitEnemiesForLasers.Count;
                    float delay = (hitOrder * 0.5f) + Main.rand.NextFloat(0f, 1.0f);
                    if (delay < 0f) delay = 0f;

                    Vector2 spawnPos = target.Center + Main.rand.NextVector2Circular(10f, 10f);
                    int p = Projectile.NewProjectile(Projectile.GetSource_OnHit(target), spawnPos, Vector2.Zero, laserType, (int)(damageDone * 0.5f), 0f, player.whoAmI, dir.X, dir.Y);
                    if (p >= 0 && p < Main.maxProjectiles)
                    {
                        Main.projectile[p].localAI[2] = delay;
                    }
                }
            }
        }

        private static readonly int[] frameContentHeight = { 18, 14, 14, 14, 24, 26, 28 };
        private static readonly int[] frameContentTop    = { 2, 6, 6, 6, 0, 0, 0 };

        // FunciÃ³n matemÃ¡tica para invertir la curva del lÃ¡tigo ("de abajo hacia arriba")
        private void MirrorWhipPoints(List<Vector2> points)
        {
            if (points == null || points.Count < 2) return;

            Vector2 origin = points[0];
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = points[i];
                Vector2 v = p - origin;
                
                // ProyecciÃ³n sobre la direcciÃ³n en la que apuntamos
                Vector2 proj = Vector2.Dot(v, direction) * direction;
                // Componente perpendicular (la curva)
                Vector2 perp = v - proj;
                
                // Invertimos la componente perpendicular
                points[i] = origin + proj - perp;
            }
        }

        // VersiÃ³n continua del mirror: en vez de invertir de golpe, interpola la
        // componente perpendicular entre la curva normal (mix=0, curva pronunciada
        // "hacia arriba") y la espejada (mix=1, "hacia abajo"). La curva hacia abajo
        // se atenÃºa con downStrength para que no se doble tan pronunciada como la
        // de arriba (queda mÃ¡s recta). Dos lÃ¡tigos con mix opuesto se cruzan cerca
        // de mix=0.5.
        private const float DownCurveStrength = 0.5f; // 1 = igual de pronunciada, 0 = totalmente recta hacia abajo

        private void MirrorWhipPointsLerp(List<Vector2> points, float mixFactor, float retractProgress = 0f)
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
                
                if (retractProgress > 0f)
                {
                    float indexFactor = i / (float)(points.Count - 1);
                    float artificialCurve = (float)System.Math.Sin(indexFactor * System.Math.PI) * 35f; 
                    
                    Vector2 artificialPerp = new Vector2(-direction.Y, direction.X) * artificialCurve;
                    if (t > 0.5f) artificialPerp = -artificialPerp;
                    
                    mixedPerp += artificialPerp * retractProgress;
                }
                
                points[i] = origin + proj + mixedPerp;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float delay = Projectile.localAI[2];
            int realFramesPassed = (int)Projectile.localAI[0] / (Projectile.extraUpdates + 1);

            if (realFramesPassed < delay)
            {
                return false; // No dibujar nada si estÃ¡ en espera
            }

            List<Vector2> list = new List<Vector2>();

            Player player = Main.player[Projectile.owner];
            int prevAnim = player.itemAnimation;
            int prevAnimMax = player.itemAnimationMax;

            // EngaÃ±ar a FillWhipControlPoints
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
                // NOTA: Se eliminÃ³ el timeOffset que cortaba la animaciÃ³n prematuramente.
                // Ahora los lÃ¡tigos cruzados duran todo su tiempo de vida y se recogen (retract)
                // visualmente al igual que un lÃ¡tigo normal, en lugar de desaparecer de golpe.
            }

            player.itemAnimation = currentFakeAnimation;
            player.itemAnimationMax = duration;

            Projectile.FillWhipControlPoints(Projectile, list);

            // Si es la tercera ola (dinÃ¡mico), aplicamos la reflexiÃ³n solo al estirarse
            if (Projectile.ai[1] == 4f && isExtending)
            {
                MirrorWhipPoints(list);
            }
            // Pareja de la cadena nueva: mirror continuo durante toda su vida sin deformar
            else if (isChainPair)
            {
                float retractProgress = isExtending ? 0f : 1f - ((float)currentFakeAnimation / (duration / 2f));
                MirrorWhipPointsLerp(list, chainMix, retractProgress);
            }

            // Restaurar valores reales
            player.itemAnimation = prevAnim;
            player.itemAnimationMax = prevAnimMax;

            if (list.Count < 2)
                return false;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("TerrariaWhipsBackport/Projectiles/PossessionProj_1").Value;
            
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            int totalFrames = 7;
            int frameWidth = texture.Width; 
            int frameHeight = texture.Height / totalFrames;

            // (la punta), el punto de partida del siguiente segmento sigue
            // siendo exactamente donde terminÃ³ el anterior -> nunca hay hueco.
            Vector2 pos = list[0];

            float lastRotation = 0f;
            bool hasRotation = false;

            // Umbral de longitud (en px^2) por debajo del cual consideramos el
            // segmento "casi nulo" y reutilizamos la rotaciÃ³n anterior en vez
            // de calcular una nueva direcciÃ³n poco fiable.
            const float minSegmentLengthSq = 4f; // ~2px

            // MÃ¡xima diferencia angular (radianes) permitida entre un segmento
            // y el anterior. Evita que la punta "chasquee" lejos del resto de
            // la curva cuando el Ãºltimo punto de control de
            // FillWhipControlPoints se adelanta bruscamente (crack del whip).
            const float maxAngleDeltaFromPrev = MathHelper.PiOver4 * 0.6f; // ~27Â°

            for (int i = 0; i < list.Count - 1; i++)
            {
                int frameIndex;
                if (i == 0)
                {
                    frameIndex = 0; // Mango
                }
                else if (i == list.Count - 2)
                {
                    // Punta final: forzamos el frame 6 para todos los lÃ¡tigos 
                    // porque su sprite es mÃ¡s delgado y encaja a la perfecciÃ³n sin sobresalir.
                    frameIndex = 6;
                }
                else if (i == list.Count - 3)
                {
                    frameIndex = 3; // Pre-punta (Ãºltimo segmento recto antes del gancho)
                }
                else
                {
                    frameIndex = 1 + (i % 2); // Cicla solo entre frame 1 y 2 para el cuerpo
                }

                int currentFrameHeight = frameHeight;
                Vector2 origin = new Vector2(frameWidth / 2f, frameContentTop[frameIndex]);

                // Solo para las puntas (frames 4, 5 y 6), recortamos la altura del frame
                // para ignorar los pÃ­xeles basura accidentales que estÃ¡n en el fondo del sprite.
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
                    // Segmento casi nulo: no confiamos en su direcciÃ³n,
                    // mantenemos la orientaciÃ³n del segmento anterior, o si es el primero, 
                    // usamos la velocidad general del lÃ¡tigo para que no apunten hacia abajo.
                    rotation = hasRotation ? lastRotation : Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                }
                else
                {
                    // Si es la punta (frames 4, 5 y 6), forzamos la misma rotaciÃ³n que la pre-punta.
                    // Como el sprite de la punta tiene 6 pÃ­xeles de "cuerpo" por encima del punto de origen,
                    // si la punta gira respecto a la pre-punta, esos 6 pÃ­xeles sobresalen por los lados.
                    // Al obligarla a seguir la misma lÃ­nea, esos pÃ­xeles se ocultan perfectamente dentro de la pre-punta.
                    if (frameIndex >= 4 && hasRotation)
                    {
                        rotation = lastRotation;
                    }
                    else
                    {
                        rotation = rawDiff.ToRotation() - MathHelper.PiOver2;

                        if (hasRotation)
                        {
                            // Limita cuÃ¡nto puede girar este segmento respecto al anterior.
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
                // en la direcciÃ³n (posiblemente limitada) con la que
                // efectivamente se va a rotar el sprite. Esto es lo que
                // garantiza que no quede un hueco visual entre segmentos.
                Vector2 drawDirection = (rotation + MathHelper.PiOver2).ToRotationVector2();
                Vector2 diff = drawDirection * length;

                Color color = Lighting.GetColor(element.ToTileCoordinates());

                float divisor = frameContentHeight[frameIndex];
                float scaleY = length / divisor;

                // Si la longitud del segmento es prÃ¡cticamente nula (el lÃ¡tigo estÃ¡ recogido),
                // ocultamos el segmento.
                if (length < 2f)
                {
                    scaleY = 0f;
                }

                // Los frames de la punta (4, 5 y 6) ya traen una curva dibujada.
                // Si permitimos que se encojan, la curva se distorsiona y se ven "aplastadas".
                // Mientras no estÃ©n ocultas (scaleY > 0), forzamos un mÃ­nimo de 1f.
                if (frameIndex >= 4 && scaleY > 0f)
                {
                    if (scaleY < 1f)
                        scaleY = 1f; // Evita puntas aplastadas
                    else if (scaleY > 1.3f)
                        scaleY = 1.3f; // Evita que se estiren de mÃ¡s y vuelen
                }

                Vector2 scale = new Vector2(1f, scaleY);
                
                // Capa base (con iluminaciÃ³n del mundo)
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                
                // Capa brillante (Glowmask pura que no es afectada por la oscuridad)
                // SOLO la dibujamos para las puntas (frames 4, 5 y 6) para evitar
                // que el cuerpo se vuelva un manchÃ³n blanco al superponerse.
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
            
            // Para la detecciÃ³n de colisiones, Terraria usa FillWhipControlPoints, el cual puede leer itemAnimation e itemAnimationMax.
            // Si el jugador hace un solo click, su itemAnimation real vuelve a 0 mientras nuestros lÃ¡tigos con delay siguen vivos.
            // Esto causa que FillWhipControlPoints colapse la hitbox al centro del jugador, haciendo que no haga daÃ±o.
            // SoluciÃ³n: Falsificamos ambos valores para la colisiÃ³n exactamente igual que en la IA y el dibujo.
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
