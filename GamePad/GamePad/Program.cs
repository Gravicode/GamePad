/* Written by Mif Masterz @ Gravicode Workshop */
using Gadgeteer.Modules.GHIElectronics;
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using System.Net;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GHI.Glide;
using GHI.Glide.Display;
using System.IO;
using Microsoft.SPOT.IO;
using System.Text;
using Microsoft.SPOT.Net.NetworkInformation;
using GHI.Networking;

namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Collapsed,
        Never,
        RootHidden
    }
}
namespace GamePad
{
    public class BoardPins
    {
        public enum PinTypes { DigitalWrite, DigitalRead, AnalogRead, AnalogWrite, None };
        public PinTypes PinType { set; get; }
        public object ThisPin { set; get; }
    }

    #region Forms
    public class Screen
    {
        public enum ScreenTypes { Splash = 0, MainMenu, Game, Gallery, Register, MyRoom, Inbox };
        public delegate void GoToFormEventHandler(ScreenTypes form, params string[] Param);
        public event GoToFormEventHandler FormRequestEvent;
        protected void CallFormRequestEvent(ScreenTypes form, params string[] Param)
        {
            // Event will be null if there are no subscribers
            if (FormRequestEvent != null)
            {
                FormRequestEvent(form, Param);
            }
        }
        protected GHI.Glide.Display.Window MainWindow { set; get; }
        public virtual void Init(params string[] Param)
        {
            //do nothing
        }

        public Screen(ref GHI.Glide.Display.Window window)
        {
            MainWindow = window;
        }
    }
    public class MainMenuForm : Screen
    {
        GHI.Glide.UI.Button BtnInbox { set; get; }

        public MainMenuForm(ref GHI.Glide.Display.Window window) : base(ref window)
        {

        }
        public void ChangeInboxCounter(int MessageCount)
        {
            if(MessageCount<=0)
                BtnInbox.Text = "Message";
            else
                BtnInbox.Text = "Message ("+MessageCount+")";
            BtnInbox.Invalidate();
        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.MenuForm));
            ArrayList control = new ArrayList();
            for (int i = 1; i < 5; i++)
            {
                var img = (GHI.Glide.UI.Image)MainWindow.GetChildByName("Img" + i);
                control.Add(img);
                GT.Picture pic = null;
                switch (i)
                {
                    case 1:
                        pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.game), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = pic.MakeBitmap();
                        break;
                    case 2:
                        pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.gallery), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = pic.MakeBitmap();
                        break;
                    case 3:
                        pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.myroom), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = pic.MakeBitmap();
                        break;
                    case 4:
                        pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.message), GT.Picture.PictureEncoding.JPEG);
                        img.Bitmap = pic.MakeBitmap();
                        break;
                }
                var Btn = (GHI.Glide.UI.Button)MainWindow.GetChildByName("Btn" + i);
                if (i == 4)
                {
                    BtnInbox = Btn;
                }
                control.Add(Btn);
                Btn.PressEvent += (sender) =>
                {
                    var btn = sender as GHI.Glide.UI.Button;
                    switch (btn.Name)
                    {
                        case "Btn1":
                            CallFormRequestEvent(ScreenTypes.Game);
                            break;
                        case "Btn2":
                            CallFormRequestEvent(ScreenTypes.Gallery);
                            break;
                        case "Btn3":
                            CallFormRequestEvent(ScreenTypes.MyRoom);
                            break;
                        case "Btn4":
                            CallFormRequestEvent(ScreenTypes.Inbox);
                            break;
                    }
                };
            }

            Glide.MainWindow = MainWindow;
            //MainWindow.Invalidate();
        }
    }
    public class SplashForm : Screen
    {
        public SplashForm(ref GHI.Glide.Display.Window window) : base(ref window)
        {

        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.SplashForm));
            var img = (GHI.Glide.UI.Image)MainWindow.GetChildByName("ImgLogo");

            GT.Picture pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.logo), GT.Picture.PictureEncoding.JPEG);
            img.Bitmap = pic.MakeBitmap();

            Glide.MainWindow = MainWindow;
            //MainWindow.Invalidate();
            Thread.Sleep(2000);
            CallFormRequestEvent(ScreenTypes.MainMenu);

        }
    }
    public class MyRoomForm : Screen
    {
        /// <summary>The TempHumid SI70 module using socket 1 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.TempHumidSI70 tempHumidSI70;

        /// <summary>The LightSense module using socket 2 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.LightSense lightSense;
        GHI.Glide.UI.TextBlock txtTemp { set; get; }
        GHI.Glide.UI.TextBlock txtHumid { set; get; }
        GHI.Glide.UI.TextBlock txtLight { set; get; }

        public MyRoomForm(ref GHI.Glide.Display.Window window, ref Gadgeteer.Modules.GHIElectronics.TempHumidSI70 tempHumidSI70, ref Gadgeteer.Modules.GHIElectronics.LightSense lightSense) : base(ref window)
        {
            this.tempHumidSI70 = tempHumidSI70;
            this.lightSense = lightSense;
        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.MyRoomForm));
            ArrayList control = new ArrayList();
            GT.Picture pic = null;

            var imgTemp = (GHI.Glide.UI.Image)MainWindow.GetChildByName("imgTemp");
            pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.temperature), GT.Picture.PictureEncoding.JPEG);
            imgTemp.Bitmap = pic.MakeBitmap();
            control.Add(imgTemp);

            var imgLight = (GHI.Glide.UI.Image)MainWindow.GetChildByName("imgLight");
            pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.light), GT.Picture.PictureEncoding.JPEG);
            imgLight.Bitmap = pic.MakeBitmap();
            control.Add(imgLight);

            var imgHumid = (GHI.Glide.UI.Image)MainWindow.GetChildByName("imgHumid");
            pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.humidity), GT.Picture.PictureEncoding.JPEG);
            imgHumid.Bitmap = pic.MakeBitmap();
            control.Add(imgHumid);
            GT.Timer timer = new GT.Timer(2000);
            var Btn = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnBack");
            control.Add(Btn);
            Btn.PressEvent += (sender) =>
            {
                timer.Stop();
                CallFormRequestEvent(ScreenTypes.MainMenu);
            };

            txtTemp = (GHI.Glide.UI.TextBlock)MainWindow.GetChildByName("txtTemp");
            txtHumid = (GHI.Glide.UI.TextBlock)MainWindow.GetChildByName("txtHumid");
            txtLight = (GHI.Glide.UI.TextBlock)MainWindow.GetChildByName("txtLight");

            Glide.MainWindow = MainWindow;

            timer.Tick += (a) =>
            {
                var measure = tempHumidSI70.TakeMeasurement();
                txtTemp.Text = Toolbox.NETMF.Tools.Round((float)measure.Temperature, 2);// + "C";
                txtHumid.Text = Toolbox.NETMF.Tools.Round((float)measure.RelativeHumidity);// + "%";
                txtLight.Text = Toolbox.NETMF.Tools.Round((float)lightSense.GetIlluminance());// + "Lux";
                //MainWindow.Graphics.DrawRectangle(txtTemp.Rect, MainWindow.BackColor, 255);
                //MainWindow.Graphics.DrawRectangle(txtHumid.Rect, MainWindow.BackColor, 255);
                //MainWindow.Graphics.DrawRectangle(txtLight.Rect, MainWindow.BackColor, 255);

                txtTemp.Invalidate();
                txtHumid.Invalidate();
                txtLight.Invalidate();
                MainWindow.Invalidate();
            };
            timer.Start();
            //MainWindow.Invalidate();
        }
    }
    public class GalleryForm : Screen
    {
        /// <summary>The Serial Camera L1 module using socket 12 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.SerialCameraL1 serialCameraL1;

        /// <summary>The SD Card module using socket 9 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.SDCard sdCard;

        /// <summary>The WiFi RS21 module using socket 3 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21;
        GHI.Glide.UI.Button BtnBack { set; get; }
        GHI.Glide.UI.Image ImgPhoto { set; get; }
        int SelectedImageIndex { set; get; } = -1;
        ArrayList ImageList { set; get; }
        public GalleryForm(ref GHI.Glide.Display.Window window, ref Gadgeteer.Modules.GHIElectronics.SerialCameraL1 serialCameraL1, ref Gadgeteer.Modules.GHIElectronics.SDCard sdCard, ref Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21) : base(ref window)
        {
            this.serialCameraL1 = serialCameraL1;
            this.sdCard = sdCard;
            this.wifiRS21 = wifiRS21;
        }

        void PopulateGallery()
        {
            ImageList = new ArrayList();
            if (sdCard.IsCardInserted && sdCard.IsCardMounted)
            {
                try
                {
                    sdCard.StorageDevice.CreateDirectory(@"Photo");
                }
                catch { }
                GT.StorageDevice storage = sdCard.StorageDevice;

                foreach (string s in storage.ListRootDirectorySubdirectories())
                {
                    Debug.Print(s);
                    if (s == "Photo")
                    {
                        foreach (string f in storage.ListFiles("\\SD\\Photo\\"))
                        {
                            var namafile = Path.GetFileNameWithoutExtension(f);
                            ImageList.Add(new string[2] { namafile, f });

                        }
                    }
                }

            }
        }

        void LoadPhoto(string FName)
        {
            if (sdCard.IsCardInserted && sdCard.IsCardMounted)
            {
                var storage = sdCard.StorageDevice;
                Bitmap bitmap = storage.LoadBitmap(FName, Bitmap.BitmapImageType.Jpeg);
                ImgPhoto.Bitmap = bitmap;
                ImgPhoto.Invalidate();
                BtnBack.Invalidate();
            }
        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.GalleryForm));
            ArrayList control = new ArrayList();
            GT.Picture pic = null;
            PopulateGallery();
            BtnBack = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnBack");
            control.Add(BtnBack);
            BtnBack.PressEvent += (sender) =>
            {
                CallFormRequestEvent(ScreenTypes.MainMenu);
            };

            ImgPhoto = (GHI.Glide.UI.Image)MainWindow.GetChildByName("ImgPhoto");
            if (ImageList.Count > 0)
            {
                pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.temperature), GT.Picture.PictureEncoding.JPEG);
                SelectedImageIndex = 0;
                LoadPhoto(((string[])ImageList[SelectedImageIndex])[1]);
            }

            control.Add(ImgPhoto);
            
            var btnPrev = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnPrev");
            control.Add(btnPrev);
            btnPrev.PressEvent += (sender) =>
            {
                if (SelectedImageIndex < 0) return;
                if (SelectedImageIndex == 0)
                    SelectedImageIndex = ImageList.Count - 1;
                else
                    SelectedImageIndex--;
                LoadPhoto(((string[])ImageList[SelectedImageIndex])[1]);
            };
            var btnNext = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnNext");
            control.Add(btnNext);
            btnNext.PressEvent += (sender) =>
            {
                if (SelectedImageIndex < 0) return;
                if (SelectedImageIndex >= ImageList.Count - 1)
                    SelectedImageIndex = 0;
                else
                    SelectedImageIndex++;
                LoadPhoto(((string[])ImageList[SelectedImageIndex])[1]);
            };

            var btnSubmit = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnSubmit");
            control.Add(btnSubmit);
            btnSubmit.PressEvent += (sender) =>
            {
                var selFile = ((string[])ImageList[SelectedImageIndex])[1];
                CallFormRequestEvent(ScreenTypes.Register, selFile);
            };

            var btnPhoto = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnPhoto");
            control.Add(btnPhoto);
            btnPhoto.PressEvent += (sender) =>
            {
                serialCameraL1.StartStreaming();
                while (!serialCameraL1.NewImageReady)
                {
                    Thread.Sleep(50);
                }

                byte[] dataImage = serialCameraL1.GetImageData();
                serialCameraL1.StopStreaming();
            
                //save to SDCard
                if (sdCard.IsCardInserted && sdCard.IsCardMounted)
                {
                    GT.StorageDevice storage = sdCard.StorageDevice;
                    var imageNumber = ImageList.Count;
                    Debug.GC(true);
                    GT.Picture picture = new GT.Picture(dataImage, GT.Picture.PictureEncoding.JPEG);

                    string pathFileName = "\\SD\\Photo\\photo-" + (imageNumber++).ToString() + ".jpg";

                    try
                    {
                        storage.WriteFile(pathFileName, picture.PictureData);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("Message: " + ex.Message + "  Inner Exception: " + ex.InnerException);
                    }
                    var namafile = Path.GetFileNameWithoutExtension(pathFileName);
                    ImageList.Add(new string[] { namafile, pathFileName });
                    SelectedImageIndex = ImageList.Count - 1;
                    LoadPhoto(((string[])ImageList[SelectedImageIndex])[1]);
                }
                else
                {
                    Glide.MessageBoxManager.Show("Please insert SD CARD", "Error");
                }

            };


            Glide.MainWindow = MainWindow;


            //MainWindow.Invalidate();
        }
    }
    public class InboxForm : Screen
    {
        private Gadgeteer.Modules.GHIElectronics.CellularRadio cellularRadio;
        private Gadgeteer.Modules.GHIElectronics.SDCard sdCard;
        private Gadgeteer.Modules.GHIElectronics.DisplayTE35 displayTE35;
        GHI.Glide.UI.Dropdown cmbInbox { set; get; }
        GHI.Glide.UI.TextBox txtMessage { set; get; }
        GHI.Glide.UI.TextBox txtReply { set; get; }
        GHI.Glide.UI.TextBox txtNoReply { set; get; }

        GHI.Glide.UI.List listMessage { set; get; }
        bool VerifySDCard()
        {
            if (!sdCard.IsCardInserted || !sdCard.IsCardMounted)
            {
                Glide.MessageBoxManager.Show("Insert SD card!", "Error", ModalButtons.Ok);
                return false;
            }

            return true;
        }
        public void PopulateList()
        {
            ArrayList options = new ArrayList();
            if (VerifySDCard())
            {
                try
                {
                    sdCard.StorageDevice.CreateDirectory(@"Inbox");
                }
                catch { }
                GT.StorageDevice storage = sdCard.StorageDevice;

                foreach (string s in storage.ListRootDirectorySubdirectories())
                {
                    Debug.Print(s);
                    if (s == "Inbox")
                    {
                        foreach (string f in storage.ListFiles("\\SD\\Inbox\\"))
                        {
                            //var x = f.Substring(f.LastIndexOf("\\")+1);
                            var namafile = Path.GetFileNameWithoutExtension(f);
                            options.Add(new object[2] { namafile, f });
                        }
                    }
                }
                if (options.Count <= 0)
                {
                    options.Add(new object[2] { "--kosong--", null });
                }
                listMessage = new GHI.Glide.UI.List(options, 300);
               
            }
        }
        public InboxForm(ref GHI.Glide.Display.Window window, ref Gadgeteer.Modules.GHIElectronics.CellularRadio cellularRadio, ref Gadgeteer.Modules.GHIElectronics.SDCard sdCard, ref Gadgeteer.Modules.GHIElectronics.DisplayTE35 displayTE35) : base(ref window)
        {
            this.cellularRadio = cellularRadio;
            this.sdCard = sdCard;
            this.displayTE35 = displayTE35;
        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.InboxForm));
            //populate inbox data
            PopulateList();
            ArrayList control = new ArrayList();
            txtMessage = (GHI.Glide.UI.TextBox)MainWindow.GetChildByName("txtMessage");
            txtReply = (GHI.Glide.UI.TextBox)MainWindow.GetChildByName("txtReply");
            txtNoReply = (GHI.Glide.UI.TextBox)MainWindow.GetChildByName("txtNoReply");
            txtMessage.TapEvent += new OnTap(Glide.OpenKeyboard);
            txtReply.TapEvent += new OnTap(Glide.OpenKeyboard);
            txtNoReply.TapEvent += new OnTap(Glide.OpenKeyboard);
            listMessage.CloseEvent += (object sender) =>
            {
                Glide.CloseList();
            };

            cmbInbox = (GHI.Glide.UI.Dropdown)MainWindow.GetChildByName("cmbInbox");
            cmbInbox.TapEvent += (object sender) =>
            {
                Glide.OpenList(sender, listMessage);
            };
            cmbInbox.ValueChangedEvent += (object sender) =>
        {
            var dropdown = (GHI.Glide.UI.Dropdown)sender;
            if (dropdown.Value == null) return;
            var data = sdCard.StorageDevice.ReadFile(dropdown.Value.ToString());
            txtMessage.Text = new string(Encoding.UTF8.GetChars(data));
            txtMessage.Invalidate();
            //Debug.Print("Dropdown value: " + dropdown.Text + " : " + dropdown.Value.ToString());
        };

            var BtnBack = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnBack");
            control.Add(BtnBack);
            BtnBack.PressEvent += (sender) =>
            {
                CallFormRequestEvent(ScreenTypes.MainMenu);
            };
            var btnSend = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnSend");
            control.Add(btnSend);
            btnSend.PressEvent += (sender) =>
            {
                //send sms...
                if (txtNoReply.Text.Length > 0 && txtReply.Text.Length > 0)
                {
                    cellularRadio.SendSms(txtNoReply.Text, txtReply.Text);
                    Glide.MessageBoxManager.Show("Message sent.");
                }
            };
            var btnClear = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnClear");
            control.Add(btnClear);
            btnClear.PressEvent += (sender) =>
            {
                txtReply.Text = string.Empty;
                txtReply.Invalidate();
            };
            Glide.MainWindow = MainWindow;
            //MainWindow.Invalidate();
        }
    }
    public class GameForm : Screen
    {
        GHI.Glide.UI.Image ImgFull { set; get; }
        Gadgeteer.Modules.GHIElectronics.DisplayTE35 displayTE35;
        bool GameIsOver = false;
        PlayerChips Turn = PlayerChips.O;
        PlayerChips Winner = PlayerChips.Blank;
        public enum PlayerChips { X, O, Blank }
        Hashtable Box { set; get; }
        Hashtable Control { set; get; }
        //imgFull
        public GameForm(ref GHI.Glide.Display.Window window, ref Gadgeteer.Modules.GHIElectronics.DisplayTE35 displayTE35) : base(ref window)
        {
            this.displayTE35 = displayTE35;
        }
        void Choose(int Pos)
        {
            if (GameIsOver) return;
            var box = (PlayerChips)Box[Pos];
            if (box == PlayerChips.Blank)
            {
                var img = (GHI.Glide.UI.Image)Control[Pos];
                Box[Pos] = Turn;
                if (Turn == PlayerChips.X)
                {
                    var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.x), GT.Picture.PictureEncoding.JPEG);
                    img.Bitmap = tmp.MakeBitmap();
                    Turn = PlayerChips.O;
                }
                else
                {
                    var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.o), GT.Picture.PictureEncoding.JPEG);
                    img.Bitmap = tmp.MakeBitmap();
                    Turn = PlayerChips.X;
                }
                img.Invalidate();
                if (CheckWin())
                {
                    GameIsOver = true;
                    //load game over
                    Bitmap bmp = null;
                    if (Winner == PlayerChips.X)
                    {
                        var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.WIN), GT.Picture.PictureEncoding.JPEG);
                        bmp = tmp.MakeBitmap();
                    }
                    else if (Winner == PlayerChips.O)
                    {
                        var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.LOSE), GT.Picture.PictureEncoding.JPEG);
                        bmp = tmp.MakeBitmap();
                    }
                    else
                    {
                        var tmp = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.draw), GT.Picture.PictureEncoding.JPEG);
                        bmp = tmp.MakeBitmap();
                    }

                    ImgFull.Visible = true;
                    ImgFull.Bitmap = bmp;
                    ImgFull.Invalidate();
                    
                    Thread.Sleep(3000);
                    CallFormRequestEvent(ScreenTypes.MainMenu);
                }
                else if (Turn == PlayerChips.O)
                {
                    Thread.Sleep(500);
                    ComMove();
                }
            }
        }

        bool EvaluatePos(PlayerChips player)
        {
            //ambil yg tinggal menang
            int BlankCounter = 0;
            int PlayerCounter = 0;
            int BlankPos = 0;
            //check horizontal
            for (int i = 1; i <= 7; i += 3)
            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;
                for (int x = i; x <= i + 2; x++)
                {
                    if ((PlayerChips)Box[x] == player) PlayerCounter++;
                    if ((PlayerChips)Box[x] == PlayerChips.Blank)
                    {
                        BlankCounter++;
                        BlankPos = x;
                    }
                }
                if (BlankCounter == 1 && PlayerCounter == 2)
                {
                    Choose(BlankPos);
                    return true;
                }
            }
            //check vertikal
            for (int i = 1; i <= 3; i++)
            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;

                for (int y = i; y <= i + 6; y += 3)
                {
                    if ((PlayerChips)Box[y] == player) PlayerCounter++;
                    if ((PlayerChips)Box[y] == PlayerChips.Blank)
                    {
                        BlankCounter++;
                        BlankPos = y;
                    }
                }
                if (BlankCounter == 1 && PlayerCounter == 2)
                {
                    Choose(BlankPos);
                    return true;
                }
            }
            //check diagonal

            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;

                for (int y = 1; y <= 9; y += 4)
                {
                    if ((PlayerChips)Box[y] == player) PlayerCounter++;
                    if ((PlayerChips)Box[y] == PlayerChips.Blank)
                    {
                        BlankCounter++;
                        BlankPos = y;
                    }
                }
                if (BlankCounter == 1 && PlayerCounter == 2)
                {
                    Choose(BlankPos);
                    return true;
                }

            }
            {
                BlankCounter = 0;
                PlayerCounter = 0;
                BlankPos = 0;
                var tmp = (PlayerChips)Box[3];
                if (tmp != PlayerChips.Blank)
                {
                    for (int y = 3; y <= 7; y += 2)
                    {
                        if ((PlayerChips)Box[y] == player) PlayerCounter++;
                        if ((PlayerChips)Box[y] == PlayerChips.Blank)
                        {
                            BlankCounter++;
                            BlankPos = y;
                        }
                    }
                    if (BlankCounter == 1 && PlayerCounter == 2)
                    {
                        Choose(BlankPos);
                        return true;
                    }
                }
            }
            return false;
        }
        void ComMove()
        {
            //cek yang langsung menang
            if (EvaluatePos(PlayerChips.O)) return;
            //halangin mush yang mau menang
            if (EvaluatePos(PlayerChips.X)) return;

            //ambil tengah
            if ((PlayerChips)Box[5] == PlayerChips.Blank)
            {
                Choose(5);
                return;
            }
            //ambil sudut
            for (int i = 1; i <= 3; i += 2)
            {
                if ((PlayerChips)Box[i] == PlayerChips.Blank)
                {
                    Choose(i);
                    return;
                }
            }
            for (int i = 7; i <= 9; i += 2)
            {
                if ((PlayerChips)Box[i] == PlayerChips.Blank)
                {
                    Choose(i);
                    return;
                }
            }
            //acak
            for (int i = 1; i <= 9; i++)
            {
                if ((PlayerChips)Box[i] == PlayerChips.Blank)
                {
                    Choose(i);
                    return;
                }
            }
        }

        bool CheckWin()
        {
            int counter = 0;
            //check horizontal
            for (int i = 1; i <= 7; i += 3)
            {
                counter = 0;
                var tmp = (PlayerChips)Box[i];
                if (tmp == PlayerChips.Blank) break;
                for (int x = i; x <= i + 2; x++)
                {
                    if (tmp != (PlayerChips)Box[x]) break;
                    counter++;
                }
                if (counter >= 3)
                {
                    Winner = tmp;
                    return true;
                }
            }
            //check vertikal
            for (int i = 1; i <= 3; i++)
            {
                counter = 0;
                var tmp = (PlayerChips)Box[i];
                if (tmp == PlayerChips.Blank) break;
                for (int y = i; y <= i + 6; y += 3)
                {
                    if (tmp != (PlayerChips)Box[y]) break;
                    counter++;
                }
                if (counter >= 3)
                {
                    Winner = tmp;
                    return true;
                }
            }
            //check diagonal

            {
                counter = 0;
                var tmp = (PlayerChips)Box[1];
                if (tmp != PlayerChips.Blank)
                {
                    for (int y = 1; y <= 9; y += 4)
                    {
                        if (tmp != (PlayerChips)Box[y]) break;
                        counter++;
                    }
                    if (counter >= 3)
                    {
                        Winner = tmp;
                        return true;
                    }
                }
            }
            {
                counter = 0;
                var tmp = (PlayerChips)Box[3];
                if (tmp != PlayerChips.Blank)
                {
                    for (int y = 3; y <= 7; y += 2)
                    {
                        if (tmp != (PlayerChips)Box[y]) break;
                        counter++;
                    }
                    if (counter >= 3)
                    {
                        Winner = tmp;
                        return true;
                    }
                }
            }
            //check all
            counter = 0;
            for (int i = 1; i <= 9; i++)
            {
                if ((PlayerChips)Box[i] != PlayerChips.Blank)
                {
                    counter++;
                }
            }
            if (counter >= 9)
            {
                Winner = PlayerChips.Blank;
                return true;
            }
            return false;
        }
        public override void Init(params string[] Param)
        {
            GameIsOver = false;
            Turn = PlayerChips.X;
            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.GameForm));
            Control = new Hashtable();
            GT.Picture pic = null;
            Box = new Hashtable();
            ImgFull = (GHI.Glide.UI.Image)MainWindow.GetChildByName("imgFull");
            ImgFull.Visible = false;
            for (int i = 1; i <= 9; i++)
            {
                var imgTemp = (GHI.Glide.UI.Image)MainWindow.GetChildByName("box" + i);
                pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.blank), GT.Picture.PictureEncoding.JPEG);
                imgTemp.Bitmap = pic.MakeBitmap();
                Control.Add(i, imgTemp);
                Box.Add(i, PlayerChips.Blank);
                imgTemp.TapEvent += (x) =>
                {
                    if (Turn == PlayerChips.X)
                    {
                        var img = x as GHI.Glide.UI.Image;
                        var PinSel = int.Parse(img.Name.Substring(3));
                        Choose(PinSel);
                    }
                };
                if (i <= 2)
                {
                    var linehor = (GHI.Glide.UI.Image)MainWindow.GetChildByName("line" + i);
                    pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.linehor), GT.Picture.PictureEncoding.JPEG);
                    linehor.Bitmap = pic.MakeBitmap();
                }
                else if (i <= 4)
                {
                    var linever = (GHI.Glide.UI.Image)MainWindow.GetChildByName("line" + i);
                    pic = new GT.Picture(Resources.GetBytes(Resources.BinaryResources.linever), GT.Picture.PictureEncoding.JPEG);
                    linever.Bitmap = pic.MakeBitmap();
                }

            }

            Glide.MainWindow = MainWindow;

            //MainWindow.Invalidate();
        }
    }
    public class RegisterForm : Screen
    {
        Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21;
        Gadgeteer.Modules.GHIElectronics.SDCard sdCard;
        string FileUrl { set; get; }
        GHI.Glide.UI.TextBox txtNama { set; get; }
        GHI.Glide.UI.TextBox txtEmail { set; get; }
        GHI.Glide.UI.TextBox txtTwitter { set; get; }

        public RegisterForm(ref GHI.Glide.Display.Window window, ref Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21, ref Gadgeteer.Modules.GHIElectronics.SDCard sdCard) : base(ref window)
        {
            this.wifiRS21 = wifiRS21;
            this.sdCard = sdCard;
        }
        public override void Init(params string[] Param)
        {

            MainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.RegisterForm));
            ArrayList control = new ArrayList();
            FileUrl = Param[0];
            var ImgPhoto = (GHI.Glide.UI.Image)MainWindow.GetChildByName("ImgPhoto");
            //load image
            if (sdCard.IsCardInserted && sdCard.IsCardMounted)
            {
                var storage = sdCard.StorageDevice;
                Bitmap bitmap = storage.LoadBitmap(FileUrl, Bitmap.BitmapImageType.Jpeg);
                ImgPhoto.Bitmap = bitmap;
                ImgPhoto.Invalidate();
            }
            txtNama = (GHI.Glide.UI.TextBox)MainWindow.GetChildByName("txtNama");
            txtEmail = (GHI.Glide.UI.TextBox)MainWindow.GetChildByName("txtEmail");
            txtTwitter = (GHI.Glide.UI.TextBox)MainWindow.GetChildByName("txtTwitter");
            txtNama.TapEvent += new OnTap(Glide.OpenKeyboard);
            txtEmail.TapEvent += new OnTap(Glide.OpenKeyboard);
            txtTwitter.TapEvent += new OnTap(Glide.OpenKeyboard);
            var btnCancel = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnCancel");
            control.Add(btnCancel);
            btnCancel.PressEvent += (sender) =>
            {

                CallFormRequestEvent(ScreenTypes.Gallery);
            };

            var btnSubmit = (GHI.Glide.UI.Button)MainWindow.GetChildByName("btnSubmit");
            control.Add(btnSubmit);
            btnSubmit.PressEvent += (sender) =>
            {
                if (wifiRS21.IsNetworkConnected)
                {
                    var storage = sdCard.StorageDevice;
                    // Create the form values
                    var FileData = sdCard.StorageDevice.ReadFile(FileUrl);
                    var qry = "?nama=" + txtNama.Text + "&email=" + txtEmail.Text + "&twitter=" + txtTwitter.Text;
                    // Create POST content                    
                    var content = Gadgeteer.Networking.POSTContent.CreateBinaryBasedContent(FileData);

                    // Create the request
                    var request = Gadgeteer.Networking.HttpHelper.CreateHttpPostRequest(
                        @"http://192.168.1.102:8001/api/upload.ashx" + qry // the URL to post to
                        , content // the form values
                        , "image/jpeg" // the mime type for an HTTP form
                    );
                    request.ResponseReceived += (HttpRequest s, HttpResponse response) =>
                    {
                        if (response.StatusCode == "200") Glide.MessageBoxManager.Show("File uploaded successfully.", "Info");
                        //var xx = response.Text;
                    };
                    // Post the form
                    request.SendRequest();
                }

            };
            Glide.MainWindow = MainWindow;

            //MainWindow.Invalidate();
        }


    }
    #endregion
    public partial class Program
    {
        
        Hashtable boardPins { set; get; }
        const string SSID = "majelis taklim";
        const string KeyWifi = "123qweasd";
        private static GHI.Glide.Display.Window MainWindow;
        private static Screen.ScreenTypes ActiveWindow { set; get; }
        private int NewMessageCounter = 0;
        Hashtable Screens { set; get; }

        #region Tunes
        void NotifySound()
        {
            Tunes.MusicNote note = new Tunes.MusicNote(Tunes.Tone.C4, 400);

            tunes.AddNote(note);

            // up
            //PlayNote(Tunes.Tone.C4);
            //PlayNote(Tunes.Tone.D4);
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.F4);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.A4);
            //PlayNote(Tunes.Tone.B4);
            //PlayNote(Tunes.Tone.C5);

            //// back down
            //PlayNote(Tunes.Tone.B4);
            //PlayNote(Tunes.Tone.A4);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.F4);
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.D4);
            //PlayNote(Tunes.Tone.C4);

            //// arpeggio
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.C5);
            //PlayNote(Tunes.Tone.G4);
            //PlayNote(Tunes.Tone.E4);
            //PlayNote(Tunes.Tone.C4);

            //tunes.Play();

            //Thread.Sleep(100);

            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.C5);
            PlayNote(Tunes.Tone.G4);
            PlayNote(Tunes.Tone.E4);
            PlayNote(Tunes.Tone.C4);

            tunes.Play();

        }
        void PlayNote(Tunes.Tone tone)
        {
            Tunes.MusicNote note = new Tunes.MusicNote(tone, 200);

            tunes.AddNote(note);
        }
        #endregion

        #region Networking

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("Network address changed");
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Debug.Print("Network availability: " + e.IsAvailable.ToString());
        }

        private void CellularRadio_OperatorRequested(CellularRadio sender, string operatorName)
        {
            Debug.Print(operatorName);
        }

        private void CellularRadio_GsmNetworkRegistrationChanged(CellularRadio sender, CellularRadio.NetworkRegistrationState networkState)
        {
            Debug.Print(networkState.ToString());
        }

        void wc_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            var state = response.StatusCode;
            string text = response.Text;
            // now that the information has been returned, disconnect from the network
            //wifiRS21.NetworkInterface.Disconnect();
        }

        // handle the network changed events
        void wifi_NetworkDown(GT.Modules.Module.NetworkModule sender, GT.Modules.Module.NetworkModule.NetworkState state)
        {
            if (state == GT.Modules.Module.NetworkModule.NetworkState.Down)
                Debug.Print("Network Up event; state = Down");
            else
                Debug.Print("Network Up event; state = Up");
        }

        void wifi_NetworkUp(GT.Modules.Module.NetworkModule sender, GT.Modules.Module.NetworkModule.NetworkState state)
        {
            if (state == GT.Modules.Module.NetworkModule.NetworkState.Up)
            {
                Debug.Print("Network Up event; state = Up");
                Debug.Print("IP:" + wifiRS21.NetworkInterface.IPAddress);
            }
            else
                Debug.Print("Network Up event; state = Down");
        }

        // borrowed from GHI's documentation
        string GetMACAddress(byte[] PhysicalAddress)
        {
            return ByteToHex(PhysicalAddress[0]) + "-"
                                + ByteToHex(PhysicalAddress[1]) + "-"
                                + ByteToHex(PhysicalAddress[2]) + "-"
                                + ByteToHex(PhysicalAddress[3]) + "-"
                                + ByteToHex(PhysicalAddress[4]) + "-"
                                + ByteToHex(PhysicalAddress[5]);
        }

        string ByteToHex(byte number)
        {
            string hex = "0123456789ABCDEF";
            return new string(new char[] { hex[(number & 0xF0) >> 4], hex[number & 0x0F] });
        }
        #endregion
        void SetupBackgroundService()
        {
            //setup wifi
            wifiRS21.DebugPrintEnabled = true;
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;           // setup events
            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            wifiRS21.NetworkDown += new GT.Modules.Module.NetworkModule.NetworkEventHandler(wifi_NetworkDown);
            wifiRS21.NetworkUp += new GT.Modules.Module.NetworkModule.NetworkEventHandler(wifi_NetworkUp);
            // use the router's DHCP server to set my network info
            if (!wifiRS21.NetworkInterface.Opened)
                wifiRS21.NetworkInterface.Open();
            if (!wifiRS21.NetworkInterface.IsDhcpEnabled)
            {
                wifiRS21.UseDHCP();
                wifiRS21.NetworkInterface.EnableDhcp();
                wifiRS21.NetworkInterface.EnableDynamicDns();
            }
            // look for avaiable networks
            var scanResults = wifiRS21.NetworkInterface.Scan();

            // go through each network and print out settings in the debug window
            foreach (GHI.Networking.WiFiRS9110.NetworkParameters result in scanResults)
            {
                Debug.Print("****" + result.Ssid + "****");
                Debug.Print("ChannelNumber = " + result.Channel);
                Debug.Print("networkType = " + result.NetworkType);
                Debug.Print("PhysicalAddress = " + GetMACAddress(result.PhysicalAddress));
                Debug.Print("RSSI = " + result.Rssi);
                Debug.Print("SecMode = " + result.SecurityMode);
            }

            // locate a specific network
            GHI.Networking.WiFiRS9110.NetworkParameters[] info = wifiRS21.NetworkInterface.Scan(SSID);
            if (info != null)
            {
                wifiRS21.NetworkInterface.Join(info[0].Ssid, KeyWifi);
                wifiRS21.UseThisNetworkInterface();
                bool res = wifiRS21.IsNetworkConnected;
                Debug.Print("Network joined");
                Debug.Print("active:" + wifiRS21.NetworkInterface.ActiveNetwork.Ssid);

                //waiting till connect...
                // After connecting, go out and get a web page. This can also be used to access web services
                //Gadgeteer.Networking.HttpRequest wc = WebClient.GetFromWeb("http://www.simpleweb.org/");
                //wc.ResponseReceived += new HttpRequest.ResponseHandler(wc_ResponseReceived);
            }

            //setup gsm 
            cellularRadio.DebugPrintEnabled = true;
            cellularRadio.PowerOn();
            new Thread(() => {
                cellularRadio.UseThisNetworkInterface("tsel", "wap", "wap123", PPPSerialModem.AuthenticationType.Pap);
                while (this.cellularRadio.NetworkInterface.IPAddress == "0.0.0.0")
                {
                    Debug.Print("Waiting on DHCP");
                    Thread.Sleep(250);
                }
            }).Start();
            /*         
            //AT Command
            cellularRadio.SendATCommand("AT+CPIN?");

            cellularRadio.SendATCommand("AT+CMGR");

            cellularRadio.SendATCommand("AT");

            // Enable GSM network registration status
            cellularRadio.SendATCommand("AT+CREG=1");

            // Enable GPRS network registration status
            cellularRadio.SendATCommand("AT+CGREG=1");

            //Set SMS mode to text
            cellularRadio.SendATCommand("AT+CMGF=1");

            cellularRadio.SendATCommand("AT+CSDH=0");
            // Set the phonebook to be stored in the SIM card
            cellularRadio.SendATCommand("AT+CPBS=\"SM\"");
            // Set the sms to be stored in the SIM card
            cellularRadio.SendATCommand("AT+CPMS=\"SM\"");
            cellularRadio.SendATCommand("AT+CNMI=2,1,0,1,0");
            //// Sets how connected lines are presented
            cellularRadio.SendATCommand("AT+COLP=1");
            //dont use this network
            
            //cellularRadio.DetachGprs();
            //cellularRadio.DisconnectTcp();
            */
            //clean up
            cellularRadio.DeleteAllSms();
           
            cellularRadio.PhoneActivityRequested += CellularRadio_PhoneActivityRequested;
            cellularRadio.IncomingCall += CellularRadio_IncomingCall;
            cellularRadio.OperatorRequested += CellularRadio_OperatorRequested;
            cellularRadio.GsmNetworkRegistrationChanged += CellularRadio_GsmNetworkRegistrationChanged;
            cellularRadio.RequestPhoneActivity();
            cellularRadio.SmsReceived += (CellularRadio sender, CellularRadio.Sms message) =>
            {
                NotifySound();
                try
                {
                    if (sdCard.IsCardInserted && sdCard.IsCardMounted)
                        sdCard.StorageDevice.CreateDirectory(@"Inbox");

                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
                if (message.Message.Length > 0)
                {
                    try
                    {
                        var ParsedMsg = message.Message.Split(' ');
                        if (ParsedMsg[0] == "REM")
                        {
                            int PinSel = int.Parse(ParsedMsg[2]);
                            if (PinSel >= (int)Gadgeteer.Socket.Pin.One && PinSel <= (int)Gadgeteer.Socket.Pin.Ten)
                            {
                                switch (ParsedMsg[1])
                                {
                                    case "DIGITALWRITE":
                                        var DW = GetBoardPin((GT.Socket.Pin)PinSel, BoardPins.PinTypes.DigitalWrite) as Gadgeteer.SocketInterfaces.DigitalOutput;
                                        DW.Write(ParsedMsg[3] == "TRUE" ? true : false);
                                        cellularRadio.SendSms(message.PhoneNumber, "WRITE PIN - " + PinSel + ":" + ParsedMsg[3]);
                                        break;
                                    case "DIGITALREAD":
                                        var DR = GetBoardPin((GT.Socket.Pin)PinSel, BoardPins.PinTypes.DigitalRead) as Gadgeteer.SocketInterfaces.DigitalInput;
                                        var r1 = DR.Read();
                                        cellularRadio.SendSms(message.PhoneNumber, "READ PIN - " + PinSel + ":" + r1);
                                        break;
                                    case "ANALOGWRITE":
                                        var AW = GetBoardPin((GT.Socket.Pin)PinSel, BoardPins.PinTypes.AnalogWrite) as Gadgeteer.SocketInterfaces.AnalogOutput;
                                        AW.WriteProportion(double.Parse(ParsedMsg[3]));
                                        cellularRadio.SendSms(message.PhoneNumber, "WRITE PIN - " + PinSel + ":" + ParsedMsg[3]);
                                        break;
                                    case "ANALOGREAD":
                                        var AR = GetBoardPin((GT.Socket.Pin)PinSel, BoardPins.PinTypes.AnalogRead) as Gadgeteer.SocketInterfaces.AnalogInput;
                                        var r2 = AR.ReadProportion();
                                        cellularRadio.SendSms(message.PhoneNumber, "READ PIN - " + PinSel + ":" + r2);
                                        break;

                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message + "-" + ex.StackTrace);
                    }
                }
                string filename = "Pesan_" + message.Timestamp.ToString("dd_MMM_yyyy_HH_mm_ss") + ".txt";
                var Pesan = Encoding.UTF8.GetBytes(message.Message);
                sdCard.StorageDevice.WriteFile("\\SD\\Inbox\\" + filename, Pesan);
                NewMessageCounter++;
                if (ActiveWindow == Screen.ScreenTypes.Inbox)
                {
                    NewMessageCounter = 0;
                    (Screens[Screen.ScreenTypes.Inbox] as InboxForm).PopulateList();
                }
                if (ActiveWindow == Screen.ScreenTypes.MainMenu)
                {
                    (Screens[Screen.ScreenTypes.MainMenu] as MainMenuForm).ChangeInboxCounter(NewMessageCounter);
                }
            };


        }

        private void CellularRadio_PhoneActivityRequested(CellularRadio sender, CellularRadio.PhoneActivity activity)
        {
            Debug.Print("activity:"+activity.ToString());
        }

        private void CellularRadio_IncomingCall(CellularRadio sender, string caller)
        {
            Debug.Print("Telp masuk");
        }

        object GetBoardPin(Gadgeteer.Socket.Pin pin, BoardPins.PinTypes tipe)
        {
            var IsNew = false;
            var CurrPin = boardPins[(int)pin] as BoardPins;
            if (CurrPin.PinType != tipe)
            {
                switch (CurrPin.PinType)
                {
                    case BoardPins.PinTypes.AnalogRead:
                        var AR = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.AnalogInput;
                        AR.Dispose();
                        break;
                    case BoardPins.PinTypes.AnalogWrite:
                        var AW = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.AnalogOutput;
                        AW.Dispose();
                        break;
                    case BoardPins.PinTypes.DigitalRead:
                        var DR = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.DigitalInput;
                        DR.Dispose();
                        break;
                    case BoardPins.PinTypes.DigitalWrite:
                        var DW = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.DigitalOutput;
                        DW.Dispose();
                        break;
                }
                IsNew = true;
                CurrPin.PinType = BoardPins.PinTypes.None;
            }
            CurrPin.PinType = tipe;
            switch (tipe)
            {
                case BoardPins.PinTypes.AnalogRead:
                    if (!IsNew)
                    {
                        var AR = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.AnalogInput;
                        return AR;
                    }
                    else
                    {
                        var AR = breadBoardX1.CreateAnalogInput((Gadgeteer.Socket.Pin)pin);
                        CurrPin.ThisPin = AR;
                        return AR;
                    }
                //break;
                case BoardPins.PinTypes.AnalogWrite:
                    if (!IsNew)
                    {
                        var AW = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.AnalogOutput;
                        return AW;
                    }
                    else
                    {
                        var AW = breadBoardX1.CreateAnalogOutput((Gadgeteer.Socket.Pin)pin);
                        CurrPin.ThisPin = AW;
                        return AW;
                    }
                //break;
                case BoardPins.PinTypes.DigitalRead:
                    if (!IsNew)
                    {
                        var DR = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.DigitalInput;
                        return DR;
                    }
                    else
                    {
                        var DR = breadBoardX1.CreateDigitalInput((Gadgeteer.Socket.Pin)pin, GT.SocketInterfaces.GlitchFilterMode.Off, GT.SocketInterfaces.ResistorMode.Disabled);
                        CurrPin.ThisPin = DR;
                        return DR;
                    }
                //break;
                case BoardPins.PinTypes.DigitalWrite:
                    if (!IsNew)
                    {
                        var DW = CurrPin.ThisPin as Gadgeteer.SocketInterfaces.DigitalOutput;
                        return DW;
                    }
                    else
                    {
                        var DW = breadBoardX1.CreateDigitalOutput((Gadgeteer.Socket.Pin)pin, false);
                        CurrPin.ThisPin = DW;
                        return DW;
                    }
                    //break;
            }
            return null;
        }

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            BLITZKRIEG 0.1 - PROGRAM CONTOH PEMANFAATAN GADGETEER SEBAGAI DEVICE MULTI-FUNGSI   
            *******************************************************************************************/
            Debug.Print("Program Started");

            //init pins
            boardPins = new Hashtable();
            for (int i = 1; i <= 10; i++)
            {
                var pin = new BoardPins();
                pin.PinType = BoardPins.PinTypes.None;
                pin.ThisPin = null;
                boardPins.Add(i, pin);
            }
            SetupBackgroundService();
            Screens = new Hashtable();
            //populate all form
            var F1 = new SplashForm(ref MainWindow);
            F1.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Splash, F1);

            var F2 = new MainMenuForm(ref MainWindow);
            F2.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.MainMenu, F2);

            var F3 = new MyRoomForm(ref MainWindow, ref tempHumidSI70, ref lightSense);
            F3.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.MyRoom, F3);

            var F4 = new InboxForm(ref MainWindow, ref cellularRadio, ref sdCard, ref displayTE35);
            F4.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Inbox, F4);

            var F5 = new GalleryForm(ref MainWindow, ref serialCameraL1, ref sdCard, ref wifiRS21);
            F5.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Gallery, F5);

            var F6 = new GameForm(ref MainWindow, ref displayTE35);
            F6.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Game, F6);

            var F7 = new RegisterForm(ref MainWindow, ref wifiRS21, ref sdCard);
            F7.FormRequestEvent += General_FormRequestEvent;
            Screens.Add(Screen.ScreenTypes.Register, F7);

            Glide.FitToScreen = true;
            GlideTouch.Initialize();

            //load splash
            LoadForm(Screen.ScreenTypes.Splash);
        }
        void LoadForm(Screen.ScreenTypes form, params string[] Param)
        {
            ActiveWindow = form;
            switch (form)
            {
                case Screen.ScreenTypes.Splash:
                case Screen.ScreenTypes.MainMenu:
                case Screen.ScreenTypes.MyRoom:
                case Screen.ScreenTypes.Inbox:
                case Screen.ScreenTypes.Gallery:
                case Screen.ScreenTypes.Game:
                case Screen.ScreenTypes.Register:

                    (Screens[form] as Screen).Init(Param);
                    break;
                default:
                    return;
                    //throw new Exception("Belum diterapkan");
            }
            if (form == Screen.ScreenTypes.Inbox) NewMessageCounter = 0;
            if (form == Screen.ScreenTypes.MainMenu)
            {
                (Screens[Screen.ScreenTypes.MainMenu] as MainMenuForm).ChangeInboxCounter(NewMessageCounter);
            }
        }
        void General_FormRequestEvent(Screen.ScreenTypes form, params string[] Param)
        {
            LoadForm(form, Param);
        }
    }
}
