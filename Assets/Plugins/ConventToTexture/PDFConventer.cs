using UnityEngine;
using UnityEngine.Events;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections;
using O2S.Components.PDFRender4NET;
using Debug = UnityEngine.Debug;

public sealed class PDFConventer
{
    PDF2TextureData config;
    MonoBehaviour holder;
    public PDFConventer(MonoBehaviour holder, PDF2TextureData config)
    {
        this.holder = holder;
        this.config = config;
    }

    private PDFFile file;
    private string pdfPath;
    private string imageFilesPath;

    public void ClearConvented()
    {
        imageFilesPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, config.imageOutputPath, config.ImageFilesPath);
        if (Directory.Exists(imageFilesPath))
        {
            Directory.Delete(imageFilesPath,true);
        }
    }

    public void StartConvent(UnityAction<string> onTextureCreated)
    {
        pdfPath = string.Format("{0}/{1}/{2}{3}", Application.streamingAssetsPath, config.PdfPath, config.pdfName.Replace(".pdf", ""), ".pdf");
        imageFilesPath = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, config.imageOutputPath, config.ImageFilesPath);

        //��ֹ�ظ�ת��
        if (Directory.Exists(imageFilesPath))
        {
            if (onTextureCreated != null) onTextureCreated(imageFilesPath);
        }
        else
        {
            GetDirectory(imageFilesPath);
            holder.StartCoroutine(OpenPDF(onTextureCreated));
        }
    }

    /// <summary>
    /// ��̨����PDF
    /// </summary>
    IEnumerator OpenPDF(UnityAction<string> onTextureCreated)
    {
        file = PDFFile.Open(pdfPath);
        string filePath;
        for (int i = 0; i < file.PageCount; i++)
        {
            Bitmap map;
            map = file.GetPageImage(i, config.definition);
            ImageFormat format = config.imageFormat ?? ImageFormat.Png;
            filePath = imageFilesPath + "/" + i + "." + format.ToString();
            if (!File.Exists(filePath))
            {
                map = SetPictureAlpha(map, (int)config.transparent);
                map.Save(filePath, format);
            }
            yield return null;
        }
        onTextureCreated(imageFilesPath);
    }
    /// <summary>  
    /// ����ͼƬ��͸����  
    /// </summary>  
    /// <param name="image">ԭͼ</param>  
    /// <param name="alpha">͸����0-255</param>  
    /// <returns></returns>  
    private Bitmap SetPictureAlpha(Bitmap image, int alpha)
    {
        //��ɫ����  
        float[][] matrixItems =
        {
                   new float[]{1,0,0,0,0},
                   new float[]{0,1,0,0,0},
                   new float[]{0,0,1,0,0},
                   new float[]{0,0,0,alpha/255f,0},
                   new float[]{0,0,0,0,1}
               };
        ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
        ImageAttributes imageAtt = new ImageAttributes();
        imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        Bitmap bmp = new Bitmap(image.Width, image.Height);
       System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
        g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAtt);
        g.Dispose();

        return bmp;
    }
    static bool ForceNewDirctory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            return true;
        }
        Directory.CreateDirectory(path);
        return false;
    }

    static bool GetDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return false;
        }
        return true;
    }
}
