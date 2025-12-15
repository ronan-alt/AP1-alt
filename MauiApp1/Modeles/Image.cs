using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using System.Drawing;

namespace MauiApp1.Modeles
{
    public class Image
    {
        /// <summary>
        /// Crée un ImageSource flouté et compressé à partir d'un fichier sur disque.
        /// Retourne null si l'image ne peut pas être décodée.
        /// </summary>
        public static ImageSource? Create(string filePath, int maxWidth = 600, int blurRadius = 12)
        {
            // Decode (safe)
            using var original = SKBitmap.Decode(filePath);
            if (original == null || original.Width == 0)
                return null;

            // Calcul du scale sans monter au-dessus de 1 (ne pas upscaler)
            float scale = (original.Width > maxWidth) ? (float)maxWidth / original.Width : 1f;
            int newWidth = (int)(original.Width * scale);
            int newHeight = (int)(original.Height * scale);

            // 1) Redimensionnement avec méthode moderne (SKBitmapResizeMethod)
            // Lanczos3 -> belle qualité pour downscale sans trop de halos; si tu veux ultra-rapide, remplace par Box.
            using var resized = original.Resize(
                new SKImageInfo(newWidth, newHeight),
                SKBitmapResizeMethod.Lanczos3
            );

            if (resized == null)
                return null;

            // 2) Dessin et application du flou
            using var surface = SKSurface.Create(new SKImageInfo(newWidth, newHeight));
            var canvas = surface.Canvas;
            canvas.Clear();

            using var paint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateBlur(blurRadius, blurRadius)
            };

            canvas.DrawBitmap(resized, 0, 0, paint);

            // 3) Snapshot et encodage JPEG compressé pour réduire la taille mémoire/IO
            using var output = surface.Snapshot();
            using var data = output.Encode(SKEncodedImageFormat.Jpeg, 70);

            if (data == null)
                return null;

            // 4) Retourner un ImageSource à partir d'un stream
            return ImageSource.FromStream(() => data.AsStream());
        }
    }

}
