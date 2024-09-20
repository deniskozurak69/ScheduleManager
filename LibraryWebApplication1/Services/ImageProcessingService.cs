using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
namespace LibraryWebApplication1.Services
{
    public class ImageProcessingService
    {
        public async Task CompressAndConvertImageAsync(string imagePath)
        {
            string compressedImagePath = Path.Combine(Path.GetDirectoryName(imagePath),
                                                      Path.GetFileNameWithoutExtension(imagePath) + "_compressed.jpg");
            using (var originalImage = Image.FromFile(imagePath))
            {
                var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 50L); 
                await Task.Run(() =>
                {
                    originalImage.Save(compressedImagePath, jpegEncoder, encoderParameters);
                });
            }
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
