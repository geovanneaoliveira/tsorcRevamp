﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace tsorcRevamp.Projectiles.Enemy {
    class GravityDistortion : ModProjectile {

        public override void SetDefaults() {
            projectile.width = 30;
            projectile.height = 30;
			projectile.scale = 1.5f; //It's working properly now, apparently for scale to work you have to override predraw and tell it to draw the projectile right
            projectile.hostile = true;
            projectile.damage = 80;
            projectile.penetrate = 2;
            projectile.tileCollide = false;
			projectile.timeLeft = 200;
        }
        public override void Kill(int timeLeft) {
            projectile.type = 79;
			for (int num36 = 0; num36 < 20; num36++)
			{
				int dust = Dust.NewDust(projectile.position, (int)(projectile.width), (int)(projectile.height), DustID.PurpleTorch, Main.rand.Next(-15, 15), Main.rand.Next(-15, 15), 100, new Color(), 9f);
				Main.dust[dust].noGravity = true;
			}
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            //Get the premultiplied, properly transparent texture from the array (if it's not transparent, you can just use Texture2D texture = Main.projectileTexture[projectile.type];
            Texture2D texture = TransparentTextureHandler.TransparentTextures[TransparentTextureHandler.TransparentTextureType.AntiGravityBlast];
            int frameHeight = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
            int startY = frameHeight * projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Color drawColor = projectile.GetAlpha(lightColor);
            Main.spriteBatch.Draw(texture,
                projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY),
                sourceRectangle, drawColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);

            return false;
        }

		float homingStrength = 0.08f;
		//ai[1] is the projectie's maximum speed, as specified when it's spawned
        public override void AI() {
			projectile.rotation += 0.5f;
			if (Main.player[(int)projectile.ai[0]].position.X < projectile.position.X) {
				if (projectile.velocity.X > -10) projectile.velocity.X -= homingStrength;
			}

			if (Main.player[(int)projectile.ai[0]].position.X > projectile.position.X) {
				if (projectile.velocity.X < 10) projectile.velocity.X += homingStrength;
			}

			if (Main.player[(int)projectile.ai[0]].position.Y < projectile.position.Y) {
				if (projectile.velocity.Y > -10) projectile.velocity.Y -= homingStrength;
			}

			if (Main.player[(int)projectile.ai[0]].position.Y > projectile.position.Y) {
				if (projectile.velocity.Y < 10) projectile.velocity.Y += homingStrength;
			}

			if(projectile.velocity.Y > projectile.ai[1])
            {
				projectile.velocity.Y = projectile.ai[1];
			}
			if (projectile.velocity.Y < -projectile.ai[1])
			{
				projectile.velocity.Y = -projectile.ai[1];
			}
			if (projectile.velocity.X > projectile.ai[1])
			{
				projectile.velocity.X = projectile.ai[1];
			}
			if (projectile.velocity.X < -projectile.ai[1])
			{
				projectile.velocity.X = -projectile.ai[1];
			}

			if (Main.rand.Next(4) == 0) {
				int dust = Dust.NewDust(new Vector2((float)projectile.position.X + 10, (float)projectile.position.Y), projectile.width, projectile.height, DustID.Fire, 0, 0, 200, Color.Red, 1f);
				Main.dust[dust].noGravity = true;
			}
			Lighting.AddLight((int)(projectile.position.X / 16f), (int)(projectile.position.Y / 16f), 0.7f, 0.2f, 0.2f);
		}

        public override void OnHitPlayer(Player target, int damage, bool crit) {
			target.AddBuff(BuffID.Gravitation, 180);
			target.AddBuff(BuffID.Confused, 180);
        }
    }
}