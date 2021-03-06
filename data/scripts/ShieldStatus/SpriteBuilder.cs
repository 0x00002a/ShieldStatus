using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace Natomic.ShieldStatus
{
    public class SpriteBuilder
        {
            public struct IndentProxy: IDisposable {
                internal SpriteBuilder parent_;
                internal int by_;
                internal IndentProxy(SpriteBuilder parent, int by)
                {
                    parent_ = parent;
                    by_ = by;
                    parent.CurrPos.X += by;
                }

                public void Dispose()
                {
                    parent_.CurrPos.X -= by_;
                }
            }
            public struct BoxedProxy {
                internal SpriteBuilder parent_;
                internal Vector2 origin_;
                public BoxedProxy(SpriteBuilder parent)
                {
                    parent_ = parent;
                    origin_ = parent.CurrPos;
                }

                public void Make(ref MySpriteDrawFrame to, int width, int line_width)
                {
                    var size = new RectangleF(origin_, new Vector2( width, parent_.CurrPos.Y - origin_.Y));

                    to.Add(parent_.MakeRect( // Right
                        start: new Vector2(origin_.X + width - line_width, origin_.Y),
                        end: new Vector2(origin_.X + width, parent_.CurrPos.Y)
                        )); 
                }
            }
            

            public float Scale;
            public RectangleF Viewport;
            public Vector2 CurrPos;
            public IMyTextSurface Surface;


            internal Vector2 NewlineCenterOffset => new Vector2(0, NewlineHeight / 4);
            internal StringBuilder str_cache_;

            public float NewlineHeight => NEWLINE_HEIGHT_BASE * Scale;

            public const float NEWLINE_HEIGHT_BASE = 37f;

            public Vector2 TextSizePx(string txt, IMyTextSurface reference = null,  string font_id = "White")
            {
                reference = reference ?? Surface;
                if (str_cache_ == null)
                {
                    str_cache_ = new StringBuilder();
                } else
                {
                    str_cache_.Clear();
                }

                str_cache_.Append(txt);
                var size = reference.MeasureStringInPixels(str_cache_, font_id, Scale);
                str_cache_.Clear();
                return size;

            }

            public IndentProxy WithIndent(int by)
            {
                return new IndentProxy(this, by);
            }

            public void AddIndent(int by)
            {
                CurrPos.X += by;
            }
            public void AddNewline()
            {
                CurrPos.Y += NewlineHeight;
            }
            public MySprite MakeRect(Vector2 start, Vector2 end, Color? fg = null)
            {
                var rect = new RectangleF(start, end - start);
                var colour = fg ?? Color.White;
                return new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Color = colour,
                    Size = rect.Size,
                    Position = rect.Center,
                    Alignment = TextAlignment.CENTER,

                };
            }
            public MySprite MakeBulletPt(Color? fg = null)
            {
                var color = fg ?? Color.White;
                var bounds = new RectangleF(CurrPos, new Vector2(NewlineHeight / 3, NewlineHeight / 3));
                return new MySprite
                {
                    Data = "Circle",
                    Type = SpriteType.TEXTURE,
                    Color = color,
                    Alignment = TextAlignment.CENTER,
                    Position = bounds.Center + NewlineCenterOffset,
                    Size = bounds.Size,
                };
            }

            public void MakeProgressBar(ICollection<MySprite> to, Vector2 size, Color bg, Color fg, double curr, double total)
            {
                var padding = new Vector2(2, 2);
                size *= Scale;

                var bg_rect = new RectangleF(CurrPos, size);

                var sprite = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareHollow",
                    Position = bg_rect.Center + NewlineCenterOffset,
                    Color = bg,
                    Alignment = TextAlignment.CENTER,
                    Size = bg_rect.Size,
                };
                to.Add(sprite);

                var fg_rect = new RectangleF(CurrPos + padding / 2, new Vector2((float)(curr * (size.X / total)), size.Y) - padding);

                sprite = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = fg_rect.Center + NewlineCenterOffset,
                    Color = fg,
                    Alignment = TextAlignment.CENTER,
                    Size = fg_rect.Size,
                };
                to.Add(sprite);
                

            }

            public MySprite MakeText(string txt, TextAlignment alignment = TextAlignment.LEFT, Color? color = null, string font_id = "White", Vector2? offset = null)
            {
                var roffset = offset ?? Vector2.Zero;
                return new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = txt,
                    Alignment = alignment,
                    RotationOrScale = Scale,
                    FontId = font_id,
                    Color = color ?? Surface?.ScriptForegroundColor ?? Color.White,
                    Position = CurrPos + roffset,
                };
            }

        }
    }
