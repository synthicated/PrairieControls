using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;


namespace PrairieControls {
    internal class Patches {
        public static Vector2 GetDirection(Vector2 x0, Vector2 x1) {
            return new Vector2(x1.X - x0.X, x1.Y - x0.Y);
        }

        public static Point Vector2ToRoundedPoint(Vector2 vec) {
            return new Point((int)Math.Round(vec.X), (int)Math.Round(vec.Y));
        }

        internal static void Postfix_Tick(AbigailGame __instance, GameTime time) {
            if(!ModEntry.config.usingCursor || AbigailGame.gameOver || AbigailGame.onStartMenu || AbigailGame.endCutscene || AbigailGame.gopherTrain) return;

            MouseState mouseWindow = Mouse.GetState();
            bool anyPressed = mouseWindow.LeftButton == ButtonState.Pressed || mouseWindow.RightButton == ButtonState.Pressed;
            if(!anyPressed) return;
            
            if(__instance.motionPause <= 0 && AbigailGame.deathTimer <= 0f && __instance.shotTimer <= 0 && AbigailGame.playerShootingDirections.Count == 0) {
                Vector2 playerCenter = new Vector2(
                    __instance.playerPosition.X + AbigailGame.TileSize / 2f,
                    __instance.playerPosition.Y + AbigailGame.TileSize / 2f);

                Vector2 mouseGame = new Vector2(mouseWindow.X, mouseWindow.Y) - AbigailGame.topLeftScreenCoordinate;
                Vector2 direction = GetDirection(playerCenter, mouseGame);
                direction.Normalize();

                Point bulletSpawnPoint = Vector2ToRoundedPoint(playerCenter + direction * 24f);
                bool isSpread = __instance.activePowerups.ContainsKey(AbigailGame.POWERUP_SPREAD);
                bool isShotgun = __instance.activePowerups.ContainsKey(AbigailGame.POWERUP_SHOTGUN);
                if(isSpread || isShotgun) {
                    float baseAngle = (float)Math.Atan2(direction.Y, direction.X);
                    int bulletsCount = isSpread ? 8 : 3;

                    float spreadRadians = isSpread ? MathHelper.ToRadians(360f) : MathHelper.ToRadians(60f);
                    float step = spreadRadians / bulletsCount;
                    float mid = isSpread ? bulletsCount / 2f : (bulletsCount - 1) / 2f;

                    for(int i = 0; i < bulletsCount; i++) {
                        float angle = baseAngle + (i - mid) * step;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                        __instance.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawnPoint, Vector2ToRoundedPoint(vel), __instance.bulletDamage));
                    }
                }
                else {
                    __instance.bullets.Add(new AbigailGame.CowboyBullet(bulletSpawnPoint, Vector2ToRoundedPoint(direction * 8f), __instance.bulletDamage));
                }

                int baseShot = __instance.shootingDelay;
                if(__instance.activePowerups.ContainsKey(AbigailGame.POWERUP_RAPIDFIRE)) {
                    baseShot /= 4;
                }

                for(int i = 0; i < __instance.fireSpeedLevel; i++) {
                    baseShot = baseShot * 3 / 4;
                }

                if(__instance.activePowerups.ContainsKey(AbigailGame.POWERUP_SHOTGUN) && !isSpread) {
                    baseShot = baseShot * 3 / 2;
                }
                __instance.shotTimer = Math.Max(baseShot, 20);

                Game1.playSound("Cowboy_gunshot");
            }
        }
    }
}
