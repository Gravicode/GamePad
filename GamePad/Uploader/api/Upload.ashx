<%@ WebHandler Language="C#" Class="Upload" %>

using System;
using System.Web;
using Microsoft.SharePoint;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Face;
class Hasil
{
    public bool Result { set; get; }
    public string Keterangan { set; get; }
}
public static class AsyncHelpers
{
    /// <summary>
    /// Execute's an async Task<T> method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task<T> method to execute</param>
    public static void RunSync(Func<Task> task)
    {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        synch.Post(async _ =>
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                synch.InnerException = e;
                throw;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }, null);
        synch.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    /// <summary>
    /// Execute's an async Task<T> method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task<T> method to execute</param>
    /// <returns></returns>
    public static T RunSync<T>(Func<Task<T>> task)
    {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        T ret = default(T);
        synch.Post(async _ =>
        {
            try
            {
                ret = await task();
            }
            catch (Exception e)
            {
                synch.InnerException = e;
                throw;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }, null);
        synch.BeginMessageLoop();
        SynchronizationContext.SetSynchronizationContext(oldContext);
        return ret;
    }

    private class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private bool done;
        public Exception InnerException { get; set; }
        readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
        readonly Queue<Tuple<SendOrPostCallback, object>> items =
            new Queue<Tuple<SendOrPostCallback, object>>();

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("We cannot send to our same thread");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (items)
            {
                items.Enqueue(Tuple.Create(d, state));
            }
            workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!done)
            {
                Tuple<SendOrPostCallback, object> task = null;
                lock (items)
                {
                    if (items.Count > 0)
                    {
                        task = items.Dequeue();
                    }
                }
                if (task != null)
                {
                    task.Item1(task.Item2);
                    if (InnerException != null) // the method threw an exeption
                    {
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                    }
                }
                else
                {
                    workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}
public class Upload : IHttpHandler
{

    public static byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[16 * 1024];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }

    public void ProcessRequest(HttpContext context)
    {

        context.Response.ContentType = "application/json";
        try
        {
            if (context.Request.InputStream.Length > 0)
            {
                var Desc = string.Empty;
                byte[] fileData = null;
                using (Stream stream = context.Request.InputStream)
                {
                    fileData = ReadFully(stream);
                    Desc = DetectFace(fileData);
                }
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPSite site = new SPSite("http://redvelvet"))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            web.AllowUnsafeUpdates = true;
                            var list = web.Lists["Gadgeteer"] as SPPictureLibrary;
                            string Nama = context.Request.QueryString["nama"];
                            string Email = context.Request.QueryString["email"];
                            string Twitter = context.Request.QueryString["twitter"];

                            string Fname = Nama + "_" + DateTime.Now.ToString("yyyy_MM_dd") + ".jpeg";
                            var prop = new System.Collections.Hashtable();
                            prop.Add("Nama", Nama);
                            prop.Add("Email", Email);
                            prop.Add("Twitter", Twitter);
                            prop.Add("Keterangan", Desc);

                            var item = list.RootFolder.Files.Add(Fname, fileData, prop, true);

                            item.Update();
                            //list.Update();

                        }
                    }

                });
            }
            var hasil = new Hasil() { Result = true, Keterangan = "oke" };
            context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(hasil));

        }
        catch (Exception ex)
        {
            var hasil = new Hasil() { Result = false, Keterangan = ex.Message + "_" + ex.StackTrace };
            context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(hasil));
        }
    }

    string DetectFace(byte[] FileGambar)
    {
        MemoryStream ms = new MemoryStream(FileGambar);
        try
        {
            var client = new FaceServiceClient(ConfigurationManager.AppSettings["FaceApiKey"]);

            var Attr = new FaceAttributeType[] { FaceAttributeType.Age, FaceAttributeType.FacialHair, FaceAttributeType.Gender, FaceAttributeType.HeadPose, FaceAttributeType.Smile };
            Microsoft.ProjectOxford.Face.Contract.Face[] faces = AsyncHelpers.RunSync<Microsoft.ProjectOxford.Face.Contract.Face[]>(() => client.DetectAsync(ms, false, true, Attr));
            Console.WriteLine(" > " + faces.Length + " detected.");
            var Desc = string.Empty;
            int counter = 0;
            foreach (var face in faces)
            {
                counter++;
                Desc += string.Format("{0}. usia: {1}, kumis {2}, jenggot {3}, senyum {4}, kelamin {5}", counter, face.FaceAttributes.Age, face.FaceAttributes.FacialHair.Moustache, face.FaceAttributes.FacialHair.Beard, face.FaceAttributes.Smile, face.FaceAttributes.Gender);
            }
            return Desc;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
        return null;


    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }



}