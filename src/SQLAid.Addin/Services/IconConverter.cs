using SQLAid.Services.Extension;
using stdole;
using System.Drawing;
using System.Windows.Forms;

namespace SQLAid.Services
{
    public class IconConverter : AxHost
    {
        public IconConverter() : base(string.Empty)
        {
        }

        public static IPictureDisp GetPictureDispFromImage(Image image)
        {
            return GetIPictureDispFromPicture(image).As<IPictureDisp>();
        }
    }
}