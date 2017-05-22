using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.WIC;
using SharpDX.Direct3D12;
using SharpDX.Direct2D1;
using SharpDX.IO;
using System.IO;

namespace SGDManagedEngine.SGD
{
    public class H1ImageWrapper
    {
        enum H1ImageType
        {
            INVALID,
            PNG,
        }

        //public SharpDX.Direct2D1.Bitmap ToBitMap
        //{
        //    //get { return new SharpDX.Direct2D1.Bitmap(m_ImageData); }
        //}

        public H1PixelFormat PixelFormat
        {
            get { return m_PixelFormat; }
        }

        // Png type of byte array
        public byte[] ToPngByteArray
        {
            get
            {
                MemoryStream memStream = new MemoryStream();                
                return memStream.ToArray();
            }
        }

        public H1ImageWrapper(String filePath)
        {
            // to manipulate WIC objects
            ImagingFactory imagingFactory = new ImagingFactory();

            // to open the file that holds the bitmap data
            NativeFileStream fileStream = new NativeFileStream(
                filePath, NativeFileMode.Open, NativeFileAccess.Read);

            BitmapDecoder bitmapDecoder = new BitmapDecoder(imagingFactory, fileStream,
                DecodeOptions.CacheOnDemand // for the time being as we won't be needing to take advantage of special cache handling
                );

            // to retrieve the frame index 0 (static image only have one frame)
            const Int32 StaticFrame = 0;
            BitmapFrameDecode frame = bitmapDecoder.GetFrame(StaticFrame);

            // convert out bitmaps to the same pixel format for the shake of normalization
            FormatConverter converter = new FormatConverter(imagingFactory);
            converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);

            // set the pixel format
            SetPixelFormat(converter.PixelFormat);

            // having the correct pixel format, we can finally create the desired SharpDX.Direct2D1.Bitmap1            
            Int32 width = converter.Size.Width;
            Int32 height = converter.Size.Height;
            Int32 stride = converter.Size.Width * 4;
            Int32 dataSize = height * stride;

            using (var buffer = new SharpDX.DataStream(dataSize, true, true))
            {
                // copy the data to the buffer
                converter.CopyPixels(stride, buffer);                

                H1GeneralBuffer generalBuffer = H1Global<H1ManagedRenderer>.Instance.CreateGeneralBuffer(Convert.ToUInt32(dataSize));

                // mapping the data to the resource (buffer)                
                generalBuffer.WriteData(buffer.DataPointer, dataSize);

                // first create texture resource                
                //H1Texture2D textureObject = H1Global<H1ManagedRenderer>.Instance.CreateTexture2D(PixelFormat, width, height, new SharpDX.Vector4(), null);
                m_tempTextureObject = H1Global<H1ManagedRenderer>.Instance.CreateTexture2D(PixelFormat, width, height, new SharpDX.Vector4(), null);

                // copy texture region
                //H1Global<H1ManagedRenderer>.Instance.CopyTextureRegion(textureObject, generalBuffer);
                H1Global<H1ManagedRenderer>.Instance.CopyTextureRegion(m_tempTextureObject, generalBuffer);
            }                

            //https://english.r2d2rigo.es/2014/08/12/loading-and-drawing-bitmaps-with-direct2d-using-sharpdx/
            //http://stackoverflow.com/questions/9602102/loading-textures-with-sharpdx-in-windows-store-app 
            //http://sharpdx.org/wiki/class-library-api/wic/
        }

        private H1ImageType ConvertExtensionToImageType(String extension)
        {
            H1ImageType type = H1ImageType.INVALID;
            if (extension.ToUpper() == "PNG")
            {
                type = H1ImageType.PNG;
            }

            return type;
        }

        private H1PixelFormat SetPixelFormat(Guid WICFormat)
        {
            m_PixelFormat = H1PixelFormat.INVALID;

            if (WICFormat == SharpDX.WIC.PixelFormat.Format32bppPRGBA)
            {
                m_PixelFormat = H1PixelFormat.R8G8B8A8;
            }

            return m_PixelFormat;
        }        

        // image contains all image data like pixel format
        private Image m_ImageData;
        private H1ImageType m_ImageType;
        private H1PixelFormat m_PixelFormat;

        //@TODO - temporary save texture index
        public H1Texture2D m_tempTextureObject;
    }
}
