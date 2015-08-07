using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ImageProcessor;
using ImageProcessor.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
/// The gallery requires the following directory structure.
/// 
/// </summary>
public class ImageGallery
{
    public static TextLayer Watermark;
    public static Size Size;

    static ImageGallery()
    {
        Watermark = new TextLayer();
        Watermark.Text = "Ingrid Hauss";
        Watermark.FontSize = 48;
        Watermark.FontColor = System.Drawing.Color.Black;
        Watermark.Opacity = 50;

        Size = new Size(300, 0);
    }

    public static IEnumerable<FileInfo> GetGalleryImages()
    {
        var imagesDir = HttpContext.Current.Server.MapPath("~/images/gallery/originals");
        if (!Directory.Exists(imagesDir))
        {
            return Enumerable.Empty<FileInfo>();
        }

        var fileInfos = Directory.GetFiles(imagesDir, "*.jpg").Select(s => new FileInfo(s));
        return fileInfos;
    }

    public static string ImgSrc(string savePath)
    {
        var siteDir = HttpContext.Current.Server.MapPath("~/");
        var relativeSavePath = savePath
            .Replace(siteDir, string.Empty)
            .Replace(" ", "%20")
            .Replace("\\", "/");
        return relativeSavePath;
    }

    public static string ImgTitle(FileInfo fileInfo)
    {
        var fileNameWithoutExtension = ImageGallery.FileNameWithoutExtension(fileInfo);
        var titleEndIndex = fileNameWithoutExtension.IndexOfAny("0123456789".ToCharArray());
        var imgTitle = fileNameWithoutExtension.Substring(0, titleEndIndex);
        return imgTitle;
    }

    public static string ImgDimesions(FileInfo fileInfo)
    {
        var fileNameWithoutExtension = ImageGallery.FileNameWithoutExtension(fileInfo);
        var dimensionsEndIndex = fileNameWithoutExtension.LastIndexOf("cm") + 2;
        var titleEndIndex = fileNameWithoutExtension.IndexOfAny("0123456789".ToCharArray());
        var imgDimensions = fileNameWithoutExtension.Substring(titleEndIndex, dimensionsEndIndex - titleEndIndex);
        return imgDimensions;
    }

    public static bool IsSold(FileInfo fileInfo)
    {
        var fileNameWithoutExtension = ImageGallery.FileNameWithoutExtension(fileInfo);
        return fileNameWithoutExtension.ToLower().Contains("sold");
    }

    public static string AddWatermarkToImage(FileInfo fileInfo)
    {
        var saveDirectory = Path.Combine(fileInfo.Directory.Parent.FullName, "processed");
        Directory.CreateDirectory(saveDirectory);

        var savePath = Path.Combine(saveDirectory, fileInfo.Name);
        var imgSrc = ImageGallery.ImgSrc(savePath);

        if (!File.Exists(savePath))
        {
            byte[] imageBytes = File.ReadAllBytes(fileInfo.FullName);
            using (MemoryStream inStream = new MemoryStream(imageBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory())
                    {
                        imageFactory
                            .Load(inStream)
                            .Resize(Size)
                            .Watermark(Watermark)
                            .Save(outStream);
                    }

                    Image.FromStream(outStream).Save(savePath, ImageFormat.Jpeg);
                }
            }
        }

        return imgSrc;
    }

    public static string FileNameWithoutExtension(FileInfo fileInfo)
    {
        return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
    }
}
