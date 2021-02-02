using System;
using Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace TAS.EverestInterop.Hitboxes {
    public class HitboxTweak {
        private static CelesteTASModuleSettings Settings => CelesteTASModule.Settings;

        public static void Load() {
            HitboxTriggerSpikes.Load();
            ActualEntityCollideHitbox.Load();
            ActualPlayerCollideHitbox.Load();
            HitboxFixer.Load();
            HitboxSimplified.Load();
            HitboxColor.Load();
            HitboxFinalBoss.Load();
            On.Monocle.Entity.DebugRender += ModHitbox;
            IL.Celeste.PlayerCollider.DebugRender += PlayerColliderOnDebugRender;
            On.Monocle.Circle.Render += CircleOnRender;
        }

        public static void Unload() {
            HitboxTriggerSpikes.Unload();
            ActualEntityCollideHitbox.Unload();
            ActualPlayerCollideHitbox.Unload();
            HitboxFixer.Unload();
            HitboxSimplified.Unload();
            HitboxColor.Unload();
            HitboxFinalBoss.Unload();
            On.Monocle.Entity.DebugRender -= ModHitbox;
            IL.Celeste.PlayerCollider.DebugRender -= PlayerColliderOnDebugRender;
            On.Monocle.Circle.Render -= CircleOnRender;
        }

        private static void ModHitbox(On.Monocle.Entity.orig_DebugRender orig, Entity self, Camera camera) {
            if (!Settings.ShowHitboxes) {
                orig(self, camera);
                return;
            }

            if (self is Puffer) {
                Vector2 bottomCenter = self.BottomCenter - Vector2.UnitY * 1;
                if (self.Scene.Tracker.GetEntity<Player>() is Player player && player.Ducking) {
                    bottomCenter -= Vector2.UnitY * 3;
                }

                Color hitboxColor = HitboxColor.EntityColor;
                if (!self.Collidable) {
                    hitboxColor *= 0.5f;
                }

                Draw.Circle(self.Position, 32f, hitboxColor, 32);
                Draw.Line(bottomCenter - Vector2.UnitX * 32, bottomCenter - Vector2.UnitX * 6, hitboxColor);
                Draw.Line(bottomCenter + Vector2.UnitX * 6, bottomCenter + Vector2.UnitX * 32, hitboxColor);
            }

            orig(self, camera);
        }

        private static void PlayerColliderOnDebugRender(ILContext il) {
            ILCursor ilCursor = new ILCursor(il);
            if (ilCursor.TryGotoNext(
                MoveType.After,
                ins => ins.MatchCall<Color>("get_HotPink")
            )) {
                ilCursor
                    .Emit(OpCodes.Ldarg_0)
                    .EmitDelegate<Func<Color, Component, Color>>((color, component) => component.Entity.Collidable ? color : color * 0.5f);
            }
        }

        private static void CircleOnRender(On.Monocle.Circle.orig_Render orig, Circle self, Camera camera, Color color) {
            if (!Settings.ShowHitboxes) {
                orig(self, camera, color);
                return;
            }

            if (self.Entity is FireBall fireBall && !fireBall.GetDynDataInstance().Get<bool>("iceMode")) {
                color = Color.Goldenrod;
            }

            orig(self, camera, color);
        }
    }
}