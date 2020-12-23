using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace prjITicket.Models.Forum
{
    public class CCropImage
    {
    }
    public class CropImageUtility
    {
        //#region -- ProcessImageCrop --
        /// <summary>
        /// Processes the image crop.
        /// </summary>
        public Dictionary<string, string> ProcessImageCrop(Bitmap currentImage, int[] sectionValue)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {
                //取得裁剪的區域座標
                int section_x1 = sectionValue[0];
                int section_x2 = sectionValue[1];
                int section_y1 = sectionValue[2];
                int section_y2 = sectionValue[3];

                //取得裁剪的圖片寬高
                int width = section_x2 - section_x1;
                int height = section_y2 - section_y1;

                //讀取原圖片
                System.Drawing.Image img = currentImage;
                Bitmap bmpImage = new Bitmap(img);
                img.Dispose();
                //從原檔案取得裁剪圖片
                System.Drawing.Image cropImage = this.CropImage(
                    bmpImage,
                    new Rectangle(section_x1, section_y1, width, height)
                );

                ////將採剪下來的圖片做縮圖處理
                //Bitmap resizeImage = this.ResizeImage(cropImage, new Size(100, 100));

                //將處理完成的圖檔儲存為JPG格式
                string fileName = Guid.NewGuid().ToString() + ".jpg";
                string savePath = "~/Content/Forum_use/ImageStore/" + fileName;


                //using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                //{
                //    //將oTarImg儲存（指定）到記憶體串流中
                //    cropImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                //    //將串流整個讀到陣列中，寫入某個路徑中的某個檔案裡
                //    using (System.IO.FileStream oFS = System.IO.File.Open(savePath, System.IO.FileMode.OpenOrCreate))
                //    { oFS.Write(stream.ToArray(), 0, stream.ToArray().Length); }
                //}



                cropImage.Save(HttpContext.Current.Server.MapPath(savePath));
                //把剪好的圖片轉回base64丟回前端
                //Bitmap bmp = new Bitmap(cropImage);

                //MemoryStream ms = new MemoryStream();
                //bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //byte[] arr = new byte[ms.Length];
                //ms.Position = 0;
                //ms.Read(arr, 0, (int)ms.Length);
                //ms.Close();
                //string CropFolder = Convert.ToBase64String(arr);


                //JSON
                result.Add("result", "Success");
                //result.Add("OriginalImage", currentImage.OriginalImage);
                result.Add("CropImage", savePath);
                //result.Add("CropFolder", CropFolder);
                //result.Add("OldCropImage", oldCropImageFileName);

                //釋放檔案資源
                cropImage.Dispose();
            }
            catch (Exception ex)
            {
                result.Add("result", "Exception");
                result.Add("msg", ex.Message);
            }
            return result;
        }

        private System.Drawing.Image CropImage(System.Drawing.Bitmap img, Rectangle cropArea)
        {
            Bitmap bmpCrop = img.Clone(cropArea, img.PixelFormat);
            return bmpCrop as System.Drawing.Image;
        }
    }


}