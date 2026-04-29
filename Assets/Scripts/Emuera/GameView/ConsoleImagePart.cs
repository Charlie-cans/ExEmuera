using Serilog;
﻿using MinorShift._Library;
using MinorShift.Emuera.Content;
using System;
using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Text;
using uEmuera.Drawing;
using uEmuera.Forms;

namespace MinorShift.Emuera.GameView
{
	class ConsoleImagePart : AConsoleDisplayPart
	{
		// EE兼容: heightPx/widthPx/yposPx 为 true 时表示px像素单位，false 表示%字体百分比单位
		public ConsoleImagePart(string resName, string resNameb, int raw_height, int raw_width = 0, int raw_ypos = 0, bool heightPx = false, bool widthPx = false, bool yposPx = false)
		{
			top = 0;
			bottom = Config.FontSize;
			Str = "";
			ResourceName = resName ?? "";
			ButtonResourceName = resNameb;

            cImage = AppContents.GetSprite(ResourceName);
			if (cImage == null) Log.ForContext("Tag", "IMG").Warning($"Sprite MISSING: {ResourceName}");
			else Log.ForContext("Tag", "IMG").Debug($"Sprite OK: {ResourceName}");
#if UNITY_EDITOR
			if (cImage == null && !string.IsNullOrEmpty(ResourceName))
				Log.ForContext("Tag", "IMG").Warning($"sprite missing: {ResourceName}");
#endif
#if !UNITY_EDITOR
            if(cImage == null)
            {
#endif
                StringBuilder sb = new StringBuilder();
                sb.Append("<img src='");
                sb.Append(ResourceName);
                if(ButtonResourceName != null)
                {
                    sb.Append("' srcb='");
                    sb.Append(ButtonResourceName);
                }
                if(raw_height != 0)
                {
                    sb.Append("' height='");
                    sb.Append(raw_height.ToString());
                }
                if(raw_width != 0)
                {
                    sb.Append("' width='");
                    sb.Append(raw_width.ToString());
                }
                if(raw_ypos != 0)
                {
                    sb.Append("' ypos='");
                    sb.Append(raw_ypos.ToString());
                }
                sb.Append("'>");
                AltText = sb.ToString();
#if !UNITY_EDITOR
                Str = AltText;
                return;
            }
#else
            if(cImage == null)
            {
                Str = AltText;
                return;
            }
#endif  
			int height = 0;
			if (raw_height == 0)//如果HTML中未指定高度或指定为0，则直接使用字体大小作为高度（px单位）。
				height = Config.FontSize;
			else//如果HTML中指定了高度，则解释为字体大小的百分比。
				height = Config.FontSize * raw_height / 100;
			//如果未指定宽度或指定为0，则根据原图像的宽高比设置宽度（px单位）。不足1的部分作为小数记录在Xsubpixel中。
			//可能会指定负值，但最终Width会在之后调整为正值。
			if (raw_width == 0)
			{
				Width = cImage.DestBaseSize.Width * height / cImage.DestBaseSize.Height;
				XsubPixel = ((float)cImage.DestBaseSize.Width * height) / cImage.DestBaseSize.Height - Width;
			}
			else if (widthPx)
			{
				Width = raw_width;
				XsubPixel = 0;
			}
			else
			{
				Width = Config.FontSize * raw_width / 100;
				XsubPixel = ((float)Config.FontSize * raw_width / 100f) - Width;
			}
			if (yposPx)
				top = raw_ypos;
			else
				top = raw_ypos * Config.FontSize / 100;
			destRect = new Rectangle(0, top, Width, height);
			if (destRect.Width < 0)
			{
				destRect.X = -destRect.Width;
				Width = -destRect.Width;
			}
			if (destRect.Height < 0)
			{
				destRect.Y = destRect.Y - destRect.Height;
				height = -destRect.Height;
			}
			bottom = top + height;
			//if(top > 0)
			//	top = 0;
			//if(bottom < Config.FontSize)
			//	bottom = Config.FontSize;
			if (ButtonResourceName != null)
			{
                if(ButtonResourceName == ResourceName)
                    cImageB = cImage;
                else
                {
                    cImageB = AppContents.GetSprite(ButtonResourceName);
                    if(cImageB != null)
                        cImageB = null;
                }
			}
		}

        public ASprite Image { get { return cImage; } }
        public ASprite ImageBackground { get { return cImageB; } }
        public Rectangle dest_rect { get { return destRect; } }

		private readonly ASprite cImage;
		private readonly ASprite cImageB;
		private readonly int top;
		private readonly int bottom;
		private readonly Rectangle destRect;
//#pragma warning disable CS0649 // 字段 'ConsoleImagePart.ia' 从未被赋值，将始终使用默认值 null。
//		private readonly ImageAttributes ia;
//#pragma warning restore CS0649 // 字段 'ConsoleImagePart.ia' 从未被赋值，将始终使用默认值 null。
		public readonly string ResourceName;
		public readonly string ButtonResourceName;
		public override int Top { get { return top; } }
		public override int Bottom { get { return bottom; } }
		
		public override bool CanDivide { get { return false; } }
		public override void SetWidth(StringMeasure sm, float subPixel)
		{
			if (this.Error)
			{
				Width = 0;
				return;
			}
			if (cImage != null)
				return;
			Width = sm.GetDisplayLength(Str, Config.Font);
			XsubPixel = subPixel;
		}

		public override string ToString()
		{
			if (AltText == null)
				return "";
			return AltText;
		}

		public override void DrawTo(Graphics graph, int pointY, bool isSelecting, bool isBackLog, TextDrawingMode mode)
		{
			//if (this.Error)
			//	return;
			//ASprite img = cImage;
			//if (isSelecting && cImageB != null)
			//	img = cImageB;
            //
			//if (img != null && img.IsCreated)
			//{
			//	Rectangle rect = destRect;
			//	//PointX微調整
			//	rect.X = destRect.X + PointX + Config.DrawingParam_ShapePositionShift;
			//	rect.Y = destRect.Y + pointY;
			//	img.GraphicsDraw(graph, rect);
			//}
			//else
			//{
			//	if (mode == TextDrawingMode.GRAPHICS)
			//		graph.DrawString(AltText, Config.Font, new SolidBrush(Config.ForeColor), new Point(PointX, pointY));
			//	else
			//		System.Windows.Forms.TextRenderer.DrawText(graph, AltText, Config.Font, new Point(PointX, pointY), Config.ForeColor, System.Windows.Forms.TextFormatFlags.NoPrefix);
			//}
		}

		public override void GDIDrawTo(int pointY, bool isSelecting, bool isBackLog)
		{
			//if (this.Error)
			//	return;
			//SpriteF img = cImage as SpriteF;//Graphicsから作成したImageはGDI対象外
			//if (isSelecting && cImageB != null)
			//	img = cImageB as SpriteF;
			//if (img != null && img.IsCreated)
			//{
			//	int x = PointX + destRect.X;
			//	int y = pointY + destRect.Y;
			//	if (!img.DestBasePosition.IsEmpty)
			//	{
			//		x = x + img.DestBasePosition.X * destRect.Width / img.SrcRectangle.Width;
			//		y = y + img.DestBasePosition.Y * destRect.Height / img.SrcRectangle.Height;
			//	}
			//	GDI.DrawImage(x, y, Width, destRect.Height, img.BaseImage.GDIhDC, img.SrcRectangle);
			//}
			//else
			//	GDI.TabbedTextOutFull(Config.Font, Config.ForeColor, AltText, PointX, pointY);
		}
	}
}
