using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ITransitionTask5.Services
{
    public static class ImageService
    {

        private static readonly Color[] _bgs;
        private static readonly Color[] _accents;

        static ImageService()
        {
            var json = File.ReadAllText("./Data/Configurations/covers.json");
            var doc = System.Text.Json.JsonDocument.Parse(json);
            _bgs = doc.RootElement.GetProperty("backgrounds")
                .EnumerateArray().Select(x => Color.ParseHex(x.GetString()!)).ToArray();
            _accents = doc.RootElement.GetProperty("accents")
                .EnumerateArray().Select(x => Color.ParseHex(x.GetString()!)).ToArray();
        }

        public static Image<Rgba32> CreateBlankImage(int width, int height)
        {
            return new Image<Rgba32>(width, height);
        }



        public static byte[] ImageToBytes(Image<Rgba32> image)
        {
            using MemoryStream ms = new MemoryStream();

            image.Save(ms, new PngEncoder { });

            return ms.ToArray();
        }


        public static void WriteToImage(Image<Rgba32> image, string text, int startingFontSize, PointF origin)
        {
            Font font;

            RichTextOptions options;

            FontRectangle fontRectangle;

            do
            {
                font = SystemFonts.CreateFont("Arial", startingFontSize);

                options = new RichTextOptions(font)
                {
                    Origin = origin
                };

                fontRectangle = TextMeasurer.MeasureBounds(text, options);

                if (fontRectangle.Width <= image.Width &&
                    fontRectangle.Height <= image.Height)
                {
                    break;
                }

                startingFontSize--;

            } while (startingFontSize > 1);

            image.Mutate(ctx =>
                ctx.DrawText(options, text, Color.WhiteSmoke));

        }


        public static void DrawVinylRecord(Image<Rgba32> image)
        {
            PointF center = GetCenterPoint(image);

            var innerPolygon = new EllipsePolygon(center, 5);
            var middlePolygon = new EllipsePolygon(center, 30);
            var outerPolygon = new EllipsePolygon(center, 100);

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.Black, outerPolygon);
                ctx.Fill(Color.DarkRed, middlePolygon);
                ctx.Fill(Color.White, innerPolygon);

                for (float radius = 40; radius <= 95; radius += 4)
                {
                    var groove = new EllipsePolygon(center, radius);

                    ctx.Draw(
                        Color.FromRgba(80, 80, 80, 120),
                        1f,
                        groove);
                }
            });
        }




        public static void DrawGuitar(Image<Rgba32> image, PointF center, float size, Color bodyColor, Color neckColor)
        {
            float cx = center.X;
            float cy = center.Y + size * 0.15f;


            var bodyPath = new PathBuilder()
                .MoveTo(new PointF(cx, cy - size * 0.42f))
                .CubicBezierTo(
                    new PointF(cx + size * 0.28f, cy - size * 0.42f),
                    new PointF(cx + size * 0.18f, cy),
                    new PointF(cx + size * 0.18f, cy))
                .CubicBezierTo(
                    new PointF(cx + size * 0.38f, cy + size * 0.10f),
                    new PointF(cx + size * 0.38f, cy + size * 0.52f),
                    new PointF(cx, cy + size * 0.52f))
                .CubicBezierTo(
                    new PointF(cx - size * 0.38f, cy + size * 0.52f),
                    new PointF(cx - size * 0.38f, cy + size * 0.10f),
                    new PointF(cx - size * 0.18f, cy))
                .CubicBezierTo(
                    new PointF(cx - size * 0.18f, cy),
                    new PointF(cx - size * 0.28f, cy - size * 0.42f),
                    new PointF(cx, cy - size * 0.42f))
                .CloseFigure()
                .Build();


            float neckX = cx - size * 0.05f;
            float neckY = center.Y - size * 0.95f;
            float neckHeight = (cy - size * 0.42f) - neckY;
            var neck = new RectangularPolygon(neckX, neckY, size * 0.10f, neckHeight);
            var headstock = new RectangularPolygon(cx - size * 0.12f, neckY - size * 0.09f, size * 0.24f, size * 0.09f);

            image.Mutate(ctx =>
            {
                ctx.Fill(neckColor, neck);
                ctx.Fill(neckColor, headstock);

                for (int i = 1; i <= 6; i++)
                {
                    var fret = new RectangularPolygon(neckX, neckY + (neckHeight / 7f) * i, size * 0.10f, size * 0.012f);
                    ctx.Fill(Color.FromRgba(200, 180, 120, 255), fret);
                }

                ctx.Fill(bodyColor, bodyPath);
                ctx.Draw(new SolidPen(Color.FromRgba(30, 15, 5, 200), size * 0.022f), bodyPath);
                ctx.Fill(Color.FromRgba(20, 20, 20, 255), new EllipsePolygon(cx, cy + size * 0.08f, size * 0.10f));
                ctx.Fill(neckColor, new RectangularPolygon(cx - size * 0.10f, cy + size * 0.28f, size * 0.20f, size * 0.04f));
            });
        }

        public static void DrawBackground(Image<Rgba32> image, Color baseColor, Color accentColor, System.Random rng)
        {
            PointF center = GetCenterPoint(image);

            image.Mutate(ctx =>
            {
                ctx.Fill(baseColor);


                int stripeCount = rng.Next(4, 9);
                for (int i = 0; i < stripeCount; i++)
                {
                    float x = rng.Next(0, image.Width);
                    float width = rng.Next(20, 80);
                    var stripe = new RectangularPolygon(x, 0, width, image.Height);
                    ctx.Fill(Color.FromRgba(
                        accentColor.ToPixel<Rgba32>().R,
                        accentColor.ToPixel<Rgba32>().G,
                                           accentColor.ToPixel<Rgba32>().B,
                                           20), stripe);
                }


                int circleCount = rng.Next(3, 7);
                for (int i = 0; i < circleCount; i++)
                {
                    float x = rng.Next(0, image.Width);
                    float y = rng.Next(0, image.Height);
                    float radius = rng.Next(30, 120);
                    var circle = new EllipsePolygon(x, y, radius);
                    ctx.Draw(Color.FromRgba(accentColor.ToPixel<Rgba32>().R,
                                           accentColor.ToPixel<Rgba32>().G,
                                           accentColor.ToPixel<Rgba32>().B,
                                           30), 2f, circle);
                }


                int points = rng.Next(8, 14);
                float outerR = image.Width * 0.38f;
                float innerR = outerR * 0.55f;
                var starPoints = new PointF[points * 2];

                for (int i = 0; i < points * 2; i++)
                {
                    float angle = (MathF.PI * i / points) - MathF.PI / 2;
                    float r = (i % 2 == 0) ? outerR : innerR;
                    starPoints[i] = new PointF(center.X + MathF.Cos(angle) * r,
                                               center.Y + MathF.Sin(angle) * r);
                }

                ctx.Fill(Color.FromRgba(accentColor.ToPixel<Rgba32>().R,
                                       accentColor.ToPixel<Rgba32>().G,
                                       accentColor.ToPixel<Rgba32>().B,
                                       25), new Polygon(starPoints));
            });
        }


        public static void DrawHeadphones(Image<Rgba32> image, PointF center, float size, Color headphonesColor)
        {
            float cx = center.X;
            float cy = center.Y;

            var headband = new PathBuilder()
                .MoveTo(new PointF(cx - size * 0.38f, cy + size * 0.10f))
                .CubicBezierTo(
                    new PointF(cx - size * 0.38f, cy - size * 0.55f),
                    new PointF(cx + size * 0.38f, cy - size * 0.55f),
                    new PointF(cx + size * 0.38f, cy + size * 0.10f))
                .Build();

            var leftCup = new EllipsePolygon(cx - size * 0.38f, cy + size * 0.22f, size * 0.16f, size * 0.22f);

            var rightCup = new EllipsePolygon(cx + size * 0.38f, cy + size * 0.22f, size * 0.16f, size * 0.22f);

            var leftInner = new EllipsePolygon(cx - size * 0.38f, cy + size * 0.22f, size * 0.09f, size * 0.13f);

            var rightInner = new EllipsePolygon(cx + size * 0.38f, cy + size * 0.22f, size * 0.09f, size * 0.13f);

            image.Mutate(ctx =>
            {
                ctx.Draw(new SolidPen(headphonesColor, size * 0.08f), headband);
                ctx.Fill(headphonesColor, leftCup);
                ctx.Fill(headphonesColor, rightCup);
                ctx.Fill(Color.FromRgba(30, 30, 30, 180), leftInner);
                ctx.Fill(Color.FromRgba(30, 30, 30, 180), rightInner);
            });
        }


        public static void DrawMicrophone(Image<Rgba32> image, PointF center, float size, Color micColor)
        {
            float cx = center.X;
            float cy = center.Y;


            var capsule = new EllipsePolygon(cx, cy - size * 0.18f, size * 0.18f, size * 0.32f);


            var body = new RectangularPolygon(cx - size * 0.10f, cy + size * 0.10f, size * 0.20f, size * 0.22f);


            var stand = new PathBuilder()
                .MoveTo(new PointF(cx - size * 0.30f, cy + size * 0.10f))
                .CubicBezierTo(
                    new PointF(cx - size * 0.30f, cy + size * 0.48f),
                    new PointF(cx + size * 0.30f, cy + size * 0.48f),
                    new PointF(cx + size * 0.30f, cy + size * 0.10f))
                .Build();


            var pole = new RectangularPolygon(cx - size * 0.03f, cy + size * 0.32f, size * 0.06f, size * 0.22f);


            var standBase = new EllipsePolygon(cx, cy + size * 0.56f, size * 0.20f, size * 0.06f);

            image.Mutate(ctx =>
            {
                ctx.Fill(micColor, capsule);
                ctx.Fill(micColor, body);
                ctx.Draw(new SolidPen(micColor, size * 0.06f), stand);
                ctx.Fill(micColor, pole);
                ctx.Fill(micColor, standBase);

                for (int i = 0; i < 4; i++)
                {
                    float lineY = cy - size * 0.38f + (size * 0.22f / 3f) * i;
                    ctx.DrawLine(
                        new SolidPen(Color.FromRgba(0, 0, 0, 80), size * 0.02f),
                        new PointF(cx - size * 0.15f, lineY),
                        new PointF(cx + size * 0.15f, lineY));
                }
            });
        }


        private static PointF GetCenterPoint(Image<Rgba32> image)
        {
            return new PointF(image.Width / 2, image.Height / 2);
        }


        public static byte[] GenerateCover(int seed, string title, string artist, string album)
        {
            var rng = new Random(seed);

            var bg = _bgs[rng.Next(_bgs.Length)];
            var accent = _accents[rng.Next(_accents.Length)];

            using var image = CreateBlankImage(600, 600);

            DrawBackground(image, bg, accent, rng);

            int icon = rng.Next(4);
            if (icon == 0) DrawGuitar(image, new PointF(300, 280), 180, Color.White, accent);
            if (icon == 1) DrawVinylRecord(image);
            if (icon == 2) DrawHeadphones(image, new PointF(300, 310), 160, Color.White);
            if (icon == 3) DrawMicrophone(image, new PointF(300, 310), 140, Color.White);

            WriteToImage(image, artist.ToUpper(), 36, new PointF(30, 30));
            WriteToImage(image, title, 48, new PointF(30, 460));
            WriteToImage(image, album, 24, new PointF(30, 540));

            return ImageToBytes(image);
        }
    }
}
