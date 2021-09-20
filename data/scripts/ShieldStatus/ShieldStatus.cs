using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRageMath;

using DefenseShields;
using VRage.Game.GUI.TextPanel;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace Natomic.ShieldStatus
{
    [MyTextSurfaceScript("ShieldStatus", "Shield Status")]
    public class ShieldStatus : MyTextSurfaceScriptBase
    {
        public override ScriptUpdate NeedsUpdate => shield_block_ == null ? ScriptUpdate.Update100 : ScriptUpdate.Update10;

        internal readonly ShieldApi ds_api_ = new ShieldApi();
        internal Exception err_ = null;
        internal readonly List<MySprite> sprites_ = new List<MySprite>();
        internal readonly SpriteBuilder builder_ = new SpriteBuilder();
        internal Vector2 progress_sprite_size_;
        internal IMyTerminalBlock shield_block_;

        internal MyIniKey scale_key;



        public ShieldStatus(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            if (!ds_api_.Load())
            {
                err_ = new Exception("failed to load defense shields");
                return;
            }
            builder_.Surface = surface;

            builder_.Viewport = new RectangleF(new Vector2(0, (surface.TextureSize.Y - surface.SurfaceSize.Y) / 2f), size);
            builder_.Scale = 1;
            progress_sprite_size_ = new Vector2(size.X / 4 * 3, SpriteBuilder.NEWLINE_HEIGHT_BASE / 4 * 3);
        }


        private float FindCurrShieldHP()
        {
            return ds_api_.GetCharge(shield_block_);
        }
        public override void Run()
        {
            var grid = (IMyCubeGrid)Block.CubeGrid;
            if (err_ == null)
            {
                if (!ds_api_.GridHasShield(grid))
                {
                    shield_block_ = null;
                }
                else if (shield_block_ == null)
                {
                    shield_block_ = ds_api_.MatchEntToShieldFast(grid, false);
                }
            }
            Draw();
        }
        private static Color ColourForPercent(float percent)
        {
           if (percent >= 60)
            {
                return Color.Green;
            } else if (percent >= 30)
            {
                return Color.Orange;
            } else if (percent >= 10)
            {
                return Color.DarkOrange;
            } else
            {
                return Color.Red;
            }
        }
        internal Vector2 CenterOfSurfaceX()
        {
            return new Vector2(Surface.SurfaceSize.X / 2f, 0);
        }
        private void DrawProgressBar()
        {
            var shield_status = FindCurrShieldHP();
            var shield_max = ds_api_.GetMaxCharge(shield_block_);
            var status_color = ColourForPercent(shield_status);
            using (var ident = builder_.WithIndent((int)(CenterOfSurfaceX().X - progress_sprite_size_.X / 2f)))
            {
                builder_.MakeProgressBar(sprites_, progress_sprite_size_, Color.White, status_color, shield_status, shield_max);
            }
            builder_.AddNewline();
            sprites_.Add(builder_.MakeText(
                $"{(100 * shield_status / shield_max):0.#} %",
                alignment: TextAlignment.CENTER,
                color: status_color,
                offset: CenterOfSurfaceX()
                ));
        }
        public override void Dispose()
        {
            ds_api_?.Unload();
        }
        private void DrawPowerUse()
        {
            var pow = ds_api_.GetPowerUsed(shield_block_);
            sprites_.Add(builder_.MakeText($"Power use: {pow}Mw", alignment: TextAlignment.RIGHT, offset: new Vector2(Surface.SurfaceSize.X, 0)));
        }
        private void DrawHeat()
        {
            var heat = ds_api_.GetShieldHeat(shield_block_);
            sprites_.Add(builder_.MakeText($"Heat: {heat}"));
        }
        private void DrawStatus()
        {
            var status = ds_api_.ShieldStatus(shield_block_);
            sprites_.Add(builder_.MakeText($"{status}", alignment: TextAlignment.CENTER, offset: CenterOfSurfaceX() ));
        }
        private void Draw()
        {
            sprites_.Clear();
            builder_.CurrPos = builder_.Viewport.Position;
            if (err_ != null)
            {
                DrawErr();
            } 
            else if (shield_block_ == null)
            {
                DrawNoShield();
            }
            else
            {
                DrawShieldInfo();
            }
            using (var frame = Surface.DrawFrame())
            {
                frame.AddRange(sprites_);
            }
        }
        private void DrawNoShield()
        {
            sprites_.Add(builder_.MakeText("No shield found", alignment: TextAlignment.CENTER, color: Color.Yellow, offset: CenterOfSurfaceX()));
        }
        private void DrawErr()
        {
            sprites_.Add(builder_.MakeText($"error during run: {err_.Message}", alignment: TextAlignment.CENTER, color: Color.Red, offset: CenterOfSurfaceX()));
        }
        private void DrawShieldInfo()
        {
            DrawStatus();
            builder_.AddNewline();
            DrawProgressBar();
            builder_.AddNewline();
        }

    }
}
