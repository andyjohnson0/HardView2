using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;


namespace uk.andyjohnson.HardView2
{
    public partial class ImagePreviewForm : Form
    {
        public ImagePreviewForm(string fileOrDirPath)
        {
            // Enter full-screen mode
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // Initial window properties.
            this.BackColor = Color.Black;
            this.Text = "HardView2";
            this.Load += this.ImagePreviewForm_Load;
            // Other properties are set by Load event handler.

            //
            SetCurrent(fileOrDirPath);
            if ((currentFile == null) || !currentFile.Exists)
            {
                Application.Exit();
                return;
            }
        }


        private bool scaleToFit = true;
        private int zoom = 0;
        private bool displayInfo = false;
        private Font infoFont = new Font("Arial", 10F);
        private Font toastFont = new Font("Arial", 12F);

        private Size currentPan = new Size(0, 0);   // Accumulated position afer drags
        private Size currentDrag = new Size(0, 0);  // Position delta during drag

        private FileInfo currentFile;
        private DirectoryInfo currentDirectory;

        private bool cursorHidden;
        private Timer cursorTimer;


        private void ImagePreviewForm_Load(object sender, EventArgs e)
        {
            var resources = new ComponentResourceManager(typeof(ImagePreviewForm));
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));

            this.Paint += this.ImagePreviewForm_Paint;
            this.KeyDown += this.ImagePreviewForm_KeyDown;
            this.MouseDown += this.ImagePreviewForm_MouseDown;
            this.MouseEnter += this.ImagePreviewForm_MouseEnter;
            this.MouseLeave += this.ImagePreviewForm_MouseLeave;
            this.MouseMove += this.ImagePreviewForm_MouseMove;
            this.MouseUp += this.ImagePreviewForm_MouseUp;
            this.MouseWheel += ImagePreviewForm_MouseWheel;

            InitCursor();

            DoRedraw();
        }


        #region Cursor handling

        private void InitCursor()
        {
            cursorHidden = false;
            cursorTimer = new Timer();
            cursorTimer.Tick += Timer_Tick;
            cursorTimer.Interval = 4000;
            cursorTimer.Start();
        }


        private void RestartCursorTimer()
        {
            cursorTimer.Stop();
            cursorTimer.Start();
        }

        private void ShowCursor(bool restartTimer = true)
        {
            if (cursorHidden)
            {
                cursorHidden = false;
                Cursor.Show();
            }
            if (restartTimer)
                RestartCursorTimer();
            else
                cursorTimer.Stop();
        }

        private void HideCursor()
        {
            if (!cursorHidden)
            {
                cursorHidden = true;
                Cursor.Hide();
            }
            RestartCursorTimer();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            HideCursor();
        }


        private void ShowMenu(ContextMenuStrip menu)
        {
            if (cursorHidden)
                Cursor.Position = new Point(this.Width / 2, this.Height / 2);
            ShowCursor(restartTimer: false);
            menu.Closed += (o, e) => { RestartCursorTimer(); };
            menu.Show(this, Cursor.Position);
        }

        #endregion Cursor handling



        #region Navigation

        private void ImagePreviewForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.Right:
                    if (currentFile != null)
                    {
                        if (!e.Shift)
                        {
                            SetCurrent(GetNextFile(currentFile, +1));
                            ResetSizeAndZoom();
                            DoRedraw();
                        }
                        else
                        {
                            currentDrag.Width += 50;
                            DoRedraw();
                        }
                    }
                    break;
                case Keys.Left:
                    if (currentFile != null)
                    {
                        if (!e.Shift)
                        {
                            SetCurrent(GetNextFile(currentFile, -1));
                            ResetSizeAndZoom();
                            DoRedraw();
                        }
                        else
                        {
                            currentDrag.Width -= 50;
                            DoRedraw();
                        }
                    }
                    break;
                case Keys.Return:
                    if (currentFile != null)
                    {
                        var files = GetFiles(currentFile.Directory);
                        if (files.Length > 1)
                        { 
                            var rnd = new Random();
                            while(true)
                            {
                                var newFile = files[rnd.Next(files.Length)];
                                if (newFile.FullName != currentFile.FullName)
                                {
                                    ResetSizeAndZoom();
                                    SetCurrent(newFile);
                                    DoRedraw();
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case Keys.Home:
                    if (currentFile != null)
                    {
                        var files = GetFiles(currentFile.Directory);
                        if (files.Length > 0)
                        {
                            SetCurrent(files[0]);
                            ResetSizeAndZoom();
                            DoRedraw();
                        }
                    }
                    break;
                case Keys.End:
                    if (currentFile != null)
                    {
                        var files = GetFiles(currentFile.Directory);
                        if (files.Length > 0)
                        {
                            SetCurrent(files[files.Length - 1]);
                            ResetSizeAndZoom();
                            DoRedraw();
                        }
                    }
                    break;
                case Keys.Space:
                    if (currentFile != null)
                    {
                        var newCurrent = GetNextFile(currentFile, +1);
                        var tempDi = currentDirectory.CreateSubdirectory("temp");
                        var newPath = Path.Combine(tempDi.FullName, currentFile.Name);
                        File.Move(currentFile.FullName, newPath);
                        if (newCurrent.FullName != currentFile.FullName)
                            SetCurrent(newCurrent);
                        else
                            SetCurrent((FileInfo)null);
                        ResetSizeAndZoom();
                        DoRedraw("File move to temp folder");
                    }
                    break;
                case Keys.Delete:
                    if (currentFile != null)
                    {
                        var newCurrent = GetNextFile(currentFile, +1);
                        var recycleBin = new Shell32.Shell().NameSpace(10);
                        recycleBin.MoveHere(currentFile.FullName);
                        if (newCurrent.FullName != currentFile.FullName)
                            SetCurrent(newCurrent);
                        else
                            SetCurrent((FileInfo)null);
                        ResetSizeAndZoom();
                        DoRedraw("File moved to recycle bin");
                    }
                    break;
                case Keys.I:
                    displayInfo = !displayInfo;
                    DoRedraw();
                    break;
                case Keys.R:
                    // Reset all
                    ResetSizeAndZoom();
                    DoRedraw();
                    break;
                case Keys.S:
                    // Toggle scale to fit
                    SetCurrent(currentFile);  // Release cached image
                    scaleToFit = !scaleToFit;
                    DoRedraw();
                    break;
                case Keys.Up:
                    if (!e.Shift)
                    {
                        if (currentDirectory != null)
                        {
                            if (currentDirectory.Parent != null)
                            {
                                ResetSizeAndZoom();
                                SetCurrent(currentDirectory.Parent);
                                if (GetFiles(currentDirectory).Length > 0)
                                {
                                    DoRedraw();
                                }
                                {
                                    DoRedraw(currentDirectory.FullName + " - No pictures found");
                                    ShowSubDirMenu();
                                }
                            }
                            else
                            {
                                ShowDrivesMenu();
                            }
                        }
                    }
                    else if (currentFile != null)
                    {
                        currentDrag.Height -= 50;
                        DoRedraw();
                    }
                    break;
                case Keys.Down:
                    if (!e.Shift)
                    {
                        if (currentDirectory != null)
                        {
                            ShowSubDirMenu();
                        }
                    }
                    else if (currentFile != null)
                    {
                        currentDrag.Height += 50;
                        DoRedraw();
                    }
                    break;
                case Keys.PageUp:
                    if (currentFile != null)
                    {
                        // Zoom out
                        zoom = Math.Max(0, zoom - 50);
                        DoRedraw();
                    }
                    break;
                 case Keys.PageDown:
                    if (currentFile != null)
                    {
                        // Zoom in
                        zoom += 50;
                        DoRedraw();
                    }
                    break;
               case Keys.F:
                    var dlg = new FolderBrowserDialog();
                    if (currentFile != null)
                        dlg.SelectedPath = currentFile.Directory.FullName;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        SetCurrent(dlg.SelectedPath);
                        DoRedraw();
                    }
                    break;
                case Keys.M:
                    ShowApplicationMenu();
                    break;
                default:
                    e.Handled = false;
                    return;
            }
        }


        private void ResetSizeAndZoom()
        {
            zoom = 0;
            scaleToFit = true;
            startOfDrag = null;
            currentDrag = new Size(0, 0);
            currentPan = new Size(0, 0);
        }


        private void ShowSubDirMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripLabel("Select Sub-Directory"));
            menu.Items[0].Font = new Font(menu.Items[0].Font, FontStyle.Bold);
            menu.Items.Add(new ToolStripSeparator());
            foreach (var subDir in GetDirectories(currentDirectory))
            {
                if ((subDir.Attributes & FileAttributes.System) == 0)
                    menu.Items.Add(subDir.Name);
            }
            if (menu.Items.Count > 2)
            {
                menu.ItemClicked += SubDirMenu_ItemClicked;
                ShowMenu(menu);
            }
        }


        private void ShowDrivesMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripLabel("Select Drive"));
            menu.Items[0].Font = new Font(menu.Items[0].Font, FontStyle.Bold);
            menu.Items.Add(new ToolStripSeparator());
            foreach (var drive in DriveInfo.GetDrives())
            {
                menu.Items.Add(drive.Name);
            }
            menu.ItemClicked += SubDirMenu_ItemClicked;
            ShowMenu(menu);
        }


        private void SubDirMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (currentDirectory != null)
            {
                var newDir = Path.Combine(currentDirectory.FullName, e.ClickedItem.Text);
                SetCurrent(new DirectoryInfo(newDir));
                DoRedraw();
            }
        }


        private void ShowApplicationMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripLabel("HardView2"));
            menu.Items[0].Font = new Font(menu.Items[0].Font, FontStyle.Bold);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(IsRegistered() ? "Unregister" : "Register").Tag = 0;
            menu.Items.Add("About").Tag = 1;
            menu.ItemClicked += SubApplicationMenu_ItemClicked;
            ShowMenu(menu);
        }


        private void SubApplicationMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            e.ClickedItem.GetCurrentParent().Hide();
            switch((int)e.ClickedItem.Tag)
            {
                case 0:
                    ToggleRegistration();
                    break;
                case 1:
                    MessageBox.Show("HardView2 by Andrew Johnson - andyjohnson.uk", "About HardView2",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        #endregion Navigation


        #region Shell integration

        private const string appRegKey = @"Software\classes\uk.andyjohnson\HardView2";
        private const string shellRegKey = @"SOFTWARE\Classes\Directory\shell\Open with HardView2";

        private bool IsRegistered()
        {
            using (var k = Registry.CurrentUser.OpenSubKey(appRegKey))
            {
                return k != null;
            }
        }


        private void ToggleRegistration()
        {
            if (IsRegistered())
            {
                Registry.CurrentUser.DeleteSubKeyTree(appRegKey);
                Registry.CurrentUser.DeleteSubKeyTree(shellRegKey);
            }
            else
            {
                var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
                var cmd = string.Format("\"{0}\" \"%1\"", exePath);

                using (var k = Registry.CurrentUser.CreateSubKey(appRegKey))
                {
                    using (var sk = k.CreateSubKey(@"shell\play\command"))
                    {
                        sk.SetValue("", cmd);
                    }
                }
                using (var k = Registry.CurrentUser.CreateSubKey(shellRegKey))
                {
                    using (var sk = k.CreateSubKey(@"command"))
                    {
                        sk.SetValue("", cmd);
                    }
                }
            }
        }


        #endregion Shell integration




        #region Image viewing

        private Image currentImage = null;  // Cached image.
        private string currentMessage = null;

        private void DoRedraw(string message = null)
        {
            this.currentMessage = message;
            this.Invalidate();
        }


        private void ImagePreviewForm_Paint(object sender, PaintEventArgs e)
        {
            if ((currentFile != null) && currentFile.Exists)
            {
                var img = (currentImage != null) ? currentImage:  Image.FromFile(currentFile.FullName);
                Point pt;
                if (scaleToFit)
                {
                    var img2 = ScaleImage(img, this.Size.Width + zoom, this.Size.Height + zoom);
                    img.Dispose();
                    img = img2;
                }
                pt = new Point((this.Width / 2) - (img.Width / 2) + currentPan.Width + currentDrag.Width,
                               (this.Height / 2) - (img.Height / 2) + currentPan.Height + currentDrag.Height);
                e.Graphics.DrawImage(img, pt);
                currentImage = img;

                if (displayInfo)
                {
                    ShowInfo(e.Graphics);
                }
            }
            else
            {
                e.Graphics.Clear(this.BackColor);
                currentMessage = currentDirectory.FullName + " - No pictures found";
            }

            if (!string.IsNullOrEmpty(currentMessage))
            {
                ShowToast(e.Graphics, currentMessage);
                currentMessage = null;
            }
        }


        // Stolen from https://stackoverflow.com/a/6501997/67316
        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }


        private void ShowInfo(Graphics g)
        {
            const long mb = 1024L * 1024L;
            const float marginX = 20F;
            const float marginY = 20F;
            const float paddingX = 5F;
            const float paddingY = 5F;

            var line1 = currentFile.FullName;
            var line1Size = g.MeasureString(line1, infoFont);
            var line2 = (currentFile.Length >= mb) ? String.Format("{0:0.0}MB", (float)currentFile.Length / (float)mb) : String.Format("{0:0.0}KB", (float)currentFile.Length / 1024F);
            var line2Size = g.MeasureString(line2, infoFont);
            var line3 = string.Format("{0}x{1}", currentImage.Width, currentImage.Height);
            var line3Size = g.MeasureString(line3, infoFont);

            var width = Math.Max(line1Size.Width, Math.Max(line2Size.Width, line3Size.Width)) + (paddingX * 2F);
            var height = line1Size.Height + line2Size.Height + line3Size.Height + (paddingY * 2F);
            g.FillRectangle(Brushes.Black, marginX, marginY, width, height);

            var y = marginY + paddingY;
            g.DrawString(line1, infoFont, Brushes.White, marginX + paddingX, y);
            y += line1Size.Height;
            g.DrawString(line2, infoFont, Brushes.White, marginX+ paddingX, y);
            y += line2Size.Height;
            g.DrawString(line3, infoFont, Brushes.White, marginX+ paddingX, y);
            y += line3Size.Height;
        }


        private void ShowToast(
            Graphics g,
            string message)
        {
            var msgSize = g.MeasureString(message, toastFont);
            var msgX = (this.Width - (int)msgSize.Width) / 2;
            var msgY = 3 * (int)msgSize.Height;

            g.FillRectangle(Brushes.Black,
                            msgX - (int)msgSize.Height, msgY - (int)msgSize.Height,
                            ((int)msgSize.Width) + (2 * (int)msgSize.Height), 3 * (int)msgSize.Height);
            g.DrawString(message, toastFont, Brushes.White, msgX, msgY);
        }


        #endregion Image viewing


        #region Pan/zoom

        private Point? startOfDrag = null;

        private void ImagePreviewForm_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom += (2 * e.Delta);
            if (zoom < 0)
                zoom = 0;
            DoRedraw();
        }


        private void ImagePreviewForm_MouseDown(object sender, MouseEventArgs e)
        {
            startOfDrag = new Point(e.X, e.Y);
        }

        private void ImagePreviewForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (startOfDrag.HasValue)
            {
                currentPan.Width += currentDrag.Width;
                currentPan.Height += currentDrag.Height;
            }
            startOfDrag = null;
            currentDrag = new Size(0, 0);
        }

        private void ImagePreviewForm_MouseMove(object sender, MouseEventArgs e)
        {
            ShowCursor();

            if (startOfDrag.HasValue)
            {
                currentDrag.Width = e.X - startOfDrag.Value.X;
                currentDrag.Height = e.Y - startOfDrag.Value.Y;
                DoRedraw();
            }
        }

        private void ImagePreviewForm_MouseEnter(object sender, EventArgs e)
        {
            startOfDrag = null;
        }

        private void ImagePreviewForm_MouseLeave(object sender, EventArgs e)
        {
            if (startOfDrag.HasValue)
            {
                currentPan.Width += currentDrag.Width;
                currentPan.Height += currentDrag.Height;
            }
            startOfDrag = null;
            currentDrag = new Size(0, 0);
        }

        #endregion Pan/zoom


        #region Image file navigation

        private void SetCurrent(string fileOrDirPath)
        {
            try
            {
                if (Directory.Exists(fileOrDirPath))
                {
                    SetCurrent(new DirectoryInfo(fileOrDirPath));
                }
                else if (File.Exists(fileOrDirPath))
                {
                    SetCurrent(new FileInfo(fileOrDirPath));
                }
                else
                {
                    SetCurrent((FileInfo)null);
                }
            }
            catch (Exception)
            {
                SetCurrent((FileInfo)null);
            }
        }


        private void SetCurrent(DirectoryInfo di)
        {
            currentDirectory = di;
            var files = GetFiles(di);
            SetCurrent((files.Count() > 0) ? files.First() : null);
        }


        private void SetCurrent(FileInfo fi)
        {
            currentFile = fi;
            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
            }
        }


        private static FileInfo GetNextFile(FileInfo fi, int offset)
        {
            var files = GetFiles(fi.Directory);
            if (files.Length > 0)
            {
                int i = GetPositionOf(fi, files);
                if (i != -1)
                {
                    i += offset;
                    if (i < 0)
                        i = files.Length - 1;
                    else if (i >= files.Length)
                        i = 0;
                    return files[i];
                }
            }
            return null;
        }


        private static FileInfo[] GetFiles(DirectoryInfo di)
        {
            var files = di.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                    .Where(s => s.FullName.EndsWith(".jpg") || s.FullName.EndsWith(".jpeg") || s.FullName.EndsWith(".png"))
                                    .OrderBy(s => s.Name);
            return files.ToArray();
        }


        private static DirectoryInfo[] GetDirectories(DirectoryInfo di)
        {
            var subdirs = di.GetDirectories("*.*").OrderBy(s => s.Name);
            return subdirs.ToArray();
        }


        private static int GetPositionOf(FileInfo fi, FileInfo[] files)
        {
            for(int i = 0; i < files.Length; i++)
            {
                if (files[i].FullName == fi.FullName)
                    return i;
            }
            return -1;
        }

        #endregion Image file navigation
    }
}
