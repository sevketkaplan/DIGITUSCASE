using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WM.Extentions
{
    public class ImageExtentions
    {
        private static string slashed = Path.DirectorySeparatorChar.ToString();
        IHostingEnvironment _hostingEnvironment;

        public ImageExtentions(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<FileDetails> ImageUpload(IFormFile pic, string path)
        {
            string result = string.Empty;
            FileDetails imgdetail = new FileDetails();
            if (pic != null && pic.Length > 0)
            {
                try
                {
                    var file = pic;
                    var uploads = Path.Combine(_hostingEnvironment.WebRootPath, path);
                    uploads.Replace(@"\\", slashed);
                    var fileName = "";
                    if (file.Length > 0)
                    {
                        fileName = file.FileName;
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                        var FileExtension = Path.GetExtension(fileName);
                        var newFileName = myUniqueFileName + FileExtension;
                        fileName = Path.Combine(_hostingEnvironment.WebRootPath + slashed + "upload" + slashed + newFileName);
                        using (var fileStream = new FileStream(fileName, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                            var resultFolderArray = file.FileName.Split(slashed);
                            var sayi = resultFolderArray.Length - 1;
                            result = resultFolderArray[sayi];
                        }
                        // image resizer kod
                        using (MagickImage image = new MagickImage(fileName))
                        {
                            try
                            {

                                MagickGeometry size = new MagickGeometry(image.Width, image.Height);
                                image.Quality = 75;
                                //if (image.Width < 1920 && image.Height < 1080)
                                //{
                                //    size = new MagickGeometry(image.Width, image.Height);
                                //    size.IgnoreAspectRatio = true;
                                //    image.Resize(size);
                                //    image.Write(Path.Combine(fileName));

                                //    MagickGeometry thumb_size = new MagickGeometry(500, 250);
                                //    thumb_size.IgnoreAspectRatio = true;
                                //    image.Resize(thumb_size);
                                //    image.Write(Path.Combine(uploads + slashed + "thumb" + slashed + myUniqueFileName + "." + newFileName));


                                //}
                                //else
                                //{
                                //    size = new MagickGeometry(1920, 1080);
                                //    size.IgnoreAspectRatio = true;
                                //    image.Resize(size);
                                //    image.Write(Path.Combine(fileName));

                                //    MagickGeometry thumb_size = new MagickGeometry(500, 250);
                                //    thumb_size.IgnoreAspectRatio = true;
                                //    image.Resize(thumb_size);
                                //    image.Write(Path.Combine(uploads + slashed + "thumb" + slashed + myUniqueFileName + "." + newFileName));
                                //}
                                size.IgnoreAspectRatio = true;
                                image.Resize(size);
                                image.Write(Path.Combine(fileName));
                                imgdetail.width = image.Width;
                                imgdetail.height = image.Height;
                                imgdetail.address = newFileName;
                                imgdetail.size = file.Length * 1024;
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                                throw;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }

            }
            return imgdetail;
        }


        public async Task imageDelete(string filename)
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath + slashed + "upload" + slashed + filename);


            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            var thumbpath = Path.Combine(_hostingEnvironment.WebRootPath + slashed + "upload" + slashed + "thumb" + slashed + filename);

            if (System.IO.File.Exists(thumbpath))
            {
                System.IO.File.Delete(thumbpath);
            }
            var popup = Path.Combine(_hostingEnvironment.WebRootPath + slashed + "upload" + slashed + "popup" + slashed + filename);

            if (System.IO.File.Exists(popup))
            {
                System.IO.File.Delete(popup);
            }
        }
        public async Task<FileDetails> FileUpload(IFormFile file, string path)
        {
            var fileName = string.Empty;
            string PathDB = string.Empty;
            var newFileName = string.Empty;
            FileDetails filedetail = new FileDetails();
            if (file.Length > 0)
            {
                //Getting FileName
                fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                //Assigning Unique Filename (Guid)
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                //Getting file Extension
                var FileExtension = Path.GetExtension(fileName);
                var uploads = Path.Combine(_hostingEnvironment.WebRootPath, path);
                //save file original name
                //PathDB = Path.Combine(_hostingEnvironment.WebRootPath, path) + "/originfiles/" + $@"\{fileName}";
                //using (FileStream fsto = System.IO.File.Create(PathDB))
                //{
                //    file.CopyTo(fsto);
                //    fsto.Flush();
                //}
                // concating  FileName + FileExtension
                newFileName = myUniqueFileName + FileExtension;

                // Combines two strings into a path.
                fileName = Path.Combine(_hostingEnvironment.WebRootPath + slashed + "upload" + slashed + newFileName);
                //fileName.Replace(@"\\", slashed);

                // if you want to store path of folder in database


                filedetail.address = newFileName;
                filedetail.size = file.Length * 1024;
                filedetail.extention = FileExtension;
                filedetail.fullpathname = fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }


            }
            // this.WatermarkTask(Path.Combine(_hostingEnvironment.WebRootPath, path), newFileName);
            return filedetail;
        }
        public async Task docDelete(string docname)
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath + slashed + "upload" + slashed + docname);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

    }
}
